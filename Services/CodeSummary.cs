
using FoundryBlazor.Extensions;

namespace Visio2023Foundry.Model;

public class CodeSummary
{
    public string What { get; set; } = "";
    public string Why { get; set; } = "";
    public string How { get; set; } = "";
    public string Filename { get; set; } = "";
    public string ImageURL { get; set; } = "";
    public string DemoURL { get; set; } = "";
    public string MemeURL { get; set; } = "";

    public void ModifyImageUrl(string path)
    {
        if ( !string.IsNullOrEmpty(ImageURL) && ImageURL.Contains("{Path}"))
        {
            ImageURL = ImageURL.Replace("{Path}", path);
            $"Replace ImageURL {ImageURL}".WriteSuccess();
        } else if ( !string.IsNullOrEmpty(ImageURL) ) {
            $"ImageURL {ImageURL}".WriteInfo();
        }
    }

        public void ModifyMemeUrl(string path)
    {
        if ( !string.IsNullOrEmpty(MemeURL) && MemeURL.Contains("{Path}"))
        {
            MemeURL = MemeURL.Replace("{Path}", path);
            $"Replace ImageURL {MemeURL}".WriteSuccess();
        } else if ( !string.IsNullOrEmpty(MemeURL) ) {
            $"ImageURL {MemeURL}".WriteInfo();
        }
    }
}
