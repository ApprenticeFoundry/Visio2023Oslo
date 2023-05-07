using BlazorComponentBus;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

using Visio2023Foundry.Services;
using Visio2023Foundry.Shape;

namespace Visio2023Foundry.Model;


public class World : FoWorkbook
{
    private IRestAPIServiceDTAR? DTARRestService { get; set; }
    private Semantic SemanticModel { get; set; }


    public World(IWorkspace space, ICommand command, DialogService dialog, IJSRuntime js, ComponentBus pubSub): 
        base(space,command,dialog,js,pubSub)
    {
        SemanticModel = new Semantic(space.GetDrawing(),pubSub);
    }



    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        "World CreateMenus".WriteWarning();
        var arena = space.GetArena();
        var menu = new Dictionary<string, Action>()
        {
            { "Clear", async () => await arena.ClearArena() },
            { "Update", async () => await arena.UpdateArena() },
            { "Test Platform", () => arena.MakeAndRenderTestPlatform() },
        };


        space.EstablishMenu3D<FoMenu3D, FoButton3D>("World", menu, true);
        //watch me extend this menu after a service call
        if (DTARRestService != null)
            Task.Run(async () =>
            {
                var docs = await DTARRestService.GetWorlds();
                docs.ForEach(item =>
                {
                    menu.Add(item.title, () =>
                    {
                        var world = MapToWorld3D(item);  //process the world just in time
                        space.GetArena()?.RenderWorld(world);
                    });
                });

                space.EstablishMenu3D<FoMenu3D,FoButton3D>("World", menu, true);
                //await PubSub.Publish<RefreshUIEvent>(new RefreshUIEvent("four"));
            });


    }

    public FoWorld3D MapToWorld3D(DT_World3D world)
    {
        var newWorld = new FoWorld3D();
        world.platforms.ForEach(item =>
        {
            var group = new FoGroup3D()
            {
                PlatformName = item.platformName,
                GlyphId = item.uniqueGuid,
                Name = item.name,
            };
            newWorld.Slot<FoGroup3D>().Add(group);
        });
        world.bodies.ForEach(item =>
        {
            var pos = item.position;
            var box = item.boundingBox;
            var body = new FoShape3D()
            {
                PlatformName = item.platformName,
                GlyphId = item.uniqueGuid,
                Name = item.name,
                Symbol = item.symbol,
                Type = item.type,
                Position = pos == null ? null : new FoVector3D(pos.xLoc, pos.yLoc, pos.zLoc),
                Rotation = pos == null ? null : new FoVector3D(pos.xAng, pos.yAng, pos.zAng),
                BoundingBox = box == null ? null : new FoVector3D(box.width, box.height, box.depth),
                Origin = box == null ? null : new FoVector3D(box.pinX, box.pinY, box.pinZ),
            };
            newWorld.Slot<FoShape3D>().Add(body);
        });
        world.labels.ForEach(item =>
        {
            var pos = item.position;
            var label = new FoText3D()
            {
                PlatformName = item.platformName,
                GlyphId = item.uniqueGuid,
                Name = item.name,
                Position = pos == null ? null : new FoVector3D(pos.xLoc, pos.yLoc, pos.zLoc),
                Text = item.text,
                Details = item.details
            };
            newWorld.Slot<FoText3D>().Add(label);
        });
        newWorld.FillPlatforms();
        return newWorld;
    }

    private void BuildPlatform(UDTO_Platform platform, FoLayoutTree<FoHero2D> source)
    {
        var shape = source.GetShape();
        var model = SemanticModel.FindModel(shape.Name);
        if (model == null)
            return;

        var pixelsPerMeter = 200;
        var label = platform.FindOrCreate<UDTO_Label>(model.guid, true);
        var details = FoHero2D.CreateTextList(model.description, 25);
        var title = $"{source.ComputeName()} {model.title}";


        label.CreateLabelAt(title, details, shape.PinX / pixelsPerMeter, shape.PinY / pixelsPerMeter, 10 * source.level);

        var children = source.GetChildren();
        children?.ForEach(item => BuildPlatform(platform, item));
    }

    // public void EstablishWorld()
    // {
    //     if (CurrentLayout == null) return;

    //     var shape = CurrentLayout.GetShape();
    //     var model = SemanticModel.FindModel(shape.Name);
    //     if (model == null)
    //         return;

    //     var world = _DTDB.Find<DT_World3D>(shape.Name);
    //     if (world == null)
    //     {
    //         var platform = new UDTO_Platform()
    //         {
    //             uniqueGuid = shape.Name,
    //             platformName = shape.Title
    //         };
    //         CurrentLayout.GetChildren()?.ForEach(item => BuildPlatform(platform, item));

    //         world = _DTDB.Establish<DT_World3D>(shape.Name);
    //         world.title = model.title;
    //         world.description = model.description;
    //         world.FillWorldFromPlatform(platform);
    //     }

    //     //DTARRestService.WorldAddOrUpdate(world);
    // }

   

}
