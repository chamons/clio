using clio.Model;
using CodeRinseRepeat.Bugzilla;

namespace clio
{
    public sealed class BugzillaIssue : IIssue
    {
        public BugzillaIssue(Bug bug)
        {
            this.Id = (int)bug.Id;
            this.Title = bug.Summary;
            this.MoreInfo = $"({bug.Product}) - {bug.Milestone} {bug.Status}";
        }

        public int Id { get; }

        public string Title { get; }

        public string MoreInfo { get; }
    }
}
