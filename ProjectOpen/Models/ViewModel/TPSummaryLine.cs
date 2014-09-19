using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectOpen.Models.ViewModel
{
    public class TPSummaryLine
    {
        public string TeamName { get; set; }
        public string FieldType { get; set; }
        public List<string> FieldCount { get; set; }
    }
}