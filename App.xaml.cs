using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
using System.Reflection;
using Timer = System.Timers.Timer;

namespace agendamento
{
    public partial class App : Application
    {
        public static SheetsService SheetsService { get; private set; }
        public static DriveService DriveService { get; private set; }
        private Analisar _analisador;
        private Notifica _notificador;
        private Timer _notificationTimer;

        public App()
        {
            InitializeComponent();

            // Configurar o evento NotificationActionTapped no LocalNotificationCenter
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationTapped;

            // Obter credenciais do Google
            var credential = GetCredential();

            // Inicializar APIs do Google Sheets e Google Drive
            SheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Agendamento API",
            });

            DriveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Agendamento API",
            });

            // Inicializar Analisar e Notifica
            _analisador = new Analisar(credential);
            _notificador = new Notifica(SheetsService);

            // Configurar Timer para verificar novos clientes periodicamente
            _notificationTimer = new Timer(30000); // 30 segundos
            _notificationTimer.Elapsed += async (sender, e) => await _notificador.NotificarClientesNovosAsync();
            _notificationTimer.AutoReset = true;
            _notificationTimer.Enabled = true;

            // Definir a página inicial
            MainPage = new NavigationPage(new MainPage());
        }

        private GoogleCredential GetCredential()
        {
            // Obter o assembly atual e buscar o recurso incorporado 'agendamento.Resources.credencial.json'
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "agendamento.Resources.credencial.json";

            // Listar todos os recursos para encontrar o nome exato
            string[] resources = assembly.GetManifestResourceNames();
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

                return GoogleCredential.FromStream(stream).CreateScoped(new[] {
                    SheetsService.Scope.Spreadsheets,
                    DriveService.Scope.DriveReadonly,
                    SheetsService.Scope.Spreadsheets
                });
            }
        }

        private void OnNotificationTapped(NotificationActionEventArgs e)
        {
            // Apenas abrir o aplicativo ao clicar na notificação
            Console.WriteLine("Notificação clicada. Aplicativo aberto.");
        }
    }
}
