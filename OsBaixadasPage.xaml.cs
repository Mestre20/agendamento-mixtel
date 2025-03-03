namespace agendamento
{
    public partial class OsBaixadasPage : ContentPage
    {
        public OsBaixadasPage()
        {
            InitializeComponent();
            CarregarOsBaixadas();
        }

        private void CarregarOsBaixadas()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var pdfFiles = Directory.GetFiles(localAppData, "*.pdf");

            if (pdfFiles.Length == 0)
            {
                ClientesLayout.Children.Add(new Label
                {
                    Text = "Nenhuma O.S. baixada encontrada",
                    FontSize = 18,
                    TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#666666"),
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20)
                });
                return;
            }

            foreach (var pdfFile in pdfFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(pdfFile);
                var creationTime = File.GetCreationTime(pdfFile);
                var timeLeft = TimeSpan.FromHours(24) - (DateTime.Now - creationTime);

                if (timeLeft.TotalMinutes <= 0)
                {
                    File.Delete(pdfFile);
                    continue;
                }

                var frame = new Frame
                {
                    CornerRadius = 12,
                    HasShadow = true,
                    BorderColor = Microsoft.Maui.Graphics.Colors.Transparent,
                    BackgroundColor = Microsoft.Maui.Graphics.Colors.White,
                    Padding = 0,
                    HeightRequest = 70,
                    Margin = new Thickness(10, 5),
                    MaximumWidthRequest = 350,
                    HorizontalOptions = LayoutOptions.Center
                };

                var button = new Button
                {
                    Text = fileName,
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

                button.Background = gradient;

                button.Clicked += async (sender, e) =>
                {
                    try
                    {
                        await Launcher.OpenAsync(new OpenFileRequest
                        {
                            File = new ReadOnlyFile(pdfFile)
                        });
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Erro", $"Não foi possível abrir o arquivo: {ex.Message}", "OK");
                    }
                };

                frame.Content = button;
                ClientesLayout.Children.Add(frame);

                // Adicionar label com tempo restante
                ClientesLayout.Children.Add(new Label
                {
                    Text = $"Expira em: {timeLeft.Hours}h {timeLeft.Minutes}m",
                    FontSize = 14,
                    TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#666666"),
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, -10, 0, 10)
                });
            }
        }
    }
}
