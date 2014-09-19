using SDD.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ProjectOpen.Models.ViewModel
{
    public class TeamSummary
    {
        public HeaderVM Header { get; set; }

        public DataSet Data { get; set; }
    }
}