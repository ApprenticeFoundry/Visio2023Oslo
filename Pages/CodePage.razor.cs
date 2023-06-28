using BlazorComponentBus;
using Foundry.Helpers;
using FoundryBlazor.Extensions;
using FoundryBlazor.PubSub;
using IoBTModules.Extensions;
using Microsoft.AspNetCore.Components;
using Visio2023Foundry.Model;

namespace Visio2023Foundry.Pages;

public class CodePageBase : ComponentBase
{
    [Inject] private ICodeDisplayService? CodeDisplay { get; set; }
    [Parameter] public string? Folder { get; set; }
    public string Path { get; set; } = "";
    public CodeManifest? Manifest { get; set; }
    public CodeSummary? Summary { get; set; }
    public List<CodeSample> Samples { get; set; } = new();


    protected override void OnInitialized()
    {
        base.OnInitialized();

  
        var data = CodeDisplay!.GetCodeLibrary();
        if (!string.IsNullOrEmpty(Folder))
        {
            Path = data.Storage;
            Manifest = data.Manifests.FirstOrDefault(x => x.Folder.Matches(Folder));
            Summary = Manifest!.Summary;
            Samples = Manifest!.Samples; //.Where(x => x.HasTip()).ToList();
            //Payload = StorageHelpers.Dehydrate<CodeManifest>(Manifest!, false);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
        }

        await base.OnAfterRenderAsync(firstRender);
    }
    public bool HasImage()
    {
        //$"ImageURL =[{Sample!.ImageURL}]".WriteInfo();
        return !string.IsNullOrEmpty(Manifest?.ImageURL);
    }
    public bool HasSummary()
    {
        //$"ImageURL =[{Sample!.ImageURL}]".WriteInfo();
        return Summary != null;
    }
    public bool HasDemo()
    {
        // $"DemoURL =[{Sample!.DemoURL}]".WriteInfo();
        return !string.IsNullOrEmpty(Manifest?.DemoURL);
    }
    public bool HasMeme()
    {
        //$"ImageURL =[{Sample!.ImageURL}]".WriteInfo();
        return !string.IsNullOrEmpty(Manifest?.MemeURL);
    }

    public static string ModifyTitle(CodeManifest manifest)
    {
        var title =  manifest.Title.Split(" - ").Last();
        return title;
    }
}


