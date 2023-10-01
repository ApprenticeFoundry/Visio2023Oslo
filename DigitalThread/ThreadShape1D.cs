using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shared;
using FoundryBlazor.Shape;
using IoBTMessage.Models;

namespace Visio2023Foundry.Targets;

public enum ThreadShape1DType
{
    None,
    Composition,
    Classification,
}

public class ThreadShape1D : FoShape1D
{

    public ThreadShape1DType ThreadShape1DType { get; set; } = ThreadShape1DType.None;

    public ThreadShape1D() : base()
    {
    }
    public ThreadShape1D TagAsComposition(DT_TargetLink model)
    {
        ThreadShape1DType = ThreadShape1DType.Composition;
        Color = "Black";
        return this;
    }
}
