using FoundryBlazor.Extensions;

namespace Visio2023Foundry.Model;

public class CodeManifest
{
    public string Folder { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Tip { get; set; } = "";
    public string ImageURL { get; set; } = "";
    public string DemoURL { get; set; } = "";
    public string MemeURL { get; set; } = "";
    public string Status { get; set; } = "................";
    public string Category { get; set; } = "";
    public CodeSummary? Summary { get; set; } 
    public List<CodeSample> Samples { get; set; } = new();

    public void ModifyImageUrl(string path)
    {
        if ( !string.IsNullOrEmpty(ImageURL) && ImageURL.Contains("{Path}"))
        {
            ImageURL = ImageURL.Replace("{Path}", path);
            $"Replace ImageURL {ImageURL}".WriteSuccess();
        } else if ( !string.IsNullOrEmpty(ImageURL) ) {
            $"ImageURL {ImageURL}".WriteInfo();
        }

        foreach (var sample in Samples)
        {
            sample.ModifyImageUrl(path);
        }
    }

    public bool HasTip() 
    {
        return !string.IsNullOrEmpty(this.Tip);
    }

    public void ModifyMemeUrl(string path)
    {
        if ( !string.IsNullOrEmpty(MemeURL) && MemeURL.Contains("{Path}"))
        {
            MemeURL = MemeURL.Replace("{Path}", path);
            $"Replace MemeURL {MemeURL}".WriteSuccess();
        } else if ( !string.IsNullOrEmpty(MemeURL) ) {
            $"MemeURL {MemeURL}".WriteInfo();
        }

        foreach (var sample in Samples)
        {
            sample.ModifyMemeUrl(path);
        }
    }
}
