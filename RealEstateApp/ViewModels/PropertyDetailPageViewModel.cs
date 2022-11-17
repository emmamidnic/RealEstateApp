using RealEstateApp.Models;

namespace RealEstateApp.ViewModels;

[QueryProperty(nameof(Property), "MyProperty")]
public class PropertyDetailPageViewModel : BaseViewModel
{
    Property property;
    public Property Monkey
    {
        get => property;
        set => SetProperty(ref property, value);
    }
}
