using BlazorComponentBus;

using FoundryBlazor.PubSub;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

using Visio2023Foundry.Model;
using Visio2023Foundry.Simulation;
using Visio2023Foundry.Targets;

namespace Visio2023Foundry.Pages;

public partial class DuelViewPage : ComponentBase, IDisposable
{
    [Inject] public NavigationManager? Navigation { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] private IToast? Toast { get; set; }
    [Inject] public IWorkspace? Workspace { get; init; }


    [Parameter]  
    public string? LoadWorkbook { get; set; }  

    public string GetVisible2D()
    {
        return Workspace!.IsViewStyle2D() ? "" : "visually-hidden";
    }

    public string GetVisible3D()
    {
        return Workspace!.IsViewStyle3D() ? "" : "visually-hidden";
    }

    protected override void OnInitialized()
    {
        Workspace!.SetBaseUrl(Navigation?.BaseUri ?? "");
       if ( Navigation != null)
            Navigation.LocationChanged += LocationChanged;

        // Workspace.EstablishWorkbook<Playground>().Name = "playground";
        // Workspace.EstablishWorkbook<Stencil>().Name = "stencil";
        // Workspace.EstablishWorkbook<TargetManager>().Name = "network";
        // Workspace.EstablishWorkbook<BoidManager>().Name = "boid"; 
        // Workspace.EstablishWorkbook<Composition>().Name = "composition"; 
        // Workspace.EstablishWorkbook<MoSimulation>().Name = "simulation"; 
        // Workspace.EstablishWorkbook<Process>().Name = "process"; 
        // Workspace.EstablishWorkbook<SignalRDemo>().Name = "SignalR"; 
    }

    public void Dispose()
    {
        Workspace?.OnDispose();
        if ( Navigation != null)
            Navigation.LocationChanged -= LocationChanged;
    }
 
    private void LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if ( e.Location.Contains("duelview") )
        {
            $"duelview LocationChanged {e.Location}".WriteInfo();
            RefreshWorkbookMenus();
        }
        StateHasChanged();
    }

    private void RefreshWorkbookMenus()
    {
        if (Workspace != null)
        {
            var found = Workspace.FindWorkbook(LoadWorkbook!);
            if (found != null)
            {
                Workspace?.SetCurrentWorkbook(found!).CurrentPage();
            } else {
                $"duelview RefreshWorkbookMenus {LoadWorkbook} not found".WriteInfo();
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (Workspace != null)
        {
            RefreshWorkbookMenus();

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
            PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
            PubSub!.SubscribeTo<ViewStyle>(OnViewStyleChanged);
            Workspace!.GetDrawing();
            Workspace!.GetArena();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        InvokeAsync(StateHasChanged);
         $"duelview OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }

    private void OnViewStyleChanged(ViewStyle e)
    {
        Workspace?.SetViewStyle(e);
        InvokeAsync(StateHasChanged);
    }

}
