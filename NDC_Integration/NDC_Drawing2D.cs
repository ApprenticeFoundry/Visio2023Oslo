using BlazorComponentBus;
using FoundryBlazor.Shape;
using Visio2023Foundry.Shape;

namespace Visio2023Foundry.Model;

public class NDC_Drawing2D : FoDrawing2D
{

    public NDC_Drawing2D(
        IPanZoomService panzoom,
        ISelectionService select,
        IPageManagement manager,
        IHitTestService hittest,
        ComponentBus pubSub
        ) : base(panzoom,select,manager,hittest,pubSub)
    {

        
        if ( interactionLookup.TryGetValue(InteractionStyle.ShapeConnecting, out BaseInteraction? obj)  ) {
            if ( obj is ShapeConnecting tool)
            {
                tool.SourceType = typeof(FoHero2D);
                tool.TargetType = typeof(FoHero2D);
            }

        }
    }



}
