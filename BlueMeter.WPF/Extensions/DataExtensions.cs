using Microsoft.Extensions.DependencyInjection;
using BlueMeter.Core.Analyze;
using BlueMeter.Core.Data;
using BlueMeter.WPF.Data;

namespace BlueMeter.WPF.Extensions;

public static class DataExtensions
{
    public static IServiceCollection AddPacketAnalyzer(this IServiceCollection services)
    {
        // return services.AddSingleton<IDataStorage, InstantizedDataStorage>()
        return services.AddSingleton<IDataStorage, DataStorageV2>()
            .AddSingleton<IPacketAnalyzer,PacketAnalyzerV2>()
            // .AddSingleton<IPacketAnalyzer, PacketAnalyzer>()
            .AddSingleton<MessageAnalyzerV2>();
    }
}