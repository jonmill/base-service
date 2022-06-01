using Grpc.Core;
using Terminal.Gui;
using Warbreaker.Protobufs;

namespace Warbreaker.Console;

internal class App : Toplevel
{
    private Window _mainFrame;
    private Login.LoginClient _loginService;
    private string _jwtToken;

    public App()
    {
        Application.Init();
        Ready += OnInit;
        _mainFrame = new Window();
        _loginService = new Login.LoginClient(Grpc.Net.Client.GrpcChannel.ForAddress("https://127.0.0.1:7224", new Grpc.Net.Client.GrpcChannelOptions()
        {
            Credentials = ChannelCredentials.Create(ChannelCredentials.SecureSsl, CallCredentials.FromInterceptor(AsyncAuthInterceptor)),
            DisposeHttpClient = true,
        }));
        _jwtToken = string.Empty;
    }

    private Task AsyncAuthInterceptor(AuthInterceptorContext context, Metadata metadata)
    {
        if (string.IsNullOrEmpty(_jwtToken) == false)
        {
            metadata.Add("Authorize", $"Bearer {_jwtToken}");
        }

        return Task.CompletedTask;
    }

    private void OnInit()
    {
        _mainFrame.X = 0;
        _mainFrame.Y = 1; // Leave a row for a menubar
        _mainFrame.Width = Dim.Fill();
        _mainFrame.Height = Dim.Fill() - 1; // Leave room for the menubar
        _mainFrame.Title = "Application Title";

        MenuBar menu = new MenuBar(new MenuBarItem[]
        {
            new MenuBarItem("_File", new MenuItem[]
            {
                new MenuItem("Log_in", "Log in to the application", Login, () => string.IsNullOrEmpty(_jwtToken)),
                new MenuItem("L_ogout", "Log out of the application", Logout, () => string.IsNullOrEmpty(_jwtToken) == false),
                new MenuItem("_Quit", "Exit the application", () => Application.RequestStop(this)),
            }),
        });
        this.Add(menu, _mainFrame);
    }

    private void Login() => LoginAsync().GetAwaiter().GetResult();

    private async Task LoginAsync()
    {
        LoginResponse response = await _loginService.LoginAsync(new LoginRequest()
        {
            Email = "Literally anything right now",
            Password = "Test",
        });
        _jwtToken = response.JwtToken;
    }

    private void Logout()
    {
        _jwtToken = string.Empty;
    }
}
