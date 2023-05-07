namespace Visio2023Foundry.Model;



public class Import_Connection
{
    public string GUID { get; set; }
    public string ParentGUID { get; set; }
    public string Type { get; set; }  
    public string Level { get; set; }  
    public string Name { get; set; }  
    public string SourceGuid { get; set; }  
    public string SinkGuid { get; set; }  
    public string SourceBOMPath { get; set; }  
    public string SinkBOMPath { get; set; } 

#pragma warning disable CS8618
    public Import_Connection()
    {

    }

#pragma warning restore CS8618
}
