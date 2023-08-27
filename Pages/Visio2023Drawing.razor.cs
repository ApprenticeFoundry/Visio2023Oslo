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

public partial class Visio2023DrawingPage : ComponentBase, IDisposable
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
        Workspace!.SetBaseUrl(Navigation?.BaseUri ?? "");
        if (Navigation != null)
            Navigation.LocationChanged += LocationChanged;


        Workspace.EstablishWorkbook<Playground>().Name = "playground";
        Workspace.EstablishWorkbook<Stencil>().Name = "stencil";
        Workspace.EstablishWorkbook<TargetManager>().Name = "network";
        Workspace.EstablishWorkbook<BoidManager>().Name = "boid"; 
        Workspace.EstablishWorkbook<Composition>().Name = "composition"; 
        Workspace.EstablishWorkbook<MoSimulation>().Name = "simulation"; 
        Workspace.EstablishWorkbook<Process>().Name = "process"; 
        Workspace.EstablishWorkbook<SignalRDemo>().Name = "Signalr";

        $"Visio2023DrawingPage OnInitialized LoadWorkbook ={LoadWorkbook}".WriteNote();

        base.OnInitialized();
    }

    public void Dispose()
    {
         Workspace?.OnDispose();
        if (Navigation != null)
            Navigation.LocationChanged -= LocationChanged;
    }



    private void LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if ( e.Location.Contains("visio2023drawing") )
        {
            $"Visio2023DrawingPage LocationChanged {e.Location}".WriteInfo();
            RefreshWorkbookMenus();
        }
        StateHasChanged();
    }

    private void RefreshWorkbookMenus()
    {
        if (Workspace != null)
        {
               $"Visio2023DrawingPage RefreshWorkbookMenus LoadWorkbook ={LoadWorkbook}".WriteNote();

            var found = Workspace.FindWorkbook(LoadWorkbook!);
            if (found != null)
            {
                Workspace?.SetCurrentWorkbook(found!).CurrentPage();
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

            var defaultHubURI = Navigation!.ToAbsoluteUri("/DrawingSyncHub").ToString();
            await Workspace.InitializedAsync(defaultHubURI!);

       $"Visio2023DrawingPage OnInitializedAsync LoadWorkbook ={LoadWorkbook}".WriteNote();

            Toast?.Info(LoadWorkbook ?? "No LoadWorkbook");
        }

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
            PubSub!.SubscribeTo<ViewStyle>(OnViewStyleChanged);
            //Toast!.Success($"Drawing Page Loaded!");
            Workspace!.GetDrawing();
            Workspace!.GetArena();

      $"Visio2023DrawingPage OnAfterRenderAsync LoadWorkbook ={LoadWorkbook}".WriteNote();

        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        InvokeAsync(StateHasChanged);
        $"Visio2023DrawingPage OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }

    private void OnViewStyleChanged(ViewStyle e)
    {
        Workspace?.SetViewStyle(e);
        InvokeAsync(StateHasChanged);
    }
}
