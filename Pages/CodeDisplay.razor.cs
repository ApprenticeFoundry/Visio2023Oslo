using System.Net;
using BlazorComponentBus;
using Foundry.Helpers;
using FoundryBlazor.Extensions;
using FoundryBlazor.PubSub;
using Microsoft.AspNetCore.Components;
using Visio2023Foundry.Model;
// using Microsoft.JSInterop;
using BlazorMonaco;
using BlazorMonaco.Editor;
using IoBTModules.Extensions;

namespace Visio2023Foundry.Pages;

public class CodeDisplayBase : ComponentBase, IDisposable
{
    [Inject] private ICodeDisplayService? CodeDisplay { get; set; }

    [Parameter] public string? Folder { get; set; }
    [Parameter] public string? Filename { get; set; }
    [Parameter] public string? GUID { get; set; }

    public StandaloneCodeEditor _editor = null!;

    public CodeSample? Sample { get; set; }
    public string Code { get; set; } = "";

    public bool HasImage()
    {
        //$"ImageURL =[{Sample!.ImageURL}]".WriteInfo();
        return !string.IsNullOrEmpty(Sample?.ImageURL);
    }
    public bool HasMeme()
    {
        //$"ImageURL =[{Sample!.ImageURL}]".WriteInfo();
        return !string.IsNullOrEmpty(Sample?.MemeURL);
    }
    public bool HasDemo()
    {
        // $"DemoURL =[{Sample!.DemoURL}]".WriteInfo();
        return !string.IsNullOrEmpty(Sample?.DemoURL);
    }
    public bool HasTip()
    {
        // $"DemoURL =[{Sample!.DemoURL}]".WriteInfo();
        return !string.IsNullOrEmpty(Sample?.Tip);
    }
    public bool HasCode()
    {
        // $"DemoURL =[{Sample!.DemoURL}]".WriteInfo();
        return !string.IsNullOrEmpty(Code);
    }


    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Code = "";
        var data = CodeDisplay!.GetCodeLibrary();
        if (!string.IsNullOrEmpty(Folder) && !string.IsNullOrEmpty(Filename))
        {
            var manifest = data.Manifests.FirstOrDefault(x => x.Folder.Matches(Folder));
            Sample = manifest!.Samples.FirstOrDefault(x => x.Filename.Matches(Filename));

            var path = System.IO.Path.Combine(data.Storage, Folder, Filename);
            if (File.Exists(path))
            {
                try
                {
                    using var streamReader = new StreamReader(path);
                    Code = await streamReader.ReadToEndAsync();
                }
                catch (Exception ex)
                {
                    ex.Message.WriteError();
                }
            }
            else
            {
                $"Code Sample File {path} not found".WriteError();
            }
        }
        else if (!string.IsNullOrEmpty(Folder) && !string.IsNullOrEmpty(GUID))
        {
            var manifest = data.Manifests.FirstOrDefault(x => x.Folder.Matches(Folder));
            Sample = manifest!.Samples.FirstOrDefault(x => x.GUID.Matches(GUID));
            Code = "";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);

            await Global.DefineTheme("my-custom-theme", new StandaloneThemeData
            {
                Base = "vs-dark",
                Inherit = true,

                Rules = new List<TokenThemeRule>
                {
                    // new TokenThemeRule { Background = "363636", Foreground = "E0E0E0" },
                    // new TokenThemeRule { Token = "keyword", Foreground = "59ADFF" },
                    // new TokenThemeRule { Token = "operator.sql", Foreground = "59ADFF" },
                    // new TokenThemeRule { Token = "number", Foreground = "66CC66" },
                    // new TokenThemeRule { Token = "string.sql", Foreground = "E65C5C" },
                    // new TokenThemeRule { Token = "comment", Foreground = "7A7A7A" },
                    new TokenThemeRule { FontStyle = "size: 48pt" }
                },
                Colors = new Dictionary<string, string>
                {
                }
            });
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            ReadOnly = true,
            //Theme = "vs-dark", //"my-custom-theme",
            FontSize = 24,
            AutomaticLayout = false,
            //Language = Sample.Language, // "typescript",
            Language = "typescript",
            Theme = "hc-black",
            StablePeek = false,
            Value = Code
        };
    }

    public async Task EditorOnDidInit()
    {
        // await _editor.AddCommand((int)KeyMod.CtrlCmd | (int)KeyCode.KeyH, (args) =>
        // {
        //     Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
        // });

        //await Task.CompletedTask;

       //_editor.CssClass = "my-editor-class";

        // var h = await _editor.GetContentHeight();
        // $"height={h}".WriteInfo();

        await _editor.Layout();

        // decorationIds = await _editor.DeltaDecorations(null, newDecorations);
        // You can now use 'decorationIds' to change or remove the decorations
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

