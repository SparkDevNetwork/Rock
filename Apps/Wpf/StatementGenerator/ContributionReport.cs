using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.ReportWriter;
using ceTe.DynamicPDF.ReportWriter.Data;
using ceTe.DynamicPDF.ReportWriter.ReportElements;
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
        /// Gets or sets the account ids.
        /// </summary>
        /// <value>
        /// The account ids.
        /// </value>
        public List<int> AccountIds { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// </summary>
        /// <value>
        /// The person unique identifier.
        /// </value>
        public int? PersonId { get; set; }

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
    public class MissingReportElementException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingReportElementException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MissingReportElementException( string message )
            : base( message )
        {

        }
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
        /// The _rock rest client
        /// </summary>
        private RockRestClient _rockRestClient = null;

        /// <summary>
        /// The Organization Address location
        /// </summary>
        private Rock.Model.Location _organizationAddressLocation = null;

        /// <summary>
        /// The _contribution statement options rest
        /// </summary>
        private Rock.Net.RestParameters.ContributionStatementOptions _contributionStatementOptionsREST = null;

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="financialTransactionQry">The financial transaction qry.</param>
        /// <returns></returns>
        public Document RunReport()
        {
            UpdateProgress( "Connecting..." );
            // Login and setup options for REST calls
            RockConfig rockConfig = RockConfig.Load();

            _rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
            _rockRestClient.Login( rockConfig.Username, rockConfig.Password );

            _contributionStatementOptionsREST = new Rock.Net.RestParameters.ContributionStatementOptions
            {
                StartDate = Options.StartDate,
                EndDate = Options.EndDate,
                AccountIds = null,
                PersonId = null,
                OrderByZipCode = true
            };

            var organizationAddressAttribute = _rockRestClient.GetData<List<Rock.Model.Attribute>>( "api/attributes", "Key eq 'OrganizationAddress'" ).FirstOrDefault();
            if ( organizationAddressAttribute != null )
            {
                Guid locationGuid = Guid.Empty;
                if ( Guid.TryParse( organizationAddressAttribute.DefaultValue, out locationGuid ) )
                {
                    _organizationAddressLocation = _rockRestClient.GetDataByGuid<Rock.Model.Location>( "api/locations", locationGuid );
                }
            }

            // If we don't have a _organizationAddressLocation, just create an empty location
            _organizationAddressLocation = _organizationAddressLocation ?? new Rock.Model.Location();

            // setup report layout and events
            DocumentLayout report = new DocumentLayout( this.Options.LayoutFile );
            Query query = report.GetQueryById( "OuterQuery" );
            if ( query == null )
            {
                throw new MissingReportElementException( "Report requires a QueryElement named 'OuterQuery'" );
            }

            query.OpeningRecordSet += mainQuery_OpeningRecordSet;

            Query orgInfoQuery = report.GetQueryById( "OrgInfoQuery" );
            if ( orgInfoQuery == null )
            {
                throw new MissingReportElementException( "Report requires a QueryElement named 'OrgInfoQuery'" );
            }

            orgInfoQuery.OpeningRecordSet += orgInfoQuery_OpeningRecordSet;

            Document doc = report.Run();
            return doc;
        }

        /// <summary>
        /// Handles the OpeningRecordSet event of the orgInfoQuery control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="OpeningRecordSetEventArgs"/> instance containing the event data.</param>
        protected void orgInfoQuery_OpeningRecordSet( object sender, OpeningRecordSetEventArgs e )
        {
            // everytime the OrgInfoSubReport is called, just give it a one row dataset with a Location object
            List<Rock.Model.Location> orgInfoList = new List<Rock.Model.Location>();
            orgInfoList.Add( _organizationAddressLocation );
            e.RecordSet = new EnumerableRecordSet( orgInfoList );
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
            UpdateProgress( "Getting Data..." );

            // get outer query data from Rock database via REST
            DataSet personGroupAddressDataSet = _rockRestClient.PostDataWithResult<Rock.Net.RestParameters.ContributionStatementOptions, DataSet>( "api/FinancialTransactions/GetContributionPersonGroupAddress", _contributionStatementOptionsREST );

            DataTable personGroupAddressDataTable = personGroupAddressDataSet.Tables[0];
            RecordCount = personGroupAddressDataTable.Rows.Count;
            e.RecordSet = new DataTableRecordSet( personGroupAddressDataTable );

            SubReport subReport = e.LayoutWriter.DocumentLayout.GetReportElementById( "InnerReport" ) as SubReport;

            if ( subReport == null )
            {
                throw new MissingReportElementException( "Report requires a QueryElement named 'InnerReport'" );
            }

            subReport.Query.OpeningRecordSet += subQuery_OpeningRecordSet;

            SubReportFooter tranListFooter = e.LayoutWriter.DocumentLayout.GetElementById( "TranListFooter" ) as SubReportFooter;

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

            int? personId = e.LayoutWriter.RecordSets.Current["PersonId"].ToString().AsInteger( false );
            int groupId = e.LayoutWriter.RecordSets.Current["GroupId"].ToString().AsInteger() ?? 0;
            string uriParam;

            if ( personId.HasValue )
            {
                uriParam = string.Format( "api/FinancialTransactions/GetContributionTransactions/{0}/{1}", groupId, personId );
            }
            else
            {
                uriParam = string.Format( "api/FinancialTransactions/GetContributionTransactions/{0}", groupId );
            }

            DataSet transactionsDataSet = _rockRestClient.PostDataWithResult<Rock.Net.RestParameters.ContributionStatementOptions, DataSet>( uriParam, _contributionStatementOptionsREST );
            DataTable transactionsDataTable = transactionsDataSet.Tables[0];

            e.RecordSet = new DataTableRecordSet( transactionsDataTable );
        }

        /// <summary>
        /// 
        /// </summary>
        public class ProgressEventArgs : EventArgs
        {
            /// <summary>
            /// Gets or sets the position.
            /// </summary>
            /// <value>
            /// The position.
            /// </value>
            public int Position { get; set; }

            /// <summary>
            /// Gets or sets the maximum.
            /// </summary>
            /// <value>
            /// The maximum.
            /// </value>
            public int Max { get; set; }

            /// <summary>
            /// Gets or sets the progress message.
            /// </summary>
            /// <value>
            /// The progress message.
            /// </value>
            public string ProgressMessage { get; set; }
        }

        /// <summary>
        /// Occurs when [configuration progress].
        /// </summary>
        public event EventHandler<ProgressEventArgs> OnProgress;
    }
}
