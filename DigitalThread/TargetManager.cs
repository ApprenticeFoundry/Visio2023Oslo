using System.Drawing;
using System.Collections.Generic;
using BlazorComponentBus;

using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using IoBTMessage.Models;
using IoBTMessage.Units;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System;
using Blazor.Extensions.Canvas.Canvas2D;

namespace Visio2023Foundry.Targets;


public class TargetManager : FoWorkbook
{
    private FoMenu2D? TargetMenu { get; set; }
    private DT_System? SystemNetwork { get; set; }
    private IDrawing Drawing { get; set; }
    private FoLayoutNetwork<ThreadShape1D, ThreadShape2D> NetworkLayout { get; set; } = new();

    //private Dictionary<string,DT_Target> ModelLookup { get; set; } = new();

    public TargetManager(IWorkspace space, IFoundryService foundry) :
        base(space, foundry)
    {
        Drawing = space.GetDrawing()!;
    }


    private void CreateMenu2D()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null || TargetMenu == null) return;

        TargetMenu.ToggleLayout();
        drawing.AddShape<FoMenu2D>(TargetMenu).AnimatedMoveTo(900, 100);
    }

    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        "TargetManager".WriteWarning();
        var menu = new Dictionary<string, Action>()
        {
            { "Clear", () => DoClear() },
            { "Create", () => DoCreate() },
            { "Step 1", () => DoStep1() },
            { "Step 5", () => DoStep5() },
            { "Attract", () => DoAttract() },
            { "Repel", () => DoRepel() },
            { "Center", () => DoCenter() },
            { "Boundry", () => DoBoundry() },
        };

        TargetMenu = space.EstablishMenu2D<FoMenu2D, FoButton2D>("Targets", menu, true);
        CreateMenu2D();
    }

    private void DoClear()
    {
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;
        drawing.ClearAll();
        CreateMenu2D();
    }

    private void DoCreate()
    {
        DoTesting();
    }

    private void DoStep1()
    {
        NetworkLayout.DoIteration(1);
    }

    private void DoStep5()
    {
        NetworkLayout.DoIteration(5);
    }

    private void DoAttract()
    {
        NetworkLayout.ToggleAttractRule();
    }

    private void DoRepel()
    {
        NetworkLayout.ToggleRepelRule();
    }

    private void DoCenter()
    {
        NetworkLayout.ToggleCenterRule();
    }

    private void DoBoundry()
    {
        NetworkLayout.ToggleBoundryRule();
    }

    public override void PreRender(int tick)
    {
        //NetworkLayout.DoLayoutStep(tick);
    }
    public override async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        await NetworkLayout.RenderLayoutNetwork(ctx, tick);
    }

    public FoLayoutNode<V> CreateNodeShape<V>(DT_Target model, Action<V, DT_Target> TagAction) where V : ThreadShape2D
    {

        var shape = Activator.CreateInstance<V>();
        shape.GlyphId = model.guid;
        //ModelLookup.Add(shape.GetGlyphId(), model);


        TagAction?.Invoke(shape, model);
        Drawing?.AddShape<V>(shape);

        Random rnd = new();

        var node = new FoLayoutNode<V>(shape, 1000 * rnd.NextDouble(), 1000 * rnd.NextDouble());
        return node;
    }

    public FoLayoutLink<U, V> CreateLinkShape<U, V>(DT_TargetLink model, Action<U, DT_TargetLink> TagAction) where U : ThreadShape1D where V : ThreadShape2D
    {

        var shape = Activator.CreateInstance<U>();
        shape.GlyphId = model.guid;

        TagAction?.Invoke(shape, model);
        Drawing?.AddShape<U>(shape);
        var link = new FoLayoutLink<U, V>(shape);
        return link;
    }


    public DT_System CreateSystemNetwork()
    {
        var system = new DT_System();

        var t0 = system.CreateTarget("PIN", "10");
        var t1 = system.CreateTarget("ASST", "20");
        //var t2 = system.CreateTarget("PROC", "30");

        system.CreateLink(t0, t1);
        //system.CreateLink(t1, t2);
        return system;
    }


    private void DoTesting()
    {
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;

        var margin = new Point(20, 50);
        var page = drawing.CurrentPage();
        var pt = new Point(page.PageWidth.AsPixels() / 4, new Length(3.0, "cm").AsPixels());


        SystemNetwork = CreateSystemNetwork();


        int count = 0;
        foreach (var item in SystemNetwork.Targets())
        {
            var node = CreateNodeShape<ThreadShape2D>(item, (shape, model) => shape.TagAsComposition(model));
            node.GetShape().MoveTo(pt.X, pt.Y).MoveBy(count, count);
            NetworkLayout.AddNode(node);
            count += 30;
        }

        foreach (var item in SystemNetwork.Links())
        {
            var link = CreateLinkShape<ThreadShape1D, ThreadShape2D>(item, (shape, model) => shape.TagAsComposition(model));
            NetworkLayout.AddLink(link);
            var source = NetworkLayout.FindTarget(item.sourceGuid);
            var sink = NetworkLayout.FindTarget(item.sinkGuid);
            if (source != null && sink != null)
                link.Connect(source, sink);
        }




    }




}
