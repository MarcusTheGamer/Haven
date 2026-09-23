using Haven.Services;
using Haven.ViewModels;
using Haven.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace Haven
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<SupabaseService>();
            builder.Services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<SupabaseService>());

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();

            builder.Services.AddTransient<SignUpViewModel>();
            builder.Services.AddTransient<SignUpPage>();

            #if ANDROID
                    builder.Services.AddSingleton<IWifiConnector, Haven.Platforms.Android.AndroidWifiConnector>();
            #endif

            builder.Services.AddTransient<WiFiProvisioningService>();
            builder.Services.AddTransient<DeviceNetworkSweepService>();
            builder.Services.AddSingleton<IDeviceRegistry, DeviceRegistry>();
            builder.Services.AddSingleton<IDeviceCommunicationService, DeviceCommunicationService>();
            
            builder.Services.AddTransient<AddDeviceViewModel>();
            builder.Services.AddTransient<AddDevicePage>();

            builder.Services.AddTransient<FindDevicesViewModel>();
            builder.Services.AddTransient<FindDevicesPage>();

            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<ProfilePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}