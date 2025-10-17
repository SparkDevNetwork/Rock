using System;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill
{

    /// <summary>
    /// Query option container used when building the base transaction query.
    /// </summary>
    internal class FinancialTransactionQueryOptions
    {
        /// <summary>
        /// Person Id filter.
        /// </summary>
        public int? PersonId { get; set; }
        /// <summary>
        /// Campus Id (from Batch.CampusId).
        /// </summary>
        public int? BatchCampusId { get; set; }
        /// <summary>
        /// Payment method (Defined Value) filter.
        /// </summary>
        public int? PaymentMethodTypeId { get; set; }
        /// <summary>
        /// Inclusive start date.
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Inclusive end date.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
