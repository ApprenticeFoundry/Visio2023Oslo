using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Canvas;
using FoundryBlazor.Shape;
using IoBTMessage.Models;

namespace Visio2023Foundry.Targets;

public enum ThreadShape2DType
{
    None,
    Composition,
    Classification,
}

public class ThreadShape2D : FoShape2D
{

    public ThreadShape2DType ThreadShape2DType { get; set; } = ThreadShape2DType.None;
    public string Title { get; set; } = "";
    public int TextTop { get; set; } = 8;
    private List<string>? TextList { get; set; }

    public ThreadShape2D() : base()
    {
        ShapeDraw = DrawRect;
    }

    //  public override void OnShapeClick(ClickStyle style, CanvasMouseArgs args)
    // {
    //     if ( Model != null && ClickStyle.MouseDown == style)
    //     {
    //         Model.IsExpanded = !Model.IsExpanded;
    //         this.Color =  Model.IsExpanded ? "Red" : "Blue";
    //     }
    // }

    public override FoGlyph2D MarkSelected(bool value)
    {
        var glue = GetMembers<FoGlue2D>();
            glue?.ForEach(item => item.MarkSelected(value));
            
        var pnts = GetMembers<FoConnectionPoint2D>();
            pnts?.ForEach(item => item.MarkSelected(value));

        return base.MarkSelected(value);
    }

    public ThreadShape2D TagAsComposition(DT_Target model)
    {
        ThreadShape2DType = ThreadShape2DType.Composition;
        //this.PostDraw = async (ctx,obj) => await this.DrawFancyPin(ctx);
        //Model = model;
        Color = "Green";
        Title = model.address;
        ResizeTo(190, 90);
        TextTop = this.Height / 4;
        var subShape = new ThreadShape2D
        {
            Color = "Yellow",
            Title = model.targetType,
            Width = this.Width,
            Height = this.Height / 2,
            PinX = this.Width / 2,
            PinY = 3 * this.Height / 4,
            TextTop = this.Height / 4,
            //PostDraw = async (ctx, obj) => await this.DrawPin(ctx)
        };
        this.Add<ThreadShape2D>(subShape);
        return this;
    }

    public ThreadShape2D TagAsClassification(DT_Target model)
    {
        ThreadShape2DType = ThreadShape2DType.Classification;
        //Model = model;
        //Tag = model.GetType().Name;
        Color = "Yellow";
        Title = model.address;
        ResizeTo(220, 60);
        TextTop = this.Height / 2;
        //this.PostDraw = async (ctx,obj) => await this.DrawFancyPin(ctx);
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
            AddConnectionPoint2D(new FoConnectionPoint2D("LEFT", lx, cy, "black"));
            AddConnectionPoint2D(new FoConnectionPoint2D("TOP", cx, ty, "black"));
            AddConnectionPoint2D(new FoConnectionPoint2D("RIGHT", rx, cy, "black"));
            AddConnectionPoint2D(new FoConnectionPoint2D("BOTTOM", cx, by, "black"));
        }
        var result = this.Members<FoConnectionPoint2D>();
        return result;
    }

    public List<string> PreSizeToFitText(string title)
    {
        if ( TextList != null ) 
            return TextList;

        TextList = CreateTextList(title, 24);
        if ( TextList.Count > 4 )
            Height = 28 * TextList.Count;
            
        return TextList;
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
            await ctx.SetTextBaselineAsync(TextBaseline.Middle);

            await ctx.SetFillStyleAsync("Black");

            var top = TextTop;

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
        if ( !IsVisible ) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await DrawDetails(ctx, tick);
        if ( !IsSelected )
            HoverDraw?.Invoke(ctx, this);
            
        await DrawTag(ctx);

        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        if (deep)
        {
            GetMembers<ThreadShape2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        }
        await ctx.RestoreAsync();
        return true;
    }

}
