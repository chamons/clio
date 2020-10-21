using System.Collections.Generic;
using System.Linq;

using Octokit;

namespace Clio.Requests
{
    public class RequestCollection
    {
        public List<ClioIssue> AllIssues;

        public List <RequestInfo> All = new List<RequestInfo> ();

        public IEnumerable<RequestInfo> Bugs => All.Where (x => x.Labels.Contains ("bug"));
        public IEnumerable<RequestInfo> Enhancements => All.Where (x => x.Labels.Contains ("enhancement"));
        public IEnumerable<RequestInfo> Infrastructure => All.Where (x => x.Labels.Contains ("note-infrastructure"));
        
        public IEnumerable<RequestInfo> Highlights => All.Where (x => x.Labels.Contains ("note-highlight"));
        public IEnumerable<RequestInfo> Breaking => All.Where (x => x.Labels.Contains ("breaking-change"));

        public RequestCollection (IEnumerable<ClioIssue> allIssues)
        {
            AllIssues = allIssues.ToList ();
        }

        public void Add (RequestInfo info)
        {
            All.Add (info);
        }
    }
}