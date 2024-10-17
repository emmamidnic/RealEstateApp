using System.Collections.ObjectModel;
using System.Diagnostics;
using RealEstateApp.Models;
namespace RealEstateApp.ViewModels
{
    [QueryProperty(nameof(BarometerMeasurement), "MyBarometerMeasurement")]
    public class HeightCalculatorPageViewModel : BaseViewModel
    {
        private double _currentPressure;
        public double CurrentPressure
        {
            get => _currentPressure;
            set => SetProperty(ref _currentPressure, value);
        }

        private double _currentAltitude;
        public double CurrentAltitude
        {
            get => _currentAltitude;
            set => SetProperty(ref _currentAltitude, value);
        }

        private string _measurementLabel;
        public string MeasurementLabel
        {
            get => _measurementLabel;
            set => SetProperty(ref _measurementLabel, value);
        }

        public double SeaLevelPressure = 1012.9;

        public bool NoShowDifference;

        private BarometerMeasurement _barometerMeasurement;
        public BarometerMeasurement BarometerMeasurement
        {
            get => _barometerMeasurement;
            set
            {
                SetProperty(ref _barometerMeasurement, value);
            }
        }

        public ObservableCollection<BarometerMeasurement> Measurements { get; set; }


        private Command saveButtonCommand;
        public Command SaveButtonCommand => saveButtonCommand ??= new Command(CreateBarometerMeasurementObject);

        public void CreateBarometerMeasurementObject()
        {
            // Initialize variables
            double previousAltitude = 0;
            NoShowDifference = false;

            // Check if there is an existing measurement to retrieve the last altitude
            if (Measurements.Count > 0)
            {
                previousAltitude = Measurements.Last().Altitude;
            }

            // Create a new measurement with the current values
            var newMeasurement = new BarometerMeasurement
            {
                Pressure = CurrentPressure,
                Altitude = CurrentAltitude,
                Label = MeasurementLabel
            };

            // If previous altitude is non-zero, calculate height change
            if (previousAltitude != 0)
            {
                newMeasurement.HeightChange = newMeasurement.Altitude - previousAltitude;
            }
            else
            {
                // If previous altitude is zero, we don't show the height change
                NoShowDifference = true;
            }

            // Add the new measurement to the collection
            Measurements.Add(newMeasurement);

            // Reset the measurement label after saving
            MeasurementLabel = string.Empty;
        }


        public HeightCalculatorPageViewModel()
        {
            Measurements = new ObservableCollection<BarometerMeasurement>();

            // Check if Barometer is available
            if (Barometer.Default.IsSupported)
            {
                Barometer.Default.ReadingChanged += OnBarometerReadingChanged;
            }
            else
            {
                Debug.WriteLine("Barometer not supported on this device.");
            }
        }

        // This method will be called when the pressure changes
        private void OnBarometerReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            // Check if the pressure change is significant (e.g., difference greater than 0.1 hPa)
            if (Math.Abs(CurrentPressure - e.Reading.PressureInHectopascals) > 0.1)
            {
                // Get the pressure reading and update the property
                CurrentPressure = e.Reading.PressureInHectopascals;
                CurrentAltitude = CalculateCurrentAltitude(CurrentPressure);
            }
        }

        // Call this method to start the barometer
        public void StartBarometer()
        {
            try
            {
                if (!Barometer.Default.IsMonitoring)
                {
                    Barometer.Default.Start(SensorSpeed.UI);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to start barometer: {ex.Message}");
            }
        }

        // Call this method to stop the barometer
        public void StopBarometer()
        {
            try
            {
                if (Barometer.Default.IsMonitoring)
                {
                    Barometer.Default.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to stop barometer: {ex.Message}");
            }
        }

        // Be sure to unsubscribe from the event to avoid memory leaks
        public void Dispose()
        {
            Barometer.Default.ReadingChanged -= OnBarometerReadingChanged;
        }

        public double CalculateCurrentAltitude(double currentPressure)
        {
            return (44307.694 * (1 - Math.Pow(currentPressure / SeaLevelPressure, 0.190284)));
        }


    }
}

