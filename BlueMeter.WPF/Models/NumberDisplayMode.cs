using BlueMeter.WPF.Properties;

namespace BlueMeter.WPF.Models;

public enum NumberDisplayMode
{
    [LocalizedDescription(ResourcesKeys.NumberDisplay_KMB)]
    KMB,
    [LocalizedDescription(ResourcesKeys.NumberDisplay_Wan)]
    Wan
}