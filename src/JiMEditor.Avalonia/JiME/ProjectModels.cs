// Ported from: JiMEditor/Common/Common.cs — ProjectType, ProjectItem (2026-04-04)

namespace JiME;

public enum ProjectType
{
	Standalone,
	Campaign
}

public class ProjectItem
{
	public string Title { get; set; } = "";
	public string Date { get; set; } = "";
	public string Description { get; set; } = "";
	public ProjectType projectType { get; set; }
	public string fileName { get; set; } = "";
	public string fileVersion { get; set; } = "";
	public string collectionIcons { get; set; } = "";
}
