using System.Collections.Generic;
using System.Linq;

namespace Clio.Requests
{
    public class RequestCollection
    {
        public List <RequestInfo> All = new List<RequestInfo> ();

        public RequestCollection ()
        {
        }

        public void Add (RequestInfo info)
        {
            All.Add (info);
        }
    }
}