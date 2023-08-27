using BlazorComponentBus;

using FoundryBlazor.PubSub;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Visio2023Foundry.Model;


namespace Visio2023Foundry.Pages;

public partial class Visio2023ArenaPage : ComponentBase
{
    [Inject] public NavigationManager? Navigation { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] private IToast? Toast { get; set; }
    [Inject] public IWorkspace? Workspace { get; init; }


    [Parameter]
    public string? LoadWorkbook { get; set; }

    protected override void OnInitialized()
    {
        Workspace?.SetBaseUrl(Navigation?.BaseUri ?? "");
    }

    protected override async Task OnInitializedAsync()
    {
        if (Workspace != null)
        {
            Workspace.ClearAllWorkbook();

            Workspace.EstablishWorkbook<Gamer>();
            if ("Boid".Matches(LoadWorkbook!))
                Workspace.EstablishWorkbook<BoidManager>();

            var url = ""; //RestAPI?.GetServerUrl() ?? "";
            Workspace.CreateCommands(Workspace, JsRuntime!, Navigation!, url);

            Workspace.CreateMenus(Workspace, JsRuntime!, Navigation!);


            var defaultHubURI = Navigation!.ToAbsoluteUri("/DrawingSyncHub").ToString();
            await Workspace.InitializedAsync(defaultHubURI!);

            //Toast?.Info(LoadWorkbook ?? "No LoadWorkbook");
        }

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
           // PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
            PubSub!.SubscribeTo<ViewStyle>(OnViewStyleChanged);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        InvokeAsync(StateHasChanged);
         $"Visio2023Page OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }

    private void OnViewStyleChanged(ViewStyle e)
    {
        Workspace?.SetViewStyle(e);
        InvokeAsync(StateHasChanged);
    }
}
