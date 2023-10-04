using System;

namespace org.lakepointe.Finance.MoreThanUs
{
    public class PledgeImportItem
    {
        public int RowId { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public Decimal OneTimeGift { get; set; }
        public Decimal PledgeTotal { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

    }
}
