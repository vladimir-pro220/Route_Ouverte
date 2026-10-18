using RouteOuverte.Services;

namespace RouteOuverte;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light; // les maquettes sont en thème clair
        RoadService.Init();
    }

    protected override Window CreateWindow(IActivationState? activationState) => new Window(new AppShell());
}