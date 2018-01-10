using System.Threading.Tasks;
using clio.Model;

namespace clio.Providers
{
    public class VstsChecker : BaseIssueValidator
    {
        public VstsChecker(SearchOptions options) : base(IssueSource.Vsts, options)
        {
            //Client = new BugzillaClient (new Uri (@"https://bugzilla.xamarin.com/jsonrpc.cgi"));
        }

        public override async Task SetupAsync()
        {
        }

        protected override async Task<IIssue> GetIssueAsync(ParsedCommit commit)
        {
            return null;
        }

    }
}
