using BlazorComponentBus;
using Foundry.Helpers;
using FoundryBlazor.Extensions;
using FoundryBlazor.PubSub;
using Microsoft.AspNetCore.Components;
using Visio2023Foundry.Model;

namespace Visio2023Foundry.Pages;

public class SamplesBase : ComponentBase
{
    [Inject] private ICodeDisplayService? CodeDisplay { get; set; }

    public List<CodeManifest> Manifests { get; set; } = new();
    public string? Payload { get; set; }

    public static string ComputeLink(CodeManifest sample)
    {
        return $"/codepage/{sample.Folder}";
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

  
        var data = CodeDisplay!.GetCodeLibrary();
        Manifests = data.Manifests;
        Payload = StorageHelpers.Dehydrate<CodeLibrary>(data, false);

       // SettingsHelpers.WriteData("Storage/StaticFiles/Code", "test.json", Payload);
    }

}
