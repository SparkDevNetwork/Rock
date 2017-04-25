using System;

namespace Rock.Slingshot.Model
{
    public class FinancialBatchImport
    {
        /// <summary>
        /// Gets or sets the financial batch foreign identifier.
        /// </summary>
        /// <value>
        /// The financial batch foreign identifier.
        /// </value>
        public int FinancialBatchForeignId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public BatchStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public enum BatchStatus
        {
            /// <summary>
            /// Pending
            /// In the process of scanning the checks to it
            /// </summary>
            Pending = 0,

            /// <summary>
            /// Open
            /// Transactions are all entered and are ready to be matched
            /// </summary>
            Open = 1,

            /// <summary>
            /// Closed
            /// All is well and good
            /// </summary>
            Closed = 2
        }

        /// <summary>
        /// Gets or sets the control amount.
        /// </summary>
        /// <value>
        /// The control amount.
        /// </value>
        public decimal ControlAmount { get; set; }

        /// <summary>
        /// Gets or sets the created by person identifier.
        /// </summary>
        /// <value>
        /// The created by person identifier.
        /// </value>
        public int? CreatedByPersonForeignId { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified by person identifier.
        /// </summary>
        /// <value>
        /// The modified by person identifier.
        /// </value>
        public int? ModifiedByPersonForeignId { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }
    }
}
