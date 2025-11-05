using System.Collections.ObjectModel;
using System.Windows;

namespace StarResonanceDpsAnalysis.WPF.Themes;

/// <summary>
///     Allows managing application dictionaries.
/// </summary>
internal class ResourceDictionaryManager(string searchNamespace)
{
    /// <summary>
    ///     Gets the namespace, e.g. the library the resource is being searched for.
    /// </summary>
    public string SearchNamespace { get; } = searchNamespace;

    /// <summary>
    ///     Shows whether the application contains the <see cref="ResourceDictionary" />.
    /// </summary>
    /// <param name="resourceLookup">Any part of the resource name.</param>
    /// <returns><see langword="false" /> if it doesn't exist.</returns>
    public bool HasDictionary(string resourceLookup)
    {
        return GetDictionary(resourceLookup) != null;
    }

    /// <summary>
    ///     Gets the <see cref="ResourceDictionary" /> if exists.
    /// </summary>
    /// <param name="resourceLookup">Any part of the resource name.</param>
    /// <returns><see cref="ResourceDictionary" />, <see langword="null" /> if it doesn't exist.</returns>
    public ResourceDictionary? GetDictionary(string resourceLookup)
    {
        var applicationDictionaries = GetApplicationMergedDictionaries();

        if (applicationDictionaries.Count == 0)
        {
            return null;
        }

        resourceLookup = resourceLookup.ToLower().Trim();
        var searchNamespaceLower = SearchNamespace.ToLower().Trim();

        foreach (var t in applicationDictionaries)
        {
            string resourceDictionaryUri;

            if (t.Source != null)
            {
                resourceDictionaryUri = t.Source.ToString().ToLower().Trim();

                if (ShouldMatchDictionary(resourceDictionaryUri, searchNamespaceLower, resourceLookup))
                {
                    return t;
                }
            }

            foreach (var t1 in t.MergedDictionaries)
            {
                if (t1?.Source == null)
                {
                    continue;
                }

                resourceDictionaryUri = t1.Source.ToString().ToLower().Trim();

                if (ShouldMatchDictionary(resourceDictionaryUri, searchNamespaceLower, resourceLookup))
                {
                    return t1;
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Determines whether a dictionary URI should match based on namespace and lookup criteria.
    /// </summary>
    private bool ShouldMatchDictionary(string uri, string searchNamespace, string resourceLookup)
    {
        // If namespace is empty or not specified, just check if the lookup matches
        if (string.IsNullOrEmpty(searchNamespace))
        {
            return uri.Contains(resourceLookup);
        }

        // Original logic: both namespace and lookup must match
        return uri.Contains(searchNamespace) && uri.Contains(resourceLookup);
    }

    /// <summary>
    ///     Shows whether the application contains the <see cref="ResourceDictionary" />.
    /// </summary>
    /// <param name="resourceLookup">Any part of the resource name.</param>
    /// <param name="newResourceUri">A valid <see cref="Uri" /> for the replaced resource.</param>
    /// <returns><see langword="true" /> if the dictionary <see cref="Uri" /> was updated. <see langword="false" /> otherwise.</returns>
    public bool UpdateDictionary(string resourceLookup, Uri? newResourceUri)
    {
        Collection<ResourceDictionary> applicationDictionaries = Application
            .Current
            .Resources
            .MergedDictionaries;

        if (applicationDictionaries.Count == 0 || newResourceUri is null)
        {
            return false;
        }

        resourceLookup = resourceLookup.ToLower().Trim();
        var searchNamespaceLower = SearchNamespace.ToLower().Trim();

        for (var i = 0; i < applicationDictionaries.Count; i++)
        {
            string sourceUri;

            if (applicationDictionaries[i].Source != null)
            {
                sourceUri = applicationDictionaries[i].Source.ToString().ToLower().Trim();

                if (ShouldMatchDictionary(sourceUri, searchNamespaceLower, resourceLookup))
                {
                    applicationDictionaries[i] = new ResourceDictionary { Source = newResourceUri };

                    return true;
                }
            }

            for (var j = 0; j < applicationDictionaries[i].MergedDictionaries.Count; j++)
            {
                if (applicationDictionaries[i].MergedDictionaries[j]?.Source == null)
                {
                    continue;
                }

                sourceUri = applicationDictionaries[i]
                    .MergedDictionaries[j]
                    .Source.ToString()
                    .ToLower()
                    .Trim();

                if (ShouldMatchDictionary(sourceUri, searchNamespaceLower, resourceLookup))
                {
                    applicationDictionaries[i].MergedDictionaries[j] =
                        new ResourceDictionary { Source = newResourceUri };

                    return true;
                }
            }
        }

        return false;
    }

    private static Collection<ResourceDictionary> GetApplicationMergedDictionaries()
    {
        return Application.Current.Resources.MergedDictionaries;
    }
}