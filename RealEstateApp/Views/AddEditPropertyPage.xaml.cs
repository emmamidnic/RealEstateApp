using Microsoft.Maui.Networking;
using RealEstateApp.ViewModels;

namespace RealEstateApp.Views;

public partial class AddEditPropertyPage : ContentPage
{
    public AddEditPropertyPage(AddEditPropertyPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        // Tjek for forbindelsen ved åbning
        CheckConnectivity();

    }

    private async void CheckConnectivity()
    {
        var current = Connectivity.Current.NetworkAccess;
        if (current != NetworkAccess.Internet)
        {
            await DisplayAlert("Ingen forbindelse", "Der er ikke nogen internetforbindelse.", "OK");
        }
    }
     
}
