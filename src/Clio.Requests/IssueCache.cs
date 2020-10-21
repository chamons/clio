using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Octokit;

using Clio.Utilities;

// Unable to serialize Octokit Issues due to https://github.com/octokit/octokit.net/issues/1762
// so wrap fields we care about in a record
public record ClioIssue (int ID, int Number, string Title, string Body, string Url, System.DateTimeOffset? ClosedAt, List<string> Labels);

public static class IssueCache
{
    static (string Owner, string Area) ParseLocation (string location)
    {
        var bits = location.Split ('/');
        if (bits.Length != 2)
            Errors.Die ("--github formatted incorrectly");
        return (bits[0], bits[1]);
    }

    public static async Task<IReadOnlyList<ClioIssue>> GetIssues (GitHubClient client, string location, bool useCache)
    {
        if (useCache && File.Exists ("clio.cache")) {
            return JsonConvert.DeserializeObject<IReadOnlyList<ClioIssue>> (File.ReadAllText("clio.cache"));
        }

        var (owner, area) = ParseLocation (location);
		var allIssues = await client.Issue.GetAllForRepository (owner, area, new RepositoryIssueRequest { State = ItemStateFilter.All });

        var allClioIssues = allIssues.Select(x => new ClioIssue(x.Id, x.Number, x.Title, x.Body, x.Url, x.ClosedAt, x.Labels.Select(l => l.Name).ToList())).ToList();

        var cache = JsonConvert.SerializeObject(allClioIssues);
        File.WriteAllText ("clio.cache", cache);
        return allClioIssues;
    }
}