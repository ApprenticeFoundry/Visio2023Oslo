using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using Visio2023Foundry.Dialogs;
using Visio2023Foundry.Shape;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using BlazorComponentBus;

namespace Visio2023Foundry.Model;


public class LCTMTools : FoWorkbook
{

    public List<LCTMShape2D> diagram2D = new();
    public List<LCTMShape1D> diagram1D = new();

    public LCTMTools(IWorkspace space, ICommand command, DialogService dialog, IJSRuntime js, ComponentBus pubSub): 
        base(space,command,dialog,js,pubSub)
    {
    }

    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        "LCTMTools CreateMenus".WriteWarning();
        space.EstablishMenu2D<FoMenu2D, FoButton2D>("Schematic", new Dictionary<string, Action>()
        {
            { "Glue Testing 1", () => DoGlueTesting1() },
            { "Glue Testing 2", () => DoGlueTesting2() },
            { "Glue Testing 3", () => DoGlueTesting3() },

            { "Import", () => ImportDrawing() },
            { "Export", () => ExportDrawing() },

        }, true);

    }

    private static string ParentOfPath(string path)
    {
        var list = path.Split('.').ToList();
        list.RemoveAt(list.Count - 1);
        return string.Join('.', list);
    }

    private static double ParseMax(string value, double max)
    {
        var result = double.Parse(value);
        return result > max ? max : result;
    }

    public void DoGlueTesting1()
    {
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;

        var s1 = drawing.AddShape(new FoShape2D("Source", 200, 100, "Green"));
        s1?.MoveTo(200, 800);

        var s3 = drawing.AddShape(new FoShape2D("Sink", 200, 100, "Blue"));
        s3?.MoveTo(800, 800);

        var s2 = new FoShape1D(s1, s3, 10, "Yellow")
        {
            Name = "Link"
        };
        drawing.AddShape(s2);

        Command.SendShapeCreate(s1);
        Command.SendShapeCreate(s3);
        Command.SendShapeCreate(s2);


        s2.GetMembers<FoGlue2D>()?.ForEach(glue => Command.SendGlue(glue));

    }

    public void DoGlueTesting2()
    {
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;

        var q1 = drawing.AddShape(new FoShape2D("Source1", 200, 100, "Green"));
        q1?.MoveTo(200, 600);


        var q3 = drawing.AddShape(new FoShape2D("Sink1", 200, 100, "Blue"));
        q3?.MoveTo(800, 600);


        var q2 = new FoShape1D("Link1", "Yellow");
        q2.GlueStartTo(q1, "RIGHT");
        q2.GlueFinishTo(q3, "LEFT");

        drawing.AddShape(q2);
    }

    public void DoGlueTesting3()
    {
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;


        var t1 = drawing.AddShape(new LCTMShape2D("SourceLCTM", 200, 200, "Green"));
        t1?.MoveTo(600, 200);

        var t3 = drawing.AddShape(new LCTMShape2D("SinkLCTM", 200, 200, "Blue"));
        t3?.MoveTo(1000, 200);

        var c1 = t1!.AddSubShape(new LCTMShape2D("C1", 100, 80, "Red"));
        var c3 = t3!.AddSubShape(new LCTMShape2D("C3", 100, 80, "Orange"));

        c1.PinX = 150;
        c1.PinY = -30;
        c1.Angle = 30;
        c1.LocPinX = (obj) => obj.Width -50;
        c3.PinY = 50;
        c3.LocPinX = (obj) => 50;
        c3.Angle = 45;



        var w1 = new LCTMShape1D("Wire", "cyan");
        w1.GlueStartTo(c1, "RIGHT");
        w1.GlueFinishTo(c3, "LEFT");
        drawing.AddShape(w1);

        var w2 = new LCTMShape1D(t1, t3, "pink");
        drawing.AddShape(w2);

        var w3 = new LCTMShape1D("last","green");
        w3.GlueStartTo(t1, "RIGHT");
        w3.GlueFinishTo(t3, "BOTTOM");
        drawing.AddShape(w3);

    }

    public void ImportDrawing()
    {
        var data = StorageHelpers.ReadData("LCTMIntegration", "Drawing.json");
        var records = StorageHelpers.Hydrate<List<Import_Drawing>>(data, false);

        data = StorageHelpers.ReadData("LCTMIntegration", "Connections.json");
        var connections = StorageHelpers.Hydrate<List<Import_Connection>>(data, false);

        var drawing = Workspace.GetDrawing();


        // "GUID": "6f895de7-6881-4e87-8421-a68d13067158",
        // "ParentGUID": "00000000-0000-0000-0000-000000000000",
        // "Type": "LCTM_System",
        // "Level": "0",
        // "Name": "LCTM Module",
        // "Text": "LCTM Module [LCTM]",
        // "Details": "",
        // "Shape": "FoShape2D",
        // "Width": "1",
        // "Height": "1",
        // "PinX": "0",
        // "PinY": "0"


        List<LCTMShape2D> subshape = new();
        Dictionary<string, LCTMShape2D> lookup = new();

        var FullWidth = 200;

        records.ForEach(rec =>
        {
            var width = drawing!.ToPixels(ParseMax(rec.Width, 3));
            var height = drawing!.ToPixels(ParseMax(rec.Height, 3));
            var pinx = drawing!.ToPixels(ParseMax(rec.PinX, 30));
            var piny = drawing!.ToPixels(ParseMax(rec.PinY, 30));

            var shape = new LCTMShape2D(rec.Name, width, height, rec.Color ?? "Green")
            {
                PinX = pinx,
                PinY = piny,
                GlyphId = rec.GUID,
                Title = rec.Text,
                Tag = rec.Type,
                Record = rec
            };

            lookup.TryAdd(shape.GlyphId, shape);
            lookup.TryAdd(rec.BOMPath, shape);
            if (shape.Tag.Matches("GPIO_Pin"))
            {
                shape.Color = "Red";
                shape.Height = 35;
                shape.Width = 120;
                shape.PinX = 0;
                subshape.Add(shape);
            }
            else if (shape.Tag.EndsWith("Channel"))
            {
                shape.Color = "Blue";
                shape.Height = 35;
                shape.Width = 120;
                shape.PinX = FullWidth;
                subshape.Add(shape);
            }
            else if (shape.Tag.Matches("SerialDevice"))
            {
                shape.Color = "Black";
                shape.Height = 35;
                shape.Width = 120;
                shape.PinX = FullWidth;
                subshape.Add(shape);
            }
            else if (shape.Tag.Matches("Door_Component"))
            {
                shape.Color = "Orange";
                shape.Height = 35;
                shape.Width = 120;
                shape.PinX = FullWidth;
                subshape.Add(shape);
            }
            else
            {
                shape.Color = "Green";
                shape.Width = FullWidth;
                drawing?.AddShape<LCTMShape2D>(shape);
            }
            diagram2D.Add(shape);
        });

        subshape.ForEach(sub =>
        {
            var parentGuid = sub.Record.ParentGUID;
            var parent = lookup[parentGuid];
            parent.AddSubShapeAndResize(sub);
        });

        //ConnectFromTo(BeagleBone.Door1, Doors.Door1.MagSensor)
        //ConnectFromTo(BeagleBone.Door1, Doors.Door1.Sensor)

        connections.ForEach(con =>
        {
            var shape = new LCTMShape1D()
            {
                GlyphId = con.GUID,
                Tag = con.Type,
                Color = "Black",
                Height = 2,
                Record = con
            };

            var sourceGuid = shape.Record.SourceBOMPath;
            var sinkGuid = ParentOfPath(shape.Record.SinkBOMPath);
            var source = lookup[sourceGuid];
            var sink = lookup[sinkGuid];
            //$"source {source}".WriteError();
            //$"sink {sink}".WriteError();



            $"source {sourceGuid}".WriteSuccess();
            $"sink {sinkGuid}".WriteWarning();


            shape.GlueStartTo(source, "LEFT");
            shape.GlueFinishTo(sink, "RIGHT");

            drawing?.AddShape<LCTMShape1D>(shape);
            diagram1D.Add(shape);
        });

    }

    public void ExportDrawing()
    {
        var drawing = Workspace.GetDrawing();

        List<Import_Drawing> export = new();
        diagram2D.ForEach(shape =>
        {
            var record = shape.Record;
            record.Width = drawing!.ToInches(shape.Width).ToString();
            record.Height = drawing!.ToInches(shape.Height).ToString();
            record.PinX = drawing!.ToInches(shape.PinX).ToString();
            record.PinY = drawing!.ToInches(shape.PinY).ToString();
            export.Add(record);

        });

        var data = StorageHelpers.Dehydrate<List<Import_Drawing>>(export, false);
        StorageHelpers.WriteData("LCTMIntegration", "LCTM_Drawing_Export.json", data);
    }
    public void SetDoCreateBlue()
    {

        Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
        {
            var shape = new FoShape2D(150, 100, "Blue");
            shape.MoveTo(args.OffsetX, args.OffsetY);
            Workspace.GetDrawing().AddShape<FoShape2D>(shape);

            shape.DoOnOpenCreate = shape.DoOnOpenEdit = async (target) =>
            {
                var parmas = new Dictionary<string, object>() {
                    { "Shape", target },
                    { "OnOk", () => {
                        Command.SendShapeCreate(target);
                        //SendToast(UserToast.Success("Created"));
                    }},
                    { "OnCancel", () => {
                        Workspace.GetDrawing()?.ExtractShapes(target.GlyphId);
                    }}
                };
                var options = new DialogOptions() { Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true };
                await Dialog.OpenAsync<ColorRectangle>("Create Square", parmas, options);
            };

            shape.OpenCreate();

        });
    }


    public void SetDoCreateText()
    {
        Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
        {
            var shape = new FoText2D(200, 100, "Red");
            shape.MoveTo(args.OffsetX, args.OffsetY);
            Workspace.GetDrawing().AddShape<FoText2D>(shape);

            shape.DoOnOpenCreate = shape.DoOnOpenEdit = async (target) =>
            {
                var parmas = new Dictionary<string, object>() {
                    { "Shape", target },
                    { "OnOk", () => {
                        Command.SendShapeCreate(target);
                        //SendToast(UserToast.Success("Created"));
                    }},
                    { "OnCancel", () => {
                        Workspace.GetDrawing()?.ExtractShapes(target.GlyphId);
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

        Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
        {
            var shape = new FoImage2D(200, 100, "Red");
            shape.MoveTo(args.OffsetX, args.OffsetY);
            Workspace.GetDrawing().AddShape<FoImage2D>(shape);

            var parmas = new Dictionary<string, object>() {
                { "Shape", shape },
                { "OnOk", () => {
                    Command.SendShapeCreate(shape);
                    //SendToast(UserToast.Success("Created"));
                    //MenuEventService.Notify("ADD_MEDIA");

                } },
                { "OnCancel", () => {
                    Workspace.GetDrawing()?.ExtractShapes(shape.GlyphId);
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
        Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
        {
            var r1 = SPEC_Image.RandomSpec();
            var shape1 = new FoImage2D(r1.width, r1.height, "Yellow")
            {
                ImageUrl = r1.url,
            };
            shape1.MoveTo(args.OffsetX, args.OffsetY);
            Workspace.GetDrawing().AddShape<FoImage2D>(shape1);

            // var r2 = SPEC_Image.RandomSpec();
            // var shape2 = new FoImage2D(r2.width, r2.height, "Blue")
            // {
            //     ImageUrl = r2.url
            // };
            // shape2.MoveTo(args.OffsetX, args.OffsetY);
            // PageManager.Add<FoImage2D>(shape2);

            //MenuEventService.Notify("ADD_MEDIA");
        });
    }


    private void SetDoAddVideo()
    {
        Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
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
             Workspace.GetDrawing().AddShape<FoVideo2D>(shape);
         });
    }



}
