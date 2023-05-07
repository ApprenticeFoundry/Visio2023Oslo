using BlazorComponentBus;
using FoundryBlazor.Extensions;
using FoundryBlazor.PubSub;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Visio2023Foundry.Model;
using Visio2023Foundry.Services;
using Visio2023Foundry.Simulation;

namespace Visio2023Foundry.Pages;

public partial class Visio2023DrawingPage : ComponentBase, IDisposable
{
    [Inject] public NavigationManager? Navigation { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] private IToast? Toast { get; set; }
    [Inject] public IWorkspace? Workspace { get; init; }
    [Inject] public IRestAPIServiceDTAR? RestAPI { get; init; }




    [Parameter]
    public string? LoadWorkbook { get; set; }

    protected override void OnInitialized()
    {
        Workspace?.SetBaseUrl(Navigation?.BaseUri ?? "");
        if (Navigation != null)
            Navigation.LocationChanged += LocationChanged;
    }

    public void Dispose()
    {
         Workspace?.OnDispose();
        if (Navigation != null)
            Navigation.LocationChanged -= LocationChanged;
    }



    private void LocationChanged(object? sender, LocationChangedEventArgs e)
    {

        // $"LocationChanged {e.Location}".WriteInfo();
        RefreshWorkPieceMenus();
        StateHasChanged();
    }

    private void RefreshWorkPieceMenus()
    {
        if (Workspace != null)
        {
            // $"RefreshWorkPieceMenus".WriteInfo();
            Workspace.ClearAllWorkbook();

            if ("Playground".Matches(LoadWorkbook!))
                Workspace.EstablishWorkbook<Playground>();
            else if ("Stencil".Matches(LoadWorkbook!))
                Workspace.EstablishWorkbook<Stencil>();
            else if ("Boid".Matches(LoadWorkbook!))
                Workspace.EstablishWorkbook<BoidManager>();
            else if ("Composition".Matches(LoadWorkbook!))
                Workspace.EstablishWorkbook<Composition>();
            else if ("Simulation".Matches(LoadWorkbook!))
                Workspace.EstablishWorkbook<MoSimulation>();
            else if ("Process".Matches(LoadWorkbook!))
                Workspace.EstablishWorkbook<Process>();
            else if ("Signalr".Matches(LoadWorkbook!))
                Workspace.EstablishWorkbook<SignalRDemo>();

            Workspace.CreateMenus(Workspace, JsRuntime!, Navigation!);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (Workspace != null)
        {
            RefreshWorkPieceMenus();

            var url = RestAPI?.GetServerUrl() ?? "";
            Workspace.CreateCommands(Workspace, JsRuntime!, Navigation!, url);

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
            //PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
            PubSub!.SubscribeTo<ViewStyle>(OnViewStyleChanged);
            Toast!.Success($"Drawing Page Loaded!");
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
