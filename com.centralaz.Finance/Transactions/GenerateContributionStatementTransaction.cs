// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Transactions;

namespace com.centralaz.Finance.Transactions
{
    public class GenerateContributionStatementTransaction : ITransaction
    {
        #region Properties

        /// <summary>
        /// Gets or sets the merge template identifier.
        /// </summary>
        /// <value>
        /// The merge template identifier.
        /// </value>
        public MergeTemplate MergeTemplate { get; set; }

        /// <summary>
        /// Gets or sets the size of the chapter.
        /// </summary>
        /// <value>
        /// The size of the chapter.
        /// </value>
        public int ChapterSize { get; set; }

        public int ChapterNumber { get; set; }

        /// <summary>
        /// Gets or sets the minimum contribution amount.
        /// </summary>
        /// <value>
        /// The minimum contribution amount.
        /// </value>
        public decimal MinimumContributionAmount { get; set; }

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

        public FinancialAccount Account1 { get; set; }

        public FinancialAccount Account2 { get; set; }

        public FinancialAccount Account3 { get; set; }

        public FinancialAccount Account4 { get; set; }

        public HttpContext Context { get; set; }
        public HttpResponse Response { get; set; }
        public int? DatabaseTimeout { get; set; }
        #endregion

        #region Methods

        public GenerateContributionStatementTransaction()
        {
        }

        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {

                MergeTemplateType mergeTemplateType = MergeTemplate.GetMergeTemplateType();
                if ( mergeTemplateType != null )
                {
                    try
                    {
                        var givingIdCount = GetGivingIdCount();
                        if ( ChapterSize <= 0 )
                        {
                            ChapterSize = 1;
                        }

                        var totalChapters = 0;
                        for ( ChapterNumber = 1; ( ( ChapterNumber - 1 ) * ChapterSize ) < givingIdCount; ChapterNumber++ )
                        {
                            totalChapters++;
                        }

                        for ( ChapterNumber = 1; ( ( ChapterNumber - 1 ) * ChapterSize ) < givingIdCount; ChapterNumber++ )
                        {
                            var mergeFields = GetMergeFields( rockContext );

                            BinaryFile outputBinaryFileDoc = null;

                            outputBinaryFileDoc = mergeTemplateType.CreateDocument( MergeTemplate, new List<object>(), mergeFields );
                            outputBinaryFileDoc.FileName = String.Format( "{0}_ContributionStatements_Chapter_{1}_of_{2}.html", DateTime.Now.ToString( "MMddyyyy" ), ChapterNumber, totalChapters );

                            if ( mergeTemplateType.Exceptions != null && mergeTemplateType.Exceptions.Any() )
                            {
                                if ( mergeTemplateType.Exceptions.Count == 1 )
                                {
                                    this.LogException( mergeTemplateType.Exceptions[0] );
                                }
                                else if ( mergeTemplateType.Exceptions.Count > 50 )
                                {
                                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions for top 50.", MergeTemplate.Name ), mergeTemplateType.Exceptions.Take( 50 ).ToList() ) );
                                }
                                else
                                {
                                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions", MergeTemplate.Name ), mergeTemplateType.Exceptions.ToList() ) );
                                }
                            }

                            // get folderPath and construct filePath
                            string relativeFolderPath = Context.Request.Form["folderPath"] ?? string.Empty;
                            string relativeFilePath = Path.Combine( relativeFolderPath, Path.GetFileName( outputBinaryFileDoc.FileName ) );
                            string rootFolderParam = Context.Request.QueryString["rootFolder"];

                            string rootFolder = string.Empty;

                            if ( !string.IsNullOrWhiteSpace( rootFolderParam ) )
                            {
                                // if a rootFolder was specified in the URL, decrypt it (it is encrypted to help prevent direct access to filesystem)
                                rootFolder = Rock.Security.Encryption.DecryptString( rootFolderParam );
                            }

                            if ( string.IsNullOrWhiteSpace( rootFolder ) )
                            {
                                // set to default rootFolder if not specified in the params
                                rootFolder = "~/Content/Finance";
                            }

                            string physicalRootFolder = Context.Request.MapPath( rootFolder );
                            string physicalContentFolderName = Path.Combine( physicalRootFolder, relativeFolderPath.TrimStart( new char[] { '/', '\\' } ) );
                            string physicalFilePath = Path.Combine( physicalContentFolderName, outputBinaryFileDoc.FileName );
                            var fileContent = outputBinaryFileDoc.ContentStream;

                            // store the content file in the specified physical content folder
                            if ( !Directory.Exists( physicalContentFolderName ) )
                            {
                                Directory.CreateDirectory( physicalContentFolderName );
                            }

                            if ( File.Exists( physicalFilePath ) )
                            {
                                File.Delete( physicalFilePath );
                            }

                            using ( var writeStream = File.OpenWrite( physicalFilePath ) )
                            {
                                if ( fileContent.CanSeek )
                                {
                                    fileContent.Seek( 0, SeekOrigin.Begin );
                                }

                                fileContent.CopyTo( writeStream );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        this.LogException( ex );
                        if ( ex is System.FormatException )
                        {
                            // nbMergeError.Text = "Error loading the merge template. Please verify that the merge template file is valid.";
                        }
                        else
                        {
                            // nbMergeError.Text = "An error occurred while merging";
                        }
                    }
                }
            }
        }

        private int GetGivingIdCount()
        {
            var count = 0;
            var query = @"DECLARE @cTRANSACTION_TYPE_CONTRIBUTION uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
DECLARE @transactionTypeContributionId int = (select top 1 Id from DefinedValue where [Guid] = @cTRANSACTION_TYPE_CONTRIBUTION)
    select GivingId
		from FinancialTransaction ft
		join FinancialTransactionDetail ftd on ftd.TransactionId = ft.Id
		join PersonAlias pa on ft.AuthorizedPersonAliasId = pa.Id
		join Person p on pa.PersonId = p.Id
		Where ft.TransactionTypeValueId = @transactionTypeContributionId
		and (@startDate is null or ft.TransactionDateTime >= @StartDate)
		and ( @endDate is null or ft.TransactionDateTime < @EndDate)
		group by GivingId
		having sum(ftd.Amount) >= @MinimumAmount";

            Dictionary<string, object> parameters = GetSqlParameters();
            var result = DbService.GetDataSet( query, System.Data.CommandType.Text, parameters );
            if ( result.Tables.Count > 0 )
            {
                var transactionDataTable = result.Tables[0];
                count = transactionDataTable.Rows.OfType<DataRow>().Count();
            }
            return count;
        }

        private Dictionary<string, object> GetMergeFields( RockContext rockContext, int? fetchCount = null )
        {
            if ( DatabaseTimeout.HasValue )
            {
                rockContext.Database.CommandTimeout = DatabaseTimeout.Value;
            }

            // Get all transactions tied to the Giving Ids
            List<TransactionSummary> transactionList = new List<TransactionSummary>();
            List<AddressSummary> addressList = new List<AddressSummary>();
            Dictionary<string, object> parameters = GetSqlParameters();
            var result = DbService.GetDataSet( "spFinance_GetGivingGroupTransactions", System.Data.CommandType.StoredProcedure, parameters );

            if ( result.Tables.Count > 1 )
            {
                var transactionDataTable = result.Tables[0];
                foreach ( var row in transactionDataTable.Rows.OfType<DataRow>().ToList() )
                {
                    transactionList.Add( new TransactionSummary
                    {
                        GivingId = row.ItemArray[0] as string,
                        TransactionCode = row.ItemArray[1] as string,
                        TransactionDateTime = row.ItemArray[2] as DateTime?,
                        Account1Amount = row.ItemArray[3] as decimal?,
                        Account2Amount = row.ItemArray[4] as decimal?,
                        Account3Amount = row.ItemArray[5] as decimal?,
                        Account4Amount = row.ItemArray[6] as decimal?,
                        OtherAmount = row.ItemArray[7] as decimal?,
                        TotalTransactionAmount = row.ItemArray[8] as decimal?
                    } );
                }

                var addressDataTable = result.Tables[01];
                foreach ( var row in addressDataTable.Rows.OfType<DataRow>().ToList() )
                {
                    addressList.Add( new AddressSummary
                    {
                        GivingId = row.ItemArray[0] as string,
                        Names = row.ItemArray[1] as string,
                        Street1 = row.ItemArray[3] as string,
                        Street2 = row.ItemArray[4] as string,
                        City = row.ItemArray[5] as string,
                        State = row.ItemArray[6] as string,
                        PostalCode = row.ItemArray[7] as string
                    } );
                }
            }

            var givingTransactionList = transactionList.GroupBy( t => t.GivingId ).Select( g => new GroupedTransaction
            {
                Key = g.Key,
                Account1Total = g.Sum( t => t.Account1Amount ),
                Account2Total = g.Sum( t => t.Account2Amount ),
                Account3Total = g.Sum( t => t.Account3Amount ),
                Account4Total = g.Sum( t => t.Account4Amount ),
                OtherTotal = g.Sum( t => t.OtherAmount ),
                TransactionTotal = g.Sum( t => t.TotalTransactionAmount ),
                Transactions = g.ToList()
            } ).ToList();

            // Dump everything into a lava object
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
            mergeFields.Add( "Transactions", givingTransactionList );
            mergeFields.Add( "Addresses", addressList );
            mergeFields.Add( "Account1", Account1 );
            mergeFields.Add( "Account2", Account2 );
            mergeFields.Add( "Account3", Account3 );
            mergeFields.Add( "Account4", Account4 );
            mergeFields.Add( "StartDate", StartDate.ToString() );
            mergeFields.Add( "EndDate", EndDate.ToString() );
            return mergeFields;
        }

        private Dictionary<string, object> GetSqlParameters()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if ( StartDate.HasValue )
            {
                parameters.Add( "startDate", StartDate.Value );
            }
            else
            {
                parameters.Add( "startDate", DateTime.MinValue );
            }
            if ( EndDate.HasValue )
            {
                parameters.Add( "endDate", EndDate.Value );
            }
            else
            {
                parameters.Add( "endDate", DateTime.MaxValue );
            }

            parameters.Add( "account1Id", Account1.Id );
            parameters.Add( "account2Id", Account2.Id );
            parameters.Add( "account3Id", Account3.Id );
            parameters.Add( "account4Id", Account4.Id );

            parameters.Add( "minimumAmount", MinimumContributionAmount );
            parameters.Add( "chapterNumber", ChapterNumber );
            parameters.Add( "chapterSize", ChapterSize );

            return parameters;
        }

