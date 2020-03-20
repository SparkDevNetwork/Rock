using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a report is run.
    /// </summary>
    public class RunReportTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the report identifier.
        /// </summary>
        /// <value>
        /// The report identifier.
        /// </value>
        public int ReportId { get; set; }

        /// <summary>
        /// Gets or sets the last run date.
        /// </summary>
        /// <value>
        /// The last run date.
        /// </value>
        public DateTime? LastRunDateTime { get; set; }

        /// <summary>
        /// The amount of time in milliseconds that it took to run the <see cref="DataView"/>
        /// </summary>
        /// <value>
        /// The time to run in ms.
        /// </value>
        public int? TimeToRunMS { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunReportTransaction"/> class.
        /// </summary>
        public RunReportTransaction()
        {
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var reportService = new ReportService( rockContext );
                var report = reportService.Get( ReportId );

                if ( report == null )
                {
                    return;
                }

                if ( LastRunDateTime != null )
                {
                    report.LastRunDateTime = LastRunDateTime;
                    report.RunCount = ( report.RunCount ?? 0 ) + 1;
                }

                // We will only update the RunCount if we were given a TimeToRun value.
                if ( TimeToRunMS != null )
                {
                    report.TimeToRunMS = TimeToRunMS;
                }

                rockContext.SaveChanges();
            }
        }
    }
}