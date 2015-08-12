// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.ReportWriter;
using ceTe.DynamicPDF.ReportWriter.Data;
using ceTe.DynamicPDF.ReportWriter.ReportElements;
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
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the account ids.
        /// </summary>
        /// <value>
        /// The account ids.
        /// </value>
        public List<int> AccountIds { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// NULL means to get all individuals
        /// </summary>
        /// <value>
        /// The person unique identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include individuals with no address].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include individuals with no address]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeIndividualsWithNoAddress { get; set; }

        /// <summary>
        /// Gets or sets the layout file.
        /// </summary>
        /// <value>
        /// The layout file.
        /// </value>
        public string LayoutFile { get; set; }

        /// <summary>
        /// Gets or sets the current report options
        /// </summary>
        /// <value>
        /// The current report options.
        /// </value>
        public static ReportOptions Current
        {
            get
            {
                return _current;
            }
        }

        private static ReportOptions _current = new ReportOptions();
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
        private Rock.Client.Location _organizationAddressLocation = null;

        /// <summary>
        /// The _account summary query
        /// </summary>
        private Query _accountSummaryQuery = null;

        /// <summary>
        /// The _contribution statement options rest
        /// </summary>
        private dynamic _contributionStatementOptionsREST = null;

        /// <summary>
        /// The _person group address data table
        /// </summary>
        private DataTable _personGroupAddressDataTable = null;

        /// <summary>
        /// the _transactionsDataTable for the current person/group 
        /// The structure of the DataTable is
        /// 
        /// DateTime TransactionDateTime
        /// string CurrencyTypeValueName
        /// string Summary (main transaction summary)
        /// DataTable Details {
        ///      int AccountId
        ///      string AccountName
        ///      string Summary (detail summary)
        ///      decimal Amount
        /// }
        /// </summary>
        private DataTable _transactionsDataTable = null;

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

            // shouldn't happen, but just in case the StartDate isn't set, set it to the first day of the current year
            DateTime firstDayOfYear = new DateTime( DateTime.Now.Year, 1, 1 );

            // note: if a specific person is specified, get them even if they don't have an address. 
            _contributionStatementOptionsREST = new 
            {
                StartDate = Options.StartDate ?? firstDayOfYear,
                EndDate = Options.EndDate,
                AccountIds = Options.AccountIds,
                IncludeIndividualsWithNoAddress = Options.PersonId.HasValue || Options.IncludeIndividualsWithNoAddress,
                PersonId = Options.PersonId,
                OrderByPostalCode = true
            };

            var organizationAddressAttribute = _rockRestClient.GetData<List<Rock.Client.Attribute>>( "api/attributes", "Key eq 'OrganizationAddress'" ).FirstOrDefault();
            if ( organizationAddressAttribute != null )
            {
                var organizationAddressAttributeValue = _rockRestClient.GetData<List<Rock.Client.AttributeValue>>( "api/AttributeValues", string.Format( "AttributeId eq {0}", organizationAddressAttribute.Id ) ).FirstOrDefault();

                Guid locationGuid = Guid.Empty;
                if ( Guid.TryParse( organizationAddressAttributeValue.Value, out locationGuid ) )
                {
                    _organizationAddressLocation = _rockRestClient.GetData<List<Rock.Client.Location>>( "api/locations", string.Format( "Guid eq guid'{0}'", locationGuid ) ).FirstOrDefault();
                }
            }

            // If we don't have a _organizationAddressLocation, just create an empty location
            _organizationAddressLocation = _organizationAddressLocation ?? new Rock.Client.Location();

            // setup report layout and events
            DocumentLayout report = new DocumentLayout( this.Options.LayoutFile );

            //// if there is an imgLogo and the path is "logo.jpg", use the logo specified in rockconfig.  
            //// We have to read the layout as Xml first to figure out what the Path of the imgLogo
            XmlDocument layoutXmlDoc = new XmlDocument();
            layoutXmlDoc.Load( this.Options.LayoutFile );
            var imageNodes = layoutXmlDoc.GetElementsByTagName( "image" );
            foreach ( var imageNode in imageNodes.OfType<XmlNode>() )
            {
                string imagePath = imageNode.Attributes["path"].Value;
                string imageId = imageNode.Attributes["id"].Value;
                if ( imageId.Equals( "imgLogo" ) && imagePath.Equals( RockConfig.DefaultLogoFile, StringComparison.OrdinalIgnoreCase ) )
                {
                    Image imgLogo = report.GetReportElementById( "imgLogo" ) as Image;
                    if ( imgLogo != null )
                    {
                        try
                        {
                            if ( !rockConfig.LogoFile.Equals( RockConfig.DefaultLogoFile, StringComparison.OrdinalIgnoreCase ) )
                            {
                                imgLogo.ImageData = ceTe.DynamicPDF.Imaging.ImageData.GetImage( rockConfig.LogoFile );
                            }
                        }
                        catch ( Exception ex )
                        {
                            throw new Exception( "Error loading Logo Image: " + rockConfig.LogoFile + "\n\n" + ex.Message );
                        }
                    }
                }
            }

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

            _accountSummaryQuery = report.GetQueryById( "AccountSummaryQuery" );

            if ( _accountSummaryQuery == null )
            {
                // not required.  Just don't do anything if it isn't there
            }
            else
            {
                _accountSummaryQuery.OpeningRecordSet += delegate( object s, OpeningRecordSetEventArgs ee )
                {
                    // create a recordset for the _accountSummaryQuery which is the GroupBy summary of AccountName, Amount
                    /*
                     The structure of _transactionsDataTable is
                     
                     DateTime TransactionDateTime
                     string CurrencyTypeValueName
                     string Summary (main transaction summary)
                     DataTable Details {
                          int AccountId
                          string AccountName
                          string Summary (detail summary)
                          decimal Amount
                     }
                     */

                    var detailsData = new DataTable();
                    detailsData.Columns.Add( "AccountId", typeof( int ) );
                    detailsData.Columns.Add( "AccountName" );
                    detailsData.Columns.Add( "Amount", typeof( decimal ) );

                    foreach ( var details in _transactionsDataTable.AsEnumerable().Select( a => ( a["Details"] as DataTable ) ) )
                    {
                        foreach ( var row in details.AsEnumerable() )
                        {
                            detailsData.Rows.Add( row["AccountId"], row["AccountName"], row["Amount"] );
                        }
                    }

                    var summaryTable = detailsData.AsEnumerable().GroupBy( g => g["AccountId"] ).Select( a => new
                    {
                        AccountName = a.Max( x => x["AccountName"].ToString() ),
                        Amount = a.Sum( x => decimal.Parse( x["Amount"].ToString() ) )
                    } ).OrderBy( o => o.AccountName );

                    ee.RecordSet = new EnumerableRecordSet( summaryTable );
                };
            }

            UpdateProgress( "Getting Data..." );

            // get outer query data from Rock database via REST now vs in mainQuery_OpeningRecordSet to make sure we have data
            DataSet personGroupAddressDataSet = _rockRestClient.PostDataWithResult<object, DataSet>( "api/FinancialTransactions/GetContributionPersonGroupAddress", _contributionStatementOptionsREST );
            _personGroupAddressDataTable = personGroupAddressDataSet.Tables[0];
            RecordCount = _personGroupAddressDataTable.Rows.Count;

            if ( RecordCount > 0 )
            {
                Document doc = report.Run();
                return doc;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Handles the OpeningRecordSet event of the orgInfoQuery control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="OpeningRecordSetEventArgs"/> instance containing the event data.</param>
        protected void orgInfoQuery_OpeningRecordSet( object sender, OpeningRecordSetEventArgs e )
        {
            // everytime the OrgInfoSubReport is called, just give it a one row dataset with a Location object
            List<Rock.Client.Location> orgInfoList = new List<Rock.Client.Location>();
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
            e.RecordSet = new DataTableRecordSet( _personGroupAddressDataTable );
            SubReport innerReport = e.LayoutWriter.DocumentLayout.GetReportElementById( "InnerReport" ) as SubReport;

            if ( innerReport == null )
            {
                throw new MissingReportElementException( "Report requires a QueryElement named 'InnerReport'" );
            }

            innerReport.Query.OpeningRecordSet += innerReport_OpeningRecordSet;

            // Transaction Detail (Accounts Breakout)
            SubReport transactionDetailReport = e.LayoutWriter.DocumentLayout.GetReportElementById( "TransactionDetailReport" ) as SubReport;

            if ( transactionDetailReport == null )
            {
                throw new MissingReportElementException( "Report requires a QueryElement named 'TransactionDetailReport'" );
            }

            transactionDetailReport.Query.OpeningRecordSet += transactionDetailReport_OpeningRecordSet;
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
        /// Handles the OpeningRecordSet event of the innerReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="OpeningRecordSetEventArgs"/> instance containing the event data.</param>
        protected void innerReport_OpeningRecordSet( object sender, OpeningRecordSetEventArgs e )
        {
            RecordIndex++;
            UpdateProgress( "Processing..." );

            int? personId = null;
            int groupId = 0;

            personId = e.LayoutWriter.RecordSets.Current["PersonId"].ToString().AsIntegerOrNull();
            groupId = e.LayoutWriter.RecordSets.Current["GroupId"].ToString().AsInteger();

            string uriParam;

            if ( personId.HasValue )
            {
                uriParam = string.Format( "api/FinancialTransactions/GetContributionTransactions/{0}/{1}", groupId, personId );
            }
            else
            {
                uriParam = string.Format( "api/FinancialTransactions/GetContributionTransactions/{0}", groupId );
            }

            DataSet transactionsDataSet = _rockRestClient.PostDataWithResult<object, DataSet>( uriParam, _contributionStatementOptionsREST );
            _transactionsDataTable = transactionsDataSet.Tables[0];

            e.RecordSet = new DataTableRecordSet( _transactionsDataTable );
        }

        /// <summary>
        /// Handles the OpeningRecordSet event of the transactionDetailReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="OpeningRecordSetEventArgs"/> instance containing the event data.</param>
        public void transactionDetailReport_OpeningRecordSet( object sender, OpeningRecordSetEventArgs e )
        {
            var detailsDataSet = e.LayoutWriter.RecordSets.Current["Details"] as DataTable;
            e.RecordSet = new DataTableRecordSet( detailsDataSet );
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
