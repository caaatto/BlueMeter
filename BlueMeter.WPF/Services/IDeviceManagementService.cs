namespace BlueMeter.WPF.Services;

using BlueMeter.WPF.Models;

public interface IDeviceManagementService
{
    Task<List<(string name, string description)>> GetNetworkAdaptersAsync();
    Task<NetworkAdapterInfo?> GetAutoSelectedNetworkAdapterAsync();
    void SetActiveNetworkAdapter(NetworkAdapterInfo adapter);
    void StopActiveCapture();
}