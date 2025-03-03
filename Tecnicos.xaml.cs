using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json; // Biblioteca para manipular JSON

namespace agendamento
{
    public partial class TecnicosPage : ContentPage
    {
        private const string SelectedTecnicoKey = "SelectedTecnico"; // Chave para armazenar o técnico selecionado
        private static readonly string TecnicosSpreadsheetId = "1u7V13H6o-mzC-b3rOTOJzxxeuahxXGjcm0pIntNwKrk";
        private const string SheetName = "Página1";
        private Dictionary<string, string> _tecnicosCache = new Dictionary<string, string>();
        private static readonly string CacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tecnicosCache.json");

        private System.Timers.Timer _longPressTimer; // Definindo explicitamente System.Timers.Timer para evitar ambiguidade
        private bool _isLongPress;

        public TecnicosPage()
        {
            InitializeComponent();
            CarregarTecnicosAsync();
            CarregarTecnicoSelecionado();
            AdicionarBotaoRedefinir();
        }

        private async void CarregarTecnicosAsync()
        {
            try
            {
                if (File.Exists(CacheFilePath))
                {
                    var cacheJson = File.ReadAllText(CacheFilePath);
                    _tecnicosCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(cacheJson);

                    // Certifique-se de que "Reginaldo" e "Joao" estão incluídos no cache
                    if (!_tecnicosCache.ContainsKey("Reginaldo"))
                    {
                        _tecnicosCache["Reginaldo"] = string.Empty;
                    }
                    if (!_tecnicosCache.ContainsKey("Joao"))
                    {
                        _tecnicosCache["Joao"] = string.Empty;
                    }

                    return;
                }

                var response = await App.SheetsService.Spreadsheets.Values.Get(TecnicosSpreadsheetId, "'Página1'!A:B").ExecuteAsync();
                var rows = response.Values;

                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        if (row.Count > 1)
                        {
                            _tecnicosCache[row[0].ToString()] = row[1].ToString();
                        }
                        else if (row.Count > 0)
                        {
                            _tecnicosCache[row[0].ToString()] = string.Empty;
                        }
                    }
                }

                // Certifique-se de que "Reginaldo" e "Joao" estão incluídos no cache
                if (!_tecnicosCache.ContainsKey("Reginaldo"))
                {
                    _tecnicosCache["Reginaldo"] = string.Empty;
                }
                if (!_tecnicosCache.ContainsKey("Joao"))
                {
                    _tecnicosCache["Joao"] = string.Empty;
                }

