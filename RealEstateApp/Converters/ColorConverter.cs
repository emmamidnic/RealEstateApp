using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Converters
{
    public class ColorConverter : IValueConverter
    {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is Tuple<BatteryState, double, bool> batteryInfo)
                {
                    var state = batteryInfo.Item1;
                    var chargeLevel = batteryInfo.Item2;
                    var isEnergySaverOn = batteryInfo.Item3;

                    // Green if in energy saver mode
                    if (isEnergySaverOn)
                        return Colors.Green;

                    // Yellow if charging
                    if (state == BatteryState.Charging)
                        return Colors.Yellow;

                    // Red if discharging and battery is below 20%
                    if (state == BatteryState.NotCharging && chargeLevel < 0.2)
                        return Colors.Red;
                }

                // Default color if none of the conditions match
                return Colors.Transparent;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
    }
}
