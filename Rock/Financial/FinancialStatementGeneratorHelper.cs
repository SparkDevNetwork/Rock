﻿// <copyright>
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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// 
    /// </summary>
    public static class FinancialStatementGeneratorHelper
    {
        #region MergeField Keys

        /// <summary>
        /// Keys to use for Lava MergeField keys
        /// </summary>
        private static class MergeFieldKey
        {
            public const string RenderMedium = "RenderMedium";
            public const string FinancialStatementTemplate = "FinancialStatementTemplate";
            public const string RenderedPageCount = "RenderedPageCount";
            public const string PersonList = "PersonList";
            public const string StatementStartDate = "StatementStartDate";
            public const string StatementEndDate = "StatementEndDate";
            public const string Salutation = "Salutation";
            public const string MailingAddress = "MailingAddress";
            public const string StreetAddress1 = "StreetAddress1";
            public const string StreetAddress2 = "StreetAddress2";
            public const string City = "City";
            public const string State = "State";
            public const string PostalCode = "PostalCode";
            public const string Country = "Country";
            public const string Transactions = "Transactions";
            public const string TransactionsNonCash = "TransactionsNonCash";
            public const string TransactionDetails = "TransactionDetails";
            public const string TransactionDetailsNonCash = "TransactionDetailsNonCash";
            public const string TotalContributionAmount = "TotalContributionAmount";
            public const string TotalContributionCount = "TotalContributionCount";
            public const string TotalContributionAmountNonCash = "TotalContributionAmountNonCash";
            public const string TotalContributionCountNonCash = "TotalContributionCountNonCash";
            public const string AccountSummary = "AccountSummary";
            public const string AccountSummaryNonCash = "AccountSummaryNonCash";
            public const string Pledges = "Pledges";
            public const string Options = "Options";
        }

        #endregion MergeField Keys

        /// <summary>
        /// Gets the financial statement generator recipients.
        /// </summary>
        /// <param name="financialStatementGeneratorOptions">The financial statement generator options.</param>
        /// <returns></returns>
        public static List<FinancialStatementGeneratorRecipient> GetFinancialStatementGeneratorRecipients( FinancialStatementGeneratorOptions financialStatementGeneratorOptions )
        {
            if ( financialStatementGeneratorOptions == null )
            {
                throw new FinancialGivingStatementArgumentException( "FinancialStatementGeneratorOptions options must be specified" );
            }

            using ( var rockContext = new RockContext() )
            {
                FinancialStatementTemplate financialStatementTemplate = new FinancialStatementTemplateService( rockContext ).Get( financialStatementGeneratorOptions.FinancialStatementTemplateId ?? 0 );
                if ( financialStatementTemplate == null )
                {
                    throw new FinancialGivingStatementArgumentException( "FinancialStatementTemplate must be specified." );
                }

                var reportSettings = financialStatementTemplate.ReportSettings;
                var transactionSettings = reportSettings.TransactionSettings;

                var financialTransactionQry = FinancialStatementGeneratorHelper.GetFinancialTransactionQuery( financialStatementGeneratorOptions, rockContext, true );
                var financialPledgeQry = FinancialStatementGeneratorHelper.GetFinancialPledgeQuery( financialStatementGeneratorOptions, rockContext, true );

                // Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
                // These are Persons that give as part of a Group.For example, Husband and Wife
                var qryGivingGroupIdsThatHaveTransactions = financialTransactionQry.Select( a => a.AuthorizedPersonAlias.Person.GivingGroupId ).Where( a => a.HasValue )
                    .Select( a => new
                    {
                        PersonId = ( int? ) null,
                        GroupId = a.Value
                    } ).Distinct();

                var qryGivingGroupIdsThatHavePledges = financialPledgeQry.Select( a => a.PersonAlias.Person.GivingGroupId ).Where( a => a.HasValue )
                    .Select( a => new
                    {
                        PersonId = ( int? ) null,
                        GroupId = a.Value
                    } ).Distinct();

                // Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
                // These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
                // to determine which address(es) the statements need to be mailed to 
                var groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
                var groupMembersQry = new GroupMemberService( rockContext ).Queryable( true ).Where( m => m.Group.GroupTypeId == groupTypeIdFamily );

                var qryIndividualGiversThatHaveTransactions = financialTransactionQry
                    .Where( a => !a.AuthorizedPersonAlias.Person.GivingGroupId.HasValue )
                    .Select( a => a.AuthorizedPersonAlias.PersonId ).Distinct()
                    .Join( groupMembersQry, p => p, m => m.PersonId, ( p, m ) => new { PersonId = ( int? ) p, GroupId = m.GroupId } );

                var qryIndividualGiversThatHavePledges = financialPledgeQry
                    .Where( a => !a.PersonAlias.Person.GivingGroupId.HasValue )
                    .Select( a => a.PersonAlias.PersonId ).Distinct()
                    .Join( groupMembersQry, p => p, m => m.PersonId, ( p, m ) => new { PersonId = ( int? ) p, GroupId = m.GroupId } );

                var unionQry = qryGivingGroupIdsThatHaveTransactions.Union( qryIndividualGiversThatHaveTransactions );

                if ( reportSettings.PledgeSettings.AccountIds.Any() )
                {
                    unionQry = unionQry.Union( qryGivingGroupIdsThatHavePledges ).Union( qryIndividualGiversThatHavePledges );
                }

                /*  Limit to Mailing Address and sort by ZipCode */
                IQueryable<GroupLocation> groupLocationsQry = FinancialStatementGeneratorHelper.GetGroupLocationQuery( rockContext );

                // Do an outer join on location so we can include people that don't have an address (if options.IncludeIndividualsWithNoAddress) //
                var unionJoinLocationQry = from pg in unionQry
                                           join l in groupLocationsQry on pg.GroupId equals l.GroupId into u
                                           from l in u.DefaultIfEmpty()
                                           select new
                                           {
                                               PersonId = pg.PersonId,
                                               GroupId = pg.GroupId,
                                               LocationId = ( int? ) l.Location.Id,
                                               Street1 = l.Location.Street1,
                                               PostalCode = l.Location.PostalCode,
                                               Country = l.Location.Country
                                           };

                var givingIdsQry = unionJoinLocationQry.Select( a => new { a.PersonId, a.GroupId, a.LocationId, a.PostalCode, a.Country, a.Street1 } );

                var localCountry = GlobalAttributesCache.Get().OrganizationLocation?.Country;

                var recipientList = givingIdsQry.ToList().Select( a =>
                    new FinancialStatementGeneratorRecipient
                    {
                        GroupId = a.GroupId,
                        PersonId = a.PersonId,
                        LocationId = a.LocationId,
                        PostalCode = a.PostalCode,
                        Country = a.Country,
                        HasValidMailingAddress = a.LocationId.HasValue && a.PostalCode.IsNotNullOrWhiteSpace() && a.Street1.IsNotNullOrWhiteSpace(),

                        // Indicate if it is a international address. Which is if Country is different than OrganizationLocation Country (and country is not blank)
                        IsInternationalAddress = a.Country.IsNotNullOrWhiteSpace() && localCountry.IsNotNullOrWhiteSpace() && !a.Country.Equals( localCountry, StringComparison.OrdinalIgnoreCase )
                    } ).ToList();

                if ( financialStatementGeneratorOptions.DataViewId.HasValue )
                {
                    var dataView = new DataViewService( new RockContext() ).Get( financialStatementGeneratorOptions.DataViewId.Value );
                    if ( dataView != null )
                    {
                        var dataViewGetQueryArgs = new DataViewGetQueryArgs();
                        var personList = dataView.GetQuery( dataViewGetQueryArgs ).OfType<Rock.Model.Person>().Select( a => new { a.Id, a.GivingGroupId } ).ToList();
                        HashSet<int> personIds = new HashSet<int>( personList.Select( a => a.Id ) );
                        HashSet<int> groupsIds = new HashSet<int>( personList.Where( a => a.GivingGroupId.HasValue ).Select( a => a.GivingGroupId.Value ).Distinct() );

                        foreach ( var recipient in recipientList.ToList() )
                        {
                            if ( recipient.PersonId.HasValue )
                            {
                                if ( !personIds.Contains( recipient.PersonId.Value ) )
                                {
                                    recipientList.Remove( recipient );
                                }
                            }
                            else
                            {
                                if ( !groupsIds.Contains( recipient.GroupId ) )
                                {
                                    recipientList.Remove( recipient );
                                }
                            }
                        }
                    }
                }

                var personGivingIdsQuery = givingIdsQry.Where( a => a.PersonId.HasValue );
                var personQuery = new PersonService( rockContext ).Queryable( true, true );

                // get a query to look up LastName for recipients that give as a group
                var qryNickNameLastNameAsIndividual = from p in personQuery
                                                      join gg in personGivingIdsQuery on p.Id equals gg.PersonId.Value
                                                      select new { gg.PersonId, p.NickName, p.LastName };

                var nickNameLastNameLookupByPersonId = qryNickNameLastNameAsIndividual
                    .Select( a => new { a.PersonId, a.NickName, a.LastName } )
                    .ToList()
                    .Where( a => a.PersonId.HasValue )
                    .GroupBy( a => a.PersonId.Value )
                    .ToDictionary( k => k.Key, v => v.Select( a => new { a.NickName, a.LastName } ).FirstOrDefault() );

                var givingLeaderGivingIdsQuery = givingIdsQry.Where( a => !a.PersonId.HasValue );
                var qryNickNameLastNameAsGivingLeader = from p in personQuery.Where( a => a.GivingGroupId.HasValue )
                                                        join gg in givingLeaderGivingIdsQuery on p.GivingGroupId equals gg.GroupId
                                                        select new { gg.GroupId, p.NickName, p.LastName, p.GivingLeaderId, PersonId = p.Id };

                // Get the NickName and LastName of the GivingLeader of each group.
                // If the Group somehow doesn't have a GivingLeader( which shouldn't happen ), use another person in that group.
                var nickNameLastNameLookupByGivingGroupId = qryNickNameLastNameAsGivingLeader
                    .Select( a => new { a.GroupId, a.NickName, a.LastName, a.GivingLeaderId, a.PersonId } )
                    .ToList()
                    .GroupBy( a => a.GroupId )
                    .ToDictionary(
                        k => k.Key,
                        v => v
                            .Select( a => new { a.NickName, a.LastName, IsGivingLeader = a.PersonId == a.GivingLeaderId } )
                            .OrderByDescending( a => a.IsGivingLeader )
                            .FirstOrDefault() );

                foreach ( var recipient in recipientList )
                {
                    if ( recipient.PersonId.HasValue )
                    {
                        var lookupValue = nickNameLastNameLookupByPersonId.GetValueOrNull( recipient.PersonId.Value );

                        // lookupValue for individual giver should never be null, but just in case, do a null check 
                        recipient.NickName = lookupValue?.NickName ?? string.Empty;
                        recipient.LastName = lookupValue?.LastName ?? string.Empty;
                    }
                    else
                    {
                        var lookupValue = nickNameLastNameLookupByGivingGroupId.GetValueOrNull( recipient.GroupId );
                        recipient.NickName = lookupValue?.NickName ?? string.Empty;
                        recipient.LastName = lookupValue?.LastName ?? string.Empty;
                    }
                }

                // A statement Generator can have more than report configuration. If they have more than one, then
                // the generator will be in charge of sorting. However, let's take care of sorting the first
                // Report Configuration. That'll help in cases where the caller doesn't support more than one report configuration
                var defaultReportConfiguration = financialStatementGeneratorOptions.ReportConfigurationList?.OrderByDescending( x => x.CreatedDateTime ).FirstOrDefault();

                if ( defaultReportConfiguration != null )
                {
                    // use C# to sort the recipients by specified PrimarySortOrder and SecondarySortOrder
                    if ( defaultReportConfiguration.PrimarySortOrder == FinancialStatementOrderBy.LastName )
                    {
                        var sortedRecipientList = recipientList.OrderBy( a => a.LastName ).ThenBy( a => a.NickName );
                        if ( defaultReportConfiguration.SecondarySortOrder == FinancialStatementOrderBy.PostalCode )
                        {
                            sortedRecipientList = sortedRecipientList.ThenBy( a => a.PostalCode );
                        }

                        recipientList = sortedRecipientList.ToList();
                    }
                    else if ( defaultReportConfiguration.PrimarySortOrder == FinancialStatementOrderBy.PostalCode )
                    {
                        var sortedRecipientList = recipientList.OrderBy( a => a.PostalCode );
                        if ( defaultReportConfiguration.SecondarySortOrder == FinancialStatementOrderBy.LastName )
                        {
                            sortedRecipientList = sortedRecipientList.ThenBy( a => a.LastName ).ThenBy( a => a.NickName );
                        }

                        recipientList = sortedRecipientList.ToList();
                    }
                }

                return recipientList;
            }
        }

        /// <summary>
        /// Gets the statement generator recipient result.
        /// </summary>
        /// <param name="financialStatementGeneratorRecipientRequest">The financial statement generator recipient request.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        public static FinancialStatementGeneratorRecipientResult GetStatementGeneratorRecipientResult( FinancialStatementGeneratorRecipientRequest financialStatementGeneratorRecipientRequest, Person currentPerson )
        {
            if ( financialStatementGeneratorRecipientRequest == null )
            {
                throw new FinancialGivingStatementArgumentException( "FinancialStatementGeneratorRecipientRequest must be specified" );
            }

            var financialStatementGeneratorOptions = financialStatementGeneratorRecipientRequest?.FinancialStatementGeneratorOptions;

            if ( financialStatementGeneratorOptions == null )
            {
                throw new FinancialGivingStatementArgumentException( "FinancialStatementGeneratorOptions must be specified" );
            }

            if ( financialStatementGeneratorRecipientRequest.FinancialStatementGeneratorRecipient == null )
            {
                throw new FinancialGivingStatementArgumentException( "FinancialStatementGeneratorRecipient must be specified" );
            }

            var groupId = financialStatementGeneratorRecipientRequest.FinancialStatementGeneratorRecipient.GroupId;
            var personId = financialStatementGeneratorRecipientRequest.FinancialStatementGeneratorRecipient.PersonId;
            var locationId = financialStatementGeneratorRecipientRequest.FinancialStatementGeneratorRecipient.LocationId;

            var recipientResult = new FinancialStatementGeneratorRecipientResult( financialStatementGeneratorRecipientRequest.FinancialStatementGeneratorRecipient );

            using ( var rockContext = new RockContext() )
            {
                FinancialStatementTemplate financialStatementTemplate = new FinancialStatementTemplateService( rockContext ).GetNoTracking( financialStatementGeneratorOptions.FinancialStatementTemplateId ?? 0 );
                if ( financialStatementTemplate == null )
                {
                    throw new FinancialGivingStatementArgumentException( "FinancialStatementTemplate must be specified." );
                }

                var reportSettings = financialStatementTemplate.ReportSettings;
                var transactionSettings = reportSettings.TransactionSettings;
                var financialTransactionQry = FinancialStatementGeneratorHelper.GetFinancialTransactionQuery( financialStatementGeneratorOptions, rockContext, false );

                var personList = new List<Person>();
                Person person = null;
                if ( personId.HasValue )
                {
                    person = new PersonService( rockContext ).Queryable( true, true ).Include( a => a.Aliases ).Include( a => a.PrimaryFamily ).Where( a => a.Id == personId.Value ).FirstOrDefault();
                    personList.Add( person );
                }
                else
                {
                    // get transactions for all the persons in the specified group that have specified that group as their GivingGroup
                    GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                    personList = groupMemberService.GetByGroupId( groupId, true ).Where( a => a.Person.GivingGroupId == groupId ).Select( s => s.Person ).Include( a => a.Aliases ).Include( a => a.PrimaryFamily ).ToList();
                    person = personList.FirstOrDefault();
                }

                // if *any* giving unit member has opted out, set the recipient as opted out
                recipientResult.OptedOut = FinancialStatementGeneratorHelper.GetOptedOutPersonIds( personList ).Any();

                var personAliasIds = personList.SelectMany( a => a.Aliases.Select( x => x.Id ) ).ToList();
                if ( personAliasIds.Count == 1 )
                {
                    var personAliasId = personAliasIds[0];
                    financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAliasId.Value == personAliasId );
                }
                else
                {
                    financialTransactionQry = financialTransactionQry.Where( a => personAliasIds.Contains( a.AuthorizedPersonAliasId.Value ) );
                }

                var financialTransactionsList = financialTransactionQry
                    .Include( a => a.FinancialPaymentDetail )
                    .Include( a => a.FinancialPaymentDetail.CurrencyTypeValue )
                    .Include( a => a.TransactionDetails )
                    .Include( a => a.TransactionDetails.Select( x => x.Account ) )
                    .OrderBy( a => a.TransactionDateTime ).ToList();

                var transactionAccountIds = transactionSettings.GetIncludedAccountIds( rockContext );

                foreach ( var financialTransaction in financialTransactionsList )
                {
                    if ( transactionAccountIds.Any() )
                    {
                        // remove any Accounts that were not included (in case there was a mix of included and not included accounts in the transaction)
                        financialTransaction.TransactionDetails = financialTransaction.TransactionDetails.Where( a => transactionAccountIds.Contains( a.AccountId ) ).ToList();
                    }

                    financialTransaction.TransactionDetails = financialTransaction.TransactionDetails.OrderBy( a => a.Account.Order ).ThenBy( a => a.Account.PublicName ).ToList();
                }

                var lavaTemplateLava = financialStatementTemplate.ReportTemplate;
                var lavaTemplateFooterHtmlFragment = financialStatementTemplate.FooterSettings.HtmlFragment;

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false, GetDeviceFamily = false, GetOSFamily = false, GetPageContext = false, GetPageParameters = false, GetCampuses = true, GetCurrentPerson = true } );
                mergeFields.Add( MergeFieldKey.RenderMedium, financialStatementGeneratorOptions.RenderMedium );
                mergeFields.Add( MergeFieldKey.FinancialStatementTemplate, financialStatementTemplate );
                mergeFields.Add( MergeFieldKey.RenderedPageCount, financialStatementGeneratorRecipientRequest.FinancialStatementGeneratorRecipient.RenderedPageCount );

                mergeFields.Add( MergeFieldKey.PersonList, personList );
                mergeFields.Add( MergeFieldKey.StatementStartDate, financialStatementGeneratorOptions.StartDate );
                var humanFriendlyEndDate = financialStatementGeneratorOptions.EndDate.HasValue ? financialStatementGeneratorOptions.EndDate.Value.AddDays( -1 ) : RockDateTime.Now.Date;
                mergeFields.Add( MergeFieldKey.StatementEndDate, humanFriendlyEndDate );

                string salutation;
                if ( personId.HasValue )
                {
                    // if the person gives as an individual, the salutation should be just the person's name
                    salutation = person.FullName;
                }
                else
                {
                    // if giving as a group, the salutation is family title
                    string familyTitle;
                    if ( person != null && person.PrimaryFamilyId == groupId )
                    {
                        // this is how familyTitle should able to be determined in most cases
                        familyTitle = person.PrimaryFamily.GroupSalutation;
                    }
                    else
                    {
                        // This could happen if the person is from multiple families, and specified groupId is not their PrimaryFamily
                        familyTitle = new GroupService( rockContext ).GetSelect( groupId, s => s.GroupSalutation );
                    }

                    if ( familyTitle.IsNullOrWhiteSpace() )
                    {
                        // shouldn't happen, just in case the familyTitle is blank, just return the person's name
                        familyTitle = person.FullName;
                    }

                    salutation = familyTitle;
                }

                mergeFields.Add( MergeFieldKey.Salutation, salutation );

                Location mailingAddress;

                if ( locationId.HasValue )
                {
                    // get the location that was specified for the recipient
                    mailingAddress = new LocationService( rockContext ).Get( locationId.Value );
                }
                else
                {
                    // for backwards compatibility, get the first address
                    IQueryable<GroupLocation> groupLocationsQry = FinancialStatementGeneratorHelper.GetGroupLocationQuery( rockContext );
                    mailingAddress = groupLocationsQry.Where( a => a.GroupId == groupId ).Select( a => a.Location ).FirstOrDefault();
                }

                mergeFields.Add( MergeFieldKey.MailingAddress, mailingAddress );

                if ( mailingAddress != null )
                {
                    mergeFields.Add( MergeFieldKey.StreetAddress1, mailingAddress.Street1 );
                    mergeFields.Add( MergeFieldKey.StreetAddress2, mailingAddress.Street2 );
                    mergeFields.Add( MergeFieldKey.City, mailingAddress.City );
                    mergeFields.Add( MergeFieldKey.State, mailingAddress.State );
                    mergeFields.Add( MergeFieldKey.PostalCode, mailingAddress.PostalCode );
                    mergeFields.Add( MergeFieldKey.Country, mailingAddress.Country );
                }
                else
                {
                    mergeFields.Add( MergeFieldKey.StreetAddress1, string.Empty );
                    mergeFields.Add( MergeFieldKey.StreetAddress2, string.Empty );
                    mergeFields.Add( MergeFieldKey.City, string.Empty );
                    mergeFields.Add( MergeFieldKey.State, string.Empty );
                    mergeFields.Add( MergeFieldKey.PostalCode, string.Empty );
                    mergeFields.Add( MergeFieldKey.Country, string.Empty );
                }

                var transactionDetailListAll = financialTransactionsList.SelectMany( a => a.TransactionDetails ).ToList();

                if ( transactionSettings.HideRefundedTransactions && transactionDetailListAll.Any( a => a.Amount < 0 ) )
                {
                    var allRefunds = transactionDetailListAll.SelectMany( a => a.Transaction.Refunds ).ToList();
                    foreach ( var refund in allRefunds )
                    {
                        foreach ( var refundedOriginalTransactionDetail in refund.OriginalTransaction.TransactionDetails )
                        {
                            // remove the refund's original TransactionDetails from the results
                            if ( transactionDetailListAll.Contains( refundedOriginalTransactionDetail ) )
                            {
                                transactionDetailListAll.Remove( refundedOriginalTransactionDetail );
                                foreach ( var refundDetailId in refund.FinancialTransaction.TransactionDetails.Select( a => a.Id ) )
                                {
                                    // remove the refund's transaction from the results
                                    var refundDetail = transactionDetailListAll.FirstOrDefault( a => a.Id == refundDetailId );
                                    if ( refundDetail != null )
                                    {
                                        transactionDetailListAll.Remove( refundDetail );
                                    }
                                }
                            }
                        }
                    }
                }

                if ( transactionSettings.HideCorrectedTransactionOnSameData && transactionDetailListAll.Any( a => a.Amount < 0 ) )
                {
                    // Hide transactions that are corrected on the same date. Transactions that have a matching negative dollar amount on the same date and same account will not be shown.

                    // get a list of dates that have at least one negative transaction
                    var transactionsByDateList = transactionDetailListAll.GroupBy( a => a.Transaction.TransactionDateTime.Value.Date ).Select( a => new
                    {
                        Date = a.Key,
                        TransactionDetails = a.ToList()
                    } )
                    .Where( a => a.TransactionDetails.Any( x => x.Amount < 0 ) )
                    .ToList();

                    foreach ( var transactionsByDate in transactionsByDateList )
                    {
                        foreach ( var negativeTransaction in transactionsByDate.TransactionDetails.Where( a => a.Amount < 0 ) )
                        {
                            // find the first transaction that has an amount that matches the negative amount (on the same day and same account)
                            // and make sure the matching transaction doesn't already have a refund associated with it
                            var correctedTransactionDetail = transactionsByDate.TransactionDetails
                                .Where( a => ( a.Amount == ( -negativeTransaction.Amount ) && a.AccountId == negativeTransaction.AccountId ) && !a.Transaction.Refunds.Any() )
                                .FirstOrDefault();
                            if ( correctedTransactionDetail != null )
                            {
                                // if the transaction was corrected, remove it, and also remove the associated correction (the negative one) transaction
                                transactionDetailListAll.Remove( correctedTransactionDetail );
                                transactionDetailListAll.Remove( negativeTransaction );
                            }
                        }
                    }
                }

                List<FinancialTransactionDetail> transactionDetailListCash = transactionDetailListAll;
                List<FinancialTransactionDetail> transactionDetailListNonCash = new List<FinancialTransactionDetail>();

                var currencyTypesForCashGiftIds = transactionSettings.CurrencyTypesForCashGiftGuids?.Select( a => DefinedValueCache.GetId( a ) ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();
                var currencyTypesForNotCashGiftIds = transactionSettings.CurrencyTypesForNonCashGuids?.Select( a => DefinedValueCache.GetId( a ) ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();

                if ( currencyTypesForCashGiftIds != null )
                {
                    // NOTE: if there isn't a FinancialPaymentDetail record, assume it is Cash
                    transactionDetailListCash = transactionDetailListCash.Where( a =>
                        ( a.Transaction.FinancialPaymentDetailId == null ) ||
                        ( a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue && currencyTypesForCashGiftIds.Contains( a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) ) ).ToList();
                }

                if ( currencyTypesForNotCashGiftIds != null )
                {
                    transactionDetailListNonCash = transactionDetailListAll.Where( a =>
                        a.Transaction.FinancialPaymentDetailId.HasValue &&
                        a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue
                        && currencyTypesForNotCashGiftIds.Contains( a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) ).ToList();
                }

                // Add Merge Fields for Transactions for custom Statements that might want to organize the output by Transaction instead of TransactionDetail
                var transactionListCash = transactionDetailListCash.GroupBy( a => a.Transaction ).Select( a => a.Key ).ToList();
                var transactionListNonCash = transactionDetailListNonCash.GroupBy( a => a.Transaction ).Select( a => a.Key ).ToList();
                mergeFields.Add( MergeFieldKey.Transactions, transactionListCash );
                mergeFields.Add( MergeFieldKey.TransactionsNonCash, transactionListNonCash );

                // Add the standard TransactionDetails and TransactionDetailsNonCash that the default Rock templates use
                mergeFields.Add( MergeFieldKey.TransactionDetails, transactionDetailListCash );
                mergeFields.Add( MergeFieldKey.TransactionDetailsNonCash, transactionDetailListNonCash );

                mergeFields.Add( MergeFieldKey.TotalContributionAmount, transactionDetailListCash.Sum( a => a.Amount ) );
                mergeFields.Add( MergeFieldKey.TotalContributionCount, transactionDetailListCash.Count() );

                mergeFields.Add( MergeFieldKey.TotalContributionAmountNonCash, transactionDetailListNonCash.Sum( a => a.Amount ) );
                mergeFields.Add( MergeFieldKey.TotalContributionCountNonCash, transactionDetailListNonCash.Count() );

                recipientResult.ContributionTotal = transactionDetailListCash.Sum( a => a.Amount );

                mergeFields.Add(
                    MergeFieldKey.AccountSummary,
                    transactionDetailListCash
                        .GroupBy( t => t.Account.PublicName )
                        .Select( s => new AccountSummaryInfo
                        {
                            Account = s.FirstOrDefault().Account,
                            Total = s.Sum( a => a.Amount ),
                            Order = s.Max( a => a.Account.Order )
                        } )
                        .OrderBy( s => s.Order ) );

                mergeFields.Add(
                    MergeFieldKey.AccountSummaryNonCash,
                    transactionDetailListNonCash
                        .GroupBy( t => t.Account.PublicName )
                        .Select( s => new AccountSummaryInfo
                        {
                            Account = s.FirstOrDefault().Account,
                            Total = s.Sum( a => a.Amount ),
                            Order = s.Max( a => a.Account.Order )
                        } )
                        .OrderBy( s => s.Order ) );

                var pledgeSettings = reportSettings.PledgeSettings;
                pledgeSettings.AccountIds = pledgeSettings.AccountIds.Where( a => a > 0 ).ToList();

                if ( pledgeSettings.AccountIds != null && pledgeSettings.AccountIds.Any() )
                {
                    var pledgeSummaryList = FinancialStatementGeneratorHelper.GetPledgeSummaryData( financialStatementGeneratorOptions, financialStatementTemplate, rockContext, personAliasIds );
                    recipientResult.PledgeTotal = pledgeSummaryList.Sum( s => s.AmountPledged );

                    // Pledges ( organized by each Account in case an account is used by more than one pledge )
                    mergeFields.Add( MergeFieldKey.Pledges, pledgeSummaryList );
                }

                mergeFields.Add( MergeFieldKey.Options, financialStatementGeneratorOptions );
                recipientResult.Html = lavaTemplateLava.ResolveMergeFields( mergeFields, currentPerson );
                if ( !string.IsNullOrEmpty( lavaTemplateFooterHtmlFragment ) )
                {
                    recipientResult.FooterHtmlFragment = lavaTemplateFooterHtmlFragment.ResolveMergeFields( mergeFields, currentPerson );
                }

                recipientResult.Html = recipientResult.Html.Trim();
            }

            return recipientResult;
        }

        /// <summary>
        /// Gets the pledge summary data.
        /// </summary>
        /// <param name="financialStatementGeneratorOptions">The financial statement generator options.</param>
        /// <param name="financialStatementTemplate">The financial statement template.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personAliasIds">The person alias ids.</param>
        /// <returns></returns>
        private static List<PledgeSummary> GetPledgeSummaryData( FinancialStatementGeneratorOptions financialStatementGeneratorOptions, FinancialStatementTemplate financialStatementTemplate, RockContext rockContext, List<int> personAliasIds )
        {
            var financialPledgeQry = GetFinancialPledgeQuery( financialStatementGeneratorOptions, rockContext, false );
            var pledgeList = financialPledgeQry.Where( p => p.PersonAliasId.HasValue && personAliasIds.Contains( p.PersonAliasId.Value ) ).Include( a => a.Account ).OrderBy( a => a.Account.Order ).ThenBy( a => a.Account.PublicName ).ToList();

            var pledgeSummaryByPledgeList = pledgeList
                                .Select( p => new
                                {
                                    p.Account,
                                    Pledge = p
                                } )
                                .ToList();

            //// Pledges but organized by Account (in case more than one pledge goes to the same account)
            //// NOTE: In the case of multiple pledges to the same account (just in case they accidentally or intentionally had multiple pledges to the same account)
            ////  -- Date Range
            ////    -- StartDate: Earliest StartDate of all the pledges for that account 
            ////    -- EndDate: Latest EndDate of all the pledges for that account
            ////  -- Amount Pledged: Sum of all Pledges to that account
            ////  -- Amount Given: 
            ////    --  The sum of transaction amounts to that account between
            ////      -- Start Date: Earliest Start Date of all the pledges to that account
            ////      -- End Date: Whatever is earlier (Statement End Date or Pledges' End Date)
            var pledgeSummaryList = pledgeSummaryByPledgeList.GroupBy( a => a.Account ).Select( a => new PledgeSummary
            {
                Account = a.Key,
                PledgeList = a.Select( x => x.Pledge ).ToList()
            } ).ToList();

            // add detailed pledge information
            if ( pledgeSummaryList.Any() )
            {
                int statementPledgeYear = financialStatementGeneratorOptions.StartDate.Value.Year;

                var transactionSettings = financialStatementTemplate.ReportSettings.TransactionSettings;
                var pledgeSettings = financialStatementTemplate.ReportSettings.PledgeSettings;

                var currencyTypesForCashGiftIds = transactionSettings.CurrencyTypesForCashGiftGuids?.Select( a => DefinedValueCache.GetId( a ) ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();
                var currencyTypesForNotCashGiftIds = transactionSettings.CurrencyTypesForNonCashGuids?.Select( a => DefinedValueCache.GetId( a ) ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();

                List<int> pledgeCurrencyTypeIds = null;
                if ( currencyTypesForCashGiftIds != null )
                {
                    pledgeCurrencyTypeIds = currencyTypesForCashGiftIds;
                    if ( pledgeSettings.IncludeNonCashGifts && currencyTypesForNotCashGiftIds != null )
                    {
                        pledgeCurrencyTypeIds = currencyTypesForCashGiftIds.Union( currencyTypesForNotCashGiftIds ).ToList();
                    }
                }

                foreach ( var pledgeSummary in pledgeSummaryList )
                {
                    DateTime adjustedPledgeEndDate = pledgeSummary.PledgeEndDate.Value.Date;
                    if ( pledgeSummary.PledgeEndDate.Value.Date < DateTime.MaxValue.Date )
                    {
                        adjustedPledgeEndDate = pledgeSummary.PledgeEndDate.Value.Date.AddDays( 1 );
                    }

                    if ( financialStatementGeneratorOptions.EndDate.HasValue )
                    {
                        if ( adjustedPledgeEndDate > financialStatementGeneratorOptions.EndDate.Value )
                        {
                            adjustedPledgeEndDate = financialStatementGeneratorOptions.EndDate.Value;
                        }
                    }

                    var pledgeFinancialTransactionDetailQry = new FinancialTransactionDetailService( rockContext ).Queryable().Where( t =>
                                                     t.Transaction.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.Transaction.AuthorizedPersonAliasId.Value )
                                                     && t.Transaction.TransactionDateTime >= pledgeSummary.PledgeStartDate
                                                     && t.Transaction.TransactionDateTime < adjustedPledgeEndDate );

                    if ( pledgeSettings.IncludeGiftsToChildAccounts )
                    {
                        // If PledgesIncludeChildAccounts = true, we'll include transactions to those child accounts as part of the pledge (but only one level deep)
                        pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t =>
                            t.AccountId == pledgeSummary.AccountId
                            ||
                            ( t.Account.ParentAccountId.HasValue && t.Account.ParentAccountId == pledgeSummary.AccountId ) );
                    }
                    else
                    {
                        pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t => t.AccountId == pledgeSummary.AccountId );
                    }

                    if ( pledgeCurrencyTypeIds != null )
                    {
                        pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t =>
                            t.Transaction.FinancialPaymentDetailId.HasValue &&
                            t.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue && pledgeCurrencyTypeIds.Contains( t.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) );
                    }

                    pledgeSummary.AmountGiven = pledgeFinancialTransactionDetailQry.Sum( t => ( decimal? ) t.Amount ) ?? 0;
                }
            }

            return pledgeSummaryList;
        }

        /// <summary>
        /// Gets the opted out person ids.
        /// </summary>
        /// <param name="personList">The person list.</param>
        /// <returns></returns>
        private static int[] GetOptedOutPersonIds( List<Person> personList )
        {
            int? doNotSendGivingStatementAttributeId = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_DO_NOT_SEND_GIVING_STATEMENT.AsGuid() )?.Id;
            if ( !doNotSendGivingStatementAttributeId.HasValue )
            {
                return new int[0];
            }

            var personIds = personList.Select( a => a.Id ).ToList();
            var optedOutPersonQry = new AttributeValueService( new RockContext() ).Queryable().Where( a => a.AttributeId == doNotSendGivingStatementAttributeId );
            if ( personIds.Count == 1 )
            {
                int entityPersonId = personIds[0];
                optedOutPersonQry = optedOutPersonQry.Where( a => a.EntityId == entityPersonId );
            }
            else
            {
                optedOutPersonQry = optedOutPersonQry.Where( a => personIds.Contains( a.EntityId.Value ) );
            }

            var optedOutPersonIds = optedOutPersonQry
                .Select( a => new
                {
                    PersonId = a.EntityId.Value,
                    a.Value
                } ).ToList().Where( a => a.Value.AsBoolean() == true ).Select( a => a.PersonId ).ToArray();

            return optedOutPersonIds;
        }

        /// <summary>
        /// Gets the group location query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static IQueryable<GroupLocation> GetGroupLocationQuery( RockContext rockContext )
        {
            var groupLocationTypeIdHome = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            var groupLocationTypeIdWork = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() )?.Id;
            var groupLocationTypeIds = new List<int>();
            if ( groupLocationTypeIdHome.HasValue )
            {
                groupLocationTypeIds.Add( groupLocationTypeIdHome.Value );
            }

            if ( groupLocationTypeIdWork.HasValue )
            {
                groupLocationTypeIds.Add( groupLocationTypeIdWork.Value );
            }

            IQueryable<GroupLocation> groupLocationsQry = null;

            if ( groupLocationTypeIds.Count == 2 )
            {
                groupLocationsQry = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.IsMailingLocation && a.GroupLocationTypeValueId.HasValue )
                    .Where( a => a.GroupLocationTypeValueId == groupLocationTypeIdHome.Value || a.GroupLocationTypeValueId == groupLocationTypeIdWork.Value );
            }
            else
            {
                groupLocationsQry = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.IsMailingLocation && a.GroupLocationTypeValueId.HasValue && groupLocationTypeIds.Contains( a.GroupLocationTypeValueId.Value ) );
            }

            return groupLocationsQry;
        }

        /// <summary>
        /// Gets the financial pledge query.
        /// </summary>
        /// <param name="financialStatementGeneratorOptions">The financial statement generator options.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="usePersonFilters">if set to <c>true</c> [use person filters].</param>
        /// <returns></returns>
        private static IQueryable<FinancialPledge> GetFinancialPledgeQuery( Rock.Financial.FinancialStatementGeneratorOptions financialStatementGeneratorOptions, RockContext rockContext, bool usePersonFilters )
        {
            FinancialStatementTemplate financialStatementTemplate = new FinancialStatementTemplateService( rockContext ).Get( financialStatementGeneratorOptions.FinancialStatementTemplateId ?? 0 );
            if ( financialStatementTemplate == null )
            {
                throw new FinancialGivingStatementArgumentException( "FinancialStatementTemplate must be specified." );
            }

            var reportSettings = financialStatementTemplate.ReportSettings;
            var transactionSettings = reportSettings.TransactionSettings;
            var pledgeSettings = reportSettings.PledgeSettings;

            // pledge information
            var pledgeQry = new FinancialPledgeService( rockContext ).Queryable();

            // only include pledges that started *before* the end date of the statement ( we don't want pledges that start AFTER the statement end date )
            if ( financialStatementGeneratorOptions.EndDate.HasValue )
            {
                pledgeQry = pledgeQry.Where( p => p.StartDate < financialStatementGeneratorOptions.EndDate.Value );
            }

            // also only include pledges that ended *after* the statement start date ( we don't want pledges that ended BEFORE the statement start date )
            pledgeQry = pledgeQry.Where( p => p.EndDate >= financialStatementGeneratorOptions.StartDate.Value );

            // Filter to specified AccountIds (if specified)
            if ( pledgeSettings.AccountIds == null || !pledgeSettings.AccountIds.Any() )
            {
                // if no PledgeAccountIds where specified, don't include any pledges
                pledgeQry = pledgeQry.Where( a => false );
            }
            else
            {
                // NOTE: Only get the Pledges that were specifically pledged to the selected accounts
                // If the PledgesIncludeChildAccounts = true, we'll include transactions to those child accounts as part of the pledge (but only one level deep)
                var selectedAccountIds = pledgeSettings.AccountIds;
                pledgeQry = pledgeQry.Where( a => a.AccountId.HasValue && selectedAccountIds.Contains( a.AccountId.Value ) );
            }

            if ( usePersonFilters )
            {
                if ( financialStatementGeneratorOptions.PersonId.HasValue )
                {
                    // If PersonId is specified, then this statement is for a specific person, so don't do any other filtering
                    string personGivingId = new PersonService( rockContext ).Queryable( true, true ).Where( a => a.Id == financialStatementGeneratorOptions.PersonId.Value ).Select( a => a.GivingId ).FirstOrDefault();
                    if ( personGivingId != null )
                    {
                        pledgeQry = pledgeQry.Where( a => a.PersonAlias.Person.GivingId == personGivingId );
                    }
                    else
                    {
                        // shouldn't happen, but just in case person doesn't exist
                        pledgeQry = pledgeQry.Where( a => false );
                    }
                }
                else
                {
                    // unless we are using a DataView for filtering, filter based on the IncludeBusiness and ExcludeInActiveIndividuals options
                    if ( !financialStatementGeneratorOptions.DataViewId.HasValue )
                    {
                        if ( !financialStatementGeneratorOptions.IncludeBusinesses )
                        {
                            int recordTypeValueIdPerson = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                            pledgeQry = pledgeQry.Where( a => a.PersonAlias.Person.RecordTypeValueId == recordTypeValueIdPerson );
                        }

                        if ( financialStatementGeneratorOptions.ExcludeInActiveIndividuals )
                        {
                            int recordStatusValueIdActive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

                            // If the ExcludeInActiveIndividuals is enabled, don't include Pledges from Inactive Individuals, but only if they give as individuals.
                            // Pledges from Giving Groups should always be included regardless of the ExcludeInActiveIndividuals option.
                            // See https://app.asana.com/0/0/1200512694724254/f
                            pledgeQry = pledgeQry.Where( a => ( a.PersonAlias.Person.RecordStatusValueId == recordStatusValueIdActive ) || a.PersonAlias.Person.GivingGroupId.HasValue );

                        }

                        /* 06/23/2021 MDP
                         * Don't exclude pledges from Deceased. If the person pledged during the specified Date/Time range (probably while they weren't deceased), include them regardless of Deceased Status.
                         * 
                         * see https://app.asana.com/0/0/1200512694724244/f
                         */
                    }
                }
            }

            return pledgeQry;
        }

        /// <summary>
        /// Gets the financial transaction query.
        /// </summary>
        /// <param name="financialStatementGeneratorOptions">The financial statement generator options.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="usePersonFilters">if set to <c>true</c> [use person filters].</param>
        /// <returns></returns>
        private static IQueryable<FinancialTransaction> GetFinancialTransactionQuery( FinancialStatementGeneratorOptions financialStatementGeneratorOptions, RockContext rockContext, bool usePersonFilters )
        {
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var financialTransactionQry = financialTransactionService.Queryable();

            // filter to specified date range
            financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDateTime >= financialStatementGeneratorOptions.StartDate );

            if ( financialStatementGeneratorOptions.EndDate.HasValue )
            {
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDateTime < financialStatementGeneratorOptions.EndDate.Value );
            }

            FinancialStatementTemplate financialStatementTemplate = new FinancialStatementTemplateService( rockContext ).Get( financialStatementGeneratorOptions.FinancialStatementTemplateId ?? 0 );
            if ( financialStatementTemplate == null )
            {
                throw new FinancialGivingStatementArgumentException( "FinancialStatementTemplate must be specified." );
            }

            var reportSettings = financialStatementTemplate.ReportSettings;
            var transactionSettings = reportSettings.TransactionSettings;

            // default to Contributions if nothing specified
            var transactionTypeContribution = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            var transactionTypeIds = transactionSettings.TransactionTypeGuids?.Select( a => DefinedValueCache.GetId( a ) ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();
            if ( transactionTypeIds == null || !transactionTypeIds.Any() )
            {
                transactionTypeIds = new List<int>();
                if ( transactionTypeContribution != null )
                {
                    transactionTypeIds.Add( transactionTypeContribution.Id );
                }
            }

            if ( transactionTypeIds.Count() == 1 )
            {
                int selectedTransactionTypeId = transactionTypeIds[0];
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionTypeValueId == selectedTransactionTypeId );
            }
            else
            {
                financialTransactionQry = financialTransactionQry.Where( a => transactionTypeIds.Contains( a.TransactionTypeValueId ) );
            }

            var includedTransactionAccountIds = transactionSettings.GetIncludedAccountIds( rockContext );

            // Filter to specified AccountIds (if specified)
            if ( !includedTransactionAccountIds.Any() )
            {
                // if TransactionAccountIds wasn't supplied, don't filter on AccountId
            }
            else
            {
                // narrow it down to recipients that have transactions involving any of the AccountIds
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDetails.Any( x => includedTransactionAccountIds.Contains( x.AccountId ) ) );
            }

            if ( usePersonFilters )
            {
                if ( financialStatementGeneratorOptions.PersonId.HasValue )
                {
                    // If PersonId is specified, then this statement is for a specific person, so don't do any other filtering
                    string personGivingId = new PersonService( rockContext ).Queryable( true, true ).Where( a => a.Id == financialStatementGeneratorOptions.PersonId.Value ).Select( a => a.GivingId ).FirstOrDefault();
                    if ( personGivingId != null )
                    {
                        financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.Person.GivingId == personGivingId );
                    }
                    else
                    {
                        // shouldn't happen, but just in case person doesn't exist
                        financialTransactionQry = financialTransactionQry.Where( a => false );
                    }
                }
                else
                {
                    // unless we are using a DataView for filtering, filter based on the IncludeBusiness and ExcludeInActiveIndividuals options
                    if ( !financialStatementGeneratorOptions.DataViewId.HasValue )
                    {
                        if ( !financialStatementGeneratorOptions.IncludeBusinesses )
                        {
                            int recordTypeValueIdPerson = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                            financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.Person.RecordTypeValueId == recordTypeValueIdPerson );
                        }

                        if ( financialStatementGeneratorOptions.ExcludeInActiveIndividuals )
                        {
                            int recordStatusValueIdActive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

                            // If the ExcludeInActiveIndividuals is enabled, don't include transactions from Inactive Individuals, but only if they give as individuals.
                            // Transactions from Giving Groups should always be included regardless of the ExcludeInActiveIndividuals option.
                            // See https://app.asana.com/0/0/1200512694724254/f
                            financialTransactionQry = financialTransactionQry.Where( a => ( a.AuthorizedPersonAlias.Person.RecordStatusValueId == recordStatusValueIdActive ) || a.AuthorizedPersonAlias.Person.GivingGroupId.HasValue );
                        }
                    }

                    /* 06/23/2021 MDP
                      Don't exclude transactions from Deceased. If the person gave doing the specified Date/Time
                      range (probably while they weren't deceased), include them regardless of Deceased Status.

                      see https://app.asana.com/0/0/1200512694724244/f
                    */
                }
            }

            return financialTransactionQry;
        }

        private class AccountSummaryInfo : RockDynamic
        {
            public string AccountName => Account?.PublicName;

            public int? ParentAccountId => Account?.ParentAccountId;

            public string ParentAccountName => Account?.ParentAccount.PublicName;

            public decimal Total { get; set; }

            public int Order { get; set; }

            public FinancialAccount Account { get; internal set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="Rock.Utility.RockDynamic" />
        private class PledgeSummary : RockDynamic
        {
            /// <summary>
            /// Gets or sets the pledge list.
            /// </summary>
            /// <value>
            /// The pledge list.
            /// </value>
            public List<Rock.Model.FinancialPledge> PledgeList { get; set; }

            /// <summary>
            /// Gets or sets the pledge start date.
            /// </summary>
            /// <value>
            /// The pledge start date.
            /// </value>
            public DateTime? PledgeStartDate => PledgeList.Min( a => a.StartDate );

            /// <summary>
            /// Gets or sets the pledge end date.
            /// </summary>
            /// <value>
            /// The pledge end date.
            /// </value>
            public DateTime? PledgeEndDate => PledgeList.Max( a => a.EndDate );

            /// <summary>
            /// Gets or sets the amount pledged.
            /// </summary>
            /// <value>
            /// The amount pledged.
            /// </value>
            public decimal AmountPledged => PledgeList.Sum( a => a.TotalAmount );

            /// <summary>
            /// Gets or sets the pledge account identifier.
            /// </summary>
            /// <value>
            /// The pledge account identifier.
            /// </value>
            public int AccountId
            {
                get
                {
                    return Account.Id;
                }
            }

            /// <summary>
            /// Gets or sets the pledge account.
            /// </summary>
            /// <value>
            /// The pledge account.
            /// </value>
            public string AccountName
            {
                get
                {
                    return Account.Name;
                }
            }

            /// <summary>
            /// Gets or sets the pledge account.
            /// </summary>
            /// <value>
            /// The pledge account.
            /// </value>
            public string AccountPublicName
            {
                get
                {
                    return Account.PublicName;
                }
            }

            /// <summary>
            /// Gets or sets the account.
            /// </summary>
            /// <value>
            /// The account.
            /// </value>
            public Rock.Model.FinancialAccount Account { get; set; }

            /// <summary>
            /// Gets the percent complete.
            /// </summary>
            /// <value>
            /// The percent complete.
            /// </value>
            public int PercentComplete => ( int ) ( ( this.AmountGiven * 100 ) / this.AmountPledged );

            /// <summary>
            /// Gets or sets the amount remaining.
            /// </summary>
            /// <value>
            /// The amount remaining.
            /// </value>
            public decimal AmountRemaining => ( this.AmountGiven > this.AmountPledged ) ? 0 : ( this.AmountPledged - this.AmountGiven );

            /// <summary>
            /// Gets or sets the amount given.
            /// </summary>
            /// <value>
            /// The amount given.
            /// </value>
            public decimal AmountGiven { get; set; }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return $"{this.AccountName} AmountGiven:{this.AmountGiven}, AmountPledged:{this.AmountPledged}";
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class FinancialGivingStatementException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialGivingStatementException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FinancialGivingStatementException( string message )
            : base( message )
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ArgumentException" />
    public class FinancialGivingStatementArgumentException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialGivingStatementArgumentException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public FinancialGivingStatementArgumentException( string message )
            : base( message )
        {
        }
    }
}
