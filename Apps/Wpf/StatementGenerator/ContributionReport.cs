using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.ReportWriter;
using ceTe.DynamicPDF.ReportWriter.Data;
using ceTe.DynamicPDF.ReportWriter.ReportElements;
using Rock.Model;
using Rock;
using Rock.Web.Cache;
using Rock.Net;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public class ReportOptions
    {
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the layout file.
        /// </summary>
        /// <value>
        /// The layout file.
        /// </value>
        public DplxFile LayoutFile { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TransactionRecord
    {
        public Person AuthorizedPerson { get; set; }
        public DateTime? TransactionDateTime { get; set; }
        public int? CurrencyTypeValueId { get; set; }
        public string Summary { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ContributionReport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContributionReport"/> class.
        /// </summary>
        public ContributionReport( ReportOptions options )
        {
            this.Options = options;
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public ReportOptions Options { get; set; }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="financialTransactionQry">The financial transaction qry.</param>
        /// <returns></returns>
        public Document RunReport()
        {
            // setup report layout and events
            DocumentLayout report = new DocumentLayout( this.Options.LayoutFile );
            Query query = report.GetQueryById( "OuterQuery" );
            query.OpeningRecordSet += mainQuery_OpeningRecordSet;

            FormattedRecordArea formattedRecordAreaToAddress = report.GetReportElementById( "lblToAddress" ) as FormattedRecordArea;
            formattedRecordAreaToAddress.LaidOut += formattedRecordAreaToAddress_LaidOut;

            Label lblTotalAmount = report.GetReportElementById( "lblTotalAmount" ) as Label;
            lblTotalAmount.LaidOut += lblTotalAmount_LaidOut;

            Document doc = report.Run();
            return doc;
        }

        /// <summary>
        /// Handles the LaidOut event of the lblTotalAmount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LabelLaidOutEventArgs"/> instance containing the event data.</param>
        protected void lblTotalAmount_LaidOut( object sender, LabelLaidOutEventArgs e )
        {
            List<TransactionRecord> transactions = e.LayoutWriter.RecordSets.Current[1] as List<TransactionRecord>;
            e.Label.Text = transactions.Sum( a => a.Amount ).ToString();
        }

        /// <summary>
        /// Handles the LaidOut event of the formattedRecordAreaToAddress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ceTe.DynamicPDF.ReportWriter.ReportElements.FormattedRecordAreaLaidOutEventArgs"/> instance containing the event data.</param>
        protected void formattedRecordAreaToAddress_LaidOut( object sender, FormattedRecordAreaLaidOutEventArgs e )
        {
            Person person = e.LayoutWriter.RecordSets.Current[0] as Person;

            PersonService personService = new PersonService();
            Group family = personService.GetFamilies( person ).FirstOrDefault();
            if ( family != null )
            {
                GroupLocation homeAddress = family.GroupLocations.Where( a => a.LocationTypeValue.Guid == new Guid( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_HOME ) ).FirstOrDefault();
                if ( homeAddress != null )
                {
                    e.FormattedTextArea.Text = string.Format(
@"
<p>{0}</p>
<p>{1}</p>
<p>{2}</p>
<p>{3}, {4} {5}</p>
", family.Name, homeAddress.Location.Street1, homeAddress.Location.Street2, homeAddress.Location.City, homeAddress.Location.State, homeAddress.Location.Zip );
                }
            }
            else
            {
                e.FormattedTextArea.Text = string.Format( "<p>{0}</p>", person.FullName );
            }
        }

        /// <summary>
        /// Gets or sets the record count.
        /// </summary>
        /// <value>
        /// The record count.
        /// </value>
        private int RecordCount { get; set; }

        /// <summary>
        /// Gets or sets the index of the record.
        /// </summary>
        /// <value>
        /// The index of the record.
        /// </value>
        private int RecordIndex { get; set; }

        /// <summary>
        /// Handles the OpeningRecordSet event of the query control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="OpeningRecordSetEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void mainQuery_OpeningRecordSet( object sender, OpeningRecordSetEventArgs e )
        {
            UpdateProgress( "Executing Query..." );

            RockConfig rockConfig = RockConfig.Load();

            RockRestClient rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
            rockRestClient.Login( rockConfig.Username, rockConfig.Password );

            var param = new Rock.Net.RestParameters.ContributionStatementOptions
            {
                StartDate = Options.StartDate,
                EndDate = Options.EndDate,
                AccountIds = null,
                PersonId = null,
                OrderByZipCode = true
            };
            var result = rockRestClient.PostDataWithResult<Rock.Net.RestParameters.ContributionStatementOptions>( "api/FinancialTransactions/GetContributionPersonGroupAddress", param );


            /*

            // get data from Rock database
            FinancialTransactionService financialTransactionService = new FinancialTransactionService();

            var qry = financialTransactionService.Queryable().AsNoTracking();

            qry = qry
                .Where( a => a.TransactionDateTime >= this.Options.StartDate )
                .Where( a => a.TransactionDateTime < this.Options.EndDate );

            var selectQry = qry.Select( a => new TransactionRecord
            {
                AuthorizedPerson = a.AuthorizedPerson,
                TransactionDateTime = a.TransactionDateTime,
                CurrencyTypeValueId = a.CurrencyTypeValueId,
                Summary = a.Summary,
                AccountId = a.TransactionDetails.FirstOrDefault().Account.Id,
                AccountName = a.TransactionDetails.FirstOrDefault().Account.Name,
                Amount = a.Amount
            } );

            var personTransactionGroupBy = selectQry.GroupBy( a => a.AuthorizedPerson );

            UpdateProgress( "Getting Data..." );
            var outerQuery = personTransactionGroupBy.OrderBy( a => a.Key.FullNameLastFirst ).ToList();

            RecordCount = outerQuery.Count();

            e.RecordSet = new EnumerableRecordSet( outerQuery );

            SubReport subReport = e.LayoutWriter.DocumentLayout.GetReportElementById( "InnerReport" ) as SubReport;
            subReport.Query.OpeningRecordSet += subQuery_OpeningRecordSet;
             * 
             */ 
        }

        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="progressMessage">The message.</param>
        private void UpdateProgress( string progressMessage )
        {
            if ( OnProgress != null )
            {
                OnProgress( this, new ProgressEventArgs { ProgressMessage = progressMessage, Position = RecordIndex, Max = RecordCount } );
            }
        }

        /// <summary>
        /// Handles the OpeningRecordSet event of the subQuery control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ceTe.DynamicPDF.ReportWriter.Data.OpeningRecordSetEventArgs"/> instance containing the event data.</param>
        protected void subQuery_OpeningRecordSet( object sender, OpeningRecordSetEventArgs e )
        {
            RecordIndex++;
            UpdateProgress( "Processing..." );
            List<TransactionRecord> transactions = e.LayoutWriter.RecordSets.Current[1] as List<TransactionRecord>;
            e.RecordSet = new EnumerableRecordSet( transactions );
        }

        /// <summary>
        /// 
        /// </summary>
        public class ProgressEventArgs : EventArgs
        {
            public int Position { get; set; }
            public int Max { get; set; }
            public string ProgressMessage { get; set; }
        }

        /// <summary>
        /// Occurs when [configuration progress].
        /// </summary>
        public event EventHandler<ProgressEventArgs> OnProgress;
    }
}
