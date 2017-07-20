using System;

namespace Rock.Slingshot.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class FinancialPledgeImport
    {
        /// <summary>
        /// Gets or sets the financial pledge foreign identifier.
        /// </summary>
        /// <value>
        /// The financial pledge foreign identifier.
        /// </value>
        public int FinancialPledgeForeignId { get; set; }

        /// <summary>
        /// Gets or sets the person foreign identifier.
        /// </summary>
        /// <value>
        /// The person foreign identifier.
        /// </value>
        public int PersonForeignId { get; set; }

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        public int? FinancialAccountForeignId { get; set; }

        /// <summary>
        /// If a person belongs to one or more groups a particular type (i.e. Family), this field 
        /// is used to distinguish which group the pledge should be associated with.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupForeignId { get; set; }

        /// <summary>
        /// Gets or sets the pledge amount that is promised to be given at the specified <see cref="PledgeFrequencyValue"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the amount to be pledged at the specified frequency.
        /// </value>
        /// <remarks>
        /// An example is that a person pledges $100.00 to be given monthly for the next year. This value will be $100.00 and the grand total of the pledge would be $1,200.00
        /// </remarks>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the pledge frequency <see cref="Rock.Model.DefinedValue" /> representing how often the pledgor is promising to give the pledge amount.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32" /> representing the pledge frequency <see cref="Rock.Model.DefinedValue"/>.
        /// </value>
        public int? PledgeFrequencyValueId { get; set; }

        /// <summary>
        /// Gets or sets the start date of the pledge period.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the start date of the pledge period.
        /// </value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the pledge period.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the end date of the pledge period.
        /// </value>
        public DateTime EndDate { get; set; }
    }
}
