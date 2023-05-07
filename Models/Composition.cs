using System.Drawing;
using BlazorComponentBus;
using FoundryBlazor;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Visio2023Foundry.Shape;

namespace Visio2023Foundry.Model;

public class Composition : FoWorkbook
{
    public Point MarginH { get; set; } = new(20, 50);
    public Point MarginV { get; set; } = new(50, 20);

    public FoLayoutTree<CompShape2D>? LayoutTree { get; set; }

    private Dictionary<string,TreeModel> ModelLookup { get; set; } = new();
    private Dictionary<string,CompShape2D> ShapeLookup { get; set; } = new();

    public Composition(IWorkspace space, ICommand command, DialogService dialog, IJSRuntime js, ComponentBus pubSub) :
        base(space, command, dialog, js, pubSub)
    {
        PubSub!.SubscribeTo<SelectionChanged>(OnSelectionChanged);
    }

    private void OnSelectionChanged(SelectionChanged e)
    {
        var drawing = Workspace.GetDrawing();
        var page = drawing.CurrentPage();

        if (LayoutTree?.GetShape() is not CompShape2D rootShape) return;

        var x = rootShape.LeftEdge();
        var y = rootShape.TopEdge();

        var rootModel = ModelLookup[rootShape.GetGlyphId()];

        if (e.Selections.FirstOrDefault() is not CompShape2D shape) return;
        var guid = shape.GetGlyphId();

        if (!ModelLookup.ContainsKey(guid)) return;

        var model = ModelLookup[guid];

        if (e.State == SelectionState.Dropped && shape == rootShape)
        {
            LayoutTree?.Layout(x, y, MarginV);
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
                
            }
        }        
    }

    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        "Composition CreateMenus".WriteWarning();
        var menu = new Dictionary<string, Action>()
        {
            { "Clear", () => DoClear() },
            { "Workspace", () => DoCreateHorizontalTree(WorkspaceModel()) },
            { "Workbook", () => DoCreateHorizontalTree(WorkbookModel()) },
            { "Drawing", () => DoCreateHorizontalTree(DrawingModel()) },
            { "FoGlyph2D Class", () => DoCreateVerticalTree(ClassModel(typeof(FoGlyph2D))) },
            { "FoGlyph3D Class", () => DoCreateVerticalTree(ClassModel(typeof(FoGlyph3D))) },
            { "FoComponent Class", () => DoCreateVerticalTree(ClassModel(typeof(FoComponent))) },
        };

        space.EstablishMenu2D<FoMenu2D, FoButton2D>("Composition", menu, true);

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

    private void DoClear()
    {
        ShapeLookup.Clear();
        LayoutTree?.ClearAll();

        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;
        drawing.ClearAll();
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

    private void DoCreateHorizontalTree(TreeModel model)
    {
        DoClear();
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;

        var page = drawing.CurrentPage();
        var pt = drawing.InchesToPixelsInset(page.PageWidth / 6, 5.0);

        LayoutTree = CreateShapeParentTree<CompShape2D>(model, (shape, tree) =>shape.TagAsComposition(tree));
        
        LayoutTree.HorizontalLayout(pt.X, pt.Y, MarginH);
        LayoutTree.HorizontalLayoutConnections<FoConnector1D>(drawing.Pages());
    }

    private void DoCreateVerticalTree(TreeModel model)
    {
        DoClear();
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;

        var page = drawing.CurrentPage();
        var pt = drawing.InchesToPixelsInset(page.PageWidth / 6, 5.0);

        LayoutTree = CreateShapeParentTree<CompShape2D>(model, (shape, tree) => shape.TagAsClassification(tree));

        LayoutTree.VerticalLayout(pt.X, pt.Y, MarginV);
        LayoutTree.VerticalLayoutConnections<FoConnector1D>(drawing.Pages());

    }




}
