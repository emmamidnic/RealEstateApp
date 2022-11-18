using RealEstateApp.ViewModels;

namespace RealEstateApp.Views;

public partial class AddEditPropertyPage : ContentPage
{
	public AddEditPropertyPage(AddEditPropertyPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}