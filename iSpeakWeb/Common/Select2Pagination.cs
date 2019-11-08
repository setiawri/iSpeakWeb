using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSpeak.Common
{
    public class Select2Pagination
    {
        public class Select2Results
        {
            public string id { get; set; }
            public string text { get; set; }
            public string info1 { get; set; }
            public string info2 { get; set; }
            public string info3 { get; set; }
        }

        public class Select2Page
        {
            public bool more { get; set; }
        }
    }
}