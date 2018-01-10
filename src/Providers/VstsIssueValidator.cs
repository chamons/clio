using System.Threading.Tasks;
using clio.Model;

namespace clio.Providers
{
    public class VstsIssueValidator : BaseIssueValidator
    {
        public VstsIssueValidator(SearchOptions options) : base(IssueSource.Vsts, options)
        {
            //Client = new BugzillaClient (new Uri (@"https://bugzilla.xamarin.com/jsonrpc.cgi"));
        }

        protected override async Task SetupAsync()
        {
        }

        public override async Task<IIssue> GetIssueAsync(int issueId)
        {
            return null;
        }

    }
}
