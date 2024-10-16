using RealEstateApp.Models;
using RealEstateApp.Services;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Input;
namespace RealEstateApp.ViewModels;

[QueryProperty(nameof(Mode), "mode")]
[QueryProperty(nameof(Property), "MyProperty")]
public class AddEditPropertyPageViewModel : BaseViewModel
{
    readonly IPropertyService service;

    public AddEditPropertyPageViewModel(IPropertyService service)
    {
        this.service = service;
        Agents = new ObservableCollection<Agent>(service.GetAgents());
    }

    public string Mode { get; set; }

    #region PROPERTIES
    public ObservableCollection<Agent> Agents { get; }

    private Property _property;
    public Property Property
    {
        get => _property;
        set
        {
            SetProperty(ref _property, value);
            Title = Mode == "newproperty" ? "Add Property" : "Edit Property";

            if (_property.AgentId != null)
            {
                SelectedAgent = Agents.FirstOrDefault(x => x.Id == _property?.AgentId);
            }
        }
    }

    private Agent _selectedAgent;
    public Agent SelectedAgent
    {
        get => _selectedAgent;
        set
        {
            if (Property != null)
            {
                _selectedAgent = value;
                Property.AgentId = _selectedAgent?.Id;
            }
        }
    }

    string statusMessage;
    public string StatusMessage
    {
        get { return statusMessage; }
        set { SetProperty(ref statusMessage, value); }
    }

    Color statusColor;
    public Color StatusColor
    {
        get { return statusColor; }
        set { SetProperty(ref statusColor, value); }
    }
    #endregion


    private Command savePropertyCommand;
    public ICommand SavePropertyCommand => savePropertyCommand ??= new Command(async () => await SaveProperty());
    private async Task SaveProperty()
    {
        if (IsValid() == false)
        {
           StatusMessage = "Please fill in all required fields";
            StatusColor = Colors.Red;
        }
        else
        {
            service.SaveProperty(Property);
            await Shell.Current.GoToAsync("///propertylist");
        }
    }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Property.Address)
            || Property.Beds == null
            || Property.Price == null
            || Property.AgentId == null)
            return false;
        return true;
    }

    private Command cancelSaveCommand;
    public ICommand CancelSaveCommand => cancelSaveCommand ??= new Command(async () => await Shell.Current.GoToAsync(".."));


    private Command getCurrentLocationCommand;
    public ICommand GetCurrentLocationCommand => getCurrentLocationCommand ??= new Command(async () => await GetCurrentLocation());

    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;

    public async Task GetCurrentLocation()
    {
        try
        {
            _isCheckingLocation = true;

            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            _cancelTokenSource = new CancellationTokenSource();

            Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

            if (location != null)
                Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

            _property.Longitude = location.Longitude;
            _property.Latitude = location.Latitude;
            OnPropertyChanged(nameof(Property));
            await UpdateAddress(location);
        }
        // Catch one of the following exceptions:
        //   FeatureNotSupportedException
        //   FeatureNotEnabledException
        //   PermissionException
        catch (Exception ex)
        {
            // Unable to get location
        }
        finally
        {
            _isCheckingLocation = false;
        }
    }

    public async Task UpdateAddress(Location location)
    {
        double lat = location.Latitude;
        double lng = location.Longitude;
        IEnumerable<Placemark> placemarks = await Geocoding.GetPlacemarksAsync(lat, lng);
        Placemark placemark = placemarks?.FirstOrDefault();

        if (placemark != null)
        {
            string address = $"{placemark.Thoroughfare}, {placemark.Locality}, {placemark.AdminArea}, {placemark.PostalCode}, {placemark.CountryName}";

            _property.Address = address;
            OnPropertyChanged(nameof(Property));
            
        }
        else
        {
            _property.Address = "Address not found";
        }

    }

    private Command updateLocationCommand;
    public ICommand UpdateLocationCommand => updateLocationCommand ??= new Command(async () => await UpdateLocation());

    public bool IsGeoCodingEnabled { get; internal set; }

    public async Task UpdateLocation()
    {
        try
        {
            if (Property.Address is null) 
            {
                await Application.Current.MainPage.DisplayAlert("Error!", "Please fill out address!", "Cancel");
            }
            var locations = await Geocoding.GetLocationsAsync(Property.Address);
            var location = locations?.FirstOrDefault();


            _property.Longitude = location.Longitude;
            _property.Latitude = location.Latitude;
            OnPropertyChanged(nameof(Property));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
            _cancelTokenSource.Cancel();
    }
}
