using RealEstateApp.Models;
using RealEstateApp.Services;
using RealEstateApp.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;


namespace RealEstateApp.ViewModels;
public class PropertyListPageViewModel : BaseViewModel
{
    private Location? _actualLocation;

    public ObservableCollection<PropertyListItem> SortedPropertiesCollection { get; } = new();
    public ObservableCollection<PropertyListItem> PropertiesCollection { get; } = new();


    private readonly IPropertyService service;

    public PropertyListPageViewModel(IPropertyService service)
    {
        Title = "Property List";
        this.service = service;
    }

    bool isRefreshing;
    public bool IsRefreshing
    {
        get => isRefreshing;
        set => SetProperty(ref isRefreshing, value);
    }

    private Command getPropertiesCommand;
    public ICommand GetPropertiesCommand => getPropertiesCommand ??= new Command(async () => await GetPropertiesAsync());

    async Task GetPropertiesAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;

            List<Property> properties = service.GetProperties();

            if (PropertiesCollection.Count != 0)
                PropertiesCollection.Clear();

            foreach (Property property in properties)
            {
                Location propertyLocation = new(property.Latitude!.Value, property.Longitude!.Value);

                if (_actualLocation is null)
                    _actualLocation = await Geolocation.GetLastKnownLocationAsync();

                double distance = _actualLocation.CalculateDistance(propertyLocation, DistanceUnits.Kilometers);

                SortedPropertiesCollection.Add(new PropertyListItem(property)
                {
                    Distance = distance
                });
            }

            List<PropertyListItem> sortedPropertyListItems = SortedPropertiesCollection.OrderBy(p => p.Distance).ToList();

            foreach (var item in sortedPropertyListItems)
            {
                PropertiesCollection.Add(item);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get monkeys: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    private Command goToDetailsCommand;
    public ICommand GoToDetailsCommand => goToDetailsCommand ??= new Command(async (object propertyListItem) => await GoToDetails(propertyListItem as PropertyListItem));
    async Task GoToDetails(PropertyListItem propertyListItem)
    {
        if (propertyListItem == null)
            return;

        await Shell.Current.GoToAsync(nameof(PropertyDetailPage), true, new Dictionary<string, object>
        {
            {"MyPropertyListItem", propertyListItem }
        });
    }

    private Command goToAddPropertyCommand;
    public ICommand GoToAddPropertyCommand => goToAddPropertyCommand ??= new Command(async () => await GotoAddProperty());
    async Task GotoAddProperty()
    {
        await Shell.Current.GoToAsync($"{nameof(AddEditPropertyPage)}?mode=newproperty", true, new Dictionary<string, object>
        {
            {"MyProperty", new Property() }
        });
    }

    private Command sortAsyncCommand;
    public ICommand SortAsyncCommand => sortAsyncCommand ??= new Command(async () => await SortAsync() );

    private async Task SortAsync()
    {
        try
        {
            _actualLocation = await Geolocation.GetLocationAsync();
            if (_actualLocation is null)
            {
                _actualLocation = await Geolocation.GetLastKnownLocationAsync();
            };

            await GetPropertiesAsync();
        }

        catch (FeatureNotSupportedException fnsEx)
        {
            Debug.WriteLine($"Feature not supported: {fnsEx.Message}");
            await Shell.Current.DisplayAlert("Error!", fnsEx.Message, "OK");
        }
        catch (FeatureNotEnabledException fneEx)
        {
            Debug.WriteLine($"Feature not enabled: {fneEx.Message}");
            await Shell.Current.DisplayAlert("Error!", fneEx.Message, "OK");
        }
        catch (PermissionException pEx)
        {
            Debug.WriteLine($"Permission not granted: {pEx.Message}");
            await Shell.Current.DisplayAlert("Error!", pEx.Message, "OK");
        }
        catch (Exception e)
        {

            await Shell.Current.DisplayAlert("Error!", e.Message, "OK");
        }
    }
}
