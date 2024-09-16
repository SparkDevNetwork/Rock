using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Financial;
using Rock.ViewModels.Blocks.Crm.PersonDetail.GivingConfiguration;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Block used to view the scheduled transactions, saved accounts and pledges of a person.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// 
    [DisplayName( "Giving Configuration" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Block used to view the scheduled transactions, saved accounts and pledges of a person." )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Add Transaction Page",
        Key = AttributeKey.AddTransactionPage,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.ADD_TRANSACTION,
        Order = 0 )]

    [IntegerField(
        "Person Token Expire Minutes",
        Key = AttributeKey.PersonTokenExpireMinutes,
        Description = "The number of minutes the person token for the transaction is valid after it is issued.",
        IsRequired = true,
        DefaultIntegerValue = 60,
        Order = 1 )]

    [IntegerField(
        "Person Token Usage Limit",
        Key = AttributeKey.PersonTokenUsageLimit,
        Description = "The maximum number of times the person token for the transaction can be used.",
        IsRequired = false,
        DefaultIntegerValue = 1,
        Order = 2 )]

    [AccountsField(
        "Accounts",
        Key = AttributeKey.Accounts,
        Description = "A selection of accounts to use for checking if transactions for the current user exist.",
        IsRequired = false,
        Order = 3 )]

    [LinkedPage(
        "Pledge Detail Page",
        Key = AttributeKey.PledgeDetailPage,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.PLEDGE_DETAIL,
        Order = 4 )]

    [IntegerField(
        "Max Years To Display",
        Description = "The maximum number of years to display (including the current year).",
        IsRequired = true,
        DefaultIntegerValue = 3,
        Order = 5,
        Key = AttributeKey.MaxYearsToDisplay )]

    [LinkedPage(
        "Contribution Statement Detail Page",
        Description = "The contribution statement detail page.",
        Order = 6,
        DefaultValue = Rock.SystemGuid.Page.CONTRIBUTION_STATEMENT_PAGE,
        Key = AttributeKey.ContributionStatementDetailPage )]

    [LinkedPage(
        "Scheduled Transaction Detail Page",
        Key = AttributeKey.ScheduledTransactionDetailPage,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.SCHEDULED_TRANSACTION,
        Order = 7 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "6B977F51-4B33-44F3-A6FF-89FCC9D1AE08" )]
    [Rock.SystemGuid.BlockTypeGuid( "BBA3A660-9A8B-4707-A553-D314C21B0A12" )]
    public partial class GivingConfiguration : RockBlockType
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string AddTransactionPage = "AddTransactionPage";
            public const string PersonTokenExpireMinutes = "PersonTokenExpireMinutes";
            public const string PersonTokenUsageLimit = "PersonTokenUsageLimit";
            public const string Accounts = "Accounts";
            public const string ContributionStatementDetailPage = "ContributionStatementDetailPage";
            public const string MaxYearsToDisplay = "MaxYearsToDisplay";
            public const string PledgeDetailPage = "PledgeDetailPage";
            public const string ScheduledTransactionDetailPage = "ScheduledTransactionDetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string AddTransactionPage = "AddTransactionPage";
            public const string PersonTokenExpireMinutes = "PersonTokenExpireMinutes";
            public const string PersonTokenUsageLimit = "PersonTokenUsageLimit";
            public const string Accounts = "Accounts";
            public const string ContributionStatementDetailPage = "ContributionStatementDetailPage";
            public const string MaxYearsToDisplay = "MaxYearsToDisplay";
            public const string PledgeDetailPage = "PledgeDetailPage";
            public const string ScheduledTransactionDetailPage = "ScheduledTransactionDetailPage";
        }

        private static class PageParameterKey
        {
            public const string ScheduledTransactionGuid = "ScheduledTransactionGuid";
            public const string PersonActionIdentifier = "rckid";
            public const string PledgeId = "PledgeId";
            public const string StatementYear = "StatementYear";
            public const string AutoEdit = "autoEdit";
            public const string ReturnUrl = "returnUrl";
            public const string PersonId = "PersonId";
        }

        protected bool IsVisible { get; set; }

        private string personActionIdentifierTransaction = string.Empty;
        private string personActionIdentifierContribution = string.Empty;
        private string personActionIdentifierPledge = string.Empty;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var personId = GetInitialEntity();

            var box = new GivingConfigurationInitializationBag
            {
                IsVisible = personId.HasValue,
                SavedAccounts = GetSavedAccounts( personId ),
                DefaultFinancialAccount = GetDefaultFinancialAccount( personId ),
                DefaultSavedAccount = GetDefaultSavedAccount( personId ),
                DefaultSavedAccountName = GetDefaultSavedAccount( personId ) != null ? GetSavedAccountName( GetDefaultSavedAccount( personId ) ) : null,

            };

            box.NavigationUrls = GetBoxNavigationUrls();

            box.PersonActionIdentifierPledge = personActionIdentifierPledge;
            box.PersonActionIdentifierTransaction = personActionIdentifierTransaction;
            box.PersonActionIdentifierContribution = personActionIdentifierContribution;

            return box;
        }

        private List<FinancialPersonSavedAccountBag> GetSavedAccounts( int? personId )
        {
            if ( !personId.HasValue )
            {
                return new List<FinancialPersonSavedAccountBag>();
            }

            var supportedGatewayIds = GetSupportedGatewayIds();
            if ( supportedGatewayIds == null || !supportedGatewayIds.Any() )
            {
                return new List<FinancialPersonSavedAccountBag>();
            }

            var service = new FinancialPersonSavedAccountService( RockContext );

            var savedAccounts = service
                .GetByPersonId( personId.Value )
                .Include( sa => sa.FinancialPaymentDetail )
                .AsNoTracking()
                .Where( sa =>
                    sa.FinancialGatewayId.HasValue &&
                    supportedGatewayIds.Contains( sa.FinancialGatewayId.Value ) )
                .OrderBy( sa => sa.IsDefault )
                .ThenByDescending( sa => sa.CreatedDateTime )
                .ToList();

            return savedAccounts.Select( sa => new FinancialPersonSavedAccountBag
            {
                Id = sa.Id,
                Guid = sa.Guid,
                Name = sa.Name,
                IsDefault = sa.IsDefault,
                IsExpired = sa.FinancialPaymentDetail?.CardExpirationDate.HasValue == true && sa.FinancialPaymentDetail.CardExpirationDate.Value < RockDateTime.Now,
                FinancialPaymentDetail = new FinancialPaymentDetailBag
                {
                    CurrencyType = sa.FinancialPaymentDetail?.CurrencyTypeValue?.Value,
                    CreditCardType = sa.FinancialPaymentDetail?.CreditCardTypeValue?.Value,
                    AccountNumberMasked = sa.FinancialPaymentDetail?.AccountNumberMasked,
                    ExpirationDate = sa.FinancialPaymentDetail?.ExpirationDate,
                    CardExpirationDate = sa.FinancialPaymentDetail?.CardExpirationDate
                }
            } ).ToList();
        }

        private FinancialPersonSavedAccountBag GetDefaultSavedAccount( int? personId )
        {
            var savedAccounts = GetSavedAccounts( personId );
            return savedAccounts?.FirstOrDefault( sa => sa.IsDefault );
        }

        private FinancialAccountBag GetDefaultFinancialAccount( int? personId )
        {
            if ( !personId.HasValue )
            {
                return null;
            }

            var financialAccount = new PersonService( RockContext ).Get( personId.Value )?.ContributionFinancialAccount;

            if ( financialAccount != null )
            {
                return new FinancialAccountBag
                {
                    Id = financialAccount.Id,
                    PublicName = financialAccount.PublicName
                };
            }

            return null;
        }

        #endregion

        #region Internal Methods

        private string GetPersonActionIdentifiers()
        {
            var personId = GetInitialEntity();


            if ( personId.HasValue )
            {
                var person = new PersonService( RockContext ).Get( personId.Value );
                if ( person != null )
                {
                    personActionIdentifierTransaction = person.GetPersonActionIdentifier( "transaction" );
                    personActionIdentifierContribution = person.GetPersonActionIdentifier( "contribution-statement" );
                    personActionIdentifierPledge = person.GetPersonActionIdentifier( "pledge" );
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            GetPersonActionIdentifiers();
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.AddTransactionPage] = this.GetLinkedPageUrl( AttributeKey.AddTransactionPage, new Dictionary<string, string>
        {
            { PageParameterKey.PersonActionIdentifier, personActionIdentifierTransaction }
        } ),
                [NavigationUrlKey.PledgeDetailPage] = this.GetLinkedPageUrl( AttributeKey.PledgeDetailPage ),
                [NavigationUrlKey.ContributionStatementDetailPage] = this.GetLinkedPageUrl( AttributeKey.ContributionStatementDetailPage, new Dictionary<string, string>
        {
            { PageParameterKey.PersonActionIdentifier, personActionIdentifierContribution }
        } ),
                [NavigationUrlKey.ScheduledTransactionDetailPage] = this.GetLinkedPageUrl( AttributeKey.ScheduledTransactionDetailPage, new Dictionary<string, string>
        {
            { PageParameterKey.ScheduledTransactionGuid, string.Empty }
        } )
            };
        }

        private List<int> GetSupportedGatewayIds()
        {
            var gatewayService = new FinancialGatewayService( RockContext );
            var activeGatewayEntityTypes = gatewayService.Queryable( "EntityType" )
                .AsNoTracking()
                .Where( fg => fg.IsActive )
                .GroupBy( fg => fg.EntityType )
                .ToList();

            var supportedTypes = Rock.Reflection.FindTypes( typeof( IAutomatedGatewayComponent ) );
            var supportedGatewayIds = new List<int>();

            foreach ( var entityType in activeGatewayEntityTypes )
            {
                if ( supportedTypes.Any( t => t.Value.FullName == entityType.Key.Name ) )
                {
                    supportedGatewayIds.AddRange( entityType.Select( fg => fg.Id ) );
                }
            }

            return supportedGatewayIds;
        }

        private string GetSavedAccountName( FinancialPersonSavedAccountBag savedAccount )
        {
            const string unnamed = "<Unnamed>";

            if ( savedAccount == null )
            {
                return unnamed;
            }

            var name = savedAccount.Name.IsNullOrWhiteSpace() ? unnamed : savedAccount.Name.Trim();

            if ( savedAccount.FinancialPaymentDetail != null )
            {
                var expirationDate = savedAccount.FinancialPaymentDetail.ExpirationDate;

                if ( !string.IsNullOrEmpty( expirationDate ) )
                {
                    name += $" ({expirationDate})";
                }
            }

            return name;
        }

        private int? GetInitialEntity()
        {
            var personId = PageParameter( "PersonId" ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                return personId;
            }

            var businessId = PageParameter( "BusinessId" ).AsIntegerOrNull();
            if ( businessId.HasValue )
            {
                return businessId;
            }

            return null;
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult SaveTextToGiveSettings( TextToGiveSettingsBag settings )
        {
            var personId = GetInitialEntity();

            if ( personId.HasValue )
            {
                var personService = new PersonService( RockContext );
                var financialAccountService = new FinancialAccountService( RockContext );

                int? savedAccountId = null;
                Guid? financialAccountGuid = null;
                int? financialAccountId = null;

                if ( !string.IsNullOrEmpty( settings.SelectedSavedAccountId ) )
                {
                    savedAccountId = settings.SelectedSavedAccountId.AsIntegerOrNull();
                }

                if ( !string.IsNullOrEmpty( settings.SelectedFinancialAccountId ) )
                {
                    financialAccountGuid = settings.SelectedFinancialAccountId.AsGuidOrNull();

                    if ( financialAccountGuid.HasValue )
                    {
                        financialAccountId = financialAccountService.GetId( financialAccountGuid.Value );
                    }

                }

                personService.ConfigureTextToGive( personId.Value, financialAccountId, savedAccountId, out _ );

                RockContext.SaveChanges();
            }

            return ActionOk();
        }

        [BlockAction]
        public BlockActionResult GetTextToGiveDetails()
        {
            var personId = GetInitialEntity();
            if ( !personId.HasValue )
            {
                return ActionBadRequest( "Invalid Person Id" );
            }

            var defaultSavedAccount = GetDefaultSavedAccount( personId );

            var personService = new PersonService( RockContext );
            var person = personService.Get( personId.Value );

            var defaultFinancialAccount = person.ContributionFinancialAccount;

            var details = new
            {
                DefaultAccountName = defaultFinancialAccount?.PublicName ?? "None",
                SavedAccountName = defaultSavedAccount != null ? GetSavedAccountName( defaultSavedAccount ) : "None",
                DefaultFinancialAccountGuid = defaultFinancialAccount?.Guid.ToString(),
                SavedAccountId = defaultSavedAccount?.Id.ToString()
            };

            return ActionOk( details );
        }

        [BlockAction]
        public BlockActionResult GetScheduledTransactions( bool includeInactive )
        {
            var personId = GetInitialEntity();

            if ( !personId.HasValue )
            {
                return ActionOk( new { Transactions = new List<FinancialScheduledTransactionBag>(), HasInactiveTransactions = false } );
            }

            var personService = new PersonService( RockContext );
            var person = personService.Get( personId.Value );
            var givingGroupId = person.GivingGroupId;

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( RockContext );
            var qry = financialScheduledTransactionService
                .Queryable( "ScheduledTransactionDetails,FinancialPaymentDetail.CurrencyTypeValue,FinancialPaymentDetail.CreditCardTypeValue" )
                .AsNoTracking();

            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                qry = qry.Where( t => t.ScheduledTransactionDetails.Any( d => accountGuids.Contains( d.Account.Guid ) ) );
            }

            if ( givingGroupId.HasValue )
            {
                // Person contributes with family
                qry = qry.Where( t => t.AuthorizedPersonAlias.Person.GivingGroupId == givingGroupId );
            }
            else
            {
                // Person contributes individually
                qry = qry.Where( t => t.AuthorizedPersonAlias.PersonId == personId.Value );
            }

            var hasInactiveTransactions = qry.Any( t => !t.IsActive );

            if ( !includeInactive )
            {
                qry = qry.Where( t => t.IsActive );
            }

            qry = qry
                .OrderBy( t => t.AuthorizedPersonAlias.Person.LastName )
                .ThenBy( t => t.AuthorizedPersonAlias.Person.NickName )
                .ThenByDescending( t => t.IsActive )
                .ThenByDescending( t => t.StartDate );

            var scheduledTransactions = qry.ToList();
            financialScheduledTransactionService.GetStatus( scheduledTransactions, true );

            var transactionBags = scheduledTransactions.Select( st => new FinancialScheduledTransactionBag
            {
                Id = st.Id,
                Guid = st.Guid,
                IsActive = st.IsActive,
                StartDate = st.StartDate,
                AuthorizedPersonAlias = new PersonAliasBag
                {
                    Id = st.AuthorizedPersonAlias.Id,
                    PersonId = st.AuthorizedPersonAlias.PersonId,
                    Person = new PersonBag
                    {
                        Id = st.AuthorizedPersonAlias.Person.Id,
                        LastName = st.AuthorizedPersonAlias.Person.LastName,
                        NickName = st.AuthorizedPersonAlias.Person.NickName
                    }
                },
                ScheduledTransactionDetails = st.ScheduledTransactionDetails.Select( std => new FinancialScheduledTransactionDetailBag
                {
                    Account = new FinancialAccountBag
                    {
                        Id = std.Account.Id,
                        PublicName = std.Account.PublicName
                    }
                } ).ToList(),
                AccountSummary = st.ScheduledTransactionDetails
            .Select( d => new AccountSummaryBag
            {
                IsOther = accountGuids.Any() && !accountGuids.Contains( d.Account.Guid ),
                Order = d.Account.Order,
                Name = d.Account.Name
            } )
            .OrderBy( d => d.IsOther )
            .ThenBy( d => d.Order )
            .Select( d => d.IsOther ? "Other" : d.Name )
            .ToList(),
                NextPaymentDate = st.NextPaymentDate,
                TotalAmount = st.TotalAmount,
                ForeignCurrencyCodeValueId = st.ForeignCurrencyCodeValueId,
                FrequencyText = st.TransactionFrequencyValue?.Value,
                FinancialPaymentDetail = new FinancialPaymentDetailBag
                {
                    CurrencyType = st.FinancialPaymentDetail?.CurrencyTypeValue?.Value,
                    CreditCardType = st.FinancialPaymentDetail?.CreditCardTypeValue?.Value,
                    AccountNumberMasked = st.FinancialPaymentDetail?.AccountNumberMasked,
                    ExpirationDate = st.FinancialPaymentDetail?.ExpirationDate
                },
                SavedAccountName = st.FinancialPaymentDetail?.FinancialPersonSavedAccount?.Name
            } ).ToList();

            return ActionOk( new { Transactions = transactionBags, HasInactiveTransactions = hasInactiveTransactions } );
        }

        [BlockAction]
        public BlockActionResult InactivateScheduledTransaction( Guid scheduledTransactionGuid )
        {
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( RockContext );
            var financialScheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionGuid );

            if ( financialScheduledTransaction?.FinancialGateway == null )
            {
                return ActionBadRequest( "Scheduled transaction not found or invalid." );
            }

            string errorMessage;
            if ( !financialScheduledTransactionService.Cancel( financialScheduledTransaction, out errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            try
            {
                financialScheduledTransactionService.GetStatus( financialScheduledTransaction, out errorMessage );
            }
            catch
            {
                // Ignore
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        [BlockAction]
        public BlockActionResult GetPledges()
        {
            var personId = GetInitialEntity();

            if ( !personId.HasValue )
            {
                return ActionOk( new List<FinancialPledge>() );
            }

            var pledgeService = new FinancialPledgeService( RockContext );
            var pledgesQry = pledgeService.Queryable( "Account, PersonAlias.Person" );

            if ( pledgesQry == null )
            {
                return ActionBadRequest( "Pledges query returned null" );
            }

            pledgesQry = pledgesQry.Where( p => p.PersonAlias.PersonId == personId.Value );

            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                pledgesQry = pledgesQry.Where( p => accountGuids.Contains( p.Account.Guid ) );
            }

            var pledges = pledgesQry.ToList();

            if ( pledges == null )
            {
                return ActionBadRequest( "Pledges list returned null." );
            }

            return ActionOk( pledges.Select( p => new FinancialPledgeBag
            {
                Guid = p.Guid,
                Id = p.Id,
                TotalAmount = p.TotalAmount,
                Account = new FinancialAccountBag
                {
                    Id = p.Account.Id,
                    PublicName = p.Account.PublicName,
                },
                PersonAlias = new PersonAliasBag
                {
                    Id = p.PersonAlias.Id,
                    PersonId = p.PersonAlias.PersonId,
                    Person = new PersonBag
                    {
                        Id = p.PersonAlias.Person.Id,
                        LastName = p.PersonAlias.Person.LastName,
                        NickName = p.PersonAlias.Person.NickName
                    }
                },
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                PledgeFrequencyValueId = p.PledgeFrequencyValueId,
                PledgeFrequencyValue = p.PledgeFrequencyValue?.Value
            } ).ToList() );
        }

        [BlockAction]
        public BlockActionResult DeletePledge( Guid pledgeGuid )
        {
            var pledgeService = new FinancialPledgeService( RockContext );
            var pledge = pledgeService.Get( pledgeGuid );

            if ( pledge == null )
            {
                return ActionBadRequest( "Pledge not found." );
            }

            string errorMessage;
            if ( !pledgeService.CanDelete( pledge, out errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            pledgeService.Delete( pledge );
            RockContext.SaveChanges();

            return ActionOk();
        }

        [BlockAction]
        public BlockActionResult GetContributionStatements()
        {
            var personId = GetInitialEntity();

            if ( !personId.HasValue )
            {
                return ActionOk( new List<ContributionStatementBag>() );
            }

            var numberOfYears = GetAttributeValue( AttributeKey.MaxYearsToDisplay ).AsInteger();

            var financialTransactionDetailService = new FinancialTransactionDetailService( RockContext );
            var personAliasIds = new PersonAliasService( RockContext )
                .Queryable()
                .Where( a => a.PersonId == personId.Value )
                .Select( a => a.Id )
                .ToList();

            var qry = financialTransactionDetailService.Queryable().AsNoTracking()
                .Where( t =>
                    t.Transaction.AuthorizedPersonAliasId.HasValue &&
                    personAliasIds.Contains( t.Transaction.AuthorizedPersonAliasId.Value ) &&
                    t.Transaction.TransactionDateTime.HasValue );

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.Accounts ) ) )
            {
                qry = qry.Where( t => t.Account.IsTaxDeductible );
            }
            else
            {
                var accountGuids = GetAttributeValue( AttributeKey.Accounts ).Split( ',' ).Select( Guid.Parse ).ToList();
                qry = qry.Where( t => accountGuids.Contains( t.Account.Guid ) );
            }

            var yearQry = qry.GroupBy( t => t.Transaction.TransactionDateTime.Value.Year )
                .Select( g => g.Key )
                .OrderByDescending( y => y );

            var statementYears = yearQry.Take( numberOfYears ).ToList();

            return ActionOk( statementYears.Select( year => new ContributionStatementBag
            {
                Year = year,
                IsCurrentYear = year == RockDateTime.Now.Year
            } ).ToList() );
        }

        [BlockAction]
        public BlockActionResult DeleteSavedAccount( Guid savedAccountGuid )
        {
            var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( RockContext );
            var financialPersonSavedAccount = financialPersonSavedAccountService.Get( savedAccountGuid );

            if ( financialPersonSavedAccount != null )
            {
                string errorMessage;
                if ( !financialPersonSavedAccountService.CanDelete( financialPersonSavedAccount, out errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                financialPersonSavedAccountService.Delete( financialPersonSavedAccount );
                RockContext.SaveChanges();
            }

            return ActionOk();
        }

        #endregion
    }
}