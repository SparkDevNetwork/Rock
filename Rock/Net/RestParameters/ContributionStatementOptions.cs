using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Net.RestParameters
{
    public class ContributionStatementOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> AccountIds { get; set; }
        public int? PersonId { get; set; }
        public bool OrderByZipCode { get; set; }
    }
}
