using System.Drawing;
using FoundryBlazor;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Units;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Visio2023Foundry.Shape;

namespace Visio2023Foundry.Model;

public class Composition : FoWorkbook
{

    public Point MarginH { get; set; } = new(20, 50);
    public Point MarginV { get; set; } = new(50, 20);

    public bool ShowLayout { get; set; } = false;
    public FoLayoutTree<CompShape2D>? LayoutTree { get; set; }


    private IDrawing Drawing { get; set; }

    private Dictionary<string,TreeModel> ModelLookup { get; set; } = new();
    private Dictionary<string,CompShape2D> ShapeLookup { get; set; } = new();

    public Composition(IWorkspace space, IFoundryService foundry) :
        base(space, foundry)
    {
        Drawing = space.GetDrawing()!;

        EstablishCurrentPage(GetType().Name, "Green").SetPageSize(600, 40, "cm");

        PubSub!.SubscribeTo<SelectionChanged>(OnSelectionChanged);
    }

    private void OnSelectionChanged(SelectionChanged e)
    {
        var drawing = Workspace.GetDrawing();
        var page = drawing.CurrentPage();

        if (LayoutTree?.GetShape() is not CompShape2D rootShape) return;

        var x = rootShape.PinX; //.LeftEdge();
        var y = rootShape.PinY; //.TopEdge();

        var rootModel = ModelLookup[rootShape.GetGlyphId()];

        if (e.Selections.FirstOrDefault() is not CompShape2D shape) return;
        var guid = shape.GetGlyphId();

        if (!ModelLookup.ContainsKey(guid)) return;

        var model = ModelLookup[guid];

        if (e.State == SelectionState.Dropped && shape == rootShape)
        {
            LayoutTree?.Layout(x, y, MarginV);
             RepositionAnimation(x, y);
            return;
        }
    
        if (e.State == SelectionState.Reselected  && shape != rootShape)
        {
            var selections = Workspace.GetSelectionService();
            selections.ClearAll();

            model.IsExpanded = !model.IsExpanded;
            var node = LayoutTree?.FindNodeWithGuid(shape.GetGlyphId());

            if ( !model.IsExpanded )
            {
                var shapes = node?.GetChildShapes(true);
                if ( shapes != null)
                {
                    shapes.ForEach(item => item.MarkSelected(true));
                    var list = page.ExtractSelected(new List<FoGlyph2D>());
                    list.ForEach(item => page.DeleteShape(item));
                }
                node?.PurgeChildren();
                LayoutTree?.Layout(x, y, MarginV);
                 RepositionAnimation(x, y);
            } 
            else if ( node != null && model.Children().Count > 0)
            {
                if ( shape.CompShape2DType == CompShape2DType.Composition )
                {
                    CreateShapeChildTree<CompShape2D>(model, node, (shape, tree) => shape.TagAsComposition(tree));
                    LayoutTree?.HorizontalLayout(x, y, MarginV);
                    LayoutTree?.HorizontalLayoutConnections<FoConnector1D>(drawing.Pages());
                }
                else
                {
                    CreateShapeChildTree<CompShape2D>(model, node, (shape, tree) => shape.TagAsClassification(tree));
                    LayoutTree?.VerticalLayout(x, y, MarginV);
                    LayoutTree?.VerticalLayoutConnections<FoConnector1D>(drawing.Pages());
                }
                RepositionAnimation(x, y);
            }
        }        
    }

    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        "Composition CreateMenus".WriteWarning();
        var menu = new Dictionary<string, Action>()
        {
            { "Clear", () => DoClear() },
            { "Show/Hide Layout", () => DoShowLayout() },
            { "Workspace", () => DoCreateHorizontalTree(WorkspaceModel()) },
            { "Workbook", () => DoCreateHorizontalTree(WorkbookModel()) },
            { "Drawing", () => DoCreateHorizontalTree(DrawingModel()) },
            { "FoGlyph2D Class", () => DoCreateVerticalTree(ClassModel(typeof(FoGlyph2D))) },
            { "FoGlyph3D Class", () => DoCreateVerticalTree(ClassModel(typeof(FoGlyph3D))) },
            { "FoComponent Class", () => DoCreateVerticalTree(ClassModel(typeof(FoComponent))) },
        };

