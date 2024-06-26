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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Security;

using Rock.Bus.Message;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Utility.Settings;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Security
{
    /// <summary>
    /// Static class for managing authorizations
    /// </summary>
    public static class Authorization
    {
        /// <summary>
        /// Available settings for SameSiteCookie
        /// </summary>
        public enum SameSiteCookieSetting
        {
            /// <summary>
            /// Do not specify a setting
            /// </summary>
            None,

            /// <summary>
            /// Lax
            /// </summary>
            Lax,

            /// <summary>
            /// Strict
            /// </summary>
            Strict
        }

        /// <summary>
        /// Authentication Level Type
        /// </summary>
        public enum AuthenticationLevel
        {
            /// <summary>
            /// TrustedLogin
            /// </summary>
            TrustedLogin = 10,

            /// <summary>
            /// TokenAuthentication
            /// </summary>
            TokenAuthentication = 20,

            /// <summary>
            /// Identified
            /// </summary>
            Identified = 30,

            /// <summary>
            /// None
            /// </summary>
            None = 40
        }

        #region Constants

        /// <summary>
        /// Cache Key
        /// </summary>
        private const string CACHE_KEY = "Rock.Security.Authorization";

        /// <summary>
        /// Authorization to view object
        /// </summary>
        public const string VIEW = "View";

        /// <summary>
        /// Authorization to edit object ( add, set properties, delete, etc )
        /// </summary>
        public const string EDIT = "Edit";

        /// <summary>
        /// Authorization to delete object (only used in few places where delete needs to be secured differently that EDIT, i.e. Financial Batch )
        /// </summary>
        public const string DELETE = "Delete";

        /// <summary>
        /// Authorization to administer object ( add child object, set security, etc)
        /// </summary>
        public const string ADMINISTRATE = "Administrate";

        /// <summary>
        /// Authorization to approve object (html, prayer, ads, etc)
        /// </summary>
        public const string APPROVE = "Approve";

        /// <summary>
        /// Authorization to interact with the object (content channel item)
        /// </summary>
        public const string INTERACT = "Interact";

        /// <summary>
        /// Authorization to refund a transaction
        /// </summary>
        public const string REFUND = "Refund";

        /// <summary>
        /// Authorization to manage the group members
        /// </summary>
        public const string MANAGE_MEMBERS = "ManageMembers";

        /// <summary>
        /// Authorization to perform scheduling
        /// </summary>
        public const string SCHEDULE = "Schedule";

        /// <summary>
        /// Authorization action for using (tagging with) the Tag.
        /// </summary>
        public const string TAG = "Tag";

        /// <summary>
        /// Unauthenticated Person Identifier cookie.
        /// </summary>
        public const string COOKIE_UNSECURED_PERSON_IDENTIFIER = ".ROCK-UnauthenticatedPersonIdentifier";

        /// <summary>
        /// Authorization to manage the steps
        /// </summary>
        public const string MANAGE_STEPS = "ManageSteps";

        /// <summary>
        /// Authorization to delete check-in attendance
        /// </summary>
        public const string DELETE_ATTENDANCE = "DeleteAttendance";

        /// <summary>
        /// Authorization to override for group requirements
        /// </summary>
        public const string OVERRIDE = "Override";

        /// <summary>
        /// Authorization to view the protection profile alert for the selected person.
        /// </summary>
        public const string VIEW_PROTECTION_PROFILE = "ViewProtectionProfile";

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private static void AddOrUpdate( Dictionary<int, Dictionary<int, Dictionary<string, List<AuthRule>>>> authorizations )
        {
            RockCache.AddOrUpdate( CACHE_KEY, authorizations );
            AuthorizationCacheWasUpdatedMessage.Publish( CACHE_KEY );
        }

        private static Dictionary<int, Dictionary<int, Dictionary<string, List<AuthRule>>>> LoadAuthorizations()
        {
            // Load the authorizations
            var authorizations = new Dictionary<int, Dictionary<int, Dictionary<string, List<AuthRule>>>>();

            var securityGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
            int securityGroupTypeId = securityGroupType?.Id ?? 0;

            // query the database for all of the entity auth rules
            var authEntityRules = new List<AuthEntityRule>();
            using ( var rockContext = new RockContext() )
            {
                foreach ( var auth in new AuthService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( t =>
                        t.Group == null ||
                        ( t.Group.IsActive && ( t.Group.IsSecurityRole || t.Group.GroupTypeId == securityGroupTypeId ) ) )
                    .OrderBy( A => A.EntityTypeId )
                    .ThenBy( A => A.EntityId )
                    .ThenBy( A => A.Action )
                    .ThenBy( A => A.Order )
                    .ThenBy( A => A.Id )
                    .Select( a => new
                    {
                        a.Id,
                        a.EntityTypeId,
                        a.EntityId,
                        a.Action,
                        a.AllowOrDeny,
                        a.SpecialRole,
                        a.PersonAliasId,
                        PersonId = a.PersonAlias != null ? a.PersonAlias.PersonId : ( int? ) null,
                        a.GroupId,
                        a.Order
                    } ) )
                {
                    authEntityRules.Add( new AuthEntityRule(
                        auth.Id,
                        auth.EntityTypeId,
                        auth.Action,
                        auth.EntityId,
                        auth.AllowOrDeny,
                        auth.SpecialRole,
                        auth.PersonId,
                        auth.PersonAliasId,
                        auth.GroupId,
                        auth.Order
                    ) );
                }
            }

            foreach ( var authEntityRule in authEntityRules )
            {
                authorizations.TryAdd( authEntityRule.EntityTypeId, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
                var entityAuths = authorizations[authEntityRule.EntityTypeId];

                entityAuths.TryAdd( authEntityRule.AuthRule.EntityId ?? 0, new Dictionary<string, List<AuthRule>>( StringComparer.OrdinalIgnoreCase ) );
                var instanceAuths = entityAuths[authEntityRule.AuthRule.EntityId ?? 0];

                instanceAuths.TryAdd( authEntityRule.Action, new List<AuthRule>() );
                var actionPermissions = instanceAuths[authEntityRule.Action];

                actionPermissions.Add( authEntityRule.AuthRule );
            }

            return authorizations;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load the static Authorizations object
        /// </summary>
        public static Dictionary<int, Dictionary<int, Dictionary<string, List<AuthRule>>>> Get()
        {
            return RockCache.GetOrAddExisting( CACHE_KEY, LoadAuthorizations ) as Dictionary<int, Dictionary<int, Dictionary<string, List<AuthRule>>>>;
        }

        /// <summary>
        /// Reloads the entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        internal static void RefreshEntity( int entityTypeId, int entityId, RockContext rockContext = null )
        {
            // Clear the current entity type's auths
            var authorizations = Get();
            if ( authorizations != null && authorizations.ContainsKey( entityTypeId ) )
            {
                var entityAuths = authorizations[entityTypeId];
                if ( entityAuths.ContainsKey( entityId ) )
                {
                    entityAuths[entityId] = new Dictionary<string, List<AuthRule>>( StringComparer.OrdinalIgnoreCase );
                    AddOrUpdate( authorizations );
                }
            }

            // Query database for the authorizations related to this entitytype, entity, and action
            List<Auth> auths;
            if ( rockContext != null )
            {
                auths = LoadAuths( entityTypeId, entityId, rockContext );
            }
            else
            {
                using ( rockContext = new RockContext() )
                {
                    auths = LoadAuths( entityTypeId, entityId, rockContext );
                }
            }

            // Update the auths
            var actions = auths.Select( a => a.Action ).Distinct().ToList();
            foreach ( var action in actions )
            {
                var newAuthRules = auths
                    .Where( a => a.Action == action )
                    .Select( auth => new AuthRule( auth ) )
                    .ToList();

                ResetAction( entityTypeId, entityId, action, newAuthRules );
            }
        }

        /// <summary>
        /// Loads the authorizations
        /// </summary>
        /// <param name="entityTypeId">The Entity Type Id</param>
        /// <param name="entityId">The Entity Id</param>
        /// <param name="rockContext">The RockContext to use</param>
        /// <returns></returns>
        private static List<Auth> LoadAuths( int entityTypeId, int entityId, RockContext rockContext )
        {
            var securityGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid(), rockContext );
            int securityGroupTypeId = securityGroupType?.Id ?? 0;

            return new AuthService( rockContext )
                .Get( entityTypeId, entityId )
                .AsNoTracking()
                .Where( t =>
                    t.Group == null ||
                    ( t.Group.IsActive && ( t.Group.IsSecurityRole || t.Group.GroupTypeId == securityGroupTypeId ) )
                )
                .OrderBy( a => a.Order ).ThenBy( a => a.Id )
                .ToList();
        }

        /// <summary>
        /// Reloads the authorizations for the specified entity and action.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="action">The action.</param>
        public static void RefreshAction( int entityTypeId, int entityId, string action )
        {
            var newAuthRules = new List<AuthRule>();

            // Query database for the authorizations related to this entitytype, entity, and action
            using ( var rockContext = new RockContext() )
            {
                var securityGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid(), rockContext );
                int securityGroupTypeId = securityGroupType?.Id ?? 0;

                foreach ( var auth in new AuthService( rockContext )
                    .GetAuths( entityTypeId, entityId, action )
                    .AsNoTracking()
                    .Where( t =>
                        t.Group == null ||
                        ( t.Group.IsActive && ( t.Group.IsSecurityRole || t.Group.GroupTypeId == securityGroupTypeId ) )
                    ) )
                {
                    newAuthRules.Add( new AuthRule( auth ) );
                }
            }

            ResetAction( entityTypeId, entityId, action, newAuthRules );
        }

        /// <summary>
        /// Reloads the action.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="action">The action.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void RefreshAction( int entityTypeId, int entityId, string action, RockContext rockContext )
        {
            var newAuthRules = new List<AuthRule>();

            var securityGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid(), rockContext );
            int securityGroupTypeId = securityGroupType?.Id ?? 0;

            // Query database for the authorizations related to this entitytype, entity, and action
            foreach ( var auth in new AuthService( rockContext )
                .GetAuths( entityTypeId, entityId, action )
                .AsNoTracking()
                .Where( t =>
                    t.Group == null ||
                    ( t.Group.IsActive && ( t.Group.IsSecurityRole || t.Group.GroupTypeId == securityGroupTypeId ) )
                ) )
            {
                newAuthRules.Add( new AuthRule( auth ) );
            }

            ResetAction( entityTypeId, entityId, action, newAuthRules );
        }

        /// <summary>
        /// Evaluates whether a selected user is allowed to perform the selected action on the selected
        /// entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="specialRole">The special role.</param>
        /// <returns></returns>
        public static bool Authorized( ISecured entity, string action, SpecialRole specialRole )
        {
            return ItemAuthorized( entity, action, specialRole, true, true ) ?? entity.IsAllowedByDefault( action );
        }

        /// <summary>
        /// Evaluates whether a selected person is allowed to perform the selected action on the selected
        /// entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static bool Authorized( ISecured entity, string action, Person person )
        {
            int recursiveCallCount = 0;
            return ItemAuthorized( entity, action, person, true, true, ref recursiveCallCount ) ?? entity.IsAllowedByDefault( action );
        }

        /// <summary>
        /// Determines whether the specified entity is private. Entity is considered private if only the current user
        /// has access.  In this scenario, the first rule would give current user access, and second rule would deny
        /// all users.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified entity is private; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPrivate( ISecured entity, string action, Person person )
        {
            if ( person == null )
            {
                return false;
            }

            var authorizations = Get();

            // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each
            // one to find the first one specific to the selected user or a role that the selected user belongs
            // to.  If a match is found return whether the user is allowed (true) or denied (false) access
            if ( authorizations == null || !authorizations.Keys.Contains( entity.TypeId ) ||
                !authorizations[entity.TypeId].Keys.Contains( entity.Id ) ||
                !authorizations[entity.TypeId][entity.Id].Keys.Contains( action ) ||
                authorizations[entity.TypeId][entity.Id][action].Count != 2 )
            {
                return false;
            }

            var firstRule = authorizations[entity.TypeId][entity.Id][action][0];
            var secondRule = authorizations[entity.TypeId][entity.Id][action][1];

            // If first rule allows current user, and second rule denies all other users then entity is private
            if ( firstRule.AllowOrDeny == 'A' &&
                 firstRule.SpecialRole == SpecialRole.None &&
                 firstRule.PersonId == person.Id &&
                 secondRule.AllowOrDeny == 'D' &&
                 secondRule.SpecialRole == SpecialRole.AllUsers )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Allows all users.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void AllowAllUsers( ISecured entity, string action, RockContext rockContext = null )
        {
            if ( rockContext != null )
            {
                MyAllowAllUsers( entity, action, rockContext );
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    MyAllowAllUsers( entity, action, myRockContext );
                }
            }
        }

        /// <summary>
        /// Makes the entity private by setting up two authorization rules, one granting the selected person, and
        /// then another that denies all other users.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void MakePrivate( ISecured entity, string action, Person person, RockContext rockContext = null )
        {
            if ( rockContext != null )
            {
                MyMakePrivate( entity, action, person, rockContext );
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    MyMakePrivate( entity, action, person, myRockContext );
                }
            }
        }

        /// <summary>
        /// Removes that two authorization rules that made the entity private.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void MakeUnPrivate( ISecured entity, string action, Person person, RockContext rockContext = null )
        {
            if ( rockContext != null )
            {
                MyMakeUnPrivate( entity, action, person, rockContext );
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    MyMakeUnPrivate( entity, action, person, myRockContext );
                }
            }
        }

        /// <summary>
        /// Updates authorization rules for the entity so that the current person is allowed to perform the specified action.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void AllowPerson( ISecured entity, string action, Person person, RockContext rockContext = null )
        {
            if ( rockContext != null )
            {
                MyAllow( entity, action, person, null, SpecialRole.None, rockContext );
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    MyAllow( entity, action, person, null, SpecialRole.None, myRockContext );
                }
            }
        }

        /// <summary>
        /// Allows the security role.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void AllowSecurityRole( ISecured entity, string action, Group group, RockContext rockContext = null )
        {
            if ( rockContext != null )
            {
                MyAllow( entity, action, null, group, SpecialRole.None, rockContext );
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    MyAllow( entity, action, null, group, SpecialRole.None, myRockContext );
                }
            }
        }

        /// <summary>
        /// Returns the authorization rules for the specified entity and action.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static List<AuthRule> AuthRules( int entityTypeId, int entityId, string action )
        {
            var rules = new List<AuthRule>();

            var authorizations = Get();

            // Find the Authrules for the given entity type, entity id, and action
            if ( authorizations == null || !authorizations.ContainsKey( entityTypeId ) )
            {
                return rules;
            }

            if ( !authorizations[entityTypeId].ContainsKey( entityId ) )
            {
                return rules;
            }

            if ( authorizations[entityTypeId][entityId].ContainsKey( action ) )
            {
                rules = authorizations[entityTypeId][entityId][action];
            }

            return rules;
        }

        /// <summary>
        /// Copies the authorization.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="targetEntity">The target entity.</param>
        public static void CopyAuthorization( ISecured sourceEntity, ISecured targetEntity )
        {
            using ( var rockContext = new RockContext() )
            {
                CopyAuthorization( sourceEntity, targetEntity, rockContext );
            }
        }

        /// <summary>
        /// Copies the authorizations from one <see cref="ISecured" /> object to another
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="targetEntity">The target entity.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">Optional action (if omitted or left blank, all actions will be copied).</param>
        /// <remarks>
        /// This method will save any previous changes made to the context
        /// </remarks>
        public static void CopyAuthorization( ISecured sourceEntity, ISecured targetEntity, RockContext rockContext, string action = "" )
        {
            var sourceEntityTypeId = sourceEntity.TypeId;
            var targetEntityTypeId = targetEntity.TypeId;

            var authService = new AuthService( rockContext );

            // Delete the current authorizations for the target entity
            foreach ( var auth in authService.Get( targetEntityTypeId, targetEntity.Id ).ToList() )
            {
                if ( string.IsNullOrWhiteSpace( action ) || auth.Action.Equals( action, StringComparison.OrdinalIgnoreCase ) )
                {
                    authService.Delete( auth );
                }
            }

            rockContext.SaveChanges();

            // Copy target auths to source auths
            var order = 0;
            foreach ( var sourceAuth in authService.Get( sourceEntityTypeId, sourceEntity.Id ).ToList() )
            {
                if ( !string.IsNullOrWhiteSpace( action ) &&
                    !sourceAuth.Action.Equals( action, StringComparison.OrdinalIgnoreCase ) ||
                    !targetEntity.SupportedActions.ContainsKey( sourceAuth.Action ) )
                {
                    continue;
                }

                var auth = new Auth
                {
                    EntityTypeId = targetEntityTypeId,
                    EntityId = targetEntity.Id,
                    Action = sourceAuth.Action,
                    AllowOrDeny = sourceAuth.AllowOrDeny,
                    GroupId = sourceAuth.GroupId,
                    PersonAliasId = sourceAuth.PersonAliasId,
                    SpecialRole = sourceAuth.SpecialRole,
                    Order = order++
                };

                authService.Add( auth );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Finds the auth rules.
        /// </summary>
        /// <param name="securableObject">The securable object.</param>
        /// <returns></returns>
        public static IQueryable<AuthRule> FindAuthRules( ISecured securableObject )
        {
            return ( from action in securableObject.SupportedActions
                     from rule in AuthRules( securableObject.TypeId, securableObject.Id, action.Key )
                     select rule ).AsQueryable();
        }

        /// <summary>
        /// Clear the static Authorizations object
        /// </summary>
        public static void Clear()
        {
            RockCache.Remove( CACHE_KEY );
        }

        /// <summary>
        /// Encodes the entity type name for use in a URL
        /// </summary>
        /// <param name="iSecuredType">Type of the item to secure.</param>
        /// <returns></returns>
        public static string EncodeEntityTypeName( Type iSecuredType )
        {
            return EncodeEntityTypeName( iSecuredType.AssemblyQualifiedName );
        }

        /// <summary>
        /// Encodes the entity type name for use in a URL
        /// </summary>
        /// <param name="assemblyQualifiedName">Assembly name of the item to secure.</param>
        /// <returns></returns>
        public static string EncodeEntityTypeName( string assemblyQualifiedName )
        {
            var b = Encoding.UTF8.GetBytes( assemblyQualifiedName );
            return Convert.ToBase64String( b );
        }

        /// <summary>
        /// Decodes the entity type name.
        /// </summary>
        /// <param name="encodedTypeName">Name of the encoded type.</param>
        /// <returns></returns>
        public static string DecodeEntityTypeName( string encodedTypeName )
        {
            var b = Convert.FromBase64String( encodedTypeName );
            return Encoding.UTF8.GetString( b );
        }

        /// <summary>
        /// Gets the auth cookie.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="isPersisted">if set to <c>true</c> [is persisted].</param>
        /// <param name="isImpersonated">if set to <c>true</c> [is impersonated].</param>
        /// <param name="isTwoFactorAuthenticated">if set to <c>true</c> [is two-factor authenticated].</param>
        private static HttpCookie GetAuthCookie( string userName, bool isPersisted, bool isImpersonated, bool isTwoFactorAuthenticated )
        {
            return GetAuthCookie( userName, isPersisted, isImpersonated, isTwoFactorAuthenticated, FormsAuthentication.Timeout );
        }

        /// <summary>
        /// Gets the auth cookie.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="isPersisted">if set to <c>true</c> [is persisted].</param>
        /// <param name="isImpersonated">if set to <c>true</c> [is impersonated].</param>
        /// <param name="expiresIn">The cookie expiration.</param>
        /// <param name="isTwoFactorAuthenticated">if set to <c>true</c> [is two-factor authenticated].</param>
        private static HttpCookie GetAuthCookie( string userName, bool isPersisted, bool isImpersonated, bool isTwoFactorAuthenticated, TimeSpan expiresIn )
        {
            var userData = new AuthenticationTicketUserData( isImpersonated, isTwoFactorAuthenticated );

            var ticket = new FormsAuthenticationTicket(
                1,
                userName,
                RockDateTime.SystemDateTime,
                RockDateTime.SystemDateTime.Add( expiresIn ),
                isPersisted,
                userData.ToJson(),
                FormsAuthentication.FormsCookiePath );
            
            var authCookie = GetAuthCookie( GetCookieDomain(), FormsAuthentication.Encrypt( ticket ) );

            if ( ticket.IsPersistent )
            {
                authCookie.Expires = ticket.Expiration;
            }

            return authCookie;
        }

        /// <summary>
        /// Gets the simple authentication cookie.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="isPersisted">if set to <c>true</c> [is persisted].</param>
        /// <param name="isImpersonated">if set to <c>true</c> [is impersonated].</param>
        /// <returns></returns>
        public static SimpleCookie GetSimpleAuthCookie( string userName, bool isPersisted, bool isImpersonated )
        {
            var authCookie = GetAuthCookie( userName, isPersisted, isImpersonated, isTwoFactorAuthenticated: false );

            if ( authCookie == null )
            {
                return null;
            }

            return new SimpleCookie
            {
                Expires = authCookie.Expires,
                Name = authCookie.Name,
                Value = authCookie.Value
            };
        }

        /// <summary>
        /// Gets the user data in an authentication ticket.
        /// </summary>
        /// <param name="formsAuthenticationTicket">The forms authentication ticket.</param>
        /// <returns>The user data.</returns>
        internal static IAuthenticationTicketUserData GetUserData( FormsAuthenticationTicket formsAuthenticationTicket )
        {
            if ( formsAuthenticationTicket == null )
            {
                return null;
            }

            var userData = formsAuthenticationTicket.UserData.FromJsonOrNull<AuthenticationTicketUserData>();

            if ( userData != null )
            {
                return userData;
            }

            /*
                10/19/2023 - JMH

                If we are here, then the ticket's user data is not a JSON AuthenticationTicketUserData object.

                The user data used to contain a stringified boolean value indicating whether the authenticated
                user is impersonated.

                Assuming the user data is in this old format, return a new AuthenticationTicketUserData object
                and try to set the IsImpersonated property to the current ticket's user data value.

                Reason: Two-Factor Authentication
             */
            return new AuthenticationTicketUserData(
                isImpersonated: formsAuthenticationTicket.UserData?.ToString().ToLower() == "true",
                isTwoFactorAuthenticated: false );
        }

        /// <summary>
        /// Sets the auth cookie.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="isPersisted">if set to <c>true</c> [is persisted].</param>
        /// <param name="isImpersonated">if set to <c>true</c> [is impersonated].</param>
        public static void SetAuthCookie( string userName, bool isPersisted, bool isImpersonated )
        {
            SetAuthCookie( userName, isPersisted, isImpersonated, isTwoFactorAuthenticated: false );
        }

        /// <summary>
        /// Sets the auth cookie.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="isPersisted">if set to <c>true</c> [is persisted].</param>
        /// <param name="isImpersonated">if set to <c>true</c> [is impersonated].</param>
        /// <param name="isTwoFactorAuthenticated">if set to <c>true</c> [is two-factor authenticated].</param>
        public static void SetAuthCookie( string userName, bool isPersisted, bool isImpersonated, bool isTwoFactorAuthenticated )
        {
            SetAuthCookie( userName, isPersisted, isImpersonated, isTwoFactorAuthenticated, FormsAuthentication.Timeout );
        }

        /// <summary>
        /// Sets the auth cookie.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="isPersisted">if set to <c>true</c> [is persisted].</param>
        /// <param name="isImpersonated">if set to <c>true</c> [is impersonated].</param>
        /// <param name="isTwoFactorAuthenticated">if set to <c>true</c> [is two-factor authenticated].</param>
        /// <param name="expiresIn">The cookie expiration.</param>
        internal static void SetAuthCookie( string userName, bool isPersisted, bool isImpersonated, bool isTwoFactorAuthenticated, TimeSpan expiresIn )
        {
            var authCookie = GetAuthCookie( userName, isPersisted, isImpersonated, isTwoFactorAuthenticated, expiresIn );
            RockPage.AddOrUpdateCookie( authCookie );

            // If cookie is for a more generic domain, we need to store that domain so that we can expire it correctly
            // when the user signs out.
            if ( authCookie.Domain.IsNullOrWhiteSpace() )
            {
                return;
            }

            var domainCookie =
                new HttpCookie( $"{FormsAuthentication.FormsCookieName}_DOMAIN", authCookie.Domain )
                {
                    HttpOnly = true,
                    Domain = authCookie.Domain,
                    Path = FormsAuthentication.FormsCookiePath,
                    Secure = FormsAuthentication.RequireSSL,
                    Expires = authCookie.Expires
                };

            RockPage.AddOrUpdateCookie( domainCookie );
        }

        /// <summary>
        /// Signs a user out of rock by deleting the appropriate forms authentication cookies
        /// </summary>
        public static void SignOut()
        {
            ExpireUnsecuredPersonIdentifierCookie();

            var domainCookieName = $"{FormsAuthentication.FormsCookieName}_DOMAIN";
            var domainCookie = HttpContext.Current.Request.Cookies[domainCookieName];

            if ( domainCookie != null )
            {
                var authCookie = GetAuthCookie( domainCookie.Value, null );
                authCookie.Expires = RockDateTime.SystemDateTime.AddDays( -1d );
                RockPage.AddOrUpdateCookie( authCookie );

                domainCookie = new HttpCookie( domainCookieName )
                {
                    HttpOnly = true,
                    Domain = authCookie.Domain,
                    Path = FormsAuthentication.FormsCookiePath,
                    Secure = FormsAuthentication.RequireSSL,
                    Expires = RockDateTime.SystemDateTime.AddDays( -1d )
                };

                RockPage.AddOrUpdateCookie( domainCookie );
            }
            else
            {
                FormsAuthentication.SignOut();
            }
        }

        /// <summary>
        /// Expires the Unsecured Person Identifier Cookie
        /// </summary>
        /// <returns></returns>
        private static void ExpireUnsecuredPersonIdentifierCookie()
        {
            if ( HttpContext.Current.Request.Cookies.AllKeys.Contains( Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER ) )
            {
                RockPage.AddOrUpdateCookie( Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER, null, RockDateTime.SystemDateTime.AddDays( -1d ) );
            }
        }

        /// <summary>
        /// Create a forms authentication cookie.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static HttpCookie GetAuthCookie( string domain, string value )
        {
            // Get the SameSite setting from the Global Attributes. If not set then default to Lax. Official IETF values are "Lax", "Strict", and "None".
            SameSiteCookieSetting sameSiteCookieSetting = GlobalAttributesCache.Get().GetValue( "core_SameSiteCookieSetting" ).ConvertToEnumOrNull<SameSiteCookieSetting>() ?? SameSiteCookieSetting.Lax;

            // If IsSecureConnection is false then check the scheme in case the web server is behind a load balancer.
            // The server could use unencrypted traffic to the balancer, which would encrypt it before sending to the browser.
            var secureSetting = HttpContext.Current.Request.IsSecureConnection || HttpContext.Current.Request.UrlProxySafe().Scheme == "https" ? ";Secure" : string.Empty;

            // For browsers to recognize SameSite=none the Secure tag is required, but it doesn't hurt to add it for all samesite settings.
            string sameSiteCookieValue = $";SameSite={sameSiteCookieSetting}{secureSetting}";

            var httpCookie = new HttpCookie( FormsAuthentication.FormsCookieName, value )
            {
                Domain = domain.IsNotNullOrWhiteSpace() ? domain : FormsAuthentication.CookieDomain,
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath + sameSiteCookieValue,
                Secure = FormsAuthentication.RequireSSL
            };

            return httpCookie;
        }

        /// <summary>
        /// Gets the domain for the forms authentication cookie. This is based on whether the current host name has an entry in the 'Domains Sharing Logins' defined type.
        /// </summary>
        /// <returns></returns>
        private static string GetCookieDomain()
        {
            // Get the domains that should be saving cookies as domain level cookies instead of the default of subdomain level.
            var dt = DefinedTypeCache.Get( SystemGuid.DefinedType.DOMAINS_SHARING_LOGINS.AsGuid() );
            var domains = dt?.DefinedValues.Select( v => v.Value ).ToList() ?? new List<string>();

            // Get the first domain in the list that the current request's host name ends with
            var host = WebRequestHelper.GetHostNameFromRequest( HttpContext.Current );
            var domain = domains.FirstOrDefault( d => host.ToLower().EndsWith( d.ToLower() ) );
            if ( !domain.IsNotNullOrWhiteSpace() )
            {
                return null;
            }

            // Make sure domain name is prefixed with a '.'
            domain = domain != null && domain.StartsWith( "." ) ? domain : $".{domain}";

            // Make sure there now at least two '.' characters (this is required for browser to store cookie).
            return domain.Count( c => c == '.' ) >= 2 ? domain : string.Empty;
        }

        /// <summary>
        /// Checks to see if a person is authorized for entity
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static bool? AuthorizedForEntity( ISecured entity, string action, Person person )
        {
            return AuthorizedForEntity( entity, action, person, false );
        }

        /// <summary>
        /// Checks to see if a person is authorized for entity or it's parent authority
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="checkParentAuthority">if set to <c>true</c> [check parent authority].</param>
        /// <returns></returns>
        public static bool? AuthorizedForEntity( ISecured entity, string action, Person person, bool checkParentAuthority )
        {
            int recursiveCallCount = 0;
            return ItemAuthorized( entity, action, person, true, checkParentAuthority, ref recursiveCallCount );
        }

        /// <summary>
        /// Sets the unsecure person identifier (COOKIE_UNSECURED_PERSON_IDENTIFIER) cookie.
        /// </summary>
        /// <param name="personAliasGuid">The person alias unique identifier.</param>
        public static void SetUnsecurePersonIdentifier( Guid personAliasGuid )
        {
            HttpCookie httpcookie = new HttpCookie( Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER );
            httpcookie.Value = personAliasGuid.ToString();
            httpcookie.Expires = RockDateTime.SystemDateTime.AddYears( 1 );
            RockPage.AddOrUpdateCookie( httpcookie );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks to see if a special role is authorized
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="isRootEntity">if set to <c>true</c> [is root entity].</param>
        /// <param name="checkParentAuthority">if set to <c>true</c> [check parent authority].</param>
        /// <returns></returns>
        private static bool? ItemAuthorized( ISecured entity, string action, SpecialRole specialRole, bool isRootEntity, bool checkParentAuthority )
        {
            var entityTypeId = entity.TypeId;

            var matchFound = false;
            var authorized = false;

            var authorizations = Get();

            // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each
            // one to find the first one specific to the selected user or a role that the selected user belongs
            // to.  If a match is found return whether the user is allowed (true) or denied (false) access
            if ( authorizations != null &&
                authorizations.Keys.Contains( entityTypeId ) &&
                authorizations[entityTypeId].Keys.Contains( entity.Id ) &&
                authorizations[entityTypeId][entity.Id].Keys.Contains( action ) )
            {
                foreach ( var authRule in authorizations[entityTypeId][entity.Id][action] )
                {
                    if ( authRule.SpecialRole != specialRole )
                    {
                        continue;
                    }

                    matchFound = true;
                    authorized = authRule.AllowOrDeny == 'A';
                    break;
                }
            }

            if ( matchFound )
            {
                return authorized;
            }

            // If no match was found for the selected user on the current entity instance, check to see if the instance
            // has a parent authority defined and if so evaluate that entities authorization rules.  If there is no
            // parent authority return the default authorization
            bool? parentAuthorized = null;

            if ( !checkParentAuthority )
            {
                return null;
            }

            if ( isRootEntity && entity.ParentAuthorityPre != null )
            {
                parentAuthorized = ItemAuthorized( entity.ParentAuthorityPre, action, specialRole, false, false );
            }

            if ( !parentAuthorized.HasValue && entity.ParentAuthority != null )
            {
                parentAuthorized = ItemAuthorized( entity.ParentAuthority, action, specialRole, false, true );
            }

            return parentAuthorized;
        }

        /// <summary>
        /// Checks to see if a person is authorized
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="isRootEntity">if set to <c>true</c> [is root entity].</param>
        /// <param name="checkParentAuthority">if set to <c>true</c> [check parent].</param>
        /// <param name="recursiveCallCount">The recursive call count.</param>
        private static bool? ItemAuthorized( ISecured entity, string action, Person person, bool isRootEntity, bool checkParentAuthority, ref int recursiveCallCount )
        {
            var entityTypeId = entity.TypeId;

            var matchFound = false;
            var authorized = false;

            // We only need the AuthRules for the specified EntityType, Entity and action, so lets get them all here.
            var authRules = Get()?.GetValueOrNull( entityTypeId )?.GetValueOrNull( entity.Id )?.GetValueOrNull( action );

            // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each
            // one to find the first one specific to the selected user or a role that the selected user belongs
            // to.  If a match is found return whether the user is allowed (true) or denied (false) access
            if ( authRules != null  )
            {
                var personGuid = person?.Guid;
                foreach ( var authRule in authRules )
                {
                    // All Users
                    if ( authRule.SpecialRole == SpecialRole.AllUsers )
                    {
                        matchFound = true;
                        authorized = authRule.AllowOrDeny == 'A';
                        break;
                    }

                    // All Authenticated Users
                    if ( authRule.SpecialRole == SpecialRole.AllAuthenticatedUsers && personGuid.HasValue )
                    {
                        matchFound = true;
                        authorized = authRule.AllowOrDeny == 'A';
                        break;
                    }

                    // All Unauthenticated Users
                    if ( authRule.SpecialRole == SpecialRole.AllUnAuthenticatedUsers && !personGuid.HasValue )
                    {
                        matchFound = true;
                        authorized = authRule.AllowOrDeny == 'A';
                        break;
                    }

                    // If rule is a special role, or the person is unknown, just continue, we don't need to check for person/role access
                    if ( authRule.SpecialRole != SpecialRole.None || person == null )
                    {
                        continue;
                    }

                    // Rule is for this person
                    if ( authRule.PersonId.HasValue && authRule.PersonId.Value == person.Id )
                    {
                        matchFound = true;
                        authorized = authRule.AllowOrDeny == 'A';
                        break;
                    }

                    // If the rule is not for a group, just keep looping, we don't need to see if they're in a role
                    if ( !authRule.GroupId.HasValue )
                    {
                        continue;
                    }

                    // Get the role
                    var role = RoleCache.Get( authRule.GroupId.Value );

                    // If the role was invalid, or person is not in the role, keep checking
                    if ( role == null || !role.IsPersonInRole( personGuid ) )
                    {
                        continue;
                    }

                    // At this point, the rule is for a group/role that user belongs to
                    matchFound = true;
                    authorized = authRule.AllowOrDeny == 'A';
                    break;
                }
            }

            if ( matchFound )
            {
                return authorized;
            }

            /* 10-31-2022 MDP

            Let make sure we aren't stuck in infinite recursion.
            We will get a stack overflow if this method is called recursively more than around 400 times, so lets limit it to 100. 
            In this situation, it is almost certainly an infinite recursion situation, so treat as if no rules were found and return NULL.
            
            */

            const long maxRecursiveCallCount = 100;
            if ( recursiveCallCount > maxRecursiveCallCount )
            {
                return null;
            }

            recursiveCallCount++;

            // If no match was found for the selected user on the current entity instance, check to see if the instance
            // has a parent authority defined and if so evaluate that entities authorization rules.  If there is no
            // parent authority return the default authorization
            bool? parentAuthorized = null;
            if ( !checkParentAuthority )
            {
                return null;
            }

            if ( isRootEntity && entity.ParentAuthorityPre != null )
            {
                parentAuthorized = ItemAuthorized( entity.ParentAuthorityPre, action, person, false, false, ref recursiveCallCount );
            }

            if ( !parentAuthorized.HasValue && entity.ParentAuthority != null )
            {
                parentAuthorized = ItemAuthorized( entity.ParentAuthority, action, person, false, true, ref recursiveCallCount );
            }

            return parentAuthorized;
        }

        /// <summary>
        /// Mies the allow all users.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void MyAllowAllUsers( ISecured entity, string action, RockContext rockContext )
        {
            var authService = new AuthService( rockContext );

            // Delete any existing rules in database
            foreach ( var auth in authService
                .GetAuths( entity.TypeId, entity.Id, action ) )
            {
                authService.Delete( auth );
            }

            rockContext.SaveChanges();

            // Create the rule in the database
            var auth1 = new Auth
            {
                EntityTypeId = entity.TypeId,
                EntityId = entity.Id,
                Order = 0,
                Action = action,
                AllowOrDeny = "A",
                SpecialRole = SpecialRole.AllUsers
            };
            authService.Add( auth1 );

            rockContext.SaveChanges();

            // Reload the static dictionary for this action
            RefreshAction( entity.TypeId, entity.Id, action, rockContext );
        }

        /// <summary>
        /// Makes the entity private for the selected action and person
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void MyMakePrivate( ISecured entity, string action, Person person, RockContext rockContext )
        {
            if ( IsPrivate( entity, action, person ) )
            {
                return;
            }

            if ( person == null )
            {
                return;
            }

            var personAlias = new PersonAliasService( rockContext ).GetPrimaryAlias( person.Id );
            if ( personAlias == null )
            {
                return;
            }

            var authService = new AuthService( rockContext );

            // Delete any existing rules in database
            foreach ( var auth in authService
                .GetAuths( entity.TypeId, entity.Id, action ) )
            {
                authService.Delete( auth );
            }

            rockContext.SaveChanges();

            // Create the rules in the database
            var auth1 = new Auth
            {
                EntityTypeId = entity.TypeId,
                EntityId = entity.Id,
                Order = 0,
                Action = action,
                AllowOrDeny = "A",
                SpecialRole = SpecialRole.None,
                PersonAlias = personAlias,
                PersonAliasId = personAlias.Id
            };
            authService.Add( auth1 );

            var auth2 = new Auth
            {
                EntityTypeId = entity.TypeId,
                EntityId = entity.Id,
                Order = 1,
                Action = action,
                AllowOrDeny = "D",
                SpecialRole = SpecialRole.AllUsers
            };
            authService.Add( auth2 );

            rockContext.SaveChanges();

            // Reload the static dictionary for this action
            RefreshAction( entity.TypeId, entity.Id, action, rockContext );
        }

        /// <summary>
        /// If the entity is currently private for selected person, removes all the rules
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void MyMakeUnPrivate( ISecured entity, string action, Person person, RockContext rockContext )
        {
            if ( !IsPrivate( entity, action, person ) )
            {
                return;
            }

            var authService = new AuthService( rockContext );

            // Delete any existing rules in database
            foreach ( var auth in authService
                .GetAuths( entity.TypeId, entity.Id, action ) )
            {
                authService.Delete( auth );
            }

            // Reload the static dictionary for this action
            RefreshAction( entity.TypeId, entity.Id, action, rockContext );
        }

        /// <summary>
        /// Creates authorization rules to make the entity private to selected person
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="group">The group.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void MyAllow( ISecured entity, string action,
            Person person = null, Group group = null, SpecialRole specialRole = SpecialRole.None,
            RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            PersonAlias personAlias = null;
            if ( person != null )
            {
                personAlias = new PersonAliasService( rockContext ).GetPrimaryAlias( person.Id );
            }

            if ( personAlias == null && group == null && specialRole == SpecialRole.None )
            {
                return;
            }

            var authService = new AuthService( rockContext );

            // Update the order for any existing rules in database
            var order = 1;
            foreach ( var existingAuth in authService
                .GetAuths( entity.TypeId, entity.Id, action ) )
            {
                existingAuth.Order = order++;
            }

            // Add the new auth (with order of zero)
            var auth = new Auth
            {
                EntityTypeId = entity.TypeId,
                EntityId = entity.Id,
                Order = 0,
                Action = action,
                AllowOrDeny = "A",
                SpecialRole = specialRole
            };
            if ( personAlias != null )
            {
                auth.PersonAlias = personAlias;
                auth.PersonAliasId = personAlias.Id;
            }

            if ( group != null )
            {
                auth.Group = group;
                auth.GroupId = group.Id;
            }

            authService.Add( auth );

            rockContext.SaveChanges();

            // Reload the static dictionary for this action
            RefreshAction( entity.TypeId, entity.Id, action, rockContext );
        }

        /// <summary>
        /// Resets the action.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="action">The action.</param>
        /// <param name="authRules">The authentication rules.</param>
        private static void ResetAction( int entityTypeId, int entityId, string action, IEnumerable<AuthRule> authRules )
        {
            var authorizations = Get();
            if ( authorizations != null )
            {
                authorizations.TryAdd( entityTypeId, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
                var entityAuths = authorizations[entityTypeId];

                entityAuths.TryAdd( entityId, new Dictionary<string, List<AuthRule>>( StringComparer.OrdinalIgnoreCase ) );
                var instanceAuths = entityAuths[entityId];

                instanceAuths.AddOrReplace( action, new List<AuthRule>() );
                var actionPermissions = instanceAuths[action];

                // Update authorization dictionary with the new rules
                actionPermissions.AddRange( authRules );
            }

            AddOrUpdate( authorizations );
        }

        #endregion

        #region Private Helper Classes        

        /// <summary>
        /// User data in an authentication ticket.
        /// </summary>
        private class AuthenticationTicketUserData : IAuthenticationTicketUserData
        {
            /// <inheritdoc />
            public bool IsImpersonated { get; }
            
            /// <inheritdoc />
            public bool IsTwoFactorAuthenticated { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="AuthenticationTicketUserData"/> class.
            /// </summary>
            /// <param name="isImpersonated">if set to <c>true</c> then authenticated user is impersonated.</param>
            /// <param name="isTwoFactorAuthenticated">if set to <c>true</c> then authenticated user is two-factor authenticated.</param>
            public AuthenticationTicketUserData( bool isImpersonated, bool isTwoFactorAuthenticated )
            {
                this.IsImpersonated = isImpersonated;
                this.IsTwoFactorAuthenticated = isTwoFactorAuthenticated;
            }
        }

        #endregion
    }

    #region Helper Class/Struct

    /// <summary>
    ///
    /// </summary>
    public class AuthEntityRule
    {
        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the authentication rule.
        /// </summary>
        /// <value>
        /// The authentication rule.
        /// </value>
        public AuthRule AuthRule { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthEntityRule"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="action">The action.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="allowOrDeny">The allow or deny.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="order">The order.</param>
        public AuthEntityRule( int id, int entityTypeId, string action, int? entityId, string allowOrDeny, SpecialRole specialRole, int? personId, int? personAliasId, int? groupId, int order )
        {
            EntityTypeId = entityTypeId;
            Action = action;
            AuthRule = new AuthRule( id, entityId, allowOrDeny, specialRole, personId, personAliasId, groupId, order );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthEntityRule"/> class.
        /// </summary>
        /// <param name="auth">The authentication.</param>
        public AuthEntityRule( Auth auth )
        {
            EntityTypeId = auth.EntityTypeId;
            Action = auth.Action;
            AuthRule = new AuthRule( auth );
        }
    }

    /// <summary>
    /// Lightweight struct to store if a particular user or role is allowed or denied access
    /// </summary>
    [Serializable]
    [DataContract]
    public struct AuthRule
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this is an allow or deny rule.  Acceptable values are "A" or "D".
        /// </summary>
        /// <value>
        /// The allow or deny.
        /// </value>
        [DataMember]
        public char AllowOrDeny { get; set; }

        /// <summary>
        /// Gets or sets the special role.
        /// </summary>
        /// <value>
        /// The special role.
        /// </value>
        [DataMember]
        public SpecialRole SpecialRole { get; set; }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        /// <value>
        /// The group id.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName
        {
            get
            {
                // All Users
                switch ( SpecialRole )
                {
                    case SpecialRole.AllUsers:
                        return "All Users";

                    case SpecialRole.AllAuthenticatedUsers:
                        return "All Authenticated Users";

                    case SpecialRole.AllUnAuthenticatedUsers:
                        return "All Un-Authenticated Users";

                    default:

                        if ( PersonId.HasValue )
                        {
                            try
                            {
                                var personService = new PersonService( new RockContext() );
                                var person = personService.Get( PersonId.Value );
                                if ( person != null )
                                {
                                    return person.FullName + " <small>(User)</small>";
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        if ( !GroupId.HasValue )
                        {
                            return "*** Unknown User/Role ***";
                        }

                        try
                        {
                            var role = RoleCache.Get( GroupId.Value );
                            if ( role != null )
                            {
                                return ( role.IsSecurityTypeGroup ? "" : "GROUP - " ) +
                                       role.Name + " <small>(Role)</small>";
                            }

                            var groupService = new GroupService( new RockContext() );
                            var group = groupService.Get( GroupId.Value );
                            if ( group != null )
                            {
                                return $"<span class='text-muted'>GROUP - {group.Name} <small>(No longer a valid active role)</small></span>";
                            }
                        }
                        catch
                        {
                            // ignored
                        }

                        return "*** Unknown User/Role ***";
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthRule" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="allowOrDeny">Allow or Deny ("A" or "D").</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="personAliasId">The person alias id.</param>
        /// <param name="groupId">The group id.</param>
        /// <param name="order">The order.</param>
        public AuthRule( int id, int? entityId, string allowOrDeny, SpecialRole specialRole, int? personId, int? personAliasId, int? groupId, int order ) : this()
        {
            Id = id;
            EntityId = entityId;
            AllowOrDeny = allowOrDeny == "A" ? 'A' : 'D';
            SpecialRole = specialRole;
            PersonId = personId;
            PersonAliasId = personAliasId;
            GroupId = groupId;
            Order = order;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthRule"/> class.
        /// </summary>
        /// <param name="auth">The auth.</param>
        public AuthRule( Auth auth ) : this()
        {
            Id = auth.Id;
            EntityId = auth.EntityId;
            AllowOrDeny = auth.AllowOrDeny == "A" ? 'A' : 'D';
            SpecialRole = auth.SpecialRole;
            PersonId = auth.PersonAlias?.PersonId;
            PersonAliasId = auth.PersonAliasId;
            GroupId = auth.GroupId;
            Order = auth.Order;
        }
    }

    /// <summary>
    /// User data in an authentication ticket.
    /// </summary>
    internal interface IAuthenticationTicketUserData
    {
        /// <summary>
        /// Gets a value indicating whether the authenticated individual is impersonated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the authenticated individual is impersonated; otherwise, <c>false</c>.
        /// </value>
        bool IsImpersonated { get; }

        /// <summary>
        /// Gets a value indicating whether the authenticated individual is two-factor authenticated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the authenticated individual is two-factor authenticated; otherwise, <c>false</c>.
        /// </value>
        bool IsTwoFactorAuthenticated { get; }
    }

    #endregion
}