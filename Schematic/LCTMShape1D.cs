using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shape;
using Visio2023Foundry.Model;

namespace Visio2023Foundry.Shape;

public class LCTMShape1D : FoShape1D
{
    public string Title { get; set; } = "";
    public string BOMPath { get; set; } = "";
    public Import_Connection Record { get; set; }

    public LCTMShape1D() : base()
    {
        Record = new Import_Connection();
    }

    public LCTMShape1D(string name, string color) : base(name, color)
    {
        Record = new Import_Connection();
    }

    public LCTMShape1D(FoGlyph2D start, FoGlyph2D finish, string color) : base(start, finish, 10, color)
    {
        Record = new Import_Connection();
    }



   public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (CannotRender()) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await Draw(ctx, tick);

        if (!IsSelected)
            HoverDraw?.Invoke(ctx, this);

        //await DrawTag(ctx);

        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        if (deep)
        {
            GetMembers<FoShape1D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
            GetMembers<FoShape2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        }

        // if (GetMembers<FoGlue2D>()?.Count > 0)
        //     await DrawTriangle(ctx, "Black");

        // await DrawStraight(ctx, "Red", tick);
        // await DrawStart(ctx, "Blue");
        // await DrawFinish(ctx, "Cyan");

        await ctx.RestoreAsync();
        return true;
    }




}
