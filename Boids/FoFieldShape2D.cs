using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shape;
using IoBTMessage.Models;

namespace Visio2023Foundry.Boids;

public class FoFieldShape2D : FoShape2D
{
    public Action<Canvas2DContext>? DrawSimulation { get; set; }
    public FoFieldShape2D() : base()
    {
        ShapeDraw = DrawBox;
        LocPinX = (obj) => 0;
        LocPinY = (obj) => 0;
    }

 
    public FoFieldShape2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
        PinX = PinY = 0;
        ShapeDraw = DrawBox;
        LocPinX = (obj) => 0;
        LocPinY = (obj) => 0;
    }



    public async Task DrawDetails(Canvas2DContext ctx, int tick)
    {
        await ctx.SaveAsync();
        if ( ShouldRender ) 
        {
            ShapeDraw?.Invoke(ctx, this);
            DrawSimulation?.Invoke(ctx);
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


        await DrawDetails(ctx, tick);
        if ( !IsSelected )
            HoverDraw?.Invoke(ctx, this);
            

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        await ctx.RestoreAsync();
        return true;
    }
}
