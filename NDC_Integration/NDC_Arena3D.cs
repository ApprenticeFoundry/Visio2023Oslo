using BlazorComponentBus;
using FoundryBlazor.Shape;
using Visio2023Foundry.Shape;

namespace Visio2023Foundry.Model;

public class NDC_Arena3D : FoArena3D
{
    public NDC_Arena3D(
        IScaledArena scaled,
        IStageManagement manager,
        ComponentBus pubSub): base(scaled, manager, pubSub)
    {

    }
}
