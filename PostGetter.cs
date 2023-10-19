using System.Text.Json;

namespace Sicl5300.Tools.Committer;

internal static class PostGetter
{
    public static List<FileAttr> GetNewerPosts()
    {
        var allFiles = Directory
            .GetFiles(Program.Config.Location, "*.md", SearchOption.AllDirectories);

        Dictionary<string, long> times = Program.Config.Attrs!.ToDictionary(x => x.Title, x => x.UpdateTime);

        List<FileAttr> r = allFiles.Select(GetAttrByPath).ToList();

        Program.Config.Attrs = r;
        File.WriteAllText(Program.SettingsLoc, JsonSerializer.Serialize(Program.Config));

        // return newer titles
        return r.Where(x =>
            {
                if (times.TryGetValue(x.Title, out var savedTime))
                {
                    return x.UpdateTime != savedTime;
                }

                // no title matched, considered as new post.
                return true;
            })
            .ToList();
    }

    private static FileAttr GetAttrByPath(string path_absolute)
    {
        var t = File.GetLastWriteTime(path_absolute);
        return new()
        {
            Title = File.ReadAllText(path_absolute).GetFrontMatter<BlogFrontMatter>().Title,
            UpdateTime = t.ToFileTime(),
            UpdateTimeReadable = t.ToString("yyyy/MM/dd HH:mm:ss")
        };
    }
}