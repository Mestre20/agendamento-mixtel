using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Reflection;

namespace agendamento
{
    public partial class TecnicoDetalhesPage : ContentPage
    {
        private const string SpreadsheetId = "1fRKzS5sAfYuWd5QMm2uwzRjL_xtTajfAdTzWIWRYWOs";
        private const string MetaSpreadsheetId = "1jSgrPahT7hDWkR7SLbZTa3x0n3wpnmjdah4qD5TMu6Q";
        private const string SheetName = "Tecnicos";
        private string _tecnico;
        private string _imageFilePath;

        public TecnicoDetalhesPage(string tecnico)
        {
            InitializeComponent();
            _tecnico = tecnico;
            Title = tecnico;

            // Definir o caminho para salvar a imagem localmente
            _imageFilePath = Path.Combine(FileSystem.AppDataDirectory, $"{tecnico}_photo.jpg");

            // Carregar a imagem do técnico
            LoadTecnicoImage();

            // Adicionar o evento de clique para atualizar a imagem quando o usuário clicar
            TecnicoImage.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await UpdateTecnicoImageAsync())
            });
        }

        private void LoadTecnicoImage()
        {
            // Verificar se a imagem existe localmente
            if (File.Exists(_imageFilePath))
            {
                // Carregar a imagem localmente
                TecnicoImage.Source = ImageSource.FromFile(_imageFilePath);
            }
            else
            {
                // Caso não exista, baixar e salvar a imagem
                UpdateTecnicoImageAsync();
            }
        }

        private async Task UpdateTecnicoImageAsync()
        {
            try
            {
                // URLs das imagens dos técnicos no Google Drive
                string imageUrl = _tecnico switch
                {
                    "Jessica" => "https://drive.google.com/uc?export=view&id=1IbWpjgxQGezqJeGo7RF0Sg2uUcJV0Q-5",
                    "Guilherme" => "https://drive.google.com/uc?export=view&id=1OH7I2kf2u24QEhCrUXna0y3AB5SIAgd3",
                    "Paulo" => "https://drive.google.com/uc?export=view&id=1rV-JWyjYZ_JY4SJ0NswPNTUlXEP5ZchI",
                    "Gerson" => "https://drive.google.com/uc?export=view&id=1LZyAJWmMDtJjxraC50JkxWsTzqFbduMO",
                    "Vinicius" => "https://drive.google.com/uc?export=view&id=1d8hIEAxKX3vJCdWto7B6vtVUtRIbfaGA",
                    "Wanderson" => "https://drive.google.com/uc?export=view&id=1YqcycujG8EG-dCuKehjgOKJOwAsDZOyq",
                    _ => null
                };

                if (string.IsNullOrEmpty(imageUrl))
                {
                    await DisplayAlert("Erro", "Foto do técnico não encontrada.", "OK");
                    return;
                }

                // Fazer o download da imagem
                var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                // Salvar a imagem localmente
                File.WriteAllBytes(_imageFilePath, imageBytes);

                // Carregar a imagem salva localmente
                TecnicoImage.Source = ImageSource.FromFile(_imageFilePath);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        private async void OnOsButtonClicked(object sender, EventArgs e)
        {
            // Salvar o conteúdo original da página
            var originalContent = Content;

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 20,
                Padding = new Thickness(20),
                BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent
            };

            // Título
            stackLayout.Children.Add(new Label
            {
                Text = "Selecione a Data",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#2b2b2b"),
                HorizontalOptions = LayoutOptions.Center
            });

            // Subtítulo
            stackLayout.Children.Add(new Label
            {
                Text = "Escolha uma data para visualizar as O.S. feitas",
                FontSize = 16,
                TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#666666"),
                HorizontalOptions = LayoutOptions.Center
            });

            // Frame para o DatePicker
            var datePickerFrame = new Frame
            {
                CornerRadius = 12,
                HasShadow = true,
                BorderColor = Microsoft.Maui.Graphics.Colors.Transparent,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.White,
                Padding = new Thickness(20),
                Margin = new Thickness(0, 10),
                HorizontalOptions = LayoutOptions.Fill,
                MaximumWidthRequest = 300
            };

            var datePicker = new DatePicker
            {
                MinimumDate = DateTime.Now.AddYears(-1),
                MaximumDate = DateTime.Now,
                Format = "dd/MM/yyyy",
                Date = DateTime.Now,
                FontSize = 20,
                TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#2b2b2b"),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            };

            datePickerFrame.Content = datePicker;
            stackLayout.Children.Add(datePickerFrame);

            // Botão Buscar
            var buscarFrame = new Frame
            {
                CornerRadius = 12,
                HasShadow = true,
                BorderColor = Microsoft.Maui.Graphics.Colors.Transparent,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.White,
                Padding = 0,
                HeightRequest = 50,
                Margin = new Thickness(0, 10),
                MaximumWidthRequest = 300,
                HorizontalOptions = LayoutOptions.Center
            };

            var buscarButton = new Button
            {
                Text = "Buscar O.S.",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Microsoft.Maui.Graphics.Colors.White,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                HorizontalOptions = LayoutOptions.Fill
            };

            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Microsoft.Maui.Graphics.Color.FromArgb("#4CAF50"), 0.0f),
                    new GradientStop(Microsoft.Maui.Graphics.Color.FromArgb("#45a049"), 1.0f)
                }
            };

            buscarButton.Background = gradient;
            buscarButton.Clicked += async (s, args) =>
            {
                DateTime selectedDate = datePicker.Date;
                await FetchOsForDateAndTecnico(selectedDate, Title);
            };

            buscarFrame.Content = buscarButton;
            stackLayout.Children.Add(buscarFrame);

            // Botão Voltar
            var voltarFrame = new Frame
            {
                CornerRadius = 12,
                HasShadow = true,
                BorderColor = Microsoft.Maui.Graphics.Colors.Transparent,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.White,
                Padding = 0,
                HeightRequest = 50,
                Margin = new Thickness(0, 10),
                MaximumWidthRequest = 300,
                HorizontalOptions = LayoutOptions.Center
            };

            var voltarButton = new Button
            {
                Text = "Voltar",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Microsoft.Maui.Graphics.Colors.White,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                HorizontalOptions = LayoutOptions.Fill
            };

            var voltarGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Microsoft.Maui.Graphics.Color.FromArgb("#2196F3"), 0.0f),
                    new GradientStop(Microsoft.Maui.Graphics.Color.FromArgb("#1976D2"), 1.0f)
                }
            };

            voltarButton.Background = voltarGradient;
            voltarButton.Clicked += (s, e) =>
            {
                // Em vez de criar um novo layout, vamos restaurar o conteúdo original
                Content = originalContent;

                // Recarregar a imagem do técnico
                LoadTecnicoImage();

                // Atualizar o nome do técnico
                TecnicoNome.Text = _tecnico;
            };

            voltarFrame.Content = voltarButton;
            stackLayout.Children.Add(voltarFrame);

            Content = stackLayout;
        }

        private async Task FetchOsForDateAndTecnico(DateTime date, string tecnico)
        {
            // Salvar o conteúdo original
            var originalContent = Content;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("credencial.json"));

                if (string.IsNullOrEmpty(resourceName))
                {
                    throw new FileNotFoundException("Credencial não encontrada. Verifique se o arquivo está marcado como recurso incorporado.");
                }

                using Stream stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    throw new FileNotFoundException($"Credencial não encontrada: {resourceName}. Verifique se o arquivo está marcado como recurso incorporado.");
                }

                var credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "agendamento"
                });

                var range = $"{SheetName}!A:C";
                SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;

                if (values != null && values.Count > 0)
                {
                    var result = values.Where(row => row.Count >= 3 &&
                                                     row[1].ToString().Equals(tecnico, StringComparison.OrdinalIgnoreCase) &&
                                                     DateTime.TryParse(row[2].ToString(), out DateTime rowDate) &&
                                                     rowDate.Date == date.Date)
                                       .Select(row => row[0].ToString())
                                       .ToList();

                    var stackLayout = new StackLayout
                    {
                        Padding = new Thickness(20),
                        Spacing = 15,
                        Children =
                        {
                            new Label
                            {
                                Text = "O.S Feitas",
                                FontSize = 24,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#2b2b2b"),
                                HorizontalOptions = LayoutOptions.Center
                            }
                        }
                    };

                    foreach (var os in result)
                    {
                        stackLayout.Children.Add(new Label
                        {
                            Text = os,
                            FontSize = 18,
                            TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#333333"),
                            HorizontalOptions = LayoutOptions.Center
                        });
                    }

                    stackLayout.Children.Add(new Label
                    {
                        Text = $"Total de O.S Feitas: {result.Count}",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#2b2b2b"),
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    });

                    // Botão Voltar
                    var backButton = new Button
                    {
                        Text = "Voltar",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Microsoft.Maui.Graphics.Colors.White,
                        BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 20, 0, 0),
                        HeightRequest = 50,
                        MinimumWidthRequest = 200
                    };

                    var backGradient = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 0),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop(Microsoft.Maui.Graphics.Color.FromArgb("#2196F3"), 0.0f),
                            new GradientStop(Microsoft.Maui.Graphics.Color.FromArgb("#1976D2"), 1.0f)
                        }
                    };

                    backButton.Background = backGradient;
                    backButton.Clicked += (s, e) =>
                    {
                        // Restaurar o conteúdo original
                        Content = originalContent;

                        // Recarregar a imagem do técnico
                        LoadTecnicoImage();

                        // Atualizar o nome do técnico
                        TecnicoNome.Text = _tecnico;

                        // Chamar OnOsButtonClicked para mostrar o DatePicker
                        OnOsButtonClicked(this, EventArgs.Empty);
                    };

                    stackLayout.Children.Add(backButton);

                    Content = stackLayout;
                }
                else
                {
                    await DisplayAlert("Erro", "Nenhuma O.S encontrada.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
                // Em caso de erro, restaurar o conteúdo original
                Content = originalContent;
            }
        }

        private async void OnMetaButtonClicked(object sender, EventArgs e)
        {
            await FetchMetaForTecnico(_tecnico);
        }

        private async Task FetchMetaForTecnico(string tecnico)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("credencial.json"));

                if (string.IsNullOrEmpty(resourceName))
                {
                    throw new FileNotFoundException("Credencial não encontrada. Verifique se o arquivo está marcado como recurso incorporado.");
                }

                using Stream stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    throw new FileNotFoundException($"Credencial não encontrada: {resourceName}. Verifique se o arquivo está marcado como recurso incorporado.");
                }

                var credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "agendamento"
                });

                var spreadsheet = await service.Spreadsheets.Get(MetaSpreadsheetId).ExecuteAsync();
                var lastSheet = spreadsheet.Sheets.LastOrDefault();
                if (lastSheet == null)
                {
                    throw new Exception("Nenhuma página encontrada na planilha de metas.");
                }
                string lastSheetName = lastSheet.Properties.Title;

                string cellAddress = tecnico switch
                {
                    "Paulo" => "R4",
                    "Guilherme" => "R6",
                    "Wanderson" => "R8",
                    "Vinicius" => "R10",
                    "Jessica" => "R12",
                    "Gerson" => "R14",
                    _ => throw new Exception("Técnico não encontrado.")
                };
                var range = $"{lastSheetName}!{cellAddress}:{cellAddress}";

                SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(MetaSpreadsheetId, range);
                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;

                if (values != null && values.Count > 0 && values[0].Count > 0)
                {
                    string metaValue = values[0][0].ToString();
                    await DisplayAlert("Meta do Técnico", $"Meta de {tecnico}: {metaValue}", "OK");
                }
                else
                {
                    await DisplayAlert("Erro", "Valor da meta não encontrado.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
        }
    }
}
