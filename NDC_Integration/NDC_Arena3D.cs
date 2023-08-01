using BlazorComponentBus;
using FoundryBlazor.Shape;
using Visio2023Foundry.Shape;

namespace Visio2023Foundry.Model;

public class NDC_Arena3D : FoArena3D
{
    public NDC_Arena3D(
        IStageManagement manager,
        ComponentBus pubSub): base(manager, pubSub)
    {

    }
}
