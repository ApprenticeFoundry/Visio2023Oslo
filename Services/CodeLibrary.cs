namespace Visio2023Foundry.Model
{
public class CodeLibrary
{

    public string Name { get; set; } = "";
    public string Storage { get; set; } = "";
    public List<CodeManifest> Manifests { get; set; } = new();
}
}