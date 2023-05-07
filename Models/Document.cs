using BlazorComponentBus;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Visio2023Foundry.Model;
using Visio2023Foundry.Services;
using Visio2023Foundry.Shape;

namespace Visio2023Foundry.Model;


public class Document : FoWorkbook
{
    private IRestAPIServiceDTAR? DTARRestService { get; set; }
    private Semantic SemanticModel { get; set; }

    public Document(IWorkspace space, ICommand command, DialogService dialog, IJSRuntime js, ComponentBus pubSub): 
        base(space,command,dialog,js,pubSub)
    {
        SemanticModel = new Semantic(space.GetDrawing(),pubSub);
    }
  
    

    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        "Document CreateMenus".WriteWarning();
        var menu = new Dictionary<string, Action>()
        {
     //       { "Test Document", () => SetDoCreateDocuments(MakeDocument()) },
        };
        
        space.EstablishMenu2D<FoMenu2D, FoButton2D>("Document", menu, true);


        //watch me extend this menu after a service call
        if ( DTARRestService != null)
            Task.Run(async () =>
            {
                var docs = await DTARRestService.GetRootDocument();
                docs?.ForEach(item =>
                {
                    if (item == null || string.IsNullOrEmpty(item.title)) return;
                   // menu.Add(item.title, () => SetDoCreateDocuments(item));
                });

                space.EstablishMenu2D<FoMenu2D,FoButton2D>("Document", menu, true);
            });

    }

    public void AttachItem<V>(FoLayoutTree<V> node, DT_AssetFile item) where V : FoHero2D
    {
        SemanticModel.AddModel(item);
        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(item, "Pink");
        Workspace.GetDrawing()?.AddShape<V>(shape);

        //$"CreateAssetFile {shape.Tag} {shape.Name}".WriteLine(ConsoleColor.Yellow);
        var child = new FoLayoutTree<V>(shape);
        node.AddChildNode(child);
    }

    public FoLayoutTree<V> CreateAssetFileShapeTree<V>(FoLayoutTree<V> node, DT_Hero model) where V : FoHero2D
    {
        var list = model.CollectAssetFiles(new List<DT_AssetFile>(), false);
        var assets = list.Where(item => item != null).ToList();
        assets.ForEach(item => AttachItem(node, item));
        return node;
    }


    public FoLayoutTree<V> CreateDocumentShapeTree<V>(DT_MILDocument model) where V : FoHero2D
    {
        SemanticModel.AddModel(model);

        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(model, "Orange");
        Workspace.GetDrawing()?.AddShape<V>(shape);

        var node = new FoLayoutTree<V>(shape);
        model.children?.ForEach(child =>
        {
            var subnode = CreateDocumentShapeTree<V>(child);
            node.AddChildNode(subnode);
        });
        CreateAssetFileShapeTree(node, model);

        return node;
    }

    // private void SetDoCreateDocuments(DT_MILDocument model)
    // {
    //     AddModel(model);

    //     _DTDB.Add<DT_MILDocument>(model);
    //     Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
    //     {
    //         CurrentLayout = CreateDocumentShapeTree<FoHero2D>(model);

    //         LayoutTree(CurrentLayout);
    //         //var shape = CurrentLayout.GetShape();
    //         //shape.Tag = $"Node: {_layout.ComputeName()}";
    //     });
    // }

    public DT_MILDocument MakeDocument()
    {
        var doc = new DT_MILDocument() { title = "Root" };

        var step1 = new DT_MILDocument() { title = "Child1" };
        doc.AddChild(step1);
        var step2 = new DT_MILDocument() { title = "Child2" };
        doc.AddChild(step2);

        SemanticModel.AddAssetFile(step1, "File1");
        SemanticModel.AddAssetFile(step2, "File2");
        return doc;
    }


}
