using BlazorComponentBus;
using FoundryBlazor.Extensions;
using FoundryBlazor.PubSub;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;

namespace Visio2023Foundry.Pages;

public class Viewer3DBase : ComponentBase
{

    [Inject] private ComponentBus? PubSub { get; set; }



    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        InvokeAsync(StateHasChanged);
        $"Viewer3DBase OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }


}
