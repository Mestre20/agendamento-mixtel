using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3; // API do Google Drive
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using iText.Kernel.Pdf; // iText7 para operações com PDF
using iText.Kernel.Pdf.Canvas.Parser; // Para extração de texto de PDF
using OfficeOpenXml; // EPPlus para operações com Excel
using System.Globalization;
using System.Numerics; // Para BigInteger
using System.Reflection; // Para acessar recursos incorporados
using System.Text.RegularExpressions;

namespace agendamento
{
    public partial class ClientesPage : ContentPage
    {
        private string _tecnico;
        private string _periodo;
        private static SheetsService _sheetsService;
        private static DriveService _driveService;
        private static readonly string SpreadsheetId = "11hayRzhS4-VRhc9m6drZ42yRNPC14SLJnwtIew36WeQ"; // ID da planilha no Google Sheets
        private static readonly string SheetName = "Clientes"; // Nome da aba da planilha

        // ID da pasta onde todas as O.S. estarão armazenadas
        private static readonly string FolderId = "1aQ6LVknX9irfsnibH8LI3d5c_1synXiK";

        // ID da planilha de técnicos
        private const string TecnicosSpreadsheetId = "1fRKzS5sAfYuWd5QMm2uwzRjL_xtTajfAdTzWIWRYWOs";
        private const string TecnicosSheetName = "Tecnicos";