        space.EstablishMenu2D<FoMenu2D, FoButton2D>("Composition", menu, true);

    }


    public void RepositionAnimation(int x, int y)
    {
        var shapes = ShapeLookup.Values.ToList();
        shapes.ForEach(shape => shape.AnimatedMoveFrom(x,y));
    }
  
    public FoLayoutTree<V> CreateShapeParentTree<V>(TreeModel model, Action<V,TreeModel> TagAction) where V : CompShape2D
    {

        var shape = Activator.CreateInstance<V>();
        ModelLookup.Add(shape.GetGlyphId(), model);
        ShapeLookup.Add(shape.GetGlyphId(), shape);

        TagAction?.Invoke(shape, model);
        Workspace.GetDrawing()?.AddShape<V>(shape);

        var node = new FoLayoutTree<V>(shape);
        return CreateShapeChildTree<V>(model, node, TagAction!);
    }


    public FoLayoutTree<V> CreateShapeChildTree<V>(TreeModel model,  FoLayoutTree<V> node, Action<V,TreeModel> TagAction ) where V : CompShape2D
    {
        if ( model.IsExpanded ) 
        {
            model.Children()?.ForEach(step =>
            {
                var subNode = CreateShapeParentTree<V>(step, TagAction!);
                node.AddChildNode(subNode);
            });
        }

        return node;
    }

    // public override async Task RenderWatermark(Canvas2DContext ctx, int tick)
    // {
    //     await LayoutTree?.RenderLayoutTree(ctx);
    // }

    private void DoClear()
    {
        ShapeLookup.Clear();
        LayoutTree?.ClearAll();

        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;
        drawing.ClearAll();
    }

    private void DoShowLayout() 
    {
        ShowLayout = !ShowLayout;
        Drawlayout();
    }

    private static TreeModel WorkspaceModel()
    {
        var model = new TreeModel()
        {
            ComponentName = "Workspace",
            ClassName = "FoWorkspace"
        };

        model.AddChild("Drawing", "FoDrawing")
            .AddChild("Page", "FoPage")
            .Add("Shape1", "FoShape2D")
            .Add("Shape2", "FoShape2D")
            .Add("Shape3", "FoShape2D");

        return model;
    }



    private static TreeModel WorkbookModel()
    {
        var model = new TreeModel()
        {
            ComponentName = "Workspace",
            ClassName = "FoWorkspace"
        };

        model.AddChild("Drawing", "FoDrawing")
            .AddChild("Page", "FoPage")
            .Add("Shape1", "FoShape2D");

        model.AddChild("Playground", "Playground");
        model.AddChild("Boid", "BoidManager");
        model.AddChild("Signalr", "SignalRDemo");
        model.AddChild("Composition", "Composition");
        return model;
    }
    private static TreeModel DrawingModel()
    {
        var model = new TreeModel()
        {
            ComponentName = "Drawing",
            ClassName = "FoDrawing"
        };

        var page1 = model.AddChild("Page1", "FoPage");
        var page2 = model.AddChild("Page2", "FoPage");
        var page3 = model.AddChild("Page3", "FoPage");

        var shape3 = page3
            .Add("Shape1", "FoShape2D")
            .Add("Shape2", "FoShape1D")
            .AddChild("Shape3", "FoShape2D");

        shape3.Add("Shape4", "FoShape2D")
             .Add("Shape5", "FoShape1D")
             .Add("Shape6", "FoShape2D");

        //shape3.IsExpanded = true;


        return model;
    }

    private static List<Type> SubClassModel(Type parent)
    {
        var types = parent.Assembly.GetTypes()
            .Where(t => t.IsClass && t.BaseType == parent)
            //.Where(t => t.IsClass && t.IsSubclassOf(parent))
            .OrderBy(t => t.Name)
            .ToList();
        return types;
    }

    private static TreeModel ClassModel(Type root)
    {

        var model = new TreeModel()
        {
            ClassName = root.Name
        };

        var children = SubClassModel(root);
        children?.ForEach(child =>
        {
            var subNode = ClassModel(child);
            model.AddChild(subNode);
        });

        return model;
    }

    private void Drawlayout()
    {
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;
         drawing.SetPostRenderAction(async (ctx,tick) => await Task.CompletedTask);

        if ( LayoutTree == null  || !ShowLayout ) return;
        drawing.SetPostRenderAction(LayoutTree.RenderLayoutTree);        
    }

    private void DoCreateHorizontalTree(TreeModel model)
    {
        DoClear();
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;

        var page = drawing.CurrentPage();
        var pt = new Point(page.PageWidth.AsPixels() / 6, new Length(5.0,"in").AsPixels());

        LayoutTree = CreateShapeParentTree<CompShape2D>(model, (shape, tree) =>shape.TagAsComposition(tree));
        
        LayoutTree.HorizontalLayout(pt.X, pt.Y, MarginH);
        LayoutTree.HorizontalLayoutConnections<FoConnector1D>(drawing.Pages());

        Drawlayout();
    }

    private void DoCreateVerticalTree(TreeModel model)
    {
        DoClear();
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;

        var page = drawing.CurrentPage();

        var pt = new Point(page.PageWidth.AsPixels() / 6, new Length(5.0, "in").AsPixels());

        LayoutTree = CreateShapeParentTree<CompShape2D>(model, (shape, tree) => shape.TagAsClassification(tree));

        LayoutTree.VerticalLayout(pt.X, pt.Y, MarginV);
        LayoutTree.VerticalLayoutConnections<FoConnector1D>(drawing.Pages());

        Drawlayout();
    }




}
