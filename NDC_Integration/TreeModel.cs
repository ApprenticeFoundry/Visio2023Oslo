namespace Visio2023Foundry.Model;

public class TreeModel
{
    public bool IsExpanded { get; set; } = true;
    public string ComponentName { get; set; } = "";
    public string ClassName { get; set; } = "";

    private readonly List<TreeModel> Members = new();
    public List<TreeModel> Children() 
    { 
        return Members; 
    }
    public TreeModel AddChild(TreeModel child)
    {
        Members.Add(child);
        return this;
    }
    public TreeModel AddChild(string comp, string cls)
    {
        var node = new TreeModel()
        {
            ComponentName = comp,
            ClassName = cls
        };
        AddChild(node);
        return node;
    }
    public TreeModel Add(string comp, string cls)
    {
        var node = new TreeModel()
        {
            ComponentName = comp,
            ClassName = cls
        };
        return AddChild(node);
    }

}
