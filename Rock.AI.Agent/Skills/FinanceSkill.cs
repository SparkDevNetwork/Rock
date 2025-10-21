using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.FinanceSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    [Description( "This skill provides an overview of connection features." )]
    [AgentSkillGuid( "4FC57368-8362-49F0-A1A2-EBC9EFDD947C" )]
    [EntityTypeGuid( "92C9469F-C158-4476-8854-EF4805EA0970" )]
    [AgentUsage( "For analytical requests, prefer the SummarizeFinancialTransactions tool. Use ListFinancialTransactions for raw transaction information when explicitly requested." )]
    internal sealed class FinanceSkill : AgentSkillComponent
    {
        #region Fields

        private readonly ILogger<FinanceSkill> _logger;
        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FinanceSkill"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        /// <param name="rockContextFactory">Factory used to create Rock data contexts.</param>
        public FinanceSkill( ILogger<FinanceSkill> logger, IRockContextFactory rockContextFactory )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
        }

        #endregion

        #region Tools

        /// <summary>
        /// Returns the list of <see cref="DefinedValue"/> items for the specified defined type when the supplied
        /// <paramref name="lookupKey"/> equals the literal "lookup". Otherwise returns <c>null</c> so the caller
        /// knows to continue normal processing.
        /// </summary>
        /// <param name="rockContext">The Rock data context.</param>
        /// <param name="definedTypeGuid">The defined type Guid (as string) to resolve.</param>
        /// <param name="lookupKey">The user supplied value which may request a lookup.</param>
        /// <returns>A collection of <see cref="KeyNameResult"/> for selection or <c>null</c>.</returns>
        private List<KeyNameResult> TryGetDefinedValueLookup( RockContext rockContext, string definedTypeGuid, string lookupKey )
        {
            if ( lookupKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( !lookupKey.Equals( "lookup", StringComparison.OrdinalIgnoreCase ) )
            {
                return null;
            }

            var paymentMethodDvs = DefinedTypeCache.Get( definedTypeGuid.AsGuid(), rockContext )
                ?.DefinedValues
                .Select( dv => new KeyNameResult
                {
                    IdKey = dv.IdKey,
                    Name = dv.Value
                } )
                .ToList();

            return paymentMethodDvs;
        }

        /// <summary>
        /// Returns active financial accounts (funds) that can be used for filtering or selection in other tools.
        /// </summary>
        /// <returns>A <see cref="RockToolResult"/> containing the accounts or <c>NoData</c> if none.</returns>
        [AgentToolGuid( "4DBAE64C-A7B9-4826-90C0-8DE4AA598FFF" )]
        public RockToolResult LookupFinancialAccounts()
        {
            using var rockContext = _rockContextFactory.CreateRockContext();

            // Load all top-level active accounts.
            var topLevelAccounts = FinancialAccountCache
                .All()
                .Where( a => a.IsActive && a.ParentAccountId == null );

            // Build hierarchical tree.
            var parentAccountResults = new List<FinancialAccountResult>();

            foreach ( var acct in topLevelAccounts )
            {
                var result = new FinancialAccountResult
                {
                    Id = acct.Id,
                    IsTaxDeductible = acct.IsTaxDeductible,
                    Name = acct.PublicName,
                    PublicDescription = acct.PublicDescription,
                    Campus = acct.CampusId.HasValue ? new CampusResult
                    {
                        Id = acct.CampusId.Value,
                        Name = acct.Campus.Name
                    } : null
                };

                var childAccts = acct.GetDescendentFinancialAccounts()
                    .Where( childAcct => childAcct.IsActive );

                foreach ( var childAcct in childAccts )
                {
                    if ( result.Children.Any( c => c.Id == childAcct.Id ) )
                    {
                        continue;
                    }

                    result.Children.Add( new FinancialAccountResult
                    {
                        Id = childAcct.Id,
                        IsTaxDeductible = childAcct.IsTaxDeductible,
                        Name = childAcct.PublicName,
                        PublicDescription = childAcct.PublicDescription,
                        ParentAccountIdKey = IdHasher.Instance.GetHash( childAcct.ParentAccountId ?? 0 ),
                        Campus = childAcct.CampusId.HasValue ? new CampusResult
                        {
                            Id = childAcct.CampusId.Value,
                            Name = childAcct.Campus.Name
                        } : null
                    } );
                }

                parentAccountResults.Add( result );
            }

            // Flatten the tree for history (a single list of all accounts + children).
            if ( !parentAccountResults.Any() )
            {
                return RockToolResult.NoData();
            }

            var trimmedForHistory = new List<object>();

            foreach ( var parent in parentAccountResults )
            {
                trimmedForHistory.Add( new
                {
                    parent.IdKey,
                    parent.Name,
                    parent.IsTaxDeductible,
                    parent.PublicDescription,
                } );

                foreach ( var child in parent.Children )
                {
                    trimmedForHistory.Add( new
                    {
                        child.IdKey,
                        child.Name,
                        child.IsTaxDeductible,
                        child.PublicDescription,
                        child.ParentAccountIdKey
                    } );
                }
            }

            return RockToolResult.Success( parentAccountResults )
                .WithHistoryContent( trimmedForHistory, "financial-accounts" );
        }

        private List<FinancialAccountCache> GetFinancialAccountsForQuery( List<string> originalAccountIds, string campusId, RockContext rockContext )
        {
            // The filtering for accounts will be handled as such:
            // A. If no accounts are specified, but a campus is specified, find all accounts for that campus. 
            // B. If accounts are specified, and no campus is specified, find all parent accounts. If a parent account has `Uses Campus Child Accounts` enabled, include all child accounts.
            // C. If both accounts and campus are specified, find all parent accounts. If a parent account has `Uses Campus Child Accounts` enabled, include only child accounts for the specified campus.
            if ( campusId.IsNullOrWhiteSpace() && !originalAccountIds.Any() )
            {
                return new List<FinancialAccountCache>();
            }

            var results = new List<FinancialAccountCache>();

            // Case A: No accounts specified, campus specified.
            if ( !originalAccountIds.Any() && campusId.IsNotNullOrWhiteSpace() )
            {
                var campusIntId = IdHasher.Instance.GetId( campusId );

                results = FinancialAccountCache.All()
                    .Where( a => a.IsActive && a.CampusId.HasValue && a.CampusId.Value == campusIntId )
                    .ToList();
            }
            // Case B: Accounts specified, no campus specified.
            else if ( originalAccountIds.Any() && campusId.IsNullOrWhiteSpace() )
            {
                // Decode multiple account ids (ignore invalid keys).
                var accountIds = originalAccountIds.Where( k => k.IsNotNullOrWhiteSpace() )
                    .Select( k => IdHasher.Instance.GetId( k ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .Distinct()
                    .ToList();

                var accounts = new List<FinancialAccountCache>();

                foreach ( var acctId in accountIds )
                {
                    var acct = FinancialAccountCache.Get( acctId, rockContext );
                    if ( acct == null )
                    {
                        continue;
                    }
                    accounts.Add( acct );

                    // Only include child accounts if Uses Campus Child Accounts is enabled.
                    if ( acct.UsesCampusChildAccounts )
                    {
                        var children = acct.ChildAccounts;

                        foreach ( var child in children )
                        {
                            var childAcct = FinancialAccountCache.Get( child.Id, rockContext );
                            if ( childAcct != null && !accounts.Any( a => a.Id == childAcct.Id ) )
                            {
                                accounts.Add( childAcct );
                            }
                        }
                    }
                }

                results = accounts;
            }
            // Case C: Both accounts and campus specified.
            else
            {
                var campusIntId = IdHasher.Instance.GetId( campusId );

                // Decode multiple account ids (ignore invalid keys).
                var accountIds = originalAccountIds.Where( k => k.IsNotNullOrWhiteSpace() )
                    .Select( k => IdHasher.Instance.GetId( k ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .Distinct()
                    .ToList();

                var accounts = new List<FinancialAccountCache>();

                foreach ( var acctId in accountIds )
                {
                    var acct = FinancialAccountCache.Get( acctId, rockContext );
                    if ( acct == null )
                    {
                        continue;
                    }
                    accounts.Add( acct );
                    // Only include child accounts for the specified campus if Uses Campus Child Accounts is enabled.
                    if ( acct.UsesCampusChildAccounts )
                    {
                        var children = acct.ChildAccounts
                            .Where( ca => ca.CampusId.HasValue && ca.CampusId.Value == campusIntId );

                        foreach ( var child in children )
                        {
                            var childAcct = FinancialAccountCache.Get( child.Id, rockContext );

                            if ( childAcct != null && !accounts.Any( a => a.Id == childAcct.Id ) )
                            {
                                accounts.Add( childAcct );
                            }
                        }
                    }
                }

                results = accounts;
            }

            return results;
        }

        /// <summary>
        /// Produces an analytic summary of financial transactions matching the supplied optional filters.
        /// Includes descriptive statistics (count, total, mean, median, standard deviation) and breakdowns by
        /// fund (account) and payment method (currency type). When a *ValueIdKey argument equals "lookup" an
        /// instructional error is returned containing selectable values instead of analytics.
        /// </summary>
        /// <param name="personIdKey">Optional Person IdKey to restrict to transactions authorized by that person.</param>
        /// <param name="campusIdKey">Optional Campus (Batch Campus) IdKey.</param>
        /// <param name="accountIdKeys">Optional Account/Fund IdKey. When supplied only amounts contributed to this fund are counted in statistics.</param>
        /// <param name="paymentMethodTypeValueIdKey">Optional currency / tender defined value IdKey or the literal "lookup".</param>
        /// <param name="startDate">Inclusive start date filter.</param>
        /// <param name="endDate">Inclusive end date filter.</param>
        /// <returns>Analytics wrapped in <see cref="FinancialTransactionSummaryResult"/>.</returns>
        [AgentToolGuid( "8AE2C3D2-6965-47E2-AC82-0D422A1EF2FC" )]
        [AgentUsage( "Any argument ending with 'ValueIdKey' must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the chosen IdKey." )]
        [AgentUsage( "Only provide a personIdKey if the request is about a specific person. Do not assume that the current person should be used." )]
        [AgentToolReturnDescription( "Summary of matching transactions: count, total, average, median, and std-dev of per-transaction amounts. Includes fund and payment-type breakdowns with amount, share of total, and contributing-transaction counts." )]
        public RockToolResult SummarizeFinancialTransactions(
            string personIdKey = null,
            string campusIdKey = null,
            List<string> accountIdKeys = null,
            string paymentMethodTypeValueIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null )
        {
            using var rockContext = _rockContextFactory.CreateRockContext();

            // Handle "lookup" for currency type defined values.
            if ( TryGetDefinedValueLookup( rockContext, Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, paymentMethodTypeValueIdKey ) is List<KeyNameResult> lookups )
            {
                return RockToolResult.Error( "Lookups Required" )
                    .WithContent( lookups )
                    .WithInstructions( "Use the following data to determine the proper IdKey for the tool." );
            }

            // Decode IdKeys.
            var personId = personIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( personIdKey ) : null;
            var campusId = campusIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( campusIdKey ) : null;
            var paymentMethodTypeId = paymentMethodTypeValueIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( paymentMethodTypeValueIdKey ) : null;


            var options = new FinancialTransactionQueryOptions
            {
                PersonId = personId,
                //BatchCampusId = campusId,
                PaymentMethodTypeId = paymentMethodTypeId,
                StartDate = startDate,
                EndDate = endDate
            };

            // Base transaction scope (no AccountId filter here on purpose).
            var baseQry = GetFinancialTransactionsQueryable( rockContext, options )
                .AsNoTracking();

            List<int> accountIds = null;
            if ( accountIdKeys?.Any() ?? false || campusIdKey.IsNotNullOrWhiteSpace() )
            {
                accountIds = GetFinancialAccountsForQuery( accountIdKeys ?? new List<string>(), campusIdKey, rockContext )
                    .Select( a => a.Id )
                    .ToList();

                if ( !accountIds.Any() )
                {
                    return RockToolResult.NoData()
                        .WithInstructions( "No active financial accounts matched the supplied accountIdKeys and/or campusIdKey." );
                }
            }

            var hasAccountFilter = accountIds?.Any() == true;

            // Project per-transaction, with detail amount filtered by provided account ids if any.
            var txAggQry = baseQry.Select( t => new
            {
                t.Id,
                t.TransactionDateTime,
                CurrencyTypeId = ( int? ) t.FinancialPaymentDetail.CurrencyTypeValueId,
                CurrencyType = t.FinancialPaymentDetail.CurrencyTypeValue != null
                    ? t.FinancialPaymentDetail.CurrencyTypeValue.Value
                    : "Unknown",
                AmountFiltered = ( hasAccountFilter
                    ? t.TransactionDetails
                        .Where( d => accountIds.Contains( d.AccountId ) )
                        .Select( d => ( decimal? ) d.Amount )
                        .Sum()
                    : t.TransactionDetails
                        .Select( d => ( decimal? ) d.Amount )
                        .Sum() ) ?? 0m
            } );

            // Materialize once for stats and currency breakdown.
            var txAgg = txAggQry.ToList();

            // Effective set for stats: if filtering by accounts only include transactions that contributed (>0), else all.
            var effectiveAmounts = ( hasAccountFilter
                ? txAgg.Where( x => x.AmountFiltered > 0m )
                : txAgg ).Select( x => x.AmountFiltered ).ToList();

            var uniqueTransactionCount = effectiveAmounts.Count;
            var totalAmount = effectiveAmounts.Sum();
            decimal averageAmount = 0m, medianAmount = 0m, stdDeviationAmount = 0m;

            if ( uniqueTransactionCount > 0 )
            {
                averageAmount = decimal.Round( effectiveAmounts.Average(), 2 );
                var ordered = effectiveAmounts.OrderBy( a => a ).ToList();
                var mid = ordered.Count / 2;
                medianAmount = ordered.Count % 2 == 1
                    ? ordered[mid]
                    : decimal.Round( ( ordered[mid - 1] + ordered[mid] ) / 2m, 2 );
                var meanD = ( double ) averageAmount;
                var variance = ordered.Sum( a => Math.Pow( ( double ) a - meanD, 2 ) ) / ordered.Count;
                stdDeviationAmount = ( decimal ) Math.Round( Math.Sqrt( variance ), 2 );
            }

            // Fund (account) rollup detail level honoring multi-account filter if provided.
            var detailProj = baseQry
                .SelectMany( t => t.TransactionDetails.Select( d => new
                {
                    TransactionId = t.Id,
                    AccountId = ( int? ) d.AccountId,
                    AccountName = d.Account != null ? d.Account.Name : "Unknown",
                    Amount = ( decimal? ) d.Amount ?? 0m
                } ) )
                .Where( x => x.AccountId != null && ( !hasAccountFilter || accountIds.Contains( x.AccountId.Value ) ) );

            var fundRows = detailProj
                .GroupBy( x => new { x.AccountId, x.AccountName } )
                .Select( g => new
                {
                    g.Key.AccountId,
                    g.Key.AccountName,
                    TotalAmount = g.Sum( x => x.Amount ),
                    UniqueTransactionCount = g.Select( x => x.TransactionId ).Distinct().Count()
                } )
                .OrderByDescending( x => x.TotalAmount )
                .ToList();

            var denom = totalAmount == 0m ? 1m : totalAmount;
            var funds = fundRows.Select( fr => new FundBreakdown
            {
                IdKey = IdHasher.Instance.GetHash( fr.AccountId!.Value ),
                Name = fr.AccountName ?? "Unknown",
                TotalAmount = fr.TotalAmount,
                PercentOfTotal = fr.TotalAmount / denom,
                UniqueTransactionCount = fr.UniqueTransactionCount
            } ).ToList();

            // Currency/tender breakdown — count only contributing (>0) transactions.
            var currencyTypeRows = txAgg
                .GroupBy( x => new { x.CurrencyTypeId, x.CurrencyType } )
                .Select( g => new
                {
                    Type = g.Key.CurrencyType ?? "Unknown",
                    UniqueTransactionCount = g.Count( x => x.AmountFiltered > 0m ),
                    TotalAmount = g.Where( x => x.AmountFiltered > 0m ).Sum( x => x.AmountFiltered )
                } )
                .OrderByDescending( r => r.TotalAmount )
                .ToList();

            var currencyTypes = currencyTypeRows.Select( r => new CurrencyTypeBreakdown
            {
                Type = r.Type,
                UniqueTransactionCount = r.UniqueTransactionCount,
                TotalAmount = r.TotalAmount,
                PercentOfTotal = totalAmount == 0m ? 0m : ( r.TotalAmount / totalAmount )
            } ).ToList();

            var result = new FinancialTransactionSummaryResult
            {
                Currency = "USD",
                Totals = new FinancialTotalsBreakdown
                {
                    UniqueTransactionCount = uniqueTransactionCount,
                    TotalAmount = totalAmount,
                    AverageAmountPerTransaction = averageAmount,
                    MedianAmountPerTransaction = medianAmount,
                    StandardDeviationAmountPerTransaction = stdDeviationAmount
                },
                Funds = funds,
                CurrencyTypes = currencyTypes
            };

            return RockToolResult.Success( result );
        }

        /// <summary>
        /// Lists individual financial transactions matching the supplied filters (at least one is required).
        /// Use this only when raw transaction data is explicitly needed; prefer the summarize tool for
        /// general analytical questions.
        ///
        /// When <paramref name="accountIdKeys"/> and/or <paramref name="campusIdKey"/> are provided, only
        /// transactions that contribute (>0) to the resolved set of accounts are returned, and the per-row
        /// TotalAmount/Accounts reflect only those contributing details (same behavior as summarize).
        /// </summary>
        /// <param name="personIdKey">Optional person IdKey.</param>
        /// <param name="campusIdKey">Optional campus (batch campus) IdKey.</param>
        /// <param name="accountIdKeys">
        /// Optional list of Account/Fund IdKeys. If supplied (or if only campusIdKey is supplied),
        /// the account set is resolved via GetFinancialAccountsForQuery and only contributions to that set are included.
        /// </param>
        /// <param name="paymentMethodTypeValueIdKey">Optional payment method type IdKey.</param>
        /// <param name="startDate">Optional inclusive start date.</param>
        /// <param name="endDate">Optional inclusive end date.</param>
        /// <param name="pageNumber">1-based page number.</param>
        /// <returns>Collection of <see cref="FinancialTransactionResult"/> records.</returns>
        [AgentToolGuid( "20FF0B2E-E403-48CE-B0C9-0CB6D80A7291" )]
        public RockToolResult ListFinancialTransactions(
            string personIdKey = null,
            string campusIdKey = null,
            List<string> accountIdKeys = null,
            string paymentMethodTypeValueIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1 )
        {
            // Require at least one filter, or punt to summarize tool.
            if ( personIdKey.IsNullOrWhiteSpace()
                && campusIdKey.IsNullOrWhiteSpace()
                && ( accountIdKeys == null || !accountIdKeys.Any() )
                && paymentMethodTypeValueIdKey.IsNullOrWhiteSpace()
                && !startDate.HasValue
                && !endDate.HasValue )
            {
                return RockToolResult.Error( "At least one filter must be provided to list financial transactions." )
                    .WithInstructions( "Call the SummarizeFinancialTransactions tool to get an aggregated form of the request." );
            }

            using var rockContext = _rockContextFactory.CreateRockContext();

            var personId = personIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( personIdKey ) : null;
            var campusId = campusIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( campusIdKey ) : null;
            var paymentMethodTypeId = paymentMethodTypeValueIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( paymentMethodTypeValueIdKey ) : null;

            var options = new FinancialTransactionQueryOptions
            {
                PersonId = personId,
                // BatchCampusId = campusId,
                PaymentMethodTypeId = paymentMethodTypeId,
                StartDate = startDate,
                EndDate = endDate
            };

            List<int> accountIds = null;
            if ( ( accountIdKeys?.Any() ?? false ) || campusIdKey.IsNotNullOrWhiteSpace() )
            {
                accountIds = GetFinancialAccountsForQuery( accountIdKeys ?? new List<string>(), campusIdKey, rockContext )
                    .Select( a => a.Id )
                    .ToList();

                if ( !accountIds.Any() )
                {
                    return RockToolResult.NoData()
                        .WithInstructions( "No active financial accounts matched the supplied accountIdKeys and/or campusIdKey." );
                }
            }

            var hasAccountFilter = accountIds?.Any() == true;

            // Paging (offset with N+1 lookahead) 
            var pgNumber = Math.Max( 1, pageNumber );
            const int basePageSize = 50;
            var offset = ( pgNumber - 1 ) * basePageSize;
            var take = basePageSize + 1; // ask for one extra row to detect hasMore

            // Base query + deterministic ordering (date desc, then id desc)
            var baseQry = GetFinancialTransactionsQueryable( rockContext, options )
                .Include( t => t.AuthorizedPersonAlias.Person )
                .Include( t => t.TransactionDetails.Select( d => d.Account ) );

            // If we have an account filter, only return transactions that actually contribute (>0)
            // to one of the filtered accounts (mirror Summarize's "effective" set).
            if ( hasAccountFilter )
            {
                baseQry = baseQry.Where( t => t.TransactionDetails.Any( d => accountIds.Contains( d.AccountId ) ) );
            }

            baseQry = baseQry.OrderByDescending( t => t.TransactionDateTime )
                .ThenByDescending( t => t.Id );

            // Project AFTER ordering, BEFORE paging
            var projectedQry = baseQry.Select( ft => new FinancialTransactionResult
            {
                Id = ft.Id,
                AuthorizedPerson = new PersonResult
                {
                    Id = ft.AuthorizedPersonAlias.PersonId,
                    NickName = ft.AuthorizedPersonAlias.Person.NickName,
                    LastName = ft.AuthorizedPersonAlias.Person.LastName,
                    IncludePublicProfile = false,
                    IncludeAvatarUrl = false
                },
                TransactionDateTime = ft.TransactionDateTime,

                // Only sum details that match the resolved account set (if any)
                TotalAmount =
                    ft.TransactionDetails
                        .Where( d => !hasAccountFilter || accountIds.Contains( d.AccountId ) )
                        .Sum( d => ( decimal? ) d.Amount ) ?? 0m,

                // And only list those matching account details
                Accounts =
                    ft.TransactionDetails
                        .Where( td => !hasAccountFilter || accountIds.Contains( td.AccountId ) )
                        .Select( td => new FinancialAccountTransactionSummaryResult
                        {
                            Amount = td.Amount,
                            Name = td.Account.Name
                        } )
                        .ToList()
            } );

            var rows = projectedQry
                .Skip( offset )
                .Take( take )
                .ToList();

            var hasMore = rows.Count > basePageSize;
            if ( hasMore )
            {
                rows.RemoveAt( rows.Count - 1 ); // drop lookahead row
            }

            var meta = new Dictionary<string, object>
            {
                { "filters", new Dictionary<string, object?>
                    {
                        { "personIdKey", personIdKey },
                        { "campusIdKey", campusIdKey },
                        { "accountIdKeys", accountIdKeys },
                        { "paymentMethodTypeValueIdKey", paymentMethodTypeValueIdKey },
                        { "startDate", startDate },
                        { "endDate", endDate }
                    }
                },
                { "pageNumber", pgNumber },
                { "pageSize", basePageSize },
                { "returnedRows", rows.Count },
                { "hasMore", hasMore }
            };

            if ( rows.Count == 0 )
            {
                return RockToolResult.NoData()
                    .WithMetadata( meta );
            }

            // Trimmed history content (unchanged)
            var trimmedForHistory = rows.Select( r => new
            {
                r.Id,
                r.TransactionDateTime,
                r.TotalAmount,
                AuthorizedPerson = new
                {
                    r.AuthorizedPerson.Id,
                    r.AuthorizedPerson.NickName,
                    r.AuthorizedPerson.LastName
                },
                PageNumber = pgNumber
            } ).ToList();

            // History key should include all accountIdKeys to keep variants distinct
            var historyKey = string.Concat(
                personIdKey,
                campusIdKey,
                accountIdKeys == null ? null : string.Join( "|", accountIdKeys ),
                paymentMethodTypeValueIdKey,
                startDate?.ToString( "o" ),
                endDate?.ToString( "o" ) ).XxHash();

            return RockToolResult.Success( rows )
                .WithMetadata( meta )
                .WithHistoryContent( trimmedForHistory, historyKey );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the base <see cref="IQueryable{FinancialTransaction}"/> applying only transaction-scope filters.
        /// Account (fund) filtering is intentionally deferred to detail-level projections to avoid excluding
        /// multi-fund transactions from analytic calculations.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="options">User-supplied query options.</param>
        /// <returns>A filtered queryable of transactions.</returns>
        private IQueryable<FinancialTransaction> GetFinancialTransactionsQueryable( RockContext rockContext, FinancialTransactionQueryOptions options )
        {
            var financialTransactionService = new FinancialTransactionService( rockContext );

            // Pull what we need and leave AccountId out here on purpose.
            var qry = financialTransactionService
                .Queryable()
                .Include( t => t.TransactionDetails )
                .Include( t => t.FinancialPaymentDetail )
                .Include( t => t.Batch ); // for CampusId

            if ( options.PersonId.HasValue )
            {
                qry = qry.Where( t => t.AuthorizedPersonAlias.PersonId == options.PersonId.Value );
            }

            // Prefer the canonical link: Transaction -> Batch -> CampusId
            if ( options.BatchCampusId.HasValue )
            {
                var campusId = options.BatchCampusId.Value;
                qry = qry.Where( t => t.Batch != null && t.Batch.CampusId == campusId );
            }

            // DO NOT filter by AccountId at the transaction level.
            // That would exclude valid transactions that include other funds.
            // We'll respect AccountId only when aggregating details.

            if ( options.PaymentMethodTypeId.HasValue )
            {
                qry = qry.Where( t => t.FinancialPaymentDetail.CurrencyTypeValueId == options.PaymentMethodTypeId.Value );
            }

            if ( options.StartDate.HasValue )
            {
                qry = qry.Where( t => t.TransactionDateTime >= options.StartDate.Value );
            }

            if ( options.EndDate.HasValue )
            {
                qry = qry.Where( t => t.TransactionDateTime <= options.EndDate.Value );
            }

            return qry;
        }

        #endregion
    }
}
