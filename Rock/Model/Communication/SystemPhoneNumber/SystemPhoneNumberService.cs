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
using System.Linq;
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class SystemPhoneNumberService
    {
        /// <summary>
        /// The lock object used when updating the legacy phone numbers in
        /// the Defined Value table.
        /// </summary>
        private static readonly object _legacyUpdateLock = new object();

        /// <summary>
        /// Gets the authorized phone numbers that match the query expression.
        /// </summary>
        /// <param name="currentPerson">The current person to use when checking authorization.</param>
        /// <returns>An enumeration of <see cref="SystemPhoneNumber"/> objects that can be viewed by <paramref name="currentPerson"/>.</returns>
        internal IEnumerable<SystemPhoneNumber> GetAuthorizedPhoneNumbers( Person currentPerson )
        {
            return GetAuthorizedPhoneNumbers( currentPerson, null );
        }

        /// <summary>
        /// Gets the authorized phone numbers that match the query expression.
        /// </summary>
        /// <param name="currentPerson">The current person to use when checking authorization.</param>
        /// <param name="queryExpression">The query expression to use when filtering phone numbers.</param>
        /// <returns>An enumeration of <see cref="SystemPhoneNumber"/> objects that match the query and can be viewed by <paramref name="currentPerson"/>.</returns>
        internal IEnumerable<SystemPhoneNumber> GetAuthorizedPhoneNumbers( Person currentPerson, Expression<Func<SystemPhoneNumber, bool>> queryExpression )
        {
            var qry = Queryable();

            if ( queryExpression != null )
            {
                qry = qry.Where( queryExpression );
            }

            return qry
                .ToList()
                .Where( pn => pn.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Deletes the legacy phone number tied to the <see cref="SystemPhoneNumber"/>.
        /// </summary>
        /// <param name="systemPhoneNumber">The system phone number that is about to be deleted.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.15", true )]
        public static void DeleteLegacyPhoneNumber( SystemPhoneNumber systemPhoneNumber )
        {
            DeleteLegacyPhoneNumber( systemPhoneNumber.Guid );
        }

        /// <summary>
        /// Deletes the specified legacy phone number. If it can't be deleted
        /// then it will be marked as inactive.
        /// </summary>
        /// <param name="definedValueGuid">The defined value unique identifier.</param>
        internal static void DeleteLegacyPhoneNumber( Guid definedValueGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedValueService = new DefinedValueService( rockContext );
                var definedValue = definedValueService.Get( definedValueGuid );

                if ( definedValue == null )
                {
                    return;
                }

                definedValueService.Delete( definedValue );

                try
                {
                    rockContext.SaveChanges();
                }
                catch
                {
                    // This is likely due to a 3rd party plugin referencing
                    // the defined value with a foreign key. Just mark it
                    // as in-active.
                    if ( definedValue.IsActive )
                    {
                        using ( var rockContext2 = new RockContext() )
                        {
                            var definedValueService2 = new DefinedValueService( rockContext2 );
                            var definedValue2 = definedValueService.Get( definedValueGuid );

                            definedValue2.IsActive = false;

                            rockContext2.SaveChanges();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the extra legacy phone numbers that no longer have an
        /// associated <see cref="SystemPhoneNumber"/>.
        /// </summary>
        internal static void DeleteExtraLegacyPhoneNumbers()
        {
            using ( var rockContext = new RockContext() )
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var definedTypeId = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_SMS_FROM ).Id;
#pragma warning restore CS0618 // Type or member is obsolete
                var systemPhoneNumberService = new SystemPhoneNumberService( rockContext );
                var definedValueService = new DefinedValueService( rockContext );

                var guidQry = systemPhoneNumberService.Queryable()
                    .Select( spn => spn.Guid );

                var valuesToDelete = definedValueService.Queryable()
                    .Where( dv => dv.DefinedTypeId == definedTypeId
                        && !guidQry.Contains( dv.Guid ) )
                    .Select( dv => dv.Guid )
                    .ToList();

                // These have to each be done in a new RockContext so that we can
                // properly detect and recover from a foreign key violation.
                foreach ( var definedValueGuid in valuesToDelete )
                {
                    DeleteLegacyPhoneNumber( definedValueGuid );
                }
            }
        }

        /// <summary>
        /// Updates the legacy defined value phone number. This creates a new
        /// RockContext and saves the changes before returning.
        /// </summary>
        /// <param name="systemPhoneNumberId">The <see cref="SystemPhoneNumber"/> identifier.</param>
        internal static void UpdateLegacyPhoneNumber( int systemPhoneNumberId )
        {
            lock ( _legacyUpdateLock )
            {
                using ( var rockContext = new RockContext() )
                {
                    var systemPhoneNumberService = new SystemPhoneNumberService( rockContext );
                    var definedValueService = new DefinedValueService( rockContext );
                    var authService = new AuthService( rockContext );

                    var systemPhoneNumber = systemPhoneNumberService.Get( systemPhoneNumberId );

                    if ( systemPhoneNumber == null )
                    {
                        return;
                    }

                    // Load the existing defined value or create a new one.
                    var definedValue = definedValueService.Get( systemPhoneNumber.Guid );

                    if ( definedValue == null )
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        var definedTypeId = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_SMS_FROM ).Id;
#pragma warning restore CS0618 // Type or member is obsolete

                        definedValue = new DefinedValue
                        {
                            Guid = systemPhoneNumber.Guid,
                            DefinedTypeId = definedTypeId
                        };

                        definedValueService.Add( definedValue );
                    }

                    definedValue.LoadAttributes( rockContext );

                    // Update the defined value to match the system phone number.
                    definedValue.Value = systemPhoneNumber.Number;
                    definedValue.Description = systemPhoneNumber.Description;
                    definedValue.IsActive = systemPhoneNumber.IsActive;
                    definedValue.Order = systemPhoneNumber.Order;
                    definedValue.SetAttributeValue( "ResponseRecipient", systemPhoneNumber.AssignedToPersonAlias?.Guid.ToStringSafe() );
                    definedValue.SetAttributeValue( "EnableResponseRecipientForwarding", systemPhoneNumber.IsSmsForwardingEnabled.ToString() );
                    definedValue.SetAttributeValue( "LaunchWorkflowOnResponseReceived", systemPhoneNumber.SmsReceivedWorkflowType?.Guid.ToStringSafe() );

                    // Get the existing auth rules so we can check if they
                    // are already correct or if we need to update them.
                    // This ignores any inherited rules from the Defined Type.
                    // It was decided after discussion that this was good enough.
                    var authRules = authService.Get( systemPhoneNumber.TypeId, systemPhoneNumber.Id ).ToList();
                    var legacyAuthRules = definedValue.Id != 0
                        ? authService.Get( definedValue.TypeId, definedValue.Id ).ToList()
                        : new List<Auth>();

                    var rulesAreEqual = IsLegacyAuthRulesEqual( authRules, legacyAuthRules );

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        definedValue.SaveAttributeValues( rockContext );

                        // If the rules are not the same, update them. This should
                        // be pretty rare and it's easier to just whack the old
                        // ones and create a whole new set of rules.
                        if ( !rulesAreEqual )
                        {
                            authService.DeleteRange( legacyAuthRules );
                            AddLegacyAuthRules( definedValue, authRules, authService );

                            rockContext.SaveChanges();
                        }
                    } );

                    // Refresh all the authorization cache data.
                    if ( !rulesAreEqual )
                    {
                        foreach ( var pair in authRules.GroupBy( a => a.Action ) )
                        {
                            Authorization.RefreshAction( definedValue.TypeId, definedValue.Id, pair.Key, rockContext );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the two auth rule sets are considered equal to
        /// each other for a legacy phone number.
        /// </summary>
        /// <param name="firstRuleSet">The first rule set.</param>
        /// <param name="secondRuleSet">The second rule set.</param>
        /// <returns><c>true</c> if auth rule sets are considered the same; otherwise, <c>false</c>.</returns>
        private static bool IsLegacyAuthRulesEqual( List<Auth> firstRuleSet, List<Auth> secondRuleSet )
        {
            if ( firstRuleSet.Count != secondRuleSet.Count )
            {
                return false;
            }

            // Order both sets in a way that lets us just loop through and
            // compare each rule in sequence.
            firstRuleSet = firstRuleSet
                .OrderBy( a => a.Action )
                .ThenBy( a => a.Order )
                .ThenBy( a => a.Id )
                .ToList();

            secondRuleSet = secondRuleSet
                .OrderBy( a => a.Action )
                .ThenBy( a => a.Order )
                .ThenBy( a => a.Id )
                .ToList();

            // Check if any rules are not equal.
            for ( int i = 0; i < firstRuleSet.Count; i++ )
            {
                var firstRule = firstRuleSet[i];
                var secondRule = secondRuleSet[i];

                var rulesAreEqual = firstRule.Action == secondRule.Action
                    && firstRule.AllowOrDeny == secondRule.AllowOrDeny
                    && firstRule.GroupId == secondRule.GroupId
                    && firstRule.Order == secondRule.Order
                    && firstRule.PersonAliasId == secondRule.PersonAliasId
                    && firstRule.SpecialRole == secondRule.SpecialRole;

                if ( !rulesAreEqual )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds the legacy authorization rules to the defined value.
        /// </summary>
        /// <param name="definedValue">The defined value.</param>
        /// <param name="newAuthRules">The new authorization rules.</param>
        /// <param name="authService">The authorization service.</param>
        private static void AddLegacyAuthRules( DefinedValue definedValue, List<Auth> newAuthRules, AuthService authService )
        {
            foreach ( var newAuthRule in newAuthRules )
            {
                var auth = new Auth
                {
                    EntityTypeId = definedValue.TypeId,
                    EntityId = definedValue.Id,
                    Action = newAuthRule.Action,
                    AllowOrDeny = newAuthRule.AllowOrDeny,
                    GroupId = newAuthRule.GroupId,
                    Order = newAuthRule.Order,
                    PersonAliasId = newAuthRule.PersonAliasId,
                    SpecialRole = newAuthRule.SpecialRole
                };

                authService.Add( auth );
            }
        }
    }
}
