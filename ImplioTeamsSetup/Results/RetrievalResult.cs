using System.Collections.Generic;

namespace ImplioTeamsSetup.Results
{
    public class RetrievalResult<T> where T: class, new()
    {
        public List<T> Results { get; set; }
    }
}
