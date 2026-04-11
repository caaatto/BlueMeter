namespace BlueMeter.Services;

public interface IApplicationStartup
{
    Task InitializeAsync();
    void Shutdown();
}
