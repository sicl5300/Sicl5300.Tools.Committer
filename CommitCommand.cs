using System.Diagnostics;
using System.Runtime.CompilerServices;
using LibGit2Sharp;
using Spectre.Cli;
using Spectre.Console;

namespace Sicl5300.Tools.Committer;

internal sealed class CommitCommand : Command<CommitCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
    }

    private static readonly Dictionary<string, string> TypePrompts = new()
    {
        { "🎨 [grey]art[/] Code formatting | improve vo", "art" },
        { "🚧 [grey]construction[/] Infrastructural changes of Hexo blogging", "construction" },
        { "🆕 [grey]new[/] New article", "new" },
        { "🆙 [grey]up[/] Update article", "up" },
        { "🔧 [grey]wrench[/] Fix bugs", "wrench" },
        { "⏪ [grey]rewind[/] Revert changes", "rewind" },
        { "⬇️ [grey]arrow_down[/] Downgrade dependencies ( node )", "arrow_down" },
        { "⬆️ [grey]arrow_up[/] Upgrade dependencies ( node )", "arrow_up" },
        { "➕ [grey]heavy_plus_sign[/] Add a dependency ( node )", "heavy_plus_sign" },
        { "➖ [grey]heavy_minus_sign[/] Remove a dependency ( node )", "heavy_minus_sign" },
        { "📝 [grey]memo[/] Documents", "memo" },
        { "🔀 [grey]twisted_rightwards_arrows[/] Merge branches", "twisted_rightwards_arrows" },
        { "🎭 [grey]performing_arts[/] Special modifications ( usually to theme )", "performing_arts" },
        { "🔴 [grey]red_circle[/] Imperative modifications | Breaking changes", "red_circle" },
        { "❎ [grey]negative_squared_cross_mark[/] Imperative deletions", "negative_squared_cross_mark" }
    };

    private static IEnumerable<FileAttr> NewerPosts
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var posts = PostGetter.GetNewerPosts();
            posts.Add(DefaultNoTitle);

            var sorted_posts = posts.OrderByDescending(x => x.UpdateTime);
            return sorted_posts;
        }
    }

    private const string No = "[I Don't Want To Use Title]";

    private static readonly FileAttr DefaultNoTitle = new FileAttr
    {
        Title = No,
        UpdateTime = long.MaxValue,
        UpdateTimeReadable = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
    };
    public override int Execute(CommandContext context, Settings settings)
    {
                var type = AnsiConsole.Prompt(
            new SelectionPrompt<KeyValuePair<string, string>>()
                .Title("Select your [aqua]commit type[/]:")
                .PageSize(8)
                .AddChoices(TypePrompts)
                .UseConverter(pair => pair.Key)
        ).Value;

        var inner_message = type switch
        {
            "new" or "up" => AnsiConsole.Prompt(
                    new SelectionPrompt<FileAttr>()
                        .Title("Select [aqua]post title[/]:")
                        .PageSize(8)
                        .AddChoices(NewerPosts)
                        .UseConverter(attr => $"{Markup.Escape(attr.Title)}, [green3]{attr.UpdateTimeReadable}[/]")
                ).Title switch
                {
                    No => AnsiConsole.Ask<string>(
                        "You do not want to use title as your commit message, please enter in [aqua]manually[/]:"),
                    var msg => msg
                },
            _ => AnsiConsole.Ask<string>("Enter your [aqua]commit message[/]:"),
        };

        var skip = AnsiConsole.Confirm("Do you want to [aqua]skip CI[/]?", false)
            ? " [skip ci]"
            : string.Empty;

        var commit_message = $":{type}: {inner_message}{skip}";

        AnsiConsole.WriteLine(Markup.Escape(commit_message));

        if (AnsiConsole.Confirm("Do you want to [aqua]commit your changes[/]?", false))
        {
            using var repo = new Repository(Program.Config.Repository);
            var status = repo.RetrieveStatus()
                .Where(x => x.State is not FileStatus.Ignored);

            if (!status.Any())
            {
                AnsiConsole.WriteLine("[red]Nothing changed, nothing to commit.[/]");
                return 1;
            }

            var filesToAdd = AnsiConsole.Prompt(new MultiSelectionPrompt<StatusEntry>()
                .Title("Add files to git")
                .PageSize(8)
                .AddChoices(status)
                .UseConverter(entry => entry.FilePath)
            ).Select(x => x.FilePath);

            foreach (var item in filesToAdd) repo.Index.Add(item);
            repo.Index.Write();

            Signature author = repo.Config.BuildSignature(DateTime.Now);
            repo.Commit(commit_message, author, author);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "git",
                    Arguments = "commit --amend --no-edit",
                    WorkingDirectory = Program.Config.Repository
                }
            };
            process.Start();
        }

        return 0;
    }
}