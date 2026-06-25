using Microsoft.AspNetCore.WebUtilities;

namespace RevestAdmin.Helpers;

public static class QueryHelper
{
    public static string? GetParam(string uri, string key)
    {
        var query = QueryHelpers.ParseQuery(new Uri(uri).Query);
        return query.TryGetValue(key, out var v) ? v.ToString() : null;
    }

    public static int GetIntParam(string uri, string key, int fallback = 1)
        => int.TryParse(GetParam(uri, key), out var v) ? v : fallback;

    public static bool? GetBoolParam(string uri, string key)
        => bool.TryParse(GetParam(uri, key), out var v) ? v : null;

    public static decimal? GetDecimalParam(string uri, string key)
        => decimal.TryParse(GetParam(uri, key), out var v) ? v : null;
}
