using System.Text.RegularExpressions;

namespace FrameToolkit.Extensions;

public static class StringCasingExtension
{
    public static string ToFirstUpperCase(this string str)
    {
        return $"{str[..1].ToUpper()}{str.Remove(0, 1).ToLower()}";
    }

    public static string? Sanitize(this string? inputString)
    {
        if (string.IsNullOrEmpty(inputString))
        {
            return inputString;
        }

        string sanitizedHtml = Regex.Replace(inputString, @"<script[^>]*?>.*?</script>",
            string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        sanitizedHtml = Regex.Replace(sanitizedHtml, @"<iframe[^>]*?>.*?</iframe>",
            string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        sanitizedHtml = Regex.Replace(sanitizedHtml, @"<(object|embed|form|style|a)[^>]*?>.*?</\1>",
            string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        string allowedTags = @"(b|i|u|em|strong|p|h1|h2|h3|h4|h5|h6|ul|ol|li|br|span|div)";
        sanitizedHtml = Regex.Replace(sanitizedHtml, @"<(?!/?(" + allowedTags + @")\b)[^>]+?>",
            string.Empty, RegexOptions.IgnoreCase);

        return sanitizedHtml;
    }
}
