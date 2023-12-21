using System.Text;
using System.Text.Json;
using Spectre.Console.Cli;

namespace Sicl5300.Tools.Committer;

public static class Program
{
    public const string SettingsLoc = "./settings.json";
    public static Settings Config { get; private set; } = null!;

    public static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Config = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsLoc)) ??
                 throw new InvalidOperationException("no settings.json");

        var app = new CommandApp();
        app.Configure(config =>
        {
            config.AddCommand<CommitCommand>("commit");
        });
        return app.Run(args);
    }
}