using RealEstateApp.Models;
using RealEstateApp.Services;
using RealEstateApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Windows.Input;
namespace RealEstateApp.ViewModels;

[QueryProperty(nameof(Mode), "mode")]
[QueryProperty(nameof(Property), "MyProperty")]
public class AddEditPropertyPageViewModel : BaseViewModel
{
    readonly IPropertyService service;
    readonly IBattery battery;

    public AddEditPropertyPageViewModel(IPropertyService service, IConnectivity connectivity, IBattery battery)
    {
        this.service = service;
        Agents = new ObservableCollection<Agent>(service.GetAgents());

        this.connectivity = connectivity;
        connectivity.ConnectivityChanged += OnConnectivityChanged;

        #region OPGAVE 3.7
        this.battery = battery;

        // Subscribe to battery level change event
        battery.BatteryInfoChanged += OnBatteryInfoChanged;

        // Check battery level initially
        CheckBatteryLevel();
        #endregion
    }

    public bool IsOnline => connectivity.NetworkAccess == NetworkAccess.Internet;
    private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsOnline));
        if (IsOnline == false)
        {
            await Shell.Current.DisplayAlert("Offline", "You must be online to use geocoding", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlert("Online", "You are now online and can use geocoding", "OK");
        }

    }

    public string Mode { get; set; }

    #region PROPERTIES
    public ObservableCollection<Agent> Agents { get; }

    private IConnectivity connectivity;
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
           Vibration.Vibrate(TimeSpan.FromSeconds(3));
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
    public ICommand CancelSaveCommand => cancelSaveCommand ??= new Command(async () =>
    {
        if (Vibration.Default.IsSupported)
        {
            Vibration.Default.Cancel();
        }

        await Shell.Current.GoToAsync("..");
    });


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


    #region OPGAVE 3.6

    private bool isSpeechPlaying;
    private CancellationTokenSource cancellationTokenSource;

    private Command textToSpeechCommand;
    private Command cancelSpeechCommand;
    public ICommand TextToSpeechCommand => textToSpeechCommand ??= new Command(ExecuteTextToSpeech);
    public ICommand CancelSpeechCommand => cancelSpeechCommand ??= new Command(ExecuteCancelSpeech, CanExecuteCancelSpeech);


    public bool IsSpeechPlaying
    {
        get => isSpeechPlaying;
        set
        {
            isSpeechPlaying = value;
            OnPropertyChanged();
            // Update CancelSpeechCommand execution status
            ((Command)CancelSpeechCommand).ChangeCanExecute();
        }
    }

    private async void ExecuteTextToSpeech()
    {
        var descriptionText = Property.Description;

        if (!string.IsNullOrWhiteSpace(descriptionText))
        {
            cancellationTokenSource = new CancellationTokenSource();
            IsSpeechPlaying = true;

            try
            {
                await TextToSpeech.SpeakAsync(descriptionText, cancelToken: cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Handle when speech is cancelled
            }
            finally
            {
                IsSpeechPlaying = false;
            }
        }
    }

    private bool CanExecuteCancelSpeech()
    {
        return cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested;
    }

    private void ExecuteCancelSpeech()
    {
        if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
        }
    }

    #endregion

    #region OPGAVE 3.7 BATTERY MANGLI AT DEBUGGA !!!
    private string batteryWarningMessage;
    public string BatteryWarningMessage
    {
        get => batteryWarningMessage;
        set => SetProperty(ref batteryWarningMessage, value);
    }
    // New property for the battery status
    private Tuple<BatteryState, double, bool> batteryStatus;
    public Tuple<BatteryState, double, bool> BatteryStatus
    {
        get => batteryStatus;
        set => SetProperty(ref batteryStatus, value);
    }

    private bool isBatteryWarningVisible;
    public bool IsBatteryWarningVisible
    {
        get => isBatteryWarningVisible;
        set => SetProperty(ref isBatteryWarningVisible, value);
    }

    private void OnBatteryInfoChanged(object sender, BatteryInfoChangedEventArgs e)
    {
        CheckBatteryLevel();
    }

    // Method to check the current battery level and show an alert if below 20%
    private void CheckBatteryLevel()
    {
        bool isEnergySaverOn = battery.EnergySaverStatus == EnergySaverStatus.On;
        BatteryStatus = new Tuple<BatteryState, double, bool>(battery.State, battery.ChargeLevel, isEnergySaverOn);
        if (battery.ChargeLevel < 0.2)
        {
            BatteryWarningMessage = "Warning: Battery level is below 20%. Please charge your device.";
            IsBatteryWarningVisible = true;
        }
        else
        {
            IsBatteryWarningVisible = false;
        }
    }

    #endregion

    #region Opgave 3.7 Flashlight
    private bool isFlashlightOn;

    private Command toggleFlashlightCommand;
    public ICommand ToggleFlashlightCommand => toggleFlashlightCommand ??= new Command(async () => await ToggleFlashlight());

    // Property to bind to the Switch in the XAML
    public bool IsFlashlightOn
    {
        get => isFlashlightOn;
        set
        {
            if (SetProperty(ref isFlashlightOn, value))
            {
                ToggleFlashlightCommand.Execute(value);
            }
        }
    }

    public async Task ToggleFlashlight()
    {
        try
        {
            if (isFlashlightOn)
                await Flashlight.Default.TurnOnAsync();
            else
                await Flashlight.Default.TurnOffAsync();
        }
        catch (FeatureNotSupportedException ex)
        {
            // Handle not supported on device exception
        }
        catch (PermissionException ex)
        {
            // Handle permission exception
        }
        catch (Exception ex)
        {
            // Unable to turn on/off flashlight
        }
    }


    #endregion

    #region OPGAVE 4.1

    private Command goToCompassCommand;
    public ICommand GoToCompassCommand => goToCompassCommand ??= new Command(async () =>
    {
        if (Property == null)
            return;

        await Shell.Current.GoToAsync(nameof(CompassPage), true, new Dictionary<string, object>()
        {
            {"MyProperty", Property}
        });
    });

    #endregion
}
