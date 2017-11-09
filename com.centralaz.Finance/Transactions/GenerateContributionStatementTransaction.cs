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
        /// Gets or sets the letter template.
        /// </summary>
        /// <value>
        /// The letter template.
        /// </value>
        public MergeTemplate LetterTemplate { get; set; }

        /// <summary>
        /// Gets or sets the statement template.
        /// </summary>
        /// <value>
        /// The statement template.
        /// </value>
        public MergeTemplate StatementTemplate { get; set; }

        /// <summary>
        /// Gets or sets the type of the binary file.
        /// </summary>
        /// <value>
        /// The type of the binary file.
        /// </value>
        public BinaryFileType BinaryFileType { get; set; }

        /// <summary>
        /// Gets or sets the requestor.
        /// </summary>
        /// <value>
        /// The requestor.
        /// </value>
        public Person Requestor { get; set; }

        /// <summary>
        /// Gets or sets the system email unique identifier.
        /// </summary>
        /// <value>
        /// The system email unique identifier.
        /// </value>
        public Guid? SystemEmailGuid { get; set; }

        /// <summary>
        /// Gets or sets the size of the chapter.
        /// </summary>
        /// <value>
        /// The size of the chapter.
        /// </value>
        public int ChapterSize { get; set; }

        /// <summary>
        /// Gets or sets the chapter number.
        /// </summary>
        /// <value>
        /// The chapter number.
        /// </value>
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

        /// <summary>
        /// Gets or sets the account1.
        /// </summary>
        /// <value>
        /// The account1.
        /// </value>
        public FinancialAccount Account1 { get; set; }

        /// <summary>
        /// Gets or sets the account2.
        /// </summary>
        /// <value>
        /// The account2.
        /// </value>
        public FinancialAccount Account2 { get; set; }

        /// <summary>
        /// Gets or sets the account3.
        /// </summary>
        /// <value>
        /// The account3.
        /// </value>
        public FinancialAccount Account3 { get; set; }

        /// <summary>
        /// Gets or sets the account4.
        /// </summary>
        /// <value>
        /// The account4.
        /// </value>
        public FinancialAccount Account4 { get; set; }

        /// <summary>
        /// Gets or sets the campuses.
        /// </summary>
        /// <value>
        /// The campuses.
        /// </value>
        public List<int> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public HttpContext Context { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public HttpResponse Response { get; set; }

        /// <summary>
        /// Gets or sets the database timeout.
        /// </summary>
        /// <value>
        /// The database timeout.
        /// </value>
        public int? DatabaseTimeout { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateContributionStatementTransaction"/> class.
        /// </summary>
        public GenerateContributionStatementTransaction()
        {
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
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

                var binaryFileService = new BinaryFileService( rockContext );

                MergeTemplateType statementTemplateType = StatementTemplate.GetMergeTemplateType();
                if ( statementTemplateType != null )
                {
                    try
                    {
                        for ( ChapterNumber = 1; ( ( ChapterNumber - 1 ) * ChapterSize ) < givingIdCount; ChapterNumber++ )
                        {
                            var fileName = String.Format( "{0}_ContributionStatements_Chapter_{1:000}_of_{2:000}.html", DateTime.Now.ToString( "MMddyyyy" ), ChapterNumber, totalChapters );
                            var mergeFields = GetStatementMergeFields( rockContext );

                            BinaryFile outputBinaryFileDoc = null;
                            var fileList = binaryFileService.Queryable().Where( b => b.FileName == fileName );
                            binaryFileService.DeleteRange( fileList );

                            outputBinaryFileDoc = statementTemplateType.CreateDocument( StatementTemplate, new List<object>(), mergeFields );
                            outputBinaryFileDoc = binaryFileService.Get( outputBinaryFileDoc.Id );

                            outputBinaryFileDoc.IsTemporary = false;
                            outputBinaryFileDoc.BinaryFileTypeId = BinaryFileType.Id;
                            outputBinaryFileDoc.FileName = fileName;
                            rockContext.SaveChanges();

                            if ( statementTemplateType.Exceptions != null && statementTemplateType.Exceptions.Any() )
                            {
                                if ( statementTemplateType.Exceptions.Count == 1 )
                                {
                                    this.LogException( statementTemplateType.Exceptions[0] );
                                }
                                else if ( statementTemplateType.Exceptions.Count > 50 )
                                {
                                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions for top 50.", StatementTemplate.Name ), statementTemplateType.Exceptions.Take( 50 ).ToList() ) );
                                }
                                else
                                {
                                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions", StatementTemplate.Name ), statementTemplateType.Exceptions.ToList() ) );
                                }
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        this.LogException( ex );
                    }
                }

                MergeTemplateType letterTemplateType = LetterTemplate.GetMergeTemplateType();
                if ( letterTemplateType != null )
                {
                    try
                    {
                        ChapterNumber = 1;
                        ChapterSize = givingIdCount + 1;
                        var fileName = String.Format( "{0}_ContributionLetter.docx", DateTime.Now.ToString( "MMddyyyy" ) );
                        var mergeObjectList = GetLetterMergeObjectList( rockContext ).Select( a => a.Value ).ToList();
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null,null );

                        BinaryFile outputBinaryFileDoc = null;
                        var fileList = binaryFileService.Queryable().Where( b => b.FileName == fileName );
                        binaryFileService.DeleteRange( fileList );

                        outputBinaryFileDoc = letterTemplateType.CreateDocument( LetterTemplate, mergeObjectList, mergeFields );
                        outputBinaryFileDoc = binaryFileService.Get( outputBinaryFileDoc.Id );

                        outputBinaryFileDoc.IsTemporary = false;
                        outputBinaryFileDoc.BinaryFileTypeId = BinaryFileType.Id;
                        outputBinaryFileDoc.FileName = fileName;
                        rockContext.SaveChanges();

                        if ( letterTemplateType.Exceptions != null && letterTemplateType.Exceptions.Any() )
                        {
                            if ( letterTemplateType.Exceptions.Count == 1 )
                            {
                                this.LogException( letterTemplateType.Exceptions[0] );
                            }
                            else if ( letterTemplateType.Exceptions.Count > 50 )
                            {
                                this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions for top 50.", LetterTemplate.Name ), letterTemplateType.Exceptions.Take( 50 ).ToList() ) );
                            }
                            else
                            {
                                this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions", LetterTemplate.Name ), letterTemplateType.Exceptions.ToList() ) );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        this.LogException( ex );
                    }
                }

                // Send email
                if ( SystemEmailGuid.HasValue )
                {
                    var emailMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    emailMergeFields.Add( "Person", Requestor );

                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );

                    var recipients = new List<RecipientData>();
                    recipients.Add( new RecipientData( Requestor.Email, emailMergeFields ) );

                    Email.Send( SystemEmailGuid.Value, recipients, appRoot );
                }
            }
        }

        /// <summary>
        /// Gets the giving identifier count.
        /// </summary>
        /// <returns></returns>
        private int GetGivingIdCount()
        {
            var count = 0;
            var query = @"DECLARE @cTRANSACTION_TYPE_CONTRIBUTION uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
DECLARE @transactionTypeContributionId int = (select top 1 Id from DefinedValue where [Guid] = @cTRANSACTION_TYPE_CONTRIBUTION)
DECLARE @cLOCATION_TYPE_HOME uniqueidentifier = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
DECLARE @cLOCATION_TYPE_WORK uniqueidentifier = 'E071472A-F805-4FC4-917A-D5E3C095C35C'
DECLARE @cGROUPTYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
    select p.GivingId
		from FinancialTransaction ft
		join FinancialTransactionDetail ftd on ftd.TransactionId = ft.Id
		join PersonAlias pa on ft.AuthorizedPersonAliasId = pa.Id
		join Person p on pa.PersonId = p.Id		
		JOIN [GroupMember] [gm] ON [gm].[PersonId] = [p].[Id]
		JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
		Join [GroupLocation] gl on gl.GroupId = g.Id
		JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
		Where ft.TransactionTypeValueId = @transactionTypeContributionId
		and (@startDate is null or ft.TransactionDateTime >= @StartDate)
		and ( @endDate is null or ft.TransactionDateTime < @EndDate)				
		and ( ( p.GivingGroupId is null AND [g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)) or p.GivingGroupId = g.Id)
		and g.CampusId in (SELECT * FROM [dbo].[ufnUtility_CsvToTable](@CampusList))
		and [gl].[IsMailingLocation] = 1
		AND [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
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

        /// <summary>
        /// Gets the merge fields.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="fetchCount">The fetch count.</param>
        /// <returns></returns>
        private Dictionary<string, object> GetStatementMergeFields( RockContext rockContext, int? fetchCount = null )
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

                var addressDataTable = result.Tables[1];
                foreach ( var row in addressDataTable.Rows.OfType<DataRow>().ToList() )
                {
                    addressList.Add( new AddressSummary
                    {
                        GivingId = row.ItemArray[0] as string,
                        AddressNames = row.ItemArray[1] as string,
                        GreetingNames = row.ItemArray[2] as string,
                        Street1 = row.ItemArray[4] as string,
                        Street2 = row.ItemArray[5] as string,
                        City = row.ItemArray[6] as string,
                        State = row.ItemArray[7] as string,
                        PostalCode = row.ItemArray[8] as string
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
            mergeFields.Add( "ChapterNumber", ChapterNumber );
            mergeFields.Add( "ChapterSize", ChapterSize );
            return mergeFields;
        }


        private Dictionary<int, object> GetLetterMergeObjectList( RockContext rockContext, int? fetchCount = null )
        {
            if ( DatabaseTimeout.HasValue )
            {
                rockContext.Database.CommandTimeout = DatabaseTimeout.Value;
            }

            // Get all transactions tied to the Giving Ids
            Dictionary<string, object> parameters = GetSqlParameters();
            var result = DbService.GetDataSet( "spFinance_GetGivingGroupAddresses", System.Data.CommandType.StoredProcedure, parameters );
            Dictionary<int, object> mergeObjectsDictionary = new Dictionary<int, object>();
            int dictionaryIndex = 0;

            if ( result.Tables.Count > 0 )
            {

                var addressDataTable = result.Tables[0];
                foreach ( var row in addressDataTable.Rows.OfType<DataRow>().ToList() )
                {
                    mergeObjectsDictionary.Add( dictionaryIndex, new AddressSummary
                    {
                        GivingId = row.ItemArray[0] as string,
                        AddressNames = row.ItemArray[1] as string,
                        GreetingNames = row.ItemArray[2] as string,
                        Street1 = row.ItemArray[4] as string,
                        Street2 = row.ItemArray[5] as string,
                        City = row.ItemArray[6] as string,
                        State = row.ItemArray[7] as string,
                        PostalCode = row.ItemArray[8] as string
                    } );
                    dictionaryIndex++;
                }
            }
            return mergeObjectsDictionary;
        }

        /// <summary>
        /// Gets the SQL parameters.
        /// </summary>
        /// <returns></returns>
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

            parameters.Add( "campusList", Campuses.AsDelimited( "," ) );

            parameters.Add( "minimumAmount", MinimumContributionAmount );
            parameters.Add( "chapterNumber", ChapterNumber );
            parameters.Add( "chapterSize", ChapterSize );

            return parameters;
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public void LogException( Exception ex )
        {
            ExceptionLogService.LogException( ex, Context );
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Grouped Transactions
        /// </summary>
        /// <seealso cref="Rock.Data.Entity{com.centralaz.Finance.Transactions.GenerateContributionStatementTransaction.GroupedTransaction}" />
        public class GroupedTransaction : Entity<GroupedTransaction>
        {
            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            [LavaInclude]
            public String Key { get; set; }

            /// <summary>
            /// Gets or sets the account1 total.
            /// </summary>
            /// <value>
            /// The account1 total.
            /// </value>
            [LavaInclude]
            public decimal? Account1Total { get; set; }

            /// <summary>
            /// Gets or sets the account2 total.
            /// </summary>
            /// <value>
            /// The account2 total.
            /// </value>
            [LavaInclude]
            public decimal? Account2Total { get; set; }

            /// <summary>
            /// Gets or sets the account3 total.
            /// </summary>
            /// <value>
            /// The account3 total.
            /// </value>
            [LavaInclude]
            public decimal? Account3Total { get; set; }

            /// <summary>
            /// Gets or sets the account4 total.
            /// </summary>
            /// <value>
            /// The account4 total.
            /// </value>
            [LavaInclude]
            public decimal? Account4Total { get; set; }

            /// <summary>
            /// Gets or sets the other total.
            /// </summary>
            /// <value>
            /// The other total.
            /// </value>
            [LavaInclude]
            public decimal? OtherTotal { get; set; }

            /// <summary>
            /// Gets or sets the transaction total.
            /// </summary>
            /// <value>
            /// The transaction total.
            /// </value>
            [LavaInclude]
            public decimal? TransactionTotal { get; set; }

            /// <summary>
            /// Gets or sets the transactions.
            /// </summary>
            /// <value>
            /// The transactions.
            /// </value>
            [LavaInclude]
            public List<TransactionSummary> Transactions { get; set; }

        }

        /// <summary>
        /// The transaction Summary
        /// </summary>
        [DotLiquid.LiquidType( "GivingId", "TransactionCode", "TransactionDateTime", "Account1Amount", "Account2Amount", "Account3Amount", "Account4Amount", "OtherAmount", "TotalTransactionAmount" )]
        public class TransactionSummary
        {
            /// <summary>
            /// Gets or sets the giving identifier.
            /// </summary>
            /// <value>
            /// The giving identifier.
            /// </value>
            public String GivingId { get; set; }

            /// <summary>
            /// Gets or sets the transaction code.
            /// </summary>
            /// <value>
            /// The transaction code.
            /// </value>
            public String TransactionCode { get; set; }

            /// <summary>
            /// Gets or sets the transaction date time.
            /// </summary>
            /// <value>
            /// The transaction date time.
            /// </value>
            public DateTime? TransactionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the account1 amount.
            /// </summary>
            /// <value>
            /// The account1 amount.
            /// </value>
            public decimal? Account1Amount { get; set; }

            /// <summary>
            /// Gets or sets the account2 amount.
            /// </summary>
            /// <value>
            /// The account2 amount.
            /// </value>
            public decimal? Account2Amount { get; set; }

            /// <summary>
            /// Gets or sets the account3 amount.
            /// </summary>
            /// <value>
            /// The account3 amount.
            /// </value>
            public decimal? Account3Amount { get; set; }

            /// <summary>
            /// Gets or sets the account4 amount.
            /// </summary>
            /// <value>
            /// The account4 amount.
            /// </value>
            public decimal? Account4Amount { get; set; }

            /// <summary>
            /// Gets or sets the other amount.
            /// </summary>
            /// <value>
            /// The other amount.
            /// </value>
            public decimal? OtherAmount { get; set; }

            /// <summary>
            /// Gets or sets the total transaction amount.
            /// </summary>
            /// <value>
            /// The total transaction amount.
            /// </value>
            public decimal? TotalTransactionAmount { get; set; }
        }

        /// <summary>
        /// The address summary for lava
        /// </summary>
        [DotLiquid.LiquidType( "GivingId", "AddressNames", "GreetingNames", "Street1", "Street2", "City", "State", "PostalCode" )]
        public class AddressSummary
        {
            /// <summary>
            /// Gets or sets the giving identifier.
            /// </summary>
            /// <value>
            /// The giving identifier.
            /// </value>
            public String GivingId { get; set; }

            /// <summary>
            /// Gets or sets the address names.
            /// </summary>
            /// <value>
            /// The address names.
            /// </value>
            public String AddressNames { get; set; }

            /// <summary>
            /// Gets or sets the greeting names.
            /// </summary>
            /// <value>
            /// The greeting names.
            /// </value>
            public String GreetingNames { get; set; }

            /// <summary>
            /// Gets or sets the street1.
            /// </summary>
            /// <value>
            /// The street1.
            /// </value>
            public String Street1 { get; set; }

            /// <summary>
            /// Gets or sets the street2.
            /// </summary>
            /// <value>
            /// The street2.
            /// </value>
            public String Street2 { get; set; }

            /// <summary>
            /// Gets or sets the city.
            /// </summary>
            /// <value>
            /// The city.
            /// </value>
            public String City { get; set; }

            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public String State { get; set; }

            /// <summary>
            /// Gets or sets the postal code.
            /// </summary>
            /// <value>
            /// The postal code.
            /// </value>
            public String PostalCode { get; set; }
        }
        #endregion
    }
}