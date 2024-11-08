using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Achievement;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Engagement.Achievements
{
    /// <summary>
    /// Tests for Giving to Account Achievements that use the database
    /// </summary>
    [TestClass]
    public class GivingToAccountAchievementTest : DatabaseTestsBase
    {
        private const string ComponentEntityTypeName = "Rock.Achievement.Component.GivingToAccountAchievement";

        private static RockContext _rockContext { get; set; }
        private static AchievementTypeService _achievementTypeService { get; set; }
        private static FinancialAccountService _accountService { get; set; }
        private static FinancialBatchService _batchService { get; set; }
        private static FinancialTransactionService _transactionService { get; set; }
        private static FinancialPaymentDetailService _paymentService { get; set; }
        private static DefinedValueService _definedValueService { get; set; }
        private static DefinedTypeService _definedTypeService { get; set; }
        private static FinancialTransactionDetailService _transactionDetailService { get; set; }

        private static int _personAliasId { get; set; }
        private static int _accountIdParent { get; set; }
        private static int _accountIdChild1 { get; set; }
        private static int _accountIdChild2 { get; set; }
        private static DateTime _startDate { get; set; }
        private static DateTime _endDate { get; set; }

        private const string CONTRIBUTION_TYPE_GUID = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION;
        private const string CHECK_GUID = Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK;
        private const string CASH_GUID = Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CASH;
        private const string CREDIT_CARD_GUID = Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD;
        private const string VISA_CARD_GUID = Rock.SystemGuid.DefinedValue.CREDITCARD_TYPE_VISA;
        private const string SOURCE_TYPE_GUID = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE;
        private static DefinedValue _transactionValue { get; set; }
        private static DefinedValue _checkValue { get; set; }
        private static DefinedValue _cashValue { get; set; }
        private static DefinedValue _creditCardValue { get; set; }
        private static DefinedValue _visaValue { get; set; }
        private static DefinedType _sourceType { get; set; }

        private const int TRANSACTION_COUNT_PARENT = 24;
        private const int TRANSACTION_COUNT_CHLID1 = 14;
        private const int TRANSACTION_COUNT_CHILD2 = 12;
        private const decimal TRANSACTION_AMOUNT = 125.25m;
        private const string KEY = "GivingToAccountAchievementTests";
        private const string GivingAccountParentGuidString = "07AAAE41-2908-4823-A442-285740BE9D02";
        private const string GivingAccountChild1GuidString = "D8336F46-2324-4F13-98BC-732D78210259";
        private const string GivingAccountChild2GuidString = "925E5901-794A-4257-9B22-1F9AC7C79B9D";

        #region Helper Enum and ID List

        private enum GivingTestTypes
        {
            SingleAchievementAccumulate1,
            SingleAchievementAccumulate5,
            SingleAchievementAccumulate52,
            MultiAchievementAccumulate1,
            MultiAchievementAccumulate5,
            OverAchievement,
            InactiveAchievement
        }

        private static List<int> _achievementIds = new List<int>( Enum.GetValues( typeof( GivingTestTypes ) ).Length );

        #endregion

        #region Setup Methods

        /// <summary>
        /// Create financial transaction data
        /// </summary>
        private static void CreateFinancialTransactionData()
        {
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();
            var personAlias = new PersonAliasService( _rockContext ).Queryable().First( pa => pa.Person.Guid == tedDeckerGuid );
            _personAliasId = personAlias.Id;

            // Create a heirarchical account reference
            var accountParent = CreateFinancialAccount( GivingAccountParentGuidString.AsGuid(), "Testing Account Parent", KEY );
            var accountChild1 = CreateFinancialAccount( GivingAccountChild1GuidString.AsGuid(), "Testing Account Child 1", KEY, accountParent.Id );
            var accountChild2 = CreateFinancialAccount( GivingAccountChild2GuidString.AsGuid(), "Testing Account Child 2", KEY, accountChild1.Id );

            _accountIdParent = accountParent.Id;
            _accountIdChild1 = accountChild1.Id;
            _accountIdChild2 = accountChild2.Id;

            var batch = CreateFinancialBatch( 1, KEY );

            CreateFinancialTransactions( _personAliasId, TRANSACTION_COUNT_PARENT, accountParent, batch );
            CreateFinancialTransactions( _personAliasId, TRANSACTION_COUNT_CHLID1, accountChild1, batch );
            CreateFinancialTransactions( _personAliasId, TRANSACTION_COUNT_CHILD2, accountChild2, batch );
        }

        private static FinancialAccount CreateFinancialAccount(
            Guid guid,
            string name,
            string key,
            int? parentAccountId = null)
        {
            var account = _accountService.GetByGuids( new List<Guid> { guid } ).FirstOrDefault();

            if ( null == account )
            {
                account = new FinancialAccount
                {
                    Guid = guid,
                    IsTaxDeductible = true,
                    IsActive = true,
                    IsPublic = true,
                    Name = name,
                    ForeignKey = key
                };

                if ( parentAccountId.HasValue )
                {
                    account.ParentAccountId = parentAccountId.Value;
                }

                _accountService.Add( account );
                _rockContext.SaveChanges( true );
            }

            return account;
        }

        private static FinancialBatch CreateFinancialBatch(
            int batchNumber,
            string key )
        {
            var name = $"Batch {batchNumber}";
            var batch = _batchService.Queryable()
                            .Where( b => b.Name == name )
                            .FirstOrDefault();

            if ( null == batch )
            {
                batch = new FinancialBatch
                {
                    Name = $"Batch {batchNumber}",
                    BatchStartDateTime = RockDateTime.Now,
                    Status = BatchStatus.Open,
                    ControlAmount = 0.0m,
                    Guid = Guid.NewGuid(),
                    CreatedDateTime = RockDateTime.Now,
                    ModifiedDateTime = RockDateTime.Now,
                    ForeignKey = key
                };

                _batchService.Add( batch );
                _rockContext.SaveChanges( true );
            }

            return batch;
        }

        private static FinancialPaymentDetail CreateFinancialPaymentDetail(
            int? currencyTypeId,
            int? cardTypeId,
            DateTime transactionDateTime )
        {
            var payment = new FinancialPaymentDetail
            {
                CurrencyTypeValueId = currencyTypeId,
                CreditCardTypeValueId = cardTypeId,
                Guid = Guid.NewGuid(),
                CreatedDateTime = transactionDateTime,
                ModifiedDateTime = RockDateTime.Now
            };

            _paymentService.Add( payment );
            _rockContext.SaveChanges( true );

            return payment;
        }

        private static FinancialTransaction CreateFinancialTransaction(
            int personAliasId,
            int batchId,
            int paymentId,
            int number,
            DateTime transactionDateTime,
            string checkMicrEncrypted,
            string checkMicrHash,
            string checkMicrParts )
        {
            var transaction = new FinancialTransaction
            {
                AuthorizedPersonAliasId = personAliasId,
                BatchId = batchId,
                TransactionDateTime = transactionDateTime,
                SundayDate = transactionDateTime.SundayDate(),
                TransactionCode = null,
                Summary = $"Giving Test {number}",
                TransactionTypeValueId = _transactionValue.Id,
                FinancialPaymentDetailId = paymentId,
                SourceTypeValueId = _sourceType.Id,
                CheckMicrEncrypted = checkMicrEncrypted,
                CheckMicrHash = checkMicrHash,
                CheckMicrParts = checkMicrParts,
                Guid = Guid.NewGuid(),
                CreatedDateTime = transactionDateTime,
                ModifiedDateTime = RockDateTime.Now,
                ShowAsAnonymous = false,
                ForeignKey = KEY
            };

            _transactionService.Add( transaction );
            _rockContext.SaveChanges( true );

            return transaction;
        }

        private static FinancialTransactionDetail CreateFinancialTransactionDetail(
            int accountId,
            int transactionId,
            int number,
            DateTime transactionDateTime )
        {
            var transactionDetail = new FinancialTransactionDetail
            {
                TransactionId = transactionId,
                AccountId = accountId,
                Amount = TRANSACTION_AMOUNT,
                Summary = $"Giving Test {number}",
                EntityTypeId = null,
                Guid = Guid.NewGuid(),
                CreatedDateTime = transactionDateTime,
                ModifiedDateTime = RockDateTime.Now
            };

            _transactionDetailService.Add( transactionDetail );
            _rockContext.SaveChanges( true );

            return transactionDetail;
        }

        private static void CreateFinancialTransactions(
            int personAliasId,
            int count,
            FinancialAccount account,
            FinancialBatch batch )
        {
            var currencyTypeGuid = string.Empty;
            for ( var i = 0; i < count; i++ )
            {
                int? currencyTypeId = 0;
                int? cardTypeId = null;
                string checkMicrEncrypted = null;
                string checkMicrHash = null;
                string checkMicrParts = null;

                var transactionDateTime = RockDateTime.Now.AddDays( i * -1 );

                if ( i == 0 )
                {
                    _endDate = _startDate = transactionDateTime.AddDays( 1 );
                }

                // Dates are going backwards in time, adjuect start
                if (transactionDateTime < _startDate)
                {
                    _startDate = transactionDateTime;
                }

                if ( i % 5 == 0 )
                {
                    // Checks
                    currencyTypeId = _checkValue.Id;
                    cardTypeId = null;
                    checkMicrEncrypted = $"{Guid.NewGuid().ToString().Replace( "-", "" )}{Guid.NewGuid().ToString().Replace( "-", "" )}{Guid.NewGuid().ToString().Replace( "-", "" )}";
                    checkMicrHash = null;
                    checkMicrParts = null;
                }
                else if ( i % 6 == 0 )
                {
                    // Cash
                    currencyTypeId = _cashValue.Id;
                    cardTypeId = null;
                    checkMicrEncrypted = null;
                    checkMicrHash = null;
                    checkMicrParts = null;
                }
                else
                {
                    // Credit Cards
                    currencyTypeId = _creditCardValue.Id;
                    cardTypeId = _visaValue.Id;
                    checkMicrEncrypted = null;
                    checkMicrHash = null;
                    checkMicrParts = null;
                }

                var payment = CreateFinancialPaymentDetail( currencyTypeId, cardTypeId, transactionDateTime );
                var transaction = CreateFinancialTransaction( personAliasId, batch.Id, payment.Id, i, transactionDateTime, checkMicrEncrypted, checkMicrHash, checkMicrParts );
                var transactionDetail = CreateFinancialTransactionDetail( account.Id, transaction.Id, i, transactionDateTime );
            }

            _startDate = _startDate.AddDays( -1 );
            _rockContext.SaveChanges( true );
        }

        private static void PopulateDefinedValues()
        {
            _transactionValue = _definedValueService.GetByGuid( CONTRIBUTION_TYPE_GUID.AsGuid() );
            _checkValue = _definedValueService.GetByGuid( CHECK_GUID.AsGuid() );
            _cashValue = _definedValueService.GetByGuid( CASH_GUID.AsGuid() );
            _creditCardValue = _definedValueService.GetByGuid( CREDIT_CARD_GUID.AsGuid() );
            _visaValue = _definedValueService.GetByGuid( VISA_CARD_GUID.AsGuid() );
            _sourceType = _definedTypeService.GetByGuid( SOURCE_TYPE_GUID.AsGuid() );
        }

        /// <summary>
        /// Delete the data created by this test class
        /// </summary>
        private static void DeleteTestData()
        {
            var paymentQuery = _paymentService.Queryable().Where( fb => fb.ForeignKey == KEY );
            _paymentService.DeleteRange( paymentQuery );

            _rockContext.SaveChanges();

            var transactionDetailQuery = _transactionDetailService.Queryable().Where( fb => fb.ForeignKey == KEY );
            _transactionDetailService.DeleteRange( transactionDetailQuery );

            _rockContext.SaveChanges();

            var transactionQuery = _transactionService.Queryable().Where( fb => fb.ForeignKey == KEY );
            _transactionService.DeleteRange( transactionQuery );

            _rockContext.SaveChanges();

            var batchQuery = _batchService.Queryable().Where( fb => fb.ForeignKey == KEY );
            _batchService.DeleteRange( batchQuery );

            _rockContext.SaveChanges();

            var accountGuid = new List<Guid>
            {
                GivingAccountParentGuidString.AsGuid(),
                GivingAccountChild1GuidString.AsGuid(),
                GivingAccountChild2GuidString.AsGuid()
            };
            var accountQuery = _accountService.Queryable().Where( fa => accountGuid.Contains( fa.Guid ) );
            _accountService.DeleteRange( accountQuery );

            _rockContext.SaveChanges();

            var achievementType = new AchievementTypeService( _rockContext );
            achievementType.DeleteRange( achievementType.Queryable().Where( at => _achievementIds.Contains( at.Id ) || at.ForeignKey == KEY ) );

            _rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates the achievement type data.
        /// </summary>
        private static void CreateAchievementTypeData()
        {
            // Create the component so it sets up the attributes.
            AchievementContainer.Instance.Refresh();
            _ = AchievementContainer.GetComponent( ComponentEntityTypeName );

            // Single Achievement Types
            var achievementSingleA1 = CreateAchievementType( "Test Giving Achievement Single, Accumulate 1", GivingAccountParentGuidString, false );
            _achievementIds.Insert( ( int ) GivingTestTypes.SingleAchievementAccumulate1, achievementSingleA1.Id );
            var achievementSingleA5 = CreateAchievementType( "Test Giving Achievement Single, Accumulate 5", GivingAccountChild2GuidString, true, numberToAccumulate: 5 );
            _achievementIds.Insert( ( int ) GivingTestTypes.SingleAchievementAccumulate5, achievementSingleA5.Id );
            var achievementSingleA52 = CreateAchievementType( "Test Giving Achievement Single, Accumulate 52", GivingAccountChild1GuidString, false, numberToAccumulate: 52 );
            _achievementIds.Insert( ( int ) GivingTestTypes.SingleAchievementAccumulate52, achievementSingleA52.Id );

            // Multi-Achievement Types
            var achievementMulti5A1 = CreateAchievementType( "Test Giving Achievement Multi-5, Accumulate 1", GivingAccountParentGuidString, false, maxAllowed: 5 );
            _achievementIds.Insert( ( int ) GivingTestTypes.MultiAchievementAccumulate1, achievementMulti5A1.Id );
            var achievementMulti5A5 = CreateAchievementType( "Test Giving Achievement Multi-5, Accumulate 5", GivingAccountChild1GuidString, true, maxAllowed: 10, numberToAccumulate: 5 );
            _achievementIds.Insert( ( int ) GivingTestTypes.MultiAchievementAccumulate5, achievementMulti5A5.Id );

            // Overachievement Type
            var achievementMultiOver = CreateAchievementType( "Test Giving Achievement Multi-Over", GivingAccountChild2GuidString, true, allowOver: true );
            _achievementIds.Insert( ( int ) GivingTestTypes.OverAchievement, achievementMultiOver.Id );

            // Inactive Achievement Type
            var achievementInactive = CreateAchievementType( "Test Giving Achievement Inactive", GivingAccountChild1GuidString, false, isActive: false );
            _achievementIds.Insert( ( int ) GivingTestTypes.InactiveAchievement, achievementInactive.Id );
        }

        private static AchievementType CreateAchievementType(
            string name,
            string accountGuid,
            bool includeChildAccounts = false,
            bool isActive = true,
            int maxAllowed = 1,
            bool allowOver = false,
            int numberToAccumulate = 1)
        {
            var achievement = new AchievementType
            {
                Name = name,
                IsActive = isActive,
                ComponentEntityTypeId = EntityTypeCache.GetId( ComponentEntityTypeName ) ?? 0,
                MaxAccomplishmentsAllowed = maxAllowed,
                AllowOverAchievement = allowOver,
                ComponentConfigJson = "{ \"GivingToAccountAchievement\": \"null\" }",
                ForeignKey = KEY
            };

            _achievementTypeService.Add( achievement );
            _rockContext.SaveChanges( true );

            achievement.LoadAttributes();
            achievement.SetAttributeValue( Rock.Achievement.Component.GivingToAccountAchievement.AttributeKey.IncludeChildFinancialAccounts, includeChildAccounts.ToString() );
            achievement.SetAttributeValue( Rock.Achievement.Component.GivingToAccountAchievement.AttributeKey.FinancialAccount, accountGuid );
            achievement.SetAttributeValue( Rock.Achievement.Component.GivingToAccountAchievement.AttributeKey.StartDateTime, _startDate.ToISO8601DateString() );
            achievement.SetAttributeValue( Rock.Achievement.Component.GivingToAccountAchievement.AttributeKey.EndDateTime, _endDate.ToISO8601DateString() );
            achievement.SetAttributeValue( Rock.Achievement.Component.GivingToAccountAchievement.AttributeKey.NumberToAccumulate, numberToAccumulate.ToString() );
            achievement.SaveAttributeValues();

            return achievement;
        }

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            _rockContext = new RockContext();
            _accountService = new FinancialAccountService( _rockContext );
            _batchService = new FinancialBatchService( _rockContext );
            _transactionService = new FinancialTransactionService( _rockContext );
            _paymentService = new FinancialPaymentDetailService( _rockContext );
            _achievementTypeService = new AchievementTypeService( _rockContext );
            _definedValueService = new DefinedValueService( _rockContext );
            _definedTypeService = new DefinedTypeService( _rockContext );
            _transactionDetailService = new FinancialTransactionDetailService( _rockContext );

            DeleteTestData();
            PopulateDefinedValues();
            CreateFinancialTransactionData();
            CreateAchievementTypeData();
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            DeleteTestData();
            _rockContext = null;
            _accountService = null;
            _batchService = null;
            _transactionService = null;
            _paymentService = null;
            _achievementTypeService = null;
            _definedValueService = null;
            _definedTypeService = null;
            _transactionDetailService = null;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var service = new AchievementAttemptService( _rockContext );
            service.DeleteRange( service.Queryable().Where( saa => _achievementIds.Contains( saa.Id ) || saa.ForeignKey == KEY ) );
            _rockContext.SaveChanges();
        }

        #endregion

        private void GivingToAccountAchievementMain(
            int attemptCount,
            int accumulateCount,
            int achievementId,
            List<int> allowedAccountIds)
        {
            var attemptsQuery = new AchievementAttemptService( _rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.AchievementTypeId == achievementId && saa.AchieverEntityId == _personAliasId )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            var achievementTypeCache = AchievementTypeCache.Get( achievementId );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );
            var transactions = component.GetSourceEntitiesQuery( achievementTypeCache, _rockContext ).Where( i => i.ForeignKey == KEY ).ToList();

            foreach ( var sourceEntity in transactions )
            {
                var ft = ( FinancialTransaction ) sourceEntity;

                // Check all of the account ids to make sure they are allowed
                // This is making sure the hierarchy logic is working as expected
                var transactionAccountIds = ft.TransactionDetails.Select( d => d.AccountId ).Distinct();
                foreach ( var accountId in transactionAccountIds )
                {
                    Assert.That.IsTrue( allowedAccountIds.Contains( accountId ) );
                }

                // See Rock.Model.Engagement.AchievementType.AchievementTypeService
                // Process each streak in it's own data context to avoid the data context changes getting too big and slow
                using ( var rockContext = new RockContext() )
                {
                    component.Process( rockContext, achievementTypeCache, sourceEntity );
                    rockContext.SaveChanges();
                }
            }

            var attempts = attemptsQuery.ToList();
            Assert.That.IsNotNull( attempts );
            Assert.That.AreEqual( attemptCount, attempts.Count );

            if ( attemptCount > 0 )
            {
                for ( int i = 0; i < attempts.Count(); i++ )
                {
                    Assert.That.IsTrue( attempts[i].Progress >= 0m );

                    if (attempts[i].Progress >= 1m)
                    {
                        Assert.That.IsTrue( attempts[i].IsClosed );
                        Assert.That.IsTrue( attempts[i].IsSuccessful );
                    }
                    else
                    {
                        Assert.That.IsFalse( attempts[i].IsClosed );
                        Assert.That.IsFalse( attempts[i].IsSuccessful );
                    }
                }
            }
        }

        /// <summary>
        /// Tests GivingToAccountAchievement for a single achievement with 1 accumulation
        /// </summary>
        [TestMethod]
        public void GivingToAccountAchievementSingleAccumulate1()
        {
            var achievementId = _achievementIds[( int ) GivingTestTypes.SingleAchievementAccumulate1];
            GivingToAccountAchievementMain( 1, 1, achievementId, new List<int> { _accountIdParent } );
        }

        /// <summary>
        /// Tests GivingToAccountAchievement for a single achievement with 5 accumulations
        /// </summary>
        [TestMethod]
        public void GivingToAccountAchievementSingleAccumulate5()
        {
            var achievementId = _achievementIds[( int ) GivingTestTypes.SingleAchievementAccumulate5];
            GivingToAccountAchievementMain( 1, 5, achievementId, new List<int> { _accountIdChild2 } );
        }

        /// <summary>
        /// Tests GivingToAccountAchievement for a single achievement with 52 accumulations
        /// </summary>
        [TestMethod]
        public void GivingToAccountAchievementSingleAccumulate52()
        {
            var achievementId = _achievementIds[( int ) GivingTestTypes.SingleAchievementAccumulate52];
            GivingToAccountAchievementMain( 1, 52, achievementId, new List<int> { _accountIdChild1, _accountIdChild2 } );
        }

        /// <summary>
        /// Tests GivingToAccountAchievement for a 5 achievements with single accumulation.
        /// </summary>
        [TestMethod]
        public void GivingToAccountAchievementFiveByOne()
        {
            var achievementId = _achievementIds[( int ) GivingTestTypes.MultiAchievementAccumulate1];
            GivingToAccountAchievementMain( 5, 1, achievementId, new List<int> { _accountIdParent } );
        }

        /// <summary>
        /// Tests GivingToAccountAchievement for a 10 achievements with 5 accumulation.
        /// </summary>
        [TestMethod]
        public void GivingToAccountAchievementTenByFive()
        {
            var achievementId = _achievementIds[( int ) GivingTestTypes.MultiAchievementAccumulate5];
            GivingToAccountAchievementMain( ( TRANSACTION_COUNT_CHLID1 + TRANSACTION_COUNT_CHILD2 ) / 5, 5, achievementId, new List<int> { _accountIdChild1, _accountIdChild2 } );
        }

        /// <summary>
        /// Tests GivingToAccountAchievement for a over achievement
        /// </summary>
        [TestMethod]
        public void GivingToAccountAchievementOver()
        {
            var achievementId = _achievementIds[( int ) GivingTestTypes.OverAchievement];
            GivingToAccountAchievementMain( TRANSACTION_COUNT_CHILD2, 1, achievementId, new List<int> { _accountIdChild2 } );
        }

        /// <summary>
        /// Tests GivingToAccountAchievement for an inactive achievement
        /// </summary>
        [TestMethod]
        public void GivingToAccountAchievementInactive()
        {
            var achievementId = _achievementIds[( int ) GivingTestTypes.InactiveAchievement];
            GivingToAccountAchievementMain( 0, 0, achievementId, new List<int> { _accountIdChild1 } );
        }
    }
}
