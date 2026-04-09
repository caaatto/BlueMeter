using BlueMeter.Properties;

namespace BlueMeter.Models;

public enum NumberDisplayMode
{
    [LocalizedDescription(ResourcesKeys.NumberDisplay_KMB)]
    KMB,
    [LocalizedDescription(ResourcesKeys.NumberDisplay_Wan)]
    Wan
}
