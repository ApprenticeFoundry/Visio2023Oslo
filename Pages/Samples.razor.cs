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

    public static string ComputeLink(CodeManifest sample)
    {
        return $"/codepage/{sample.Folder}";
    }
    public static string ComputeComplete(CodeManifest sample)
    {
        var complete =  sample.Status.Matches("Done") ? "btn btn-secondary m-1" : "btn btn-warning m-1";
        complete =  sample.Status.Matches("Show") ? "btn btn-success m-1" : complete;
        return complete;
    }
    protected override void OnInitialized()
    {
        base.OnInitialized();

  
        var data = CodeDisplay!.GetCodeLibrary();
        Manifests = data.Manifests.Where(item => item.HasTip()).ToList();
        //Payload = StorageHelpers.Dehydrate<CodeLibrary>(data, false);


       // SettingsHelpers.WriteData("Storage/StaticFiles/Code", "test.json", Payload);
    }

}
