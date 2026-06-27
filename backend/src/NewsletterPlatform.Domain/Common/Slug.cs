using System.Text.RegularExpressions;

namespace NewsletterPlatform.Domain.Common;

public static class Slug
{
    private static readonly Regex Allowed = new(@"^[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$", RegexOptions.Compiled);

    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var slug = input.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9-]", "");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }

    public static bool IsValid(string slug) => !string.IsNullOrWhiteSpace(slug) && Allowed.IsMatch(slug);
}