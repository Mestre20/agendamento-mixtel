using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Microsoft.Maui.Storage; // Para Preferences

namespace agendamento
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false); // Remove a barra de navegação
            AtualizarContadorClientes();
        }

        private async void OnIniciarAgendamentoClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            
            // Animação suave do botão
            await button.ScaleTo(0.95, 50, Easing.CubicOut);
            await button.ScaleTo(1, 50, Easing.CubicIn);

            // Navegação simples
            await Navigation.PushAsync(new TecnicosPage());
        }

        private async void OnExibirOsBaixadasClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            await button.ScaleTo(0.80, 100);
            await button.ScaleTo(1, 100);

            await Navigation.PushAsync(new OsBaixadasPage());
        }

        private async void AtualizarContadorClientes()
        {
            try
            {
                var tecnicoSelecionado = Preferences.Get("SelectedTecnico", string.Empty);
                if (string.IsNullOrEmpty(tecnicoSelecionado))
                {
                    ClientesHojeLabel.Text = "0 O.S";
                    OsFinalizadasLabel.Text = "0 O.S";
                    TecnicoLabel.Text = "Nenhum técnico selecionado";
                    return;
                }

                // IDs das planilhas
                string spreadsheetIdAgendadas = "11hayRzhS4-VRhc9m6drZ42yRNPC14SLJnwtIew36WeQ";
                string spreadsheetIdFinalizadas = "1fRKzS5sAfYuWd5QMm2uwzRjL_xtTajfAdTzWIWRYWOs";

                // Buscar O.S. agendadas
                var responseAgendadas = await App.SheetsService.Spreadsheets.Values.Get(spreadsheetIdAgendadas, "Clientes!A:D").ExecuteAsync();
                if (responseAgendadas.Values != null)
                {
                    int clientesHoje = 0;
                    foreach (var row in responseAgendadas.Values.Skip(1)) // Pula o cabeçalho
                    {
                        if (row.Count >= 3 && row[2].ToString().Trim().ToLower() == tecnicoSelecionado.ToLower())
                        {
                            clientesHoje++;
                        }
                    }
                    ClientesHojeLabel.Text = $"{clientesHoje} O.S";
                    TecnicoLabel.Text = $"Técnico: {tecnicoSelecionado}";
                }

                // Buscar O.S. finalizadas
                var responseFinalizadas = await App.SheetsService.Spreadsheets.Values.Get(spreadsheetIdFinalizadas, "Tecnicos!A:C").ExecuteAsync();
                if (responseFinalizadas.Values != null)
                {
                    int osFinalizadas = 0;
                    var dataAtual = DateTime.Now.ToString("dd/MM/yy"); // Usar a data atual

                    System.Diagnostics.Debug.WriteLine($"Total de linhas na planilha: {responseFinalizadas.Values.Count}");
                    System.Diagnostics.Debug.WriteLine($"Buscando para data: {dataAtual}");
                    System.Diagnostics.Debug.WriteLine($"Técnico selecionado: {tecnicoSelecionado}");

                    // Pegar as últimas 40 linhas
                    var totalLinhas = responseFinalizadas.Values.Count;
                    var startIndex = Math.Max(1, totalLinhas - 40); // Começa do 1 para pular o cabeçalho
                    
                    for (int i = totalLinhas - 1; i >= startIndex; i--)
                    {
                        try
                        {
                            var row = responseFinalizadas.Values[i];
                            if (row.Count >= 3)
                            {
                                string tecnico = row[1].ToString().Trim().ToLower(); // Coluna B - Técnico
                                string dataStr = row[2].ToString().Trim(); // Coluna C - Data

                                System.Diagnostics.Debug.WriteLine($"Linha {i}: Técnico={tecnico}, Data={dataStr}");

                                if (tecnico == tecnicoSelecionado.ToLower() && dataStr == dataAtual)
                                {
                                    osFinalizadas++;
                                    System.Diagnostics.Debug.WriteLine($"Encontrada O.S. na linha {i}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Erro na linha {i}: {ex.Message}");
                            continue;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Total de O.S. finalizadas encontradas: {osFinalizadas}");
                    OsFinalizadasLabel.Text = $"{osFinalizadas} O.S";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao atualizar contador: {ex.Message}", "OK");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            AtualizarContadorClientes();
        }
    }
} 