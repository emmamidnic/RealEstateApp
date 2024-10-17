using RealEstateApp.ViewModels;

namespace RealEstateApp.Views;

public partial class HeightCalculatorPage : ContentPage
{
	private HeightCalculatorPageViewModel _viewModel;
	public HeightCalculatorPage()
	{
		InitializeComponent();
		_viewModel = new HeightCalculatorPageViewModel();
		BindingContext = _viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		_viewModel.StartBarometer();
    }
	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.StopBarometer();
	}

}