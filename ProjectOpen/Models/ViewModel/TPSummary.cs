using SDD.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectOpen.Models.ViewModel
{
    public class TPSummary
    {
        public HeaderVM Header { get; set; }

        public List<TPSummaryLine> lines { get; set; }

    }
}