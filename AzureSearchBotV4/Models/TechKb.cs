using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSearchBotV4.Models
{
    public class TechKb
    {
        public string Id { get; set; }

        public string KbId { get; set; }

        public string Question { get; set; }

        public string QuickSteps { get; set; }

        public string DetailedSteps { get; set; }
    }
}
