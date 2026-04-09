using BlueMeter.Core.Analyze;
using BlueMeter.Core.Data;
using BlueMeter.WPF.Data; // IDataStorage lives under this namespace in BlueMeter.Core (legacy WPF naming)
using Microsoft.Extensions.DependencyInjection;

namespace BlueMeter.Extensions;

public static class DataExtensions
{
    public static IServiceCollection AddPacketAnalyzer(this IServiceCollection services)
    {
        return services.AddSingleton<IDataStorage, DataStorageV2>()
            .AddSingleton<IPacketAnalyzer, PacketAnalyzerV2>()
            .AddSingleton<MessageAnalyzerV2>();
    }
}
