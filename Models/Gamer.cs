using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using IoBTMessage.Extensions;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QRCoder;
using Radzen;
using Visio2023Foundry.Dialogs;
using System.Timers;

namespace Visio2023Foundry.Model;


public class Gamer : FoWorkbook, IDisposable
{
    private System.Timers.Timer StatusTimer { get; set; } = new(1000);


    public Gamer(IWorkspace space, IFoundryService foundry) :
        base(space,foundry)
    {
    }

    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        var arena = space.GetArena();
        space.EstablishMenu3D<FoMenu3D, FoButton3D>("Gamer", new Dictionary<string, Action>()
        {
            { "Clear", async () => await arena.ClearArena() },
                        { "Axis", () => DoLoad3dModel("fiveMeterAxis.glb")},
            { "Porshe 911", () => DoLoad3dModel("porsche_911.glb")},
            { "Part 1", () => DoLoad3dModel("part1.glb")},
            { "Part 2", () => DoLoad3dModel("part2.glb")},
           // { "Part 3", () => DoLoad3dModel("test.glb")},
            { "Jet", () => DoLoad3dModel("jet.glb")},
            { "Barrel", () => DoLoad3dModel("barrel.glb")},
            { "Mustang 1965", () => DoLoad3dModel("mustang_1965.glb")},
            { "Power Tower", () => DoLoad3dModel("power_tower.glb")},
            { "T Rex", () => DoLoad3dModel("T_Rex.glb")},
            { "Stress", () => DoLoad3dStress("jet.glb",1500)},
            { "Add Cube", () => DoAddCube()},
            { "Test World", () => DoTestWorld()}
        }, true);


    }

    public void DoLoad3dStress(string filename, int count)
    {
        var arena = Workspace.GetArena();
        if (arena == null) return;

        // var baseURL = Path.Join(Workspace.GetBaseUrl(), "storage", "StaticFiles");
        var baseURL = $"{Workspace.GetBaseUrl()}storage/StaticFiles";
        baseURL.WriteSuccess();
        arena.StressTest3DModelFromFile("3DModels", filename, baseURL, count);
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

    public void DoLoad3dModelFolder(string folder)
    {
        var arena = Workspace.GetArena();
        if (arena == null) return;

        var baseURL = $"{Workspace.GetBaseUrl()}storage/StaticFiles";

        string path = Directory.GetCurrentDirectory();
        var source  = Path.Combine(path, "storage", "StaticFiles",folder);
        source.WriteSuccess();

        var files = Directory.GetFiles(source);
        foreach (string fileName in files)
        {
            var name = Path.GetFileName(fileName);
            $"Loading {name}".WriteNote();
            arena.Load3DModelFromFile(folder, name, baseURL);
        }
    }

    private void DoAddCube()
    {
        var id = Guid.NewGuid().ToString();
        var shape3D = new FoShape3D(id, "green");
        shape3D.CreateBox(id, .1, .1, .1);

        var arena = Workspace.GetArena();
        var stage = arena.CurrentStage();
        stage.AddShape<FoShape3D>(shape3D);
        arena.UpdateArena();
    }

    private void OnSensorPulse(IArena arena, FoText3D label, FoShape3D shape3D)
    {
        var sec = DateTime.Now.Second;
        label.UpdateText($"Ticker {sec}");

        var x = sec * 0.1;
        shape3D.UpdateMeshPosition(x, 1.0, 1.0);
        label.UpdateMeshPosition(x, 1.0, 1.0);

        //var stage = arena.CurrentStage();
        arena.UpdateArena();
    }

    private void DoTestWorld()
    {
        var arena = Workspace.GetArena();
        var stage = arena.CurrentStage();

        var sec = DateTime.Now.Second;

        var label = new FoText3D("Ticker");
        label.CreateTextAt($"Ticker {sec}", -6.0, 2.0, 1.0);
        stage.AddShape<FoText3D>(label);

        var id = "BlueCube";
        var shape3D = new FoShape3D(id, "blue");
        shape3D.CreateBox(id, .25, .25, .25);

        stage.AddShape<FoShape3D>(shape3D);

        if (!StatusTimer.Enabled)
        {
            StatusTimer.Elapsed += (sender, evt) => OnSensorPulse(arena, label, shape3D);
            StatusTimer.Start();
        }
    }

    public void Dispose()
    {
        StatusTimer.Stop();
        StatusTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}
