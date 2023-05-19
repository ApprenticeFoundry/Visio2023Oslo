
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Visio2023Foundry.Dialogs;

namespace Visio2023Foundry.Model;


public class SteveArrow: FoShape1D
{
    public SteveArrow():base()
    {
        Height = 50;
        ShapeDraw = async (ctx, obj) => await DrawSteveArrowAsync(ctx, obj.Width, obj.Height, obj.Color);
    }

    private static async Task DrawSteveArrowAsync(Canvas2DContext ctx, int width, int height, string color)
    {
        var headWidth = 40;
        var bodyHeight = height / 4;
        var bodyWidth = width - headWidth;

        await ctx.SetFillStyleAsync(color);
        var y = (height - bodyHeight) / 2.0;
        await ctx.FillRectAsync(0, y, bodyWidth, bodyHeight);

        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(bodyWidth, 0);
        await ctx.LineToAsync(width, height / 2);
        await ctx.LineToAsync(bodyWidth,height);
        await ctx.LineToAsync(bodyWidth, 0);
        await ctx.ClosePathAsync();
        await ctx.FillAsync();

        await ctx.SetFillStyleAsync("#fff");
        await ctx.FillTextAsync("→", width /2, height / 2, 20);
    }
}


public class SignalRDemo : FoWorkbook
{

    public SignalRDemo(IWorkspace space, ICommand command, DialogService dialog, IJSRuntime js, ComponentBus pubSub): 
        base(space,command,dialog,js,pubSub)
    {
        StorageHelpers.RegisterLookupType<SteveArrow>();
    }


    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        var OpenNew = async () =>
        {
            var target = nav!.ToAbsoluteUri("/");
            try
            {
                //  /visio2023drawing/Signalr
                var url = $"{target}/visio2023drawing/Signalr";
                await js.InvokeAsync<object>("open", url); //, "_blank", "height=600,width=1200");
            }
            catch { }
        };

        space.EstablishMenu2D<FoMenu2D, FoButton2D>("SignalR", new Dictionary<string, Action>()
        {
            { "Open New", () => OpenNew()},
            { "Tug of War", () => SetDoTugOfWar()},
            { "Ring", () => CreateGroupPlayground()},
            { "Glue", () => CreateGluePlayground()},
            { "Blue Shape", () => SetDoCreateBlue()},
            { "Text Shape", () => SetDoCreateText()},
            { "Image Shape", () => SetDoCreateImage()},
            { "Image URL", () => SetDoAddImage()},
              { "Start", () => StartHub()},
                { "Stop", () => StopHub()},
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

    private void SetDoTugOfWar()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var s1 = new FoShape2D(50, 50, "Blue");
        s1.MoveTo(300, 200);
        var s2 = new FoShape2D(50, 50, "Orange");
        s2.MoveTo(500, 200);

        drawing.AddShape(s1);  
        drawing.AddShape(s2);

        var wire2 = new SteveArrow();

        //wire2.GlueStartTo(s1);
        //wire2.GlueFinishTo(s2);
      
        wire2.GlueStartTo(s1, "RIGHT");
        wire2.GlueFinishTo(s2, "LEFT");
        drawing.AddShape(wire2);

        Command.SendShapeCreate(s2);
        Command.SendShapeCreate(s1);
        Command.SendShapeCreate(wire2);
        wire2.GetMembers<FoGlue2D>()?.ForEach(glue => Command.SendGlue(glue));
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
