using Haven.Services;
using Haven.ViewModels;
using Haven.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using ZXing.Net.Maui.Controls;

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
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<SupabaseService>();
            builder.Services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<SupabaseService>());

            builder.Services.AddSingleton<IFamilyService, FamilyService>();
            builder.Services.AddSingleton<ICurrentFamilyService, CurrentFamilyService>();
            builder.Services.AddSingleton<IPostAuthRouter, PostAuthRouter>();

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();

            builder.Services.AddTransient<SignUpViewModel>();
            builder.Services.AddTransient<SignUpPage>();

            builder.Services.AddTransient<FamilySetupPage>();

            builder.Services.AddTransient<CreateFamilyViewModel>();
            builder.Services.AddTransient<CreateFamilyPage>();

            builder.Services.AddTransient<InviteToFamilyViewModel>();
            builder.Services.AddTransient<InviteToFamilyPage>();

            builder.Services.AddTransient<ManageFamilyViewModel>();
            builder.Services.AddTransient<ManageFamilyPage>();

            builder.Services.AddTransient<FamilyQrCodeViewModel>();
            builder.Services.AddTransient<FamilyQrCodePage>();

            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MainPage>();

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