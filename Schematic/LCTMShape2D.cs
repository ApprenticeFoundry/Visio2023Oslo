using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using Visio2023Foundry.Model;
using IoBTMessage.Models;

namespace Visio2023Foundry.Shape;

public class LCTMShape2D : FoShape2D
{

    public string Title { get; set; } = "";
    public string BOMPath { get; set; } = "";

    public Import_Drawing Record { get; set; }


    public LCTMShape2D() : base()
    {
        ShapeDraw = DrawRect;
        Record = new Import_Drawing();
    }

 
    public LCTMShape2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
        PinX = PinY = 0;
        ShapeDraw = DrawRect;
        Record = new Import_Drawing();
    }



    //this is the location for the glue to attach to


    public LCTMShape2D TagWithModel(DT_Title model, string color)
    {
        Tag = model.GetType().Name;
        Name = model.guid;
        Color = color;
        Title = model.title;
        ResizeTo(250, 120);
        //PreSizeToFitText(model.title);
        return this;
    }

    public override bool SmashGlue()
    {
        GetMembers<LCTMShape2D>()?.ForEach(item => item.SmashGlue());
        return base.SmashGlue();
    }



    public LCTMShape2D AddSubShape(LCTMShape2D subShape) 
    {
        var type = subShape.Tag;
        subShape.Title = subShape.Name;

        subShape.Level = Level + 1;
        subShape.GetParent = () => this;

        var count = Members<LCTMShape2D>().Where(item => item.Tag.Matches(type)).Count();
        Add<LCTMShape2D>(subShape);
        return subShape;
    }

    public LCTMShape2D AddSubShapeAndResize(LCTMShape2D subShape) 
    {
        AddSubShape(subShape);
        var type = subShape.Tag;
        var count = Members<LCTMShape2D>().Where(item => item.Tag.Matches(type)).Count();

        subShape.PinY = 10 + count * 40;

        var min = subShape.PinY + subShape.Height;
        Height = Math.Max(Height, min);
        return subShape;
    }

    public List<string> PreSizeToFitText(string title)
    {
        if (string.IsNullOrEmpty(title)) return new List<string>();

        var list = CreateTextList(title, 24);
        if ( list.Count > 4 )
            Height = 28 * list.Count;
            
        return list;
    }
    public static List<string> CreateTextList(string text, int max)
    {
        var list = new List<string>();
        if ( text == null || string.IsNullOrEmpty(text)) return list;

        var line = "";
        foreach (var word in text.Split(" ").ToList())
        {
            if (line.Length + word.Length <= max)
            {
                line = $"{line} {word}";
            } 
            else 
            {
                list.Add(line);
                line = word;
            }
        }
        if ( line.Length > 0 )
            list.Add(line);

        return list;
    }
    public async Task DrawText(Canvas2DContext ctx, int tick)
    {
        if (!string.IsNullOrEmpty(Title) )
        {
            var FontSpec = $"normal 14px sans-serif";
            await ctx.SetFontAsync(FontSpec);

            await ctx.SetTextAlignAsync(TextAlign.Center);
            await ctx.SetTextBaselineAsync(TextBaseline.Top);

            await ctx.SetFillStyleAsync("White");

            var top = 8;

            //write code to debounce/memoize  this value
            var list = PreSizeToFitText(Title);
            foreach (var item in list)
            {
               await ctx.FillTextAsync(item, Width / 2, top);
               top += 25;

            }
            //await DrawPin(ctx);
        }
    }

    public async Task DrawDetails(Canvas2DContext ctx, int tick)
    {
        await ctx.SaveAsync();
        if ( ShouldRender ) 
        {
            ShapeDraw?.Invoke(ctx, this);
            await DrawText(ctx, tick);
        }
        else
        {
            await ctx.SetLineWidthAsync(10);
            await ctx.SetStrokeStyleAsync("Yellow");
            await ctx.StrokeRectAsync(0, 0, Width, Height);
        }

        await ctx.RestoreAsync();
    }






    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (CannotRender()) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await DrawDetails(ctx, tick);
        if (!IsSelected)
            HoverDraw?.Invoke(ctx, this);

        //await DrawTag(ctx);
        //await DrawText(ctx, tick);

        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        if (deep)
        {
            GetMembers<FoShape1D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
            GetMembers<FoShape2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
            GetMembers<LCTMShape2D>()?.ForEach(async child => {
                //child.IsSelected = IsSelected;
                await child.RenderDetailed(ctx, tick, deep);
            });
        }

        if (GetMembers<FoGlue2D>()?.Count > 0)
            await DrawTriangle(ctx, "Black");


        await ctx.RestoreAsync();
        return true;
    }

}
