using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Radzen;
using Visio2023Foundry.Boids;
namespace Visio2023Foundry.Model;


public class BoidManager : FoWorkbook
{

    private readonly BoidField Simulation;


    public BoidManager(IWorkspace space, ICommand command, DialogService dialog, IJSRuntime js, ComponentBus pubSub) :
        base(space, command, dialog, js, pubSub)
    {
        Simulation = new BoidField(space, command);

        StorageHelpers.RegisterLookupType<Boid>();
    }

    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        var OpenNew = async () =>
        {

            try
            {
                await js.InvokeAsync<object>("open", nav.Uri); //, "_blank", "height=600,width=1200");
            }
            catch { }
        };


        var arena = space.GetArena();
        space.EstablishMenu2D<FoMenu2D, FoButton2D>("Boids", new Dictionary<string, Action>()
        {
            { "Toggle Field Shape", () => Simulation.ToggleFieldShape()},
            { "Start/Stop", () => Simulation.ToggleBoids()},
            { "Boids +5", () => Simulation.BoidsAdd5()},
            { "Boids -5", () => Simulation.BoidsSub5()},
            { "Boids +25", () => Simulation.BoidsAdd25()},
            { "Boids +100", () => Simulation.BoidsAdd100()},
            { "Clear 3D", async () => await arena.ClearArena() },
            { "Jet", () => DoLoad3dModel("jet.glb")},
             { "T Rex", () => DoLoad3dModel("T_Rex.glb")},
            { "Open New Window", () => OpenNew()},
            { "Start Hub", () => StartHub()},
            { "Stop Hub", () => StopHub()},
       }, true);

    }

    public void StartHub()
    {
        Command.StartHub();
    }
    public void StopHub()
    {
        Command.StopHub();
    }

    public void DoLoad3dModel(string filename)
    {
        var arena = Workspace.GetArena();
        if (arena == null) return;


        // var baseURL = Path.Join(Workspace.GetBaseUrl(), "storage", "StaticFiles");
        var baseURL = $"{Workspace.GetBaseUrl()}storage/StaticFiles";
        baseURL.WriteSuccess();
        arena.Load3DModelFromFile("3DModels", filename, baseURL);
    }

    public override bool SetSignalRHub(HubConnection hub, string panid)
    {
        hub.On<D2D_ModelCreate>("ModelCreate", (obj) => Simulation.BoidModelCreate(obj));

        hub.On<D2D_ModelUpdate>("ModelUpdate", (obj) => Simulation.BoidModelUpdate(obj));

        hub.On<D2D_ModelDestroy>("ModelDestroy", (obj) => Simulation.BoidModelDestroy(obj));

        hub.On<D2D_ModelUpdate>("ModelReflect", (obj) => Simulation.BoidModelReflect(obj));

        return true;
    }

    public List<Boid> Advance(bool bounceOffWalls = true, bool wrapAroundEdges = false)
    {
        var list = Simulation.Advance(bounceOffWalls, wrapAroundEdges);
        return list;
    }

    public override void PreRender(int tick)
    {
        if (Simulation.IsRunning)
        {
            Advance();
            //Simulation.PreRender(tick);
        }
    }
    public override async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        //watermark is used to render the boids  and move 3D boids
        if (Simulation.IsRunning)
            await Simulation.RenderWatermark(ctx, tick);
    }

}
