
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using IoBTMessage.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QRCoder;
using Radzen;

namespace Visio2023Foundry.Model;


public class Playground : FoWorkbook
{

    private FoMenu2D? PlaygroundMenu { get; set; }
    private IDrawing Drawing { get; set; }

    public Playground(IWorkspace space, IFoundryService foundry) :
        base(space,foundry)
    {
        Drawing = space.GetDrawing()!;
        // Thread Overview - Canvas Background = #faf8cf
        EstablishCurrentPage(GetType().Name, "#faf8cf").SetPageSize(60, 40, "cm");
    }
    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        "Playground CreateMenus".WriteWarning();
        PlaygroundMenu = space.EstablishMenu2D<FoMenu2D, FoButton2D>("Playground", new Dictionary<string, Action>()
        {

            { "Clear", () => DoClear() },
            { "Guid Test", () => CreateGuidTest()},
            { "Group", () => CreateGroupShape()},
            { "Ring", () => CreateRingGroupPlayground()},
            { "Multi-Shape", () => CreateMultiShapePlayground()},
            { "Glue 1D", () => CreateGlue1DPlayground()},
            { "Glue 2D", () => CreateGlue2DPlayground()},
            { "Ticker", () => CreateTickPlayground()},     
            { "Line", () => CreateLinePlayground()},
            { "Menu", () => CreateMenuPlayground()},
            { "Lets Dance", () => LetsDance()},
        }, true);
    }

    private void DoClear()
    {
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;
        drawing.ClearAll();
    }

    private void CreateGuidTest()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var s1 = new FoShape2D(100, 100, "Orange");
        s1.MoveTo(100, 100);
        s1.GlyphId.WriteNote();

        var TargetId1 = s1.GetGlyphId();
        var Payload = CodingExtensions.Dehydrate(s1, false);

        TargetId1.WriteInfo();

        var s2 = CodingExtensions.Hydrate<FoShape2D>(Payload, false);
        s2.MoveTo(300, 100);

        var TargetId2 = s2.GetGlyphId();
        TargetId2.WriteInfo();

        drawing.AddShape(s1);
        drawing.AddShape(s2);       
    }

    private void CreateCapturePlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var s3 = drawing.AddShape(new FoShape2D(100, 100, "Cyan"));
        s3?.MoveTo(800, 200);
        var s4 = drawing.AddShape(new FoShape2D(50, 50, "Green"));
        s4?.MoveTo(10, 10);

        drawing.CurrentPage().ExtractWhere<FoShape2D>(child => child == s4);
        s3?.CaptureShape<FoShape2D>(s4!, false);

        Command.SendShapeCreate(s3);
    }


    private void CreateGroupShape()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var c1 = drawing.AddShape<FoGroup2D>(new FoGroup2D(100, 100, "Cyan"));
        c1?.MoveTo(600,290);
        c1?.Add<FoShape2D>(new FoShape2D(20, 60, "Red"));
        c1?.Add<FoShape2D>(new FoShape2D(60, 30, "Green")).MoveTo(30, 20);
        c1?.CaptureShape<FoShape2D>(new FoShape2D(20, 30, "Blue")).MoveTo(100, 90);

        Command.SendShapeCreate(c1);
    }


    private void CreateGlue1DPlayground()
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

    private void CreateGlue2DPlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var s1 = new FoShape2D(200, 200, "Green");
        drawing.AddShape(s1);
        s1.MoveTo(400, 200);

        var text = "Foundry Canvas QR Code";
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);
        var base64 = Convert.ToBase64String(qrCodeImage);
        var dataURL = $"data:image/png;base64,{base64}";

        var q1 = new FoImage2D(80, 80, "White")
        {
            ImageUrl = dataURL,
            ScaleX = 0.10,
            ScaleY = 0.10,
        };
        q1.MoveTo(400, 400);
        drawing.AddShape<FoImage2D>(q1);

        q1.ContextLink = (obj,tick) => {
            obj.PinX = s1.PinX;
            obj.PinY = s1.PinY + s1.Height/2;
        };

        s1.AnimatedMoveTo(800, 300);
    }

   private void CreateTickPlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var s1 = new FoShape2D(200, 200, "Green");
        drawing.AddShape(s1);
        s1.MoveTo(200, 200);

        var s2 = new FoShape2D(200, 25, "Blue")
        {
            LocPinX = (obj) => obj.Width / 4
        };
        drawing.AddShape(s2);

        s2.ContextLink = (obj,tick) => {
            obj.PinX = s1.PinX;
            obj.PinY = s1.PinY;
            obj.Angle += 1;
        };
    }

    private void CreateRingGroupPlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var radius = 100;
        int cnt = 0;
        for (int i = 0; i <= 360; i += 30)
        {
            var a = Math.PI / 180.0 * i;
            var x = (int)(radius * Math.Cos(a)) + 600;
            var y = (int)(radius * Math.Sin(a)) + 300;
            var shape = new FoShape2D(30, 30, "Cyan");
            shape.MoveTo(x, y);
            shape.ShapeDraw = (cnt++ % 3 == 0) ? shape.DrawCircle : shape.DrawRect;
            drawing.AddShape<FoShape2D>(shape);
            Command.SendShapeCreate(shape);
        }
    }

    private void CreateMultiShapePlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var shape = new FoShape2D(300, 300, "Red");
        shape.AnimatedMoveTo(200, 200);

        var mock = new MockDataGenerator();
        var list = new List<Action<Canvas2DContext, FoGlyph2D>>()
        {
            shape.DrawCircle,
            shape.DrawRect,
            shape.DrawBox,
            async (ctx,obj) => await obj.DrawTriangle(ctx, obj.Color),
            async (ctx,obj) => await obj.DrawStar(ctx, obj.Color),
        };


        shape.ContextLink = (obj,tick) => 
        {
            if ( tick == 0 ) return;
            var change = tick % 30;
            //$"{tick} {change}".WriteLine(ConsoleColor.Yellow);
            if ( change == 0 ) {
                obj.Color = mock.GenerateColor();
                var i = mock.GenerateInt(0, list.Count);
                obj.ShapeDraw = list[i];
            }
        };

        drawing.AddShape<FoShape2D>(shape);
        Command.SendShapeCreate(shape);
    }
    private void LetsDance()
    {
        var rand = new Random();
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;

        var pages = drawing.Pages();
        var page = pages.CurrentPage();


        pages.Selections().ForEach(shape =>
        {
            //$"{shape.Name} Animations".WriteLine(ConsoleColor.DarkYellow);

            var X = page?.Width / 2 + 600 * (0.5 - rand.NextDouble());
            var Y = page?.Height / 2 + 600 * (0.5 - rand.NextDouble());
            FoGlyph2D.Animations.Tween(shape, new { PinX = X, PinY = Y }, 2)
                .OnUpdate((arg) =>
                {
                    FoGlyph2D.ResetHitTesting = true;
                    //$"{arg} working".WriteLine(ConsoleColor.DarkYellow);
                });

            var factor = 3 * rand.NextDouble();
            FoGlyph2D.Animations.Tween(shape, new { Width = factor * shape.Width, Height = factor * shape.Height }, 3);

            if (shape is FoImage2D)
                FoGlyph2D.Animations.Tween(shape, new { ScaleX = 2 * rand.NextDouble(), ScaleY = 2 * rand.NextDouble() }, 3);

            var angle = shape.Angle + 360.0 * rand.NextDouble();
            FoGlyph2D.Animations.Tween(shape, new { Angle = angle }, 4);
        });
    }

    private void CreateLinePlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var s1 = drawing.AddShape(new FoConnector1D(50, 50, 400, 100, "Red"));
        Command.SendShapeCreate(s1);
        var s2 = drawing.AddShape(new FoConnector1D(50, 100, 600, 100, "Blue"));
        Command.SendShapeCreate(s2);
    }
    
   
    private void CreateMenuPlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null || PlaygroundMenu == null) return;

        PlaygroundMenu.ToggleLayout();
        drawing.AddShape<FoMenu2D>(PlaygroundMenu).AnimatedMoveTo(100, 100);
        PlaygroundMenu.Angle = 0;

    }
    
     

 
}
