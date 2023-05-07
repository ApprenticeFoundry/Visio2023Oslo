using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shape;
using IoBTMessage.Models;

namespace Visio2023Foundry.Shape;

public class FoHero2D : FoShape2D
{
    public string Title { get; set; } = "";
    public FoHero2D() : base()
    {
        ShapeDraw = DrawRect;
    }

 
    public FoHero2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
        PinX = PinY = 0;
        ShapeDraw = DrawRect;
    }

    public FoHero2D TagWithModel(DT_Title model, string color)
    {
        Tag = model.GetType().Name;
        Name = model.name ?? model.guid;
        GlyphId = model.guid;
        Color = color;
        Title = model.title;
        ResizeTo(250, 120);
        //PreSizeToFitText(model.title);
        return this;
    }

    public override List<FoConnectionPoint2D> GetConnectionPoints() 
    {
        if ( !this.HasSlot<FoConnectionPoint2D>()) 
        {
            var lx = LeftX();
            var ty = TopY();
            var rx = RightX();
            var by = BottomY();
            var cx = CenterX();
            var cy = CenterY();
            AddConnectionPoint2D(new FoConnectionPoint2D("LEFT", lx, cy, "Yellow"));
            AddConnectionPoint2D(new FoConnectionPoint2D("TOP", cx, ty, "Yellow"));
            AddConnectionPoint2D(new FoConnectionPoint2D("RIGHT", rx, cy, "Yellow"));
            AddConnectionPoint2D(new FoConnectionPoint2D("BOTTOM", cx, by, "Yellow"));
        }
        var result = this.Members<FoConnectionPoint2D>();
        return result;
    }

    public List<string> PreSizeToFitText(string title)
    {
        if (title == null) return new List<string>();

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
        if (!string.IsNullOrEmpty(Title))
        {
            var FontSpec = $"normal bold 18px sans-serif";
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
            await DrawPin(ctx);
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
        if ( !IsVisible ) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await DrawDetails(ctx, tick);
        if ( !IsSelected )
            HoverDraw?.Invoke(ctx, this);
            

        if ( !string.IsNullOrEmpty(Tag))
        {
            await ctx.SetTextAlignAsync(TextAlign.Left);
            await ctx.SetTextBaselineAsync(TextBaseline.Top);

            await ctx.SetFillStyleAsync("Black");
            await ctx.FillTextAsync(Tag, LeftX() + 2, TopY() + 3);
        }

        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        if (deep)
        {
            GetMembers<FoShape2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
            GetMembers<FoImage2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        }
        await ctx.RestoreAsync();
        return true;
    }

    public override List<FoImage2D> CollectImages(List<FoImage2D> list, bool deep = true)
    {
        GetMembers<FoImage2D>()?.ForEach(item => item.CollectImages(list, deep));
        //GetMembers<FoShape2D>()?.ForEach(item => item.CollectImages(list, deep));
        return list;
    }

    public override List<FoVideo2D> CollectVideos(List<FoVideo2D> list, bool deep = true)
    {
        //GetMembers<FoShape2D>()?.ForEach(item => item.CollectVideos(list, deep));
        return list;
    }
}
