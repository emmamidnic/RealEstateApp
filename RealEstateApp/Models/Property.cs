// Install-Package Fody
// Install-Package PropertyChanged.Fody
// Tilføj attribute:  [AddINotifyPropertyChangedInterface]

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RealEstateApp.Models
{
    public class Property
    {
        public Property()
        {
            Id = Guid.NewGuid().ToString();

            ImageUrls = new List<string>();
        }

        //string _name;
        //public string Name { get => _name; set { SetProperty(ref _name, value); } }

        public string Id { get; set; }
        public string Address { get; set; }
        public int? Price { get; set; }
        public string Description { get; set; }
        public int? Beds { get; set; }
        public int? Baths { get; set; }
        public int? Parking { get; set; }
        public int? LandSize { get; set; }
        public string AgentId { get; set; }
        public List<string> ImageUrls { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string MainImageUrl => ImageUrls?.FirstOrDefault() ?? GlobalSettings.Instance.NoImageUrl;

        #region INotifyPropertyChanged
        protected bool SetProperty<T>(ref T backingStore, T value,
                         [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
