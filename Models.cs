using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Sicl5300.Tools.Committer;

public sealed class BlogFrontMatter
{
    [YamlMember(Alias = "title")]
    public string Title { get; set; } = string.Empty;
}

public sealed class Settings
{
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("attrs")]
    public List<FileAttr>? Attrs { get; set; } = new();

    [JsonPropertyName("repo")]
    public string Repository { get; set; } = string.Empty;
}

public sealed class FileAttr
{
    public string Title { get; set; } = string.Empty;

    public long UpdateTime { get; set; } = 0;

    [JsonIgnore] public string UpdateTimeReadable { get; set; } = string.Empty;
}