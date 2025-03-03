using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace agendamento
{
    public static class GoogleSheetsAPI
    {
        static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static readonly string ApplicationName = "Agendamento API";
        static readonly string SpreadsheetId = "11hayRzhS4-VRhc9m6drZ42yRNPC14SLJnwtIew36WeQ"; // Substitua com o ID da sua planilha
        static readonly string SheetName = "Clientes"; // Substitua com o nome da aba da planilha

        public static SheetsService Service;

        public static void Initialize()
        {
            GoogleCredential credential;

            // Carregar as credenciais do arquivo JSON
            using (var stream = new FileStream("credencial.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            // Inicializa o serviço Google Sheets
            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public static async Task<IList<IList<object>>> GetSheetDataAsync()
        {
            if (Service == null)
            {
                throw new Exception("Serviço não inicializado. Certifique-se de que o método Initialize() foi chamado.");
            }

            var range = $"{SheetName}!A2:E";  // Defina o intervalo correto da planilha
            var request = Service.Spreadsheets.Values.Get(SpreadsheetId, range);
            var response = await request.ExecuteAsync();
            return response.Values;
        }
    }
}
