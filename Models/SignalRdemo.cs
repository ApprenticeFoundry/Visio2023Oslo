
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using Visio2023Foundry.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using BlazorComponentBus;
using FoundryBlazor.Message;
using IoBTMessage.Models;

namespace Visio2023Foundry.Model;


public class SignalRDemo : FoWorkbook
{

    public SignalRDemo(IWorkspace space, ICommand command, DialogService dialog, IJSRuntime js, ComponentBus pubSub): 
        base(space,command,dialog,js,pubSub)
    {
    }
    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        var OpenNew = async () =>
        {
            var target = nav!.ToAbsoluteUri("/");
            try
            {
                await js.InvokeAsync<object>("open", target); //, "_blank", "height=600,width=1200");
            }
            catch { }
        };

        space.EstablishMenu2D<FoMenu2D, FoButton2D>("SignalR", new Dictionary<string, Action>()
        {
            { "Open New", () => OpenNew()},
            { "Guid Test", () => CreateGuidTest()},
            { "Group", () => CreateGroupShape()},
            { "Ring", () => CreateGroupPlayground()},
            { "Glue", () => CreateGluePlayground()},
            { "Line", () => CreateLinePlayground()},
            { "Blue Shape", () => SetDoCreateBlue()},
            { "Text Shape", () => SetDoCreateText()},
            { "Image Shape", () => SetDoCreateImage()},
            { "Image URL", () => SetDoAddImage()},
        }, true);
    }

    private void CreateGuidTest()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var s1 = new FoShape2D(100, 100, "Orange");
        s1.MoveTo(100, 100);
        s1.GlyphId.WriteNote();

        var TargetId1 = s1.GetGlyphId();
        var Payload = StorageHelpers.Dehydrate(s1, false);

        TargetId1.WriteInfo();

        var s2 = StorageHelpers.Hydrate<FoShape2D>(Payload, false);
        s2.MoveTo(300, 100);

        var TargetId2 = s2.GetGlyphId();
        TargetId2.WriteInfo();

        drawing.AddShape(s1);
        drawing.AddShape(s2);       
    }



    private void CreateGroupShape()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var c1 = drawing.AddShape<FoGroup2D>(new FoGroup2D(100, 100, "Cyan"));
        c1?.MoveTo(1200, 90);
        c1?.Add<FoShape2D>(new FoShape2D(20, 30, "Red"));
        c1?.Add<FoShape2D>(new FoShape2D(20, 30, "Green")).MoveTo(30, 20);
        c1?.CaptureShape<FoShape2D>(new FoShape2D(20, 30, "Blue")).MoveTo(100, 90);

        Command.SendShapeCreate(c1);
    }


    private void CreateGluePlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var s1 = drawing.AddShape(new FoShape2D(200, 100, "Green"));
        s1?.MoveTo(200, 200);

        var s2 = drawing.AddShape(new FoShape2D(200, 100, "Orange"));
        s2?.MoveTo(800, 400);

        var s3 = drawing.AddShape(new FoShape2D(200, 100, "Blue"));
        s3?.MoveTo(800, 200);

        var wire2 = new FoShape1D(s1, s2, 10, "Red");
        drawing.AddShape(wire2);

        var wire1 = new FoShape1D(s1, s3, 10, "Yellow");
        drawing.AddShape(wire1);

        Command.SendShapeCreate(s2);
        Command.SendShapeCreate(s1);
        Command.SendShapeCreate(s3);
        Command.SendShapeCreate(wire1);
        Command.SendShapeCreate(wire2);

        wire1.GetMembers<FoGlue2D>()?.ForEach(glue => Command.SendGlue(glue));
        wire2.GetMembers<FoGlue2D>()?.ForEach(glue => Command.SendGlue(glue));
    }

    private void CreateGroupPlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var radius = 100;
        int cnt = 0;
        for (int i = 0; i <= 360; i += 30)
        {
            var a = Math.PI / 180.0 * i;
            var x = (int)(radius * Math.Cos(a)) + 1200;
            var y = (int)(radius * Math.Sin(a)) + 300;
            var shape = new FoShape2D(30, 30, "Cyan");
            shape.MoveTo(x, y);
            shape.ShapeDraw = (cnt++ % 3 == 0) ? shape.DrawCircle : shape.DrawRect;
            drawing.AddShape<FoShape2D>(shape);
            Command.SendShapeCreate(shape);
        }
    }



    public void SetDoCreateBlue()
    {
        var drawing = Workspace.GetDrawing();

        var shape = new FoShape2D(150, 100, "Blue");

        drawing.AddShape<FoShape2D>(shape);
        Command.SendShapeCreate(shape);
        Command.SendToast(ToastType.Success,"Created");

    }


    public void SetDoCreateText()
    {
        var drawing = Workspace.GetDrawing();

        var shape = new FoText2D(200, 100, "Red");
        drawing.AddShape<FoText2D>(shape);
        Command.SendShapeCreate(shape);
        Command.SendToast(ToastType.Success,"Created");
    }

    private void SetDoCreateImage()
    {
        var drawing = Workspace.GetDrawing();

        var shape = new FoImage2D(200, 100, "Red");
        drawing.AddShape<FoImage2D>(shape);

        Command.SendShapeCreate(shape);
        Command.SendToast(ToastType.Success, "Created");


    }

    private void SetDoAddImage()
    {
        var drawing = Workspace.GetDrawing();

            var r1 = SPEC_Image.RandomSpec();
            var shape = new FoImage2D(r1.width, r1.height, "Yellow")
            {
                ImageUrl = r1.url,
            };

            drawing.AddShape<FoImage2D>(shape);
            Command.SendShapeCreate(shape);
            Command.SendToast(ToastType.Success, "Created");
        
    }
    private void CreateLinePlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var s1 = drawing.AddShape(new FoConnector1D(200, 200, 400, 400, "Red"));
        Command.SendShapeCreate(s1);
        var s2 = drawing.AddShape(new FoConnector1D(200, 400, 400, 600, "Blue"));
        Command.SendShapeCreate(s2);
    }
    

}
