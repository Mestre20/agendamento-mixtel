using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace agendamento
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseLocalNotification() // Configurar notificações
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            SolicitarPermissaoNotificacao();
            VerificarPerm();

            return app;
        }

        private static async void SolicitarPermissaoNotificacao()
        {
            const string NotificationPermissionKey = "NotificationPermissionRequested";

            if (!Preferences.ContainsKey(NotificationPermissionKey))
            {
                bool permissionGranted = false;

#if ANDROID
                // No Android, as permissões são geralmente solicitadas automaticamente
                // ou podem ser gerenciadas no sistema.
                permissionGranted = true; // Assume que a permissão é concedida
#elif IOS
                // No iOS, permissões devem ser solicitadas explicitamente
#else
                // Outras plataformas não requerem uma solicitação explícita
                permissionGranted = true;
#endif

                Preferences.Set(NotificationPermissionKey, permissionGranted);
            }
        }

        private static async void VerificarPerm()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
        }
    }
}
