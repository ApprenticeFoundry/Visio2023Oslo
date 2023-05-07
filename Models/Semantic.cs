using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;
using IoBTMessage.Models;
using IoBTModules.Extensions;
using QRCoder;
using Radzen;
using SkiaSharp;
using Visio2023Foundry.Shape;

namespace Visio2023Foundry.Model;


public class Semantic
{
    private IDrawing Drawing { get; set; }

    private FoLayoutTree<FoHero2D>? CurrentLayout { get; set; }
    private readonly Dictionary<string, DT_Title> _Models = new();




    public Semantic(IDrawing drawing, ComponentBus pubSub) 
    {

        Drawing = drawing;

        //always ready to be part of the solution

        pubSub.SubscribeTo<AttachAssetFileEvent>(obj =>
        {
            "AttachAssetFileEvent".WriteInfo();
            if (FindModel(obj.AssetGuid) is DT_AssetFile asset)
            {
                if (FindModel(obj.TargetGuid) is DT_Hero target)
                {
                    AddAssetReference(target, asset);
                    if (CurrentLayout != null && obj.AssetShape != null && obj.TargetShape != null)
                    {
                        var node = CurrentLayout.FindNodeWithName(obj.TargetShape.Name);
                        var child = new FoLayoutTree<FoHero2D>((FoHero2D)obj.AssetShape);
                        node?.AddChildNode(child);
                        LayoutTree(CurrentLayout);
                    }
                }
            };
        });
    }


    public DT_Title? FindModel(string? key)
    {
        if (!string.IsNullOrEmpty(key) && _Models.TryGetValue(key, out DT_Title? model)) return model;
        return null;
    }

    public DT_Title AddModel(DT_Title model)
    {
        if (!_Models.ContainsKey(model.guid))
            _Models.Add(model.guid, model);

        return model;
    }  

    private static FoImage2D AddQRCodeShape(UDTO_File file)
    {
        var qrGenerator = new QRCodeGenerator();
        var url = file.url ?? "NO URL";
        var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

        var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);

        var shape = new FoImage2D(file.filename, 200, 200, "White");

        var bitmap = SKBitmap.Decode(qrCodeImage);
        var resized = bitmap.Resize(new SKImageInfo(shape.Width, shape.Height), SKFilterQuality.Medium);
        var png = resized.Encode(SKEncodedImageFormat.Png, 2);

        using var memoryStream = new MemoryStream();
        png.AsStream().CopyTo(memoryStream);
        var base64 = Convert.ToBase64String(memoryStream.ToArray());
        var dataURL = $"data:image/png;base64,{base64}";
        shape.ImageUrl = dataURL;
        return shape;
    }



    public void AttachItem<V>(FoLayoutTree<V> node, DT_AssetFile item) where V : FoHero2D
    {
        AddModel(item);
        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(item, "Pink");
        Drawing.AddShape<V>(shape);

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

    public void LayoutTree(FoLayoutTree<FoHero2D> layout)
    {
        var margin = new Point(20, 50);
        var page = Drawing.CurrentPage();
        var pt = Drawing.InchesToPixelsInset(page.PageWidth / 3, 5.0);

        layout.HorizontalLayout(pt.X, pt.Y, margin);
        layout.HorizontalLayoutConnections<FoConnector1D>(Drawing.Pages());
    }

    public async Task RenderTree(Canvas2DContext ctx)
    {
        if (CurrentLayout != null)
            await CurrentLayout.RenderTree(ctx);
    }

    public DT_AssetFile AddAssetFile(DT_Hero hero, string filename)
    {
        var asset = new DT_AssetFile() { filename = filename };
        return AddAssetReference(hero, asset);
    }

    public DT_AssetFile AddAssetReference(DT_Hero hero, DT_AssetFile asset)
    {
        var docRef = new DT_AssetReference()
        {
            asset = asset,
            assetGuid = asset.guid,
            heroGuid = hero.guid,
        };
        hero.AddAssetReference<DT_AssetReference>(docRef);
        return asset;
    }

   public DT_AssetFile CreateAssetFile(UDTO_File source)
    {
        var asset = new DT_AssetFile
        {
            name = source.filename.CleanToFilename(),
            title = source.filename,
            filename = source.filename,
            docType = source.mimeType,
            url = source.url
        };
        AddModel(asset);
        return asset;
    }
}