using System;

namespace org.lakepointe.Finance.MoreThanUs
{
    public class PledgeFileConfiguration
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int FinancialAccountId { get; set; }
        public string PledgeSource { get; set; }
    }
}
