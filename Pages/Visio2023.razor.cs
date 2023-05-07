using BlazorComponentBus;
using FoundryBlazor.Extensions;
using FoundryBlazor.PubSub;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Visio2023Foundry.Model;

using Visio2023Foundry.Simulation;

namespace Visio2023Foundry.Pages;

public partial class Visio2023Page : ComponentBase, IDisposable
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
        Workspace?.SetBaseUrl(Navigation?.BaseUri ?? "");
       if ( Navigation != null)
            Navigation.LocationChanged += LocationChanged;
    }

    public void Dispose()
    {
        Workspace?.OnDispose();
        if ( Navigation != null)
            Navigation.LocationChanged -= LocationChanged;
    }
 
    private void LocationChanged(object? sender, LocationChangedEventArgs e)
    {
       // $"LocationChanged {e.Location}".WriteInfo();
        RefreshWorkbookMenus();
        StateHasChanged();
    }

    private void RefreshWorkbookMenus()
    {
         if (Workspace != null) {
           // $"RefreshWorkPieceMenus".WriteInfo();
            Workspace.ClearAllWorkbook();

            if ( "Boid".Matches(LoadWorkbook!) )
                Workspace.EstablishWorkbook<BoidManager>();
                
            Workspace.CreateMenus(Workspace, JsRuntime!, Navigation!);
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
