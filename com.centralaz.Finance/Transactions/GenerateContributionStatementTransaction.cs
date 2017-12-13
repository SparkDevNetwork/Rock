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
using com.centralaz.Finance.Utility;

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
        /// Gets or sets the ExcludedGroupId.
        /// </summary>
        /// <value>
        /// The ExcludedGroupId.
        /// </value>
        public int? ExcludedGroupId { get; set; }

        /// <summary>
        /// Gets or sets the campuses.
        /// </summary>
        /// <value>
        /// The campuses.
        /// </value>
        public List<int> CampusIds { get; set; }

        /// <summary>
        /// Gets or sets the StatementFrequencyAttribute.
        /// </summary>
        /// <value>
        /// The StatementFrequencyAttribute.
        /// </value>
        public Rock.Web.Cache.AttributeCache DefinedTypeAttribute { get; set; }

        /// <summary>
        /// Gets or sets the StatementFrequencies.
        /// </summary>
        /// <value>
        /// The StatementFrequencies.
        /// </value>
        public List<int> DefinedValueIds { get; set; }

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
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );

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
            var query = @"select GivingId from [dbo].[_com_centralaz_Finance_GetGivingIdsAndRowNumbers](@StartDate, @EndDate, @MinimumAmount, @CampusList, @DefinedTypeAttributeId, @DefinedValueList, @ExcludedGroupId)";

            Dictionary<string, object> parameters = FinanceHelper.GetSqlParameters(
                startDate: StartDate,
                endDate: EndDate,
                account1Id: Account1.Id,
                account2Id: Account2.Id,
                account3Id: Account3.Id,
                account4Id: Account4.Id,
                excludedGroupId: ExcludedGroupId,
                campusIds: CampusIds,
                definedTypeAttributeId: DefinedTypeAttribute.Id,
                definedValueIds: DefinedValueIds,
                minimumContributionAmount: MinimumContributionAmount,
                chapterNumber: ChapterNumber,
                chapterSize: ChapterSize
                );

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
            Dictionary<string, object> parameters = FinanceHelper.GetSqlParameters(
                startDate: StartDate,
                endDate: EndDate,
                account1Id: Account1.Id,
                account2Id: Account2.Id,
                account3Id: Account3.Id,
                account4Id: Account4.Id,
                excludedGroupId: ExcludedGroupId,
                campusIds: CampusIds,
                definedTypeAttributeId: DefinedTypeAttribute.Id,
                definedValueIds: DefinedValueIds,
                minimumContributionAmount: MinimumContributionAmount,
                chapterNumber: ChapterNumber,
                chapterSize: ChapterSize               
                );

            var mergeFields = FinanceHelper.GetFinancialStatementTransactionsAndAddresses( parameters );
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
            Dictionary<string, object> parameters = FinanceHelper.GetSqlParameters(
                startDate: StartDate,
                endDate: EndDate,
                account1Id: Account1.Id,
                account2Id: Account2.Id,
                account3Id: Account3.Id,
                account4Id: Account4.Id,
                excludedGroupId: ExcludedGroupId,
                campusIds: CampusIds,
                definedTypeAttributeId: DefinedTypeAttribute.Id,
                definedValueIds: DefinedValueIds,
                minimumContributionAmount: MinimumContributionAmount,
                chapterNumber: ChapterNumber,
                chapterSize: ChapterSize,
                returnTransactions: false
                );
            var result = DbService.GetDataSet( "_com_centralaz_Finance_GetGivingGroupTransactions", System.Data.CommandType.StoredProcedure, parameters );
            Dictionary<int, object> mergeObjectsDictionary = new Dictionary<int, object>();
            int dictionaryIndex = 0;

            if ( result.Tables.Count > 0 )
            {

                var addressDataTable = result.Tables[0];
                foreach ( var row in addressDataTable.Rows.OfType<DataRow>().ToList() )
                {
                    mergeObjectsDictionary.Add( dictionaryIndex, new FinanceHelper.AddressSummary
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
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public void LogException( Exception ex )
        {
            ExceptionLogService.LogException( ex, Context );
        }

        #endregion
    }
}