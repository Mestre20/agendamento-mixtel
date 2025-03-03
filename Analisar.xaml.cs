using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Timer = System.Timers.Timer;

namespace agendamento
{
    public class Analisar
    {
        private static SheetsService _sheetsService;
        private static readonly string ClientesSpreadsheetId = "11hayRzhS4-VRhc9m6drZ42yRNPC14SLJnwtIew36WeQ";
        private static readonly string ClientesSheetName = "Clientes";
        private static readonly string TecnicosSpreadsheetId = "1KgPhYwomM3AMtyl3DiniksEc44acEwsfZkQ010sWFXA";
        private static readonly string TecnicosSheetName = "Página1";
        private static Timer _timer;

        public Analisar(GoogleCredential credential)
        {
            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Agendamento API",
            });

            // Configurar o timer para executar a cada 30 segundos
            _timer = new Timer(100000);
            _timer.Elapsed += async (sender, e) => await AnalisarPlanilhaAsync();
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private async Task AnalisarPlanilhaAsync()
        {
            try
            {
                var response = await _sheetsService.Spreadsheets.Values.Get(ClientesSpreadsheetId, $"{ClientesSheetName}!A:D").ExecuteAsync();
                if (response.Values == null) return;

                var clientesExistentes = new HashSet<string>();
                var clientesResponse = await _sheetsService.Spreadsheets.Values.Get(TecnicosSpreadsheetId, $"{TecnicosSheetName}!A:C").ExecuteAsync();
                if (clientesResponse.Values != null)
                {
                    foreach (var row in clientesResponse.Values)
                    {
                        if (row.Count >= 3)
                        {
                            string clienteNome = row[0].ToString();
                            string tecnico = row[1].ToString();
                            string periodo = row[2].ToString();

                            clientesExistentes.Add($"{clienteNome.ToLower()}_{tecnico.ToLower()}_{periodo.ToLower()}");
                        }
                    }
                }

                var novosClientes = new List<IList<object>>();
                foreach (var row in response.Values.Skip(1)) // Pular cabeçalho
                {
                    // Pular linhas vazias
                    if (row.Count < 4 || string.IsNullOrWhiteSpace(row[0]?.ToString()) || string.IsNullOrWhiteSpace(row[1]?.ToString()) || string.IsNullOrWhiteSpace(row[2]?.ToString()) || string.IsNullOrWhiteSpace(row[3]?.ToString()))
                    {
                        continue;
                    }

                    string nome = row[0].ToString();
                    string telefone = row[1].ToString();
                    string tecnico = row[2].ToString();
                    string periodo = row[3].ToString();

                    string clienteIdentificador = $"{nome.ToLower()}_{tecnico.ToLower()}_{periodo.ToLower()}";
                    if (!clientesExistentes.Contains(clienteIdentificador))
                    {
                        clientesExistentes.Add(clienteIdentificador);
                        novosClientes.Add(new List<object> { nome, tecnico, periodo });
                    }
                }

                if (novosClientes.Any())
                {
                    var body = new ValueRange
                    {
                        Values = novosClientes
                    };

                    var appendRequest = _sheetsService.Spreadsheets.Values.Append(body, TecnicosSpreadsheetId, $"{TecnicosSheetName}!A:C");
                    appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                    await appendRequest.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao analisar planilha: {ex.Message}");
            }
        }
    }
}