        public void LogException( Exception ex )
        {
            ExceptionLogService.LogException( ex, Context );
        }

        #endregion

        #region Helper Classes
        public class GroupedTransaction : Entity<GroupedTransaction>
        {
            [LavaInclude]
            public String Key { get; set; }

            [LavaInclude]
            public decimal? Account1Total { get; set; }

            [LavaInclude]
            public decimal? Account2Total { get; set; }

            [LavaInclude]
            public decimal? Account3Total { get; set; }

            [LavaInclude]
            public decimal? Account4Total { get; set; }          

            [LavaInclude]
            public decimal? OtherTotal { get; set; }

            [LavaInclude]
            public decimal? TransactionTotal { get; set; }

            [LavaInclude]
            public List<TransactionSummary> Transactions { get; set; }

        }

        [DotLiquid.LiquidType( "GivingId", "TransactionCode", "TransactionDateTime", "Account1Amount", "Account2Amount", "Account3Amount", "Account4Amount", "OtherAmount", "TotalTransactionAmount" )]
        public class TransactionSummary
        {
            public String GivingId { get; set; }
            public String TransactionCode { get; set; }

            public DateTime? TransactionDateTime { get; set; }

            public decimal? Account1Amount { get; set; }
            public decimal? Account2Amount { get; set; }
            public decimal? Account3Amount { get; set; }
            public decimal? Account4Amount { get; set; }
            public decimal? OtherAmount { get; set; }
            public decimal? TotalTransactionAmount { get; set; }
        }

        [DotLiquid.LiquidType( "GivingId", "Names", "Street1", "Street2", "City", "State", "PostalCode" )]
        public class AddressSummary
        {
            public String GivingId { get; set; }
            public String Names { get; set; }
            public String Street1 { get; set; }
            public String Street2 { get; set; }
            public String City { get; set; }
            public String State { get; set; }
            public String PostalCode { get; set; }
        }
        #endregion
    }
}