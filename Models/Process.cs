using System.Drawing;
using FoundryBlazor.Shared;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Units;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

using Visio2023Foundry.Shape;

namespace Visio2023Foundry.Model;


public class Process : FoWorkbook
{


    private Semantic SemanticModel { get; set; }    
    private IDrawing Drawing { get; set; }
    
    public Process(IWorkspace space, IFoundryService foundry): 
        base(space,foundry)
    {
        SemanticModel = new Semantic(space.GetDrawing(),foundry.PubSub());
        Drawing = space.GetDrawing()!;
        // Thread Overview - Canvas Background = #faf8cf
        EstablishCurrentPage(GetType().Name, "#faf8cf").SetPageSize(60, 40, "cm");
    }



    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        "Process CreateMenus".WriteWarning();
        var menu = new Dictionary<string, Action>()
        {
            { "Test Process", () => SetDoCreateProcess(MakeProcess()) },
        };
        
        space.EstablishMenu2D<FoMenu2D, FoButton2D>("Process", menu, true);




    }

    public FoLayoutTree<V> CreatePlanShapeTree<V>(DT_Hero model) where V : FoHero2D
    {
        SemanticModel.AddModel(model);

        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(model, "Red").ResizeTo(250, 80);
        Workspace.GetDrawing()?.AddShape<V>(shape);

        var node = new FoLayoutTree<V>(shape);
        model.Children()?.ForEach(step =>
        {
            var subNode = CreateStepShapeTree<V>(step);
            node.AddChildNode(subNode);
        });

        CreateAssetFileShapeTree(node, model);

        return node;
    }

    private void SetDoCreateProcess(DT_ProcessPlan model)
    {
        SemanticModel.AddModel(model);
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;

        //_DTDB.Add<DT_ProcessPlan>(model);
        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var margin = new Point(20, 50);
            var page = drawing.CurrentPage();
            var pt = new Point(page.PageWidth.AsPixels() / 2, new Length(5.0,"in").AsPixels());

            var layoutTree = CreatePlanShapeTree<FoHero2D>(model);
            layoutTree.Layout(pt.X, pt.Y, margin, TreeLayoutRules.ProcessLayout);
            layoutTree.HorizontalLayoutConnections<FoConnector1D>(drawing.Pages());

            //var shape = CurrentLayout.GetShape();
            //shape.Tag = $"Node: {_layout.ComputeName()}";
        });
    }
    public FoLayoutTree<V> CreateItemShapeTree<V>(DT_Hero model) where V : FoHero2D
    {
        SemanticModel.AddModel(model);

        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(model, "Blue");
        Workspace.GetDrawing().AddShape<V>(shape);

        var node = new FoLayoutTree<V>(shape);
        CreateAssetFileShapeTree(node, model);

        return node;
    }

    public FoLayoutTree<V> CreateStepShapeTree<V>(DT_Hero model) where V : FoHero2D
    {
        SemanticModel.AddModel(model);

        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(model, "Black").ResizeTo(250, 90);
        Workspace.GetDrawing().AddShape<V>(shape);

        var node = new FoLayoutTree<V>(shape);
        model.Children()?.ForEach(item =>
        {
            var subNode = CreateItemShapeTree<V>(item);
            node.AddChildNode(subNode);
        });
        CreateAssetFileShapeTree(node, model);

        return node;
    }

    public FoLayoutTree<V> CreateAssetFileShapeTree<V>(FoLayoutTree<V> node, DT_Hero model) where V : FoHero2D
    {
        var list = model.CollectAssetFiles(new List<DT_AssetFile>(), false);
        var assets = list.Where(item => item != null).ToList();
        assets.ForEach(item => AttachItem(node, item));
        return node;
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
    public DT_ProcessPlan MakeProcess()
    {
        var process = new DT_ProcessPlan();

        var step1 = new DT_ProcessStep();
        process.AddProcessStep(step1);

        var item1_1 = new DT_StepItem();
        var item1_2 = new DT_StepItem();
        var item1_3 = new DT_StepItem();
        var item1_4 = new DT_StepItem();
        var item1_5 = new DT_StepItem();
        step1.AddStepDetail<DT_StepItem>(item1_1);
        step1.AddStepDetail<DT_StepItem>(item1_2);
        step1.AddStepDetail<DT_StepItem>(item1_3);
        step1.AddStepDetail<DT_StepItem>(item1_4);
        step1.AddStepDetail<DT_StepItem>(item1_5);

        var step2 = new DT_ProcessStep();
        process.AddProcessStep(step2);

        var item2_1 = new DT_StepItem();
        var item2_2 = new DT_StepItem();
        var item2_3 = new DT_StepItem();
        var item2_4 = new DT_StepItem();
        SemanticModel.AddAssetFile(item2_4, "File1 item2_4");
        SemanticModel.AddAssetFile(item2_4, "File2 item2_4");

        step2.AddStepDetail<DT_StepItem>(item2_1);
        step2.AddStepDetail<DT_StepItem>(item2_2);
        step2.AddStepDetail<DT_StepItem>(item2_3);
        step2.AddStepDetail<DT_StepItem>(item2_4);


        var step3 = new DT_ProcessStep();
        process.AddProcessStep(step3);
        SemanticModel.AddAssetFile(step3, "File1 step3");
        SemanticModel.AddAssetFile(step3, "File2 step3");

        var item3_1 = new DT_StepItem();
        var item3_2 = new DT_StepItem();
        var item3_3 = new DT_StepItem();
        var item3_4 = new DT_StepItem();
        var item3_5 = new DT_StepItem();
        step3.AddStepDetail<DT_StepItem>(item3_1);
        step3.AddStepDetail<DT_StepItem>(item3_2);
        step3.AddStepDetail<DT_StepItem>(item3_3);
        step3.AddStepDetail<DT_StepItem>(item3_4);
        step3.AddStepDetail<DT_StepItem>(item3_5);

        var step4 = new DT_ProcessStep();
        process.AddProcessStep(step4);

        var item4_1 = new DT_StepItem();
        var item4_2 = new DT_StepItem();

        step4.AddStepDetail<DT_StepItem>(item4_1);
        step4.AddStepDetail<DT_StepItem>(item4_2);


        return process;
    }

   

}
