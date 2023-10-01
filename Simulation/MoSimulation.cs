using BlazorComponentBus;
using FoundryBlazor.Shared;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace Visio2023Foundry.Simulation;


public class MoSimulation : FoWorkbook
{
    private IDrawing Drawing { get; set; }
    public MoSimulation(IWorkspace space, IFoundryService foundry) :
        base(space,foundry)
    {
        Drawing = space.GetDrawing()!;
        // Thread Overview - Canvas Background = #faf8cf
        EstablishCurrentPage(GetType().Name, "#faf8cf").SetPageSize(60, 40, "cm");
    }

    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        space.EstablishMenu2D<FoMenu2D, FoButton2D>("Simulation", new Dictionary<string, Action>()
        {
            { "Clock", () => SetDoCreateClock()},
            { "Biometric", () => SetDoCreateBiometric()},
            { "Position", () => SetDoCreatePosition()},
            { "Chat", () => SetDoCreateChat()},
            { "System", () => SetDoCreateSystem()},
            { "Image", () => SetDoCreateImage()},
        }, true);

    }


    public MoComponent? FindModel(string ModelId)
    {
        var model = Slot<MoComponent>().FindWhere(child => child.ModelId == ModelId).FirstOrDefault();
        return model;
    }

    public void SetDoCreateTVA(IJSRuntime js, string name, string url)
    {
        var options = new DialogOptions() { Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true };

        // async void OpenTVAUrl(string target)
        // {
        //     try { await js.InvokeAsync<object>("open", target); }
        //     catch { };
        // }

    //     Drawing.SetDoCreate((CanvasMouseArgs args) =>
    //     {             
    //         var model = new MoTVANode(name, url, "I AM CANVAS", OpenTVAUrl);
    //         Add<MoComponent>(model);

    //         model.CreateTVALink();
    //         var shape = new FoCompound2D(260, 70, "Green")
    //         {
    //             GlyphId = model.ModelId
    //         };
    //         shape.MoveTo(args.OffsetX, args.OffsetY);
    //         model.ApplyExternalMethods(shape);
            
    //         PageManager.Add<FoCompound2D>(shape);

    //         var parmas = new Dictionary<string, object>() {
    //             { "Shape", shape },
    //             { "Model", model },
    //             { "OnOk", () => {

    //             } },
    //             { "OnCancel", () => {
 
    //             } }
    //         };

    //         // Task.Run(async () =>
    //         // {
    //         //     await DialogService!.OpenAsync<TVAConfigure>("Create TVA", parmas, options);
    //         // });

    //     });
     }


    private void SetDoCreateClock()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoClock("Clock");
            Add<MoComponent>(model);

            var shape = new FoCompound2D(120, 120, "Cyan")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);
            drawing.AddShape<FoCompound2D>(shape);
        });
    }

    private void SetDoCreateBiometric()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoBiometricDataNode();
            Add<MoComponent>(model);

            var shape = new FoCompound2D(130, 70, "Red")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);
            drawing.AddShape<FoCompound2D>(shape);
        });
    }



    private void SetDoCreatePosition()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoPositionDataNode();
            Add<MoComponent>(model);

            var shape = new FoCompound2D(130, 70, "Green")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);
            drawing.AddShape<FoCompound2D>(shape);
        });
    }

    private void SetDoCreateChat()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoChatMessageDataNode();
            Add<MoComponent>(model);

            var shape = new FoCompound2D(150, 70, "Blue")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);
            drawing.AddShape<FoCompound2D>(shape);
        });
    }

    private void SetDoCreateSystem()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoSystemDataNode();
            Add<MoComponent>(model);

            var shape = new FoCompound2D(130, 70, "Cyan")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);    
            drawing.AddShape<FoCompound2D>(shape);
        });
    }

    private void SetDoCreateImage()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoImageDataNode();
            Add<MoComponent>(model);

            var shape = new FoCompound2D(130, 70, "Cyan")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);    
            drawing.AddShape<FoCompound2D>(shape);
        });
    }

    public MoOutward? ModelConnect(string StartID, string FinishID)
    {
        var Start = FindModel(StartID);
        if ( Start != null)
            $"Start {Start.Name} {Start.GetType().Name}".WriteLine(ConsoleColor.DarkCyan);

        var Finish = FindModel(FinishID);
        if ( Finish != null)
            $"Finish {Finish.Name} {Finish.GetType().Name}".WriteLine(ConsoleColor.DarkCyan);  

        if ( Start != null && Finish != null) {
            var result = Start.Add<MoOutward>(new MoOutward(FinishID, Start, Finish));
            Finish.Add<MoInward>(new MoInward(StartID, Finish, Start));
            Start.Start();
            return result;
        }
        return null;
    }

    public void ModelDisconnect(string StartID, string FinishID)
    {
        var Start = FindModel(StartID);
        if ( Start != null)
            $"Start {Start.Name} {Start.GetType().Name}".WriteLine(ConsoleColor.DarkCyan);

        var Finish = FindModel(FinishID);
        if ( Finish != null)
            $"Finish {Finish.Name} {Finish.GetType().Name}".WriteLine(ConsoleColor.DarkCyan);  

        if ( Start != null && Finish != null) {
            Start.Stop();
            Start.Remove<MoOutward>(FinishID);
            Finish.Remove<MoInward>(StartID);
        }
    }
}
