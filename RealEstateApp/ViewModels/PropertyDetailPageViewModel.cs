using Android.Telecom;
using RealEstateApp.Models;
using RealEstateApp.Services;
using RealEstateApp.Views;
using System.Windows.Input;

namespace RealEstateApp.ViewModels;

[QueryProperty(nameof(PropertyListItem), "MyPropertyListItem")]
public class PropertyDetailPageViewModel : BaseViewModel
{
    private readonly IPropertyService service;
    public PropertyDetailPageViewModel(IPropertyService service)
    {
        this.service = service;
    }

    Property property;
    public Property Property { get => property; set { SetProperty(ref property, value); } }

    Vendor vendor;
    public Vendor Vendor { get => vendor; set { SetProperty(ref vendor, value); } }


    Agent agent;
    public Agent Agent { get => agent; set { SetProperty(ref agent, value); } }


    PropertyListItem propertyListItem;
    public PropertyListItem PropertyListItem
    {
        set
        {
            SetProperty(ref propertyListItem, value);
           
            Property = propertyListItem.Property;
            Agent = service.GetAgents().FirstOrDefault(x => x.Id == Property.AgentId);
        }
    }

    private Command editPropertyCommand;
    public ICommand EditPropertyCommand => editPropertyCommand ??= new Command(async () => await GotoEditProperty());
    async Task GotoEditProperty()
    {
        if (property == null) 
            return;

        await Shell.Current.GoToAsync(nameof(AddEditPropertyPage), true, new Dictionary<string, object>()
        {
            {"MyProperty", Property}
        });
    }

    #region Opgave 5.1
    private Command vendorActionSheetCommand;
    public Command VendorActionSheetCommand => vendorActionSheetCommand ??= new Command(async () => await VendorActionSheet());
    public async Task VendorActionSheet()
    {
        string chosen = await Shell.Current.DisplayActionSheet("Choose one: ", "Cancel", null, "Call", "SMS", "Email");
        if (chosen == "Call")
        {
            PhoneDialer.Open(Property.Vendor.Phone);
        }
        else if (chosen == "SMS")
        {
            await Sms.ComposeAsync(new SmsMessage()
            {
                Recipients = new List<string>
                {
                    Property.Vendor.Phone
                },
            });
        }
        else if (chosen == "Email")
        {
            await Email.ComposeAsync(new EmailMessage()
            {
                To = new List<string> { Property.Vendor.Email },
                Subject = Property.Address
            });
        }
    }
    #endregion
}
