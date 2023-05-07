namespace Visio2023Foundry.Model;

#pragma warning disable CS8602

public class Import_Drawing
{
    public string GUID { get; set; }
    public string ParentGUID { get; set; }
    public string BOMPath { get; set; }  
    public string Type { get; set; }  
    public string Level { get; set; }  
    public string Name { get; set; }  
    public string Text { get; set; }  
    public string Details { get; set; }  
    public string Shape { get; set; } 

    public string Width { get; set; }
    public string Height { get; set; }

    public string PinX { get; set; }
    public string PinY { get; set; }

    public string Color { get; set; }
#pragma warning disable CS8618
    public Import_Drawing()
    {

    }
#pragma warning restore CS8618
}

#pragma warning restore CS8602