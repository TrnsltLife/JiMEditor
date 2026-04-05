// Ported from: JiMEditor/Common/FileManager.cs — GetProjects() via lightweight JObject reads (no full Scenario/Campaign models) (2026-04-04)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Linq;

namespace JiME;

public static class ProjectFolderScanner
{
	static string? Tok(JObject jo, params string[] names)
	{
		foreach (var n in names)
		{
			var t = jo[n];
			if (t != null && t.Type != JTokenType.Null)
				return t.Value<string>();
		}
		return null;
	}

	static readonly HashSet<string> ReservedCampaignDirs = new(StringComparer.OrdinalIgnoreCase)
	{
		"Exports", "Saves", "Skins", "Languages", "Tiles"
	};

	/// <summary>Matches JiME Collection.ID → FontCharacter for list icons (see JiMEditor Models/Collection.cs).</summary>
	static readonly (int Id, string Icon)[] CollectionIconOrder =
	{
		(1, "r"), (2, "v"), (3, "p"), (4, "d"), (5, "w"), (6, "c")
	};

	public static IReadOnlyList<ProjectItem> GetProjects()
	{
		string basePath = UserJourneyPaths.GetBasePath();
		if (!Directory.Exists(basePath))
		{
			try
			{
				Directory.CreateDirectory(basePath);
			}
			catch
			{
				return Array.Empty<ProjectItem>();
			}
		}

		var items = new List<ProjectItem>();
		var di = new DirectoryInfo(basePath);

		foreach (var dInfo in di.GetDirectories())
		{
			if (ReservedCampaignDirs.Contains(dInfo.Name))
				continue;

			string jsonPath = Path.Combine(basePath, dInfo.Name, dInfo.Name + ".json");
			if (!File.Exists(jsonPath))
				continue;

			try
			{
				var jo = JObject.Parse(File.ReadAllText(jsonPath));
				items.Add(new ProjectItem
				{
					projectType = ProjectType.Campaign,
					Title = Tok(jo, "campaignName", "CampaignName") ?? dInfo.Name,
					fileVersion = Tok(jo, "fileVersion", "FileVersion") ?? "",
					fileName = dInfo.Name,
					Date = File.GetLastWriteTime(jsonPath).ToString("M/d/yyyy", CultureInfo.InvariantCulture),
					collectionIcons = BuildCampaignCollectionIcons(Path.Combine(basePath, dInfo.Name), jo)
				});
			}
			catch
			{
				// skip unreadable campaign folder
			}
		}

		foreach (var fi in di.GetFiles("*.jime"))
		{
			try
			{
				var jo = JObject.Parse(File.ReadAllText(fi.FullName));
				items.Add(new ProjectItem
				{
					Title = Tok(jo, "scenarioName", "ScenarioName") ?? fi.Name,
					projectType = ReadProjectType(jo),
					Date = Tok(jo, "saveDate", "SaveDate") ?? "",
					fileName = fi.Name,
					fileVersion = Tok(jo, "fileVersion", "FileVersion") ?? "",
					collectionIcons = IconsFromCollectionArray(
						(jo["collections"] ?? jo["Collections"]) as JArray)
				});
			}
			catch
			{
				// skip corrupt file
			}
		}

		return items;
	}

	static ProjectType ReadProjectType(JObject jo)
	{
		var t = jo["projectType"] ?? jo["ProjectType"];
		if (t == null)
			return ProjectType.Standalone;
		if (t.Type == JTokenType.Integer)
			return t.Value<int>() == (int)ProjectType.Campaign ? ProjectType.Campaign : ProjectType.Standalone;
		if (t.Type == JTokenType.String && Enum.TryParse<ProjectType>(t.Value<string>(), true, out var parsed))
			return parsed;
		return ProjectType.Standalone;
	}

	static string IconsFromCollectionArray(JArray? arr)
	{
		if (arr == null || arr.Count == 0)
			return "";
		var set = new HashSet<int>();
		foreach (var id in arr)
		{
			if (id?.Type == JTokenType.Integer)
				set.Add(id.Value<int>());
		}
		return IconsFromSet(set);
	}

	static string IconsFromSet(HashSet<int> ids)
	{
		var chars = new List<string>();
		foreach (var (id, icon) in CollectionIconOrder)
		{
			if (ids.Contains(id))
				chars.Add(icon);
		}
		return string.Concat(chars);
	}

	static string BuildCampaignCollectionIcons(string campaignDir, JObject campaignJo)
	{
		var ids = new HashSet<int>();
		var sc = (campaignJo["scenarioCollection"] ?? campaignJo["ScenarioCollection"]) as JArray;
		if (sc != null)
		{
			foreach (var item in sc)
			{
				if (item is not JObject sObj)
					continue;
				var fn = Tok(sObj, "fileName", "FileName");
				if (string.IsNullOrEmpty(fn))
					continue;
				var path = Path.Combine(campaignDir, fn);
				MergeCollectionsFromJime(path, ids);
			}
		}
		else
		{
			foreach (var path in Directory.GetFiles(campaignDir, "*.jime"))
				MergeCollectionsFromJime(path, ids);
		}

		return IconsFromSet(ids);
	}

	static void MergeCollectionsFromJime(string path, HashSet<int> ids)
	{
		if (!File.Exists(path))
			return;
		try
		{
			var jo = JObject.Parse(File.ReadAllText(path));
			var arr = (jo["collections"] ?? jo["Collections"]) as JArray;
			if (arr == null)
				return;
			foreach (var id in arr)
			{
				if (id?.Type == JTokenType.Integer)
					ids.Add(id.Value<int>());
			}
		}
		catch
		{
			// ignore
		}
	}
}
