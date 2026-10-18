using RouteOuverte.Models;
using RouteOuverte.PageModels;

namespace RouteOuverte.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}