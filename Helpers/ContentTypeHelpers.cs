using System.Text;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http;
using FoundryRulesAndUnits.Extensions;

namespace Foundry.Helpers;

public static class ContentTypeHelpers
{
    private static FileExtensionContentTypeProvider? _provider;


    public static FileExtensionContentTypeProvider MIMETypeProvider()
    {
        if (_provider == null)
        {
            _provider = new FileExtensionContentTypeProvider();

            foreach (KeyValuePair<string, string> c in FileExtensionHelpers.MIMETypeData())
            {
                _provider.Mappings[c.Key] = c.Value;
            }
        }

        return _provider;
    }


    public static string GetMIMEType(this string fileName)
    {
        var provider = ContentTypeHelpers.MIMETypeProvider();

        if (!provider.TryGetContentType(fileName, out string? contentType) && contentType == null)
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }

    public static async Task<string> ReadAsStringAsync(this IFormFile file)
    {
        var result = new StringBuilder();
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            while (reader.Peek() >= 0)
                result.AppendLine(await reader.ReadLineAsync());
        }
        return result.ToString();
    }
}