        public ClientesPage(string tecnico, string periodo)
        {
            InitializeComponent();
            _tecnico = tecnico.Trim().ToLower();
            _periodo = periodo.Trim().ToLower();

            // Definir propriedades visuais da página com o mesmo gradiente do MainPage
            this.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Microsoft.Maui.Graphics.Color.FromArgb("#f0f9ff"), Offset = 0.0f },
                    new GradientStop { Color = Microsoft.Maui.Graphics.Color.FromArgb("#e1f2ff"), Offset = 1.0f }
                }
            };
            Title = "Lista de Clientes";

            // Inicializar APIs do Google Sheets e Google Drive
            InitializeGoogleApis();

            // Carregar os dados dos clientes de forma assíncrona
            _ = CarregarClientesAsync();
        }

        private async Task CarregarClientesAsync()
        {
            var filePath = await BaixarPlanilhaAsync();
            var clientes = new List<(string Nome, string Telefone, string Coordenada, int RowIndex)>();

            if (filePath == null)
            {
                await DisplayAlert("Erro", "Não foi possível baixar a planilha. Verifique sua conexão.", "OK");
                return;
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Considerando que a primeira aba é a que precisamos

                    int row = 2; // Assumindo que a primeira linha é o cabeçalho
                    while (row <= worksheet.Dimension.End.Row)
                    {
                        string nome = worksheet.Cells[row, 1].Text;

                        // Correção na extração do telefone
                        object rawValue = worksheet.Cells[row, 2].Value;
                        string telefone;
                        if (rawValue is double telefoneNumber)
                        {
                            // Converter para BigInteger para evitar perda de precisão
                            BigInteger telefoneBigInt = new BigInteger(telefoneNumber);
                            telefone = telefoneBigInt.ToString();
                        }
                        else
                        {
                            telefone = worksheet.Cells[row, 2].Text;
                        }

                        string tecnico = worksheet.Cells[row, 3].Text.Trim().ToLower();
                        string periodo = worksheet.Cells[row, 4].Text.Trim().ToLower();
                        string coordenada = worksheet.Cells[row, 5].Text;

                        // Verificar se a linha está em branco, se estiver, apenas pular para a próxima linha
                        if (string.IsNullOrWhiteSpace(nome) &&
                            string.IsNullOrWhiteSpace(telefone) &&
                            string.IsNullOrWhiteSpace(tecnico) &&
                            string.IsNullOrWhiteSpace(periodo) &&
                            string.IsNullOrWhiteSpace(coordenada))
                        {
                            row++;
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(tecnico) &&
                            tecnico == _tecnico &&
                            periodo == _periodo)
                        {
                            clientes.Add((nome, telefone, coordenada, row));
                        }

                        row++;
                    }
                }

                if (clientes.Count == 0)
                {
                    await DisplayAlert("Aviso", "Nenhum cliente encontrado para o técnico e período selecionados.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar clientes: {ex.Message}", "OK");
                return;
            }

            // Criar os botões dos clientes com base nos dados filtrados
            var clientesLayout = new StackLayout
            {
                Padding = new Thickness(20),
                Spacing = 15,
                Children = { }
            };

            foreach (var cliente in clientes)
            {
                var button = new Button
                {
                    Text = cliente.Nome,
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Microsoft.Maui.Graphics.Colors.White,
                    BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                    HeightRequest = 50,
                    Margin = new Thickness(10, 5),
                    MaximumWidthRequest = 300,
                    HorizontalOptions = LayoutOptions.Center
                };

                // Adicionar gradiente ao botão
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

                button.Background = gradient;

                // Manter o evento de clique original
                button.Clicked += async (sender, args) =>
                    await OnClienteButtonClicked(cliente.Nome, cliente.Telefone, cliente.Coordenada, cliente.RowIndex);

                clientesLayout.Children.Add(button);
            }

            var scrollView = new ScrollView
            {
                Content = clientesLayout,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            Content = new StackLayout
            {
                Padding = new Thickness(10),
                Spacing = 20,
                Children =
                {
                    new Frame
                    {
                        Content = new Label
                        {
                            Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase($"Clientes para Técnico: {_tecnico} ({_periodo})".ToLower()),
                            FontSize = 28,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.Black,
                            HorizontalOptions = LayoutOptions.Center
                        },
                        BackgroundColor = Colors.LightBlue,
                        CornerRadius = 15,
                        Padding = new Thickness(20),
                        Margin = new Thickness(0, 15),
                        HorizontalOptions = LayoutOptions.FillAndExpand
                    },
                    scrollView
                }
            };
        }

        private async Task<string> BaixarPlanilhaAsync()
        {
            try
            {
                // URL da planilha pública exportada em formato Excel
                string url = "https://docs.google.com/spreadsheets/d/11hayRzhS4-VRhc9m6drZ42yRNPC14SLJnwtIew36WeQ/export?format=xlsx";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "clientes.xlsx");

                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    return filePath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao baixar a planilha: {ex.Message}");
                return null;
            }
        }

        private static void InitializeGoogleApis()
        {
            GoogleCredential credential;

            // Obter o assembly atual e buscar o recurso incorporado 'agendamento.Resources.credencial.json'
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "agendamento.Resources.credencial.json";

            // Listar todos os recursos para encontrar o nome exato
            string[] resources = assembly.GetManifestResourceNames();
            foreach (var res in resources)
            {
                Console.WriteLine($"Recurso encontrado: {res}");
            }

            // Verificar se o recurso existe
            var matchedResourceName = Array.Find(resources, res => res.EndsWith("credencial.json"));
            if (string.IsNullOrEmpty(matchedResourceName))
            {
                throw new FileNotFoundException($"Arquivo de credenciais não encontrado como recurso incorporado: {resourceName}");
            }

            // Carregar o recurso incorporado
            using (var stream = assembly.GetManifestResourceStream(matchedResourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Arquivo de credenciais não encontrado como recurso incorporado: {matchedResourceName}");
                }

                credential = GoogleCredential.FromStream(stream).CreateScoped(new[] { SheetsService.Scope.Spreadsheets, DriveService.Scope.DriveReadonly, SheetsService.Scope.Spreadsheets });
            }

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Agendamento API",
            });

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Agendamento API",
            });
        }

        public async Task OnClienteButtonClicked(string nome, string telefone, string coordenada, int rowIndex)
        {
            string titulo = nome.Length > 20 ? nome.Substring(0, 20) + "..." : nome;

            // Caminho do arquivo O.S. no dispositivo
            var osFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{nome}.pdf");

            // Verificar se o arquivo existe e se é mais novo que 24 horas
            bool osExists = false;
            if (File.Exists(osFilePath))
            {
                var creationTime = File.GetCreationTime(osFilePath);
                if (DateTime.Now - creationTime < TimeSpan.FromHours(24))
                {
                    osExists = true;
                }
                else
                {
                    File.Delete(osFilePath);
                }
            }

            // Criar um popup personalizado em vez de usar ActionSheet
            var popupPage = new ContentPage
            {
                BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent
            };

            var mainFrame = new Frame
            {
                BackgroundColor = Microsoft.Maui.Graphics.Colors.White,
                CornerRadius = 20,
                Margin = new Thickness(20),
                Padding = new Thickness(15),
                HasShadow = true
            };

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 10
            };

            // Título
            stackLayout.Children.Add(new Label
            {
                Text = $"Cliente: {titulo}",
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                TextColor = Microsoft.Maui.Graphics.Colors.DarkSlateGray,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 15)
            });

            // Lista de opções
            var opcoes = new List<(string Texto, string Icone, Action Acao)>
            {
                ("Ligar para Cliente", "📞", async () => await PlacePhoneCall(telefone)),
                ("Copiar Número", "📋", async () => await CopyToClipboard(telefone)),
                ("Copiar Nome", "👤", async () => await CopyToClipboard(nome)),
                ("WhatsApp", "💬", async () => await OpenWhatsApp(telefone)),
                (osExists ? "Abrir O.S." : "Baixar O.S.", "📄", async () => await HandleOS(osExists, nome)),
                ("Rota para Cliente", "🗺️", async () => await OpenMaps(coordenada, nome)),
                ("Extrair Rota do PDF", "📍", async () => await ExtrairCoordenadaDoPdfENavegar(nome)),
                ("O.S. Finalizada", "✅", async () => await FinalizarOSAsync(nome, telefone, coordenada, rowIndex))
            };

            foreach (var opcao in opcoes)
            {
                var button = new Button
                {
                    Text = $"{opcao.Icone} {opcao.Texto}",
                    FontSize = 18,
                    TextColor = Microsoft.Maui.Graphics.Colors.White,
                    BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                    Margin = new Thickness(0, 5),
                    HeightRequest = 50,
                    HorizontalOptions = LayoutOptions.Fill
                };

                // Gradiente para o botão
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

                button.Background = gradient;

                // Adicionar evento de clique
                button.Clicked += async (s, e) =>
                {
                    await Navigation.PopModalAsync();
                    opcao.Acao();
                };

                stackLayout.Children.Add(button);
            }

            // Botão Cancelar
            var cancelButton = new Button
            {
                Text = "Cancelar",
                FontSize = 18,
                TextColor = Microsoft.Maui.Graphics.Colors.White,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                Margin = new Thickness(0, 15, 0, 0),
                HeightRequest = 50
            };

            var cancelGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Microsoft.Maui.Graphics.Color.FromArgb("#dc3545"), 0.0f),
                    new GradientStop(Microsoft.Maui.Graphics.Color.FromArgb("#c82333"), 1.0f)
                }
            };

            cancelButton.Background = cancelGradient;
            cancelButton.Clicked += async (s, e) => await Navigation.PopModalAsync();

            stackLayout.Children.Add(cancelButton);
            mainFrame.Content = stackLayout;

            var scrollView = new ScrollView
            {
                Content = mainFrame,
                VerticalOptions = LayoutOptions.Center
            };

            popupPage.Content = scrollView;

            await Navigation.PushModalAsync(popupPage);
        }

        // Métodos auxiliares para as ações
        private async Task PlacePhoneCall(string telefone)
        {
            try
            {
                PhoneDialer.Default.Open(telefone);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Não foi possível realizar a chamada: " + ex.Message, "OK");
            }
        }

        private async Task CopyToClipboard(string text)
        {
            try
            {
                await Clipboard.Default.SetTextAsync(text);
                await DisplayAlert("Sucesso", "Texto copiado para a área de transferência!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Não foi possível copiar o texto: " + ex.Message, "OK");
            }
        }

        private async Task OpenWhatsApp(string telefone)
        {
            try
            {
                string url = $"https://wa.me/{telefone}";
                await Launcher.Default.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Não foi possível abrir o WhatsApp: " + ex.Message, "OK");
            }
        }

        private async Task HandleOS(bool osExists, string nome)
        {
            if (osExists)
            {
                await AbrirOSAsync(nome);
            }
            else
            {
                await BaixarOSAsync(nome);
            }
        }

        private async Task OpenMaps(string coordenada, string nome)
        {
            try
            {
                if (string.IsNullOrEmpty(coordenada))
                {
                    // Se não houver coordenada direta, tenta extrair do PDF
                    await ExtrairCoordenadaDoPdfENavegar(nome);
                    return;
                }

                var coordenadas = coordenada.Split(',');
                if (coordenadas.Length == 2)
                {
                    string latitude = coordenadas[0].Trim();
                    string longitude = coordenadas[1].Trim();

                    // Tenta abrir no Google Maps com navegação
                    string url = $"https://www.google.com/maps/dir/?api=1&destination={latitude},{longitude}&travelmode=driving";

                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        // URL específica para Apple Maps no iOS
                        url = $"http://maps.apple.com/?daddr={latitude},{longitude}&dirflg=d";
                    }

                    await Launcher.OpenAsync(new Uri(url));
                }
                else
                {
                    // Se as coordenadas não estiverem no formato correto, tenta extrair do PDF
                    await ExtrairCoordenadaDoPdfENavegar(nome);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Não foi possível abrir o mapa: " + ex.Message, "OK");
            }
        }

        private async Task FinalizarOSAsync(string nome, string telefone, string coordenada, int rowIndex)
        {
            try
            {
                // Trava a interface
                this.IsEnabled = false;

                // Verificar se o cliente já foi adicionado à planilha de técnicos para evitar duplicidade no mesmo dia
                if (!await ClienteJaAdicionadoATecnicos(nome, _tecnico))
                {
                    // Adicionar os dados na planilha de técnicos
                    await AdicionarNaPlanilhaTecnicos(nome, _tecnico);
                }

                // Limpar a linha correspondente na planilha original de clientes
                var range = $"{SheetName}!A{rowIndex}:E{rowIndex}"; // Ajuste as colunas conforme necessário

                var clearRequest = _sheetsService.Spreadsheets.Values.Clear(new ClearValuesRequest(), SpreadsheetId, range);
                await clearRequest.ExecuteAsync();

                // Recarregar a lista de clientes para refletir a limpeza
                await CarregarClientesAsync();

                // Notificação de sucesso
                await DisplayAlert("Sucesso", "Cliente finalizado com sucesso!", "OK");
            }
            catch (Google.GoogleApiException ex)
            {
                await DisplayAlert("Erro", "Erro ao finalizar O.S.: " + ex.Error.Message, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Não foi possível finalizar a O.S.: " + ex.Message, "OK");
            }
            finally
            {
                // Libera a interface
                this.IsEnabled = true;
            }
        }

        private async Task<bool> ClienteJaAdicionadoATecnicos(string clienteNome, string tecnico)
        {
            try
            {
                var response = await _sheetsService.Spreadsheets.Values.Get(TecnicosSpreadsheetId, $"{TecnicosSheetName}!A:C").ExecuteAsync();
                if (response.Values != null)
                {
                    foreach (var row in response.Values)
                    {
                        if (row.Count >= 3 &&
                            row[0].ToString().Equals(clienteNome, StringComparison.OrdinalIgnoreCase) &&
                            row[1].ToString().Equals(tecnico, StringComparison.OrdinalIgnoreCase) &&
                            row[2].ToString().Equals(DateTime.Now.ToString("dd/MM/yy")))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Erro ao verificar duplicidade na planilha de técnicos: " + ex.Message, "OK");
            }
            return false;
        }

        private async Task AdicionarNaPlanilhaTecnicos(string clienteNome, string tecnico)
        {
            try
            {
                // Obter a data atual no formato dd/MM/aa
                string dataFinalizacao = DateTime.Now.ToString("dd/MM/yy");

                // Dados a serem adicionados: Nome do cliente, técnico e data
                var valores = new List<IList<object>> {
                    new List<object> { clienteNome, tecnico, dataFinalizacao }
                };

                var body = new ValueRange
                {
                    Values = valores
                };

                var appendRequest = _sheetsService.Spreadsheets.Values.Append(body, TecnicosSpreadsheetId, $"{TecnicosSheetName}!A:C");
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                await appendRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Erro ao adicionar os dados na planilha de técnicos: " + ex.Message, "OK");
            }
        }

        private async Task BaixarOSAsync(string clienteNome)
        {
            try
            {
                var request = _driveService.Files.List();
                request.Q = $"mimeType='application/pdf' and '{FolderId}' in parents and name contains '{clienteNome}'";
                request.Spaces = "drive";

                var result = await request.ExecuteAsync();
                var file = result.Files.FirstOrDefault();

                if (file == null)
                {
                    await DisplayAlert("Erro", $"Nenhum arquivo PDF encontrado para o cliente {clienteNome}.", "OK");
                    return;
                }

                var downloadRequest = _driveService.Files.Get(file.Id);
                var stream = new MemoryStream();
                await downloadRequest.DownloadAsync(stream);

                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{clienteNome}.pdf");
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    stream.WriteTo(fileStream);
                }

                // Abrir o arquivo PDF após baixá-lo
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });

                await DisplayAlert("Sucesso", $"Arquivo {clienteNome}.pdf baixado com sucesso!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao baixar O.S.: {ex.Message}", "OK");
            }
        }

        private async Task AbrirOSAsync(string clienteNome)
        {
            try
            {
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{clienteNome}.pdf");

                if (File.Exists(filePath))
                {
                    // Verificar se o arquivo tem menos de 24 horas
                    var creationTime = File.GetCreationTime(filePath);
                    if (DateTime.Now - creationTime < TimeSpan.FromHours(24))
                    {
                        // Abrir o arquivo PDF
                        await Launcher.OpenAsync(new OpenFileRequest
                        {
                            File = new ReadOnlyFile(filePath)
                        });
                    }
                    else
                    {
                        // Excluir o arquivo se tiver mais de 24 horas
                        File.Delete(filePath);
                        await DisplayAlert("Aviso", "O arquivo O.S. expirou e foi removido. Por favor, baixe novamente.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Erro", "Arquivo O.S. não encontrado no dispositivo.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao abrir O.S.: {ex.Message}", "OK");
            }
        }

        // Adicione estes métodos na classe ClientesPage

        // Função para extrair coordenada do PDF e abrir Google Maps
        private async Task ExtrairCoordenadaDoPdfENavegar(string clienteNome)
        {
            try
            {
                var pdfFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{clienteNome}.pdf");

                if (!File.Exists(pdfFilePath))
                {
                    await DisplayAlert("Erro", "Arquivo PDF não encontrado.", "OK");
                    return;
                }

                var coordenadas = await ExtrairCoordenadasDoPdfAsync(pdfFilePath);

                if (coordenadas != null && coordenadas.Count > 0)
                {
                    var coordenada = coordenadas[0];
                    // Separar a coordenada em latitude e longitude
                    var coordenadasPartes = coordenada.Split(',');
                    if (coordenadasPartes.Length == 2)
                    {
                        string latitude = coordenadasPartes[0].Trim();
                        string longitude = coordenadasPartes[1].Trim();

                        // Formatar a URL para abrir no Google Maps
                        string url = $"https://www.google.com/maps/dir/?api=1&destination={latitude},{longitude}";
                        await Launcher.OpenAsync(new Uri(url));
                    }
                    else
                    {
                        await DisplayAlert("Erro", "Coordenadas inválidas para o cliente.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Erro", "Nenhuma coordenada encontrada no PDF.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao extrair coordenadas do PDF: {ex.Message}", "OK");
            }
        }

        // Função para extrair coordenadas de um PDF usando iText7
        private async Task<List<string>> ExtrairCoordenadasDoPdfAsync(string pdfFilePath)
        {
            var coordenadas = new List<string>();

            try
            {
                using (var pdfReader = new PdfReader(pdfFilePath))
                {
                    using (var pdfDoc = new PdfDocument(pdfReader))
                    {
                        for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                        {
                            var text = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page));
                            var coordPattern = @"-?\d{1,3}\.\d+,\s?-?\d{1,3}\.\d+";
                            var matches = Regex.Matches(text, coordPattern);
                            foreach (Match match in matches)
                            {
                                coordenadas.Add(match.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o arquivo PDF: {ex.Message}");
            }

            return coordenadas;
        }
    }
}
