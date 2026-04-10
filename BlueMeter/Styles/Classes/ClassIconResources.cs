using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ClassesEnum = BlueMeter.Core.Models.Classes;

namespace BlueMeter.Styles.Classes;

/// <summary>
/// Populates <see cref="Application.Resources"/> with the per-class profession icons
/// that <c>ClassesToIconConverter</c> (and anything else that looks up an icon for a
/// <see cref="BlueMeter.Core.Models.Classes"/> value) pulls out of the resource dictionary.
///
/// The WPF project did this with a 150-line <c>ClassesEnum.Images.xaml</c> dictionary full
/// of keyed <c>DrawingImage</c> geometries and <c>BitmapImage</c> entries. The Avalonia
/// port takes a simpler route: we already link every <c>*_Profession.png</c> file from
/// <c>BlueMeter.Assets</c> into this assembly under <c>avares://BlueMeter/Assets/Images/</c>,
/// so we just load them as <see cref="Bitmap"/> instances and add them to the resource
/// dictionary under both key forms the converter tries:
///   <list type="bullet">
///     <item>the <see cref="BlueMeter.Core.Models.Classes"/> enum value itself</item>
///     <item>a string key of the form <c>Classes{Name}Icon</c></item>
///   </list>
/// The vector <c>DrawingImage</c> fallbacks from the WPF file are dropped — nothing in
/// the ported code references them directly, and the PNGs are the canonical source.
/// </summary>
internal static class ClassIconResources
{
    public static void Register(Application app)
    {
        var resources = app.Resources;

        // Unknown placeholder: reuse the shield-knight icon as a sane fallback so the
        // converter never returns null mid-render. Callers that care can still check
        // for a specific "no class identified" state.
        TryLoad(resources, ClassesEnum.Unknown, "ShieldKnight");

        TryLoad(resources, ClassesEnum.Stormblade, "Stormblade");
        TryLoad(resources, ClassesEnum.FrostMage, "FrostMage");
        TryLoad(resources, ClassesEnum.WindKnight, "WindKnight");
        TryLoad(resources, ClassesEnum.VerdantOracle, "VerdantOracle");
        TryLoad(resources, ClassesEnum.HeavyGuardian, "HeavyGuardian");
        TryLoad(resources, ClassesEnum.Marksman, "Marksman");
        TryLoad(resources, ClassesEnum.ShieldKnight, "ShieldKnight");
        TryLoad(resources, ClassesEnum.SoulMusician, "SoulMusician");
    }

    private static void TryLoad(IResourceDictionary resources, ClassesEnum key, string assetBaseName)
    {
        try
        {
            var uri = new Uri($"avares://BlueMeter/Assets/Images/{assetBaseName}_Profession.png");
            using var stream = AssetLoader.Open(uri);
            var bitmap = new Bitmap(stream);

            // The enum-value key is what ClassesToIconConverter tries first.
            resources[key] = bitmap;

            // The string key mirrors the WPF BitmapImage entries (ClassesShieldKnightIcon etc.)
            // so existing XAML StaticResource lookups keep working.
            resources[$"Classes{assetBaseName}Icon"] = bitmap;

            // The special "ClassesUnknownIcon" key is referenced as a FallbackValue in
            // DpsStatisticsView.xaml — expose it under that exact string when we register
            // the Unknown slot.
            if (key == ClassesEnum.Unknown)
            {
                resources["ClassesUnknownIcon"] = bitmap;
            }
        }
        catch
        {
            // Missing asset: leave the slot empty. ClassesToIconConverter returns null
            // when the lookup misses, and the bound Image falls back to its FallbackValue.
        }
    }
}
