using Microsoft.Extensions.Logging;
using RealEstateApp.Repositories;
using RealEstateApp.Services;
using RealEstateApp.ViewModels;
using RealEstateApp.Views;

namespace RealEstateApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("fa-solid-900.ttf", "FA-solid");
            });

        builder.Services.AddSingleton<IPropertyService, MockRepository>();
        builder.Services.AddSingleton<PropertyListPage>();
        builder.Services.AddSingleton<PropertyListPageViewModel>();

        builder.Services.AddTransient<PropertyDetailPage>();
        builder.Services.AddTransient<PropertyDetailPageViewModel>();

        builder.Services.AddTransient<AddEditPropertyPage>();
        builder.Services.AddTransient<AddEditPropertyPageViewModel>();
        builder.Services.AddSingleton(Connectivity.Current);
        builder.Services.AddSingleton<IBattery>(Battery.Default);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
