using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryRulesAndUnits.Extensions;

namespace Visio2023Foundry.Targets;


public class TargetManager : FoWorkbook
{
    private FoMenu2D? TargetMenu { get; set; }
    private DT_System? SystemNetwork { get; set; }
    private IDrawing Drawing { get; set; }
    private FoLayoutNetwork<ThreadShape1D, ThreadShape2D> NetworkLayout { get; set; } = new();


    public TargetManager(IWorkspace space, IFoundryService foundry) :
        base(space, foundry)
    {
        Drawing = space.GetDrawing()!;
        NetworkLayout.Boundary = new(50, 50, 1700, 1200);

        Drawing = space.GetDrawing()!;
        EstablishCurrentPage(GetType().Name, "blue").SetPageSize(60, 40, "cm");
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
            { "Menu", () => CreateMenu2D() },
            { "C Item", () => DoCreateItem() },
            { "C Network", () => DoCreateNetwork() },
            { "Step 1", () => DoStep1() },
            { "Go", () => DoStep5000() },
            { "Random", () => DoRandom() },
            { "Attract", () => DoAttract() },
            { "Repell", () => DoRepell() },
            { "Center", () => DoCenter() },
            { "Boundry", () => DoBoundry() },
            { "Save", () => DoSave() },
               { "Restore", () => DoRestore() },
        };

        TargetMenu = space.EstablishMenu2D<FoMenu2D, FoButton2D>("Targets", menu, true);
        CreateMenu2D();
    }

    private void DoRestore()
    {
        throw new NotImplementedException();
    }

    private void DoSave()
    {
        var text = "Hello, world!";
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);

        Workspace.LocalFileSave("HelloWorld.txt", bytes);
    }

    private void DoClear()
    {
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;
        drawing.ClearAll();
        //CreateMenu2D();
    }

    private void DoCreateItem()
    {
        DoClear();
        SystemNetwork = CreateSystemItem();
        var X = NetworkLayout.XCenter();
        var Y = NetworkLayout.YCenter();
        foreach (var item in GenerateShapes())
        {
            item.MoveTo(X, Y);
        }
        DoRandom();

    }

    private void DoCreateNetwork()
    {
        DoClear();
        SystemNetwork = CreateSystemNetwork();
        var X = NetworkLayout.XCenter();
        var Y = NetworkLayout.YCenter();
        foreach (var item in GenerateShapes())
        {
            item.MoveTo(X, Y);
        }
        DoRandom();
    }

    private void DoStep1()
    {
        NetworkLayout.DoIteration(1);
    }

    private void DoStep5000()
    {
        NetworkLayout.DoIteration(5000);
    }

    private void DoRandom()
    {
        NetworkLayout.DoRandomRule();
    }
    private void DoAttract()
    {
        NetworkLayout.DoAttractRule();
    }

    private void DoRepell()
    {
        NetworkLayout.DoRepellRule();
    }

    private void DoCenter()
    {
        NetworkLayout.DoCenterRule();
    }

    private void DoBoundry()
    {
        NetworkLayout.DoBoundryRule();
    }

    public override void PreRender(int tick)
    {
        NetworkLayout.DoLayoutStep(tick);
    }
    public override async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        await NetworkLayout.RenderLayoutNetwork(ctx, tick);
    }

    public FoLayoutNode<V> CreateNodeShape<V>(DT_Target model, Action<V, DT_Target> TagAction) where V : ThreadShape2D
    {

        var shape = Activator.CreateInstance<V>();
        shape.GlyphId = model.guid;


        TagAction?.Invoke(shape, model);
        Drawing.AddShape<V>(shape);

        var node = new FoLayoutNode<V>(shape, 0, 0);

        shape.AfterMatrixRefresh((obj) =>
        {
            if ( node.X != obj.PinX || node.Y != obj.PinY)
            {
                $"Moved by user {obj.Name} {node.X} {node.Y}".WriteInfo();
            }
            node.X = obj.PinX;
            node.Y = obj.PinY;
        });

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

    public DT_System CreateSystemItem()
    {
        var system = new DT_System();

        var t0 = system.CreateTarget("PIN", "10");

        return system;
    }

    public DT_System CreateSystemNetwork()
    {
        var system = new DT_System();

        var t0 = system.CreateTarget("PIN", "10");
        var t1 = system.CreateTarget("ASST", "20");
        var t2 = system.CreateTarget("PROC", "30");
        var t3 = system.CreateTarget("CAD", "30");
        var t4 = system.CreateTarget("CAD", "31");
        var t5 = system.CreateTarget("CAD", "32");
        system.CreateLink(t0, t1);
        system.CreateLink(t1, t2);
        system.CreateLink(t1, t3);
        system.CreateLink(t3, t4);
           system.CreateLink(t4, t5);
        return system;
    }


    private List<FoLayoutNode<ThreadShape2D>> GenerateShapes()
    {
        var list = NetworkLayout.GetNodes();

        var drawing = Workspace.GetDrawing();
        if (drawing == null || SystemNetwork == null)
            return list;


        foreach (var item in SystemNetwork.Targets())
        {
            var node = CreateNodeShape<ThreadShape2D>(item, (shape, model) => shape.TagAsComposition(model));
            NetworkLayout.AddNode(node);
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
        return list;
    }




}