                File.WriteAllText(CacheFilePath, JsonConvert.SerializeObject(_tecnicosCache));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar técnicos: {ex.Message}", "OK");
            }
        }

        private void CarregarTecnicoSelecionado()
        {
            var tecnicoSalvo = Preferences.Get(SelectedTecnicoKey, string.Empty);

            if (!string.IsNullOrEmpty(tecnicoSalvo))
            {
                foreach (var button in TecnicosLayout.Children.OfType<Button>())
                {
                    button.IsVisible = button.Text == tecnicoSalvo;
                }
            }
        }

        private async void OnTecnicoSelected(object sender, EventArgs e)
        {
            if (_isLongPress) return;

            await ((Button)sender).ScaleTo(0.80, 100);
            await ((Button)sender).ScaleTo(1, 100);

            string tecnico = ((Button)sender).Text;

            if (!Preferences.ContainsKey(SelectedTecnicoKey) || Preferences.Get(SelectedTecnicoKey, string.Empty) != tecnico)
            {
                bool acessoPermitido = await VerificarOuSolicitarSenha(tecnico);
                if (!acessoPermitido)
                {
                    return;
                }
            }

            Preferences.Set(SelectedTecnicoKey, tecnico);
            await Navigation.PushAsync(new PeriodoPage(tecnico));
        }

        private async Task<bool> VerificarOuSolicitarSenha(string tecnico)
        {
            if (_tecnicosCache.TryGetValue(tecnico, out string senhaExistente))
            {
                if (string.IsNullOrEmpty(senhaExistente))
                {
                    string novaSenha = await DisplayPromptAsync("Criar Senha", $"Técnico {tecnico}, crie uma senha:", "OK", "Cancelar");

                    if (!string.IsNullOrEmpty(novaSenha))
                    {
                        await AtualizarSenhaNaPlanilha(tecnico, novaSenha);
                        _tecnicosCache[tecnico] = novaSenha;
                        SalvarCache();
                        await DisplayAlert("Sucesso", "Senha criada com sucesso!", "OK");
                        return true;
                    }
                    else
                    {
                        await DisplayAlert("Aviso", "Operação cancelada. Senha não criada.", "OK");
                        return false;
                    }
                }
                else
                {
                    string senhaDigitada = await DisplayPromptAsync("Senha Requerida", $"Digite a senha para o técnico {tecnico}:", "OK", "Cancelar");

                    if (senhaDigitada == senhaExistente)
                    {
                        await DisplayAlert("Sucesso", "Acesso permitido!", "OK");
                        return true;
                    }
                    else
                    {
                        await DisplayAlert("Erro", "Senha incorreta. Tente novamente.", "OK");
                        return false;
                    }
                }
            }

            await DisplayAlert("Erro", "Técnico não encontrado.", "OK");
            return false;
        }

        private async Task AtualizarSenhaNaPlanilha(string tecnico, string senha)
        {
            try
            {
                var rowIndex = _tecnicosCache.Keys.ToList().IndexOf(tecnico) + 1;
                var range = $"{SheetName}!B{rowIndex}";
                var body = new ValueRange
                {
                    Values = new List<IList<object>> { new List<object> { senha } }
                };

                var updateRequest = App.SheetsService.Spreadsheets.Values.Update(body, TecnicosSpreadsheetId, range);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

                await updateRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao atualizar a senha: {ex.Message}", "OK");
            }
        }

        private void SalvarCache()
        {
            try
            {
                File.WriteAllText(CacheFilePath, JsonConvert.SerializeObject(_tecnicosCache));
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", $"Erro ao salvar o cache: {ex.Message}", "OK");
            }
        }

        private void AdicionarBotaoRedefinir()
        {
            var frame = new Frame
            {
                CornerRadius = 12,
                HasShadow = true,
                BorderColor = Microsoft.Maui.Graphics.Colors.Transparent,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.White,
                Padding = 0,
                HeightRequest = 50,
                Margin = new Thickness(10, 5),
                MaximumWidthRequest = 300,
                HorizontalOptions = LayoutOptions.Center
            };

            var redefinirButton = new Button
            {
                Text = "Redefinir Técnico",
                FontSize = 18,
                TextColor = Microsoft.Maui.Graphics.Colors.White,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            // Adicionar gradiente ao botão
            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Microsoft.Maui.Graphics.Color.FromArgb("#2196F3"), 0.0f),
                    new GradientStop(Microsoft.Maui.Graphics.Color.FromArgb("#1976D2"), 1.0f)
                }
            };

            redefinirButton.Background = gradient;

            redefinirButton.Clicked += async (sender, args) =>
            {
                Preferences.Remove(SelectedTecnicoKey);

                // Animação do botão
                await redefinirButton.ScaleTo(0.80, 100);
                await redefinirButton.ScaleTo(1, 100);

                // Recarregar a página atual
                var currentPage = new TecnicosPage();
                var navigation = Application.Current.MainPage as NavigationPage;
                await navigation.PopAsync();
                await navigation.PushAsync(currentPage);
            };

            frame.Content = redefinirButton;
            TecnicosLayout.Children.Insert(0, frame);
        }

        private void OnTecnicoPressed(object sender, EventArgs e)
        {
            _isLongPress = false;
            _longPressTimer = new System.Timers.Timer(2000); // Especificando o namespace para evitar ambiguidade
            _longPressTimer.Elapsed += async (s, args) =>
            {
                _isLongPress = true;
                _longPressTimer.Stop();

                var button = (Button)sender;
                string tecnico = button.Text;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    bool acessoPermitido = await VerificarOuSolicitarSenha(tecnico);
                    if (acessoPermitido)
                    {
                        await Navigation.PushAsync(new TecnicoDetalhesPage(tecnico));
                    }
                });
            };
            _longPressTimer.Start();
        }

        private void OnTecnicoReleased(object sender, EventArgs e)
        {
            _longPressTimer?.Stop();
            _longPressTimer = null;
        }
    }
}
