using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using IoBTMessage.Extensions;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QRCoder;
using Radzen;
using Visio2023Foundry.Dialogs;

namespace Visio2023Foundry.Model;

	public class SPEC_ImageURL
	{
        public int width { get; set; } = 0;
        public int height { get; set; } = 0;
        public string url { get; set; } = "";

		//https://stackoverflow.com/questions/60797390/generate-random-image-by-url
		public static SPEC_ImageURL RandomSpec()
		{
			var gen = new MockDataGenerator();
			var width = 50 * gen.GenerateInt(1, 6);
			var height = 50 * gen.GenerateInt(1, 6);
			return new SPEC_ImageURL()
			{
				url = $"https://picsum.photos/{width}/{height}",
				width = width,
				height = height,
			};
		}
	}

public class Stencil : FoWorkbook
{

    public Stencil(IWorkspace space, ICommand command, DialogService dialog, IJSRuntime js, ComponentBus pubSub): 
        base(space, command, dialog, js, pubSub)
    {
    }

    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        space.EstablishMenu2D<FoMenu2D, FoButton2D>("Create", new Dictionary<string, Action>()
        {
            { "GPT4 Arrow", () => SetDoCreateGPT4Arrow()},
            { "Steve Arrow", () => SetDoCreateSteveArrow()},
            { "Blue Shape", () => SetDoCreateBlue()},
            { "Text Shape", () => SetDoCreateText()},
            { "Image URL", () => SetDoAddImage()},
            { "Video URL", () => SetDoAddVideo()},
            { "Glue", () => CreateGluePlayground()},
            { "QR Code", () => SetDoAddQRCode()},
        }, true);

    }

    public void SetDoCreateBlue()
    {
        var drawing = Workspace.GetDrawing();
        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var shape = new FoShape2D(150, 100, "Blue");
            shape.MoveTo(args.OffsetX, args.OffsetY);
            drawing.AddShape<FoShape2D>(shape);

            shape.DoOnOpenCreate = shape.DoOnOpenEdit = async (target) =>
            {
                var parmas = new Dictionary<string, object>() {
                    { "Shape", target },
                    { "OnOk", () => {
                        Command.SendShapeCreate(target);
                        Command.SendToast(ToastType.Success,"Created");
                    }},
                    { "OnCancel", () => {
                        drawing.ExtractShapes(target.GlyphId);
                    }}
                };
                var options = new DialogOptions() { Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true };
                await Dialog.OpenAsync<ColorRectangle>("Create Color Square", parmas, options);
            };

            shape.OpenCreate();

        });
    }

    public void SetDoCreateGPT4Arrow()
    {
        var drawing = Workspace.GetDrawing();

        var shape = new FoShape2D(150, 100, "Blue");
        shape.MoveTo(300, 200);
        shape.ShapeDraw = async (ctx, obj) =>
        {
            await DrawGPT4ArrowAsync(ctx, obj.Width, obj.Height, obj.Color);
        };
        drawing.AddShape<FoShape2D>(shape);
    }

    private static async Task DrawGPT4ArrowAsync(Canvas2DContext ctx, int width, int height, string color)
    {
        var headWidth = height / 2;
        var bodyHeight = width / 4;
        var bodyWidth = Math.Max((width / 4) * 3, width - headWidth);

        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(0, height / 2);
        await ctx.LineToAsync(bodyWidth, height / 2);
        await ctx.LineToAsync(bodyWidth, (height / 2) - (bodyHeight / 2));
        await ctx.LineToAsync(width, height / 2);
        await ctx.LineToAsync(bodyWidth, (height / 2) + (bodyHeight / 2));
        await ctx.LineToAsync(bodyWidth, height / 2);
        await ctx.ClosePathAsync();

        await ctx.SetFillStyleAsync(color);
        await ctx.FillAsync();
        await ctx.SetFillStyleAsync("#fff");
        await ctx.FillTextAsync("→", width - headWidth + 3, (height / 2) + 5, 20);
    }

    public void SetDoCreateSteveArrow()
    {
        var drawing = Workspace.GetDrawing();

        var shape = new FoShape2D(150, 100, "Blue");
        shape.MoveTo(300, 200);
        shape.ShapeDraw = async (ctx, obj) =>
        {
            await DrawSteveArrowAsync(ctx, obj.Width, obj.Height, obj.Color);
        };
        drawing.AddShape<FoShape2D>(shape);
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

    private void CreateGluePlayground()
    {
        var drawing = Workspace.GetDrawing();
        if ( drawing == null) return;

        var s1 = drawing.AddShape(new FoShape2D(50, 50, "Green"));
        s1?.MoveTo(200, 200);

        var s2 = drawing.AddShape(new FoShape2D(50, 50, "Orange"));
        s2?.MoveTo(800, 400);


        var wire2 = new FoShape1D("Arrow", "Red")
        {
            Height = 50,
            ShapeDraw = async (ctx, obj) => await DrawSteveArrowAsync(ctx, obj.Width, obj.Height, obj.Color)
        };
        wire2.GlueStartTo(s1, "RIGHT");
        wire2.GlueFinishTo(s2, "LEFT");
        drawing.AddShape(wire2);


        Command.SendShapeCreate(s2);
        Command.SendShapeCreate(s1);

        Command.SendShapeCreate(wire2);

        wire2.GetMembers<FoGlue2D>()?.ForEach(glue => Command.SendGlue(glue));
    }
    public void SetDoCreateText()
    {
        var drawing = Workspace.GetDrawing();
        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var shape = new FoText2D(200, 100, "Red");
            shape.MoveTo(args.OffsetX, args.OffsetY);
            drawing.AddShape<FoText2D>(shape);

            shape.DoOnOpenCreate = shape.DoOnOpenEdit = async (target) =>
            {
                var parmas = new Dictionary<string, object>() {
                    { "Shape", target },
                    { "OnOk", () => {
                        Command.SendShapeCreate(target);
                        Command.SendToast(ToastType.Success,"Created");
                    }},
                    { "OnCancel", () => {
                        drawing.ExtractShapes(target.GlyphId);
                    }}
                };
                var options = new DialogOptions() { Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true };
                await Dialog.OpenAsync<TextRectangle>("Create Text", parmas, options);
            };

            shape.OpenCreate();

        });
    }

    private void SetDoCreateImage()
    {
        var options = new DialogOptions() { Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true };

        var drawing = Workspace.GetDrawing();
        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var shape = new FoImage2D(200, 100, "Red");
            shape.MoveTo(args.OffsetX, args.OffsetY);
            drawing.AddShape<FoImage2D>(shape);

            var parmas = new Dictionary<string, object>() {
                { "Shape", shape },
                { "OnOk", () => {
                    Command.SendShapeCreate(shape);
                    Command.SendToast(ToastType.Success, "Created");
                }},
                { "OnCancel", () => {
                    drawing.ExtractShapes(shape.GlyphId);
                }}
            };

            Task.Run(async () =>
            {
                await Dialog.OpenAsync<ImageUploaderDialog>("Upload Image", parmas, options);
            });

        });

    }




    private void SetDoAddImage()
    {
        var drawing = Workspace.GetDrawing();
        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var r1 = SPEC_ImageURL.RandomSpec();
            var shape = new FoImage2D(r1.width, r1.height, "Yellow")
            {
                ImageUrl = r1.url,
                PinX = args.OffsetX,
                PinY = args.OffsetY
            };
            drawing.AddShape<FoImage2D>(shape);
            Command.SendShapeCreate(shape);
            Command.SendToast(ToastType.Success, $"Created {r1.width} {r1.height}");
        });
    }

    private void SetDoAddQRCode()
    {
        var drawing = Workspace.GetDrawing();
        drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var text = "Foundry Canvas QR Code Test Text";
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);
            var base64 = Convert.ToBase64String(qrCodeImage);
            var dataURL = $"data:image/png;base64,{base64}";
           // $"qr base64={dataURL[..30]}".WriteLine(ConsoleColor.Yellow);

            var shape = new FoImage2D(80, 80, "White")
            {
                ImageUrl = dataURL,
                ScaleX = 0.5,
                ScaleY = 0.5,
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            drawing.AddShape<FoImage2D>(shape);
            Command.SendShapeCreate(shape);
            Command.SendToast(ToastType.Success, "Created");
        });
    }

    //https://www.nuget.org/packages/QRCoder/
    //private void SetDoAddQRCodeImage()
    //{
    //    using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
    //    using (QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q))
    //    using (QRCode qrCode = new QRCode(qrCodeData))
    //    {
    //        Bitmap qrCodeImage = qrCode.GetGraphic(20);
    //    }

    //    Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
    //    {
    //        var r1 = SPEC_Image.RandomSpec();
    //        var shape1 = new FoImage2D(r1.width, r1.height, "Yellow")
    //        {
    //            ImageUrl = r1.url,
    //        };
    //        shape1.MoveTo(args.OffsetX, args.OffsetY);
    //        Workspace.GetDrawing()?.AddShape<FoImage2D>(shape1);
    //    });
    //}


    private void SetDoAddVideo()
    {
        var drawing = Workspace.GetDrawing();
        drawing.SetDoCreate((CanvasMouseArgs args) =>
         {
             var r1 = new UDTO_Image()
             {
                 width = 400,
                 height = 300,
                 url = "https://upload.wikimedia.org/wikipedia/commons/7/79/Big_Buck_Bunny_small.ogv"
             };
             var shape = new FoVideo2D(r1.width, r1.height, "Yellow")
             {
                 ImageUrl = r1.url,
                 ScaleX = 2.0,
                 ScaleY = 2.0,
                 JsRuntime = JsRuntime
             };
             shape.MoveTo(args.OffsetX, args.OffsetY);
             drawing.AddShape<FoVideo2D>(shape);
             Command.SendShapeCreate(shape);
             Command.SendToast(ToastType.Success, "Created");
         });
    }



}
