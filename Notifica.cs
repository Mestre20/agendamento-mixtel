using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Plugin.LocalNotification;

namespace agendamento
{
    public class Notifica
    {
        private static SheetsService _googleSheetsService;
        private static readonly string TecnicosSpreadsheetId = "1KgPhYwomM3AMtyl3DiniksEc44acEwsfZkQ010sWFXA";
        private static readonly string SheetName = "Página1";
        private const string SelectedTecnicoKey = "SelectedTecnico";

        public Notifica(SheetsService googleSheetsService)
        {
            _googleSheetsService = googleSheetsService;
        }

        public async Task NotificarClientesNovosAsync()
        {
            try
            {
                // Obter o técnico salvo nas preferências
                var tecnicoSalvo = Preferences.Get(SelectedTecnicoKey, string.Empty);
                if (string.IsNullOrEmpty(tecnicoSalvo))
                {
                    Console.WriteLine("Nenhum técnico selecionado.");
                    return;
                }

                // Obter clientes da planilha
                var response = await _googleSheetsService.Spreadsheets.Values.Get(TecnicosSpreadsheetId, $"{SheetName}!A:D").ExecuteAsync();
                if (response.Values == null || !response.Values.Any())
                {
                    Console.WriteLine("Planilha vazia ou sem dados.");
                    return;
                }

                int rowIndex = 1; // Começar após o cabeçalho
                var dataAtual = DateTime.Now.Date;

                foreach (var row in response.Values.Skip(1)) // Ignorar o cabeçalho
                {
                    if (row.Count >= 3)
                    {
                        string nomeCliente = row[0]?.ToString() ?? string.Empty;
                        string tecnico = row[1]?.ToString() ?? string.Empty;
                        string periodo = row[2]?.ToString() ?? string.Empty;
                        string dataNotificacao = row.Count > 3 ? row[3]?.ToString() ?? string.Empty : string.Empty;

                        // Verificar se o técnico do cliente é o técnico salvo e se não foi notificado hoje
                        if (tecnico.Equals(tecnicoSalvo, StringComparison.OrdinalIgnoreCase) &&
                            (string.IsNullOrEmpty(dataNotificacao) || !DateTime.TryParse(dataNotificacao, out var dataParse) || dataParse.Date != dataAtual))
                        {
                            // Enviar notificação local
                            EnviarNotificacaoLocal(nomeCliente, tecnico, periodo);
                            // Atualizar a data de notificação na planilha
                            await AtualizarDataNotificacaoAsync(rowIndex);
                        }
                    }
                    rowIndex++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao notificar clientes: {ex.Message}");
            }
        }

        private void EnviarNotificacaoLocal(string nomeCliente, string tecnico, string periodo)
        {
            try
            {
                var notification = new NotificationRequest
                {
                    NotificationId = new Random().Next(1000),
                    Title = "Novo Cliente Adicionado",
                    Description = $"Cliente: {nomeCliente}\nPeríodo: {periodo}",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Now.AddSeconds(1) // Notificação imediata
                    }
                };

                LocalNotificationCenter.Current.Show(notification);
                Console.WriteLine("Notificação local exibida.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao exibir notificação local: {ex.Message}");
            }
        }

        private async Task AtualizarDataNotificacaoAsync(int rowIndex)
        {
            try
            {
                var range = $"{SheetName}!D{rowIndex + 1}"; // Ajustar índice da linha
                var body = new ValueRange
                {
                    Values = new List<IList<object>> { new List<object> { DateTime.Now.ToString("yyyy-MM-dd") } }
                };

                var updateRequest = _googleSheetsService.Spreadsheets.Values.Update(body, TecnicosSpreadsheetId, range);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

                await updateRequest.ExecuteAsync();
                Console.WriteLine($"Data de notificação atualizada na linha {rowIndex + 1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar data de notificação: {ex.Message}");
            }
        }
    }
}
