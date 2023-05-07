using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryBlazor.PubSub;
using FoundryBlazor.Solutions;
using FoundryBlazor.Extensions;

namespace Visio2023Foundry.Pages;

public class Viewer2DBase : ComponentBase
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
        $"Viewer2DBase OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }  
}
