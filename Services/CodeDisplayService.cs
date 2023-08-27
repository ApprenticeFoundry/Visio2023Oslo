
using Foundry.Helpers;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;

namespace Visio2023Foundry.Model;

public interface ICodeDisplayService
{
    CodeLibrary GetCodeLibrary();
}
public class CodeDisplayService : ICodeDisplayService
{
    public CodeLibrary Data { get; set; }

    public CodeDisplayService()
    {
        Data = GetCodeLibrary();
    }

    public CodeLibrary GetCodeLibrary()
    {
        Data ??= CreateCodeLibrary();
        return Data;
    }


    public CodeLibrary CreateCodeLibrary()
    {
        var storage = "storage/StaticFiles/Code/";
        var manifests = new List<CodeManifest>();
        var collection = Directory.GetDirectories(storage);
        foreach (var path in collection)
        {
            $"Loading Folder {path}".WriteInfo();
            var text = SettingsHelpers.ReadData(path, "manifest.json");
            var manifest = SettingsHelpers.Hydrate<CodeManifest>(text, false);

            manifest.Folder = path.Split("/").Last();
            manifest.ModifyImageUrl(path);
            manifest.ModifyMemeUrl(path);
            manifests.Add(manifest);
        }

        var library = new CodeLibrary()
        {
            Name = "Samples",
            Storage = storage,
            Manifests = manifests.OrderBy(x => x.Title).ToList()
        };

        return library;
    }
}