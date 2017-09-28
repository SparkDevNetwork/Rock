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
using System.Text;
using System.Web;
using System.Web.Security;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// Static class for managing authorizations
    /// </summary>
    public static class Authorization
    {

        #region Constants

        /// <summary>
        /// Authorization to view object
        /// </summary>
        public const string VIEW = "View";

        /// <summary>
        /// Authorization to edit object ( add, set properties, delete, etc )
        /// </summary>
        public const string EDIT = "Edit";

        /// <summary>
        /// Authorization to delete object (only used in few places where delete needs to be securred differently that EDIT, i.e. Financial Batch )
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

        #endregion

        #region Private Properties

        // locking object
        private static object _lock = new object();

        // Static dictionary of all authorization records
        private static Dictionary<int, Dictionary<int, Dictionary<string, List<AuthRule>>>> _authorizations;

        #endregion

        #region Public Methods

        /// <summary>
        /// Load the static Authorizations object
        /// </summary>
        public static bool Load()
        {
            bool justLoaded = false;

            // Check to see if authorizations have already been loaded
            bool alreadyLoaded = false;
            lock ( _lock )
            {
                alreadyLoaded = _authorizations != null;
            }

            // If not loaded...
            if ( !alreadyLoaded )
            {
                List<AuthEntityRule> authEntityRules = new List<AuthEntityRule>();

                var securityGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
                int securityGroupTypeId = securityGroupType != null ? securityGroupType.Id : 0;

                // query the database for all of the entity auth rules
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

                // Then load them into static dictionary
                lock ( _lock )
                {
                    // Make sure authorizations still haven't been loaded (by another thread while this thread was querying database)
                    if ( _authorizations == null )
                    {
                        // Load the authorizations
                        _authorizations = new Dictionary<int, Dictionary<int, Dictionary<string, List<AuthRule>>>>();

                        foreach ( AuthEntityRule authEntityRule in authEntityRules )
                        {
                            _authorizations.AddOrIgnore( authEntityRule.EntityTypeId, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
                            var entityAuths = _authorizations[authEntityRule.EntityTypeId];

                            entityAuths.AddOrIgnore( authEntityRule.AuthRule.EntityId ?? 0, new Dictionary<string, List<AuthRule>>() );
                            var instanceAuths = entityAuths[authEntityRule.AuthRule.EntityId ?? 0];

                            instanceAuths.AddOrIgnore( authEntityRule.Action, new List<AuthRule>() );
                            var actionPermissions = instanceAuths[authEntityRule.Action];

                            actionPermissions.Add( authEntityRule.AuthRule );
                        }

                        justLoaded = true;
                    }
                }
            }

            return justLoaded;
        }

        /// <summary>
        /// Reloads the entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void ReloadEntity( int entityTypeId, int entityId, RockContext rockContext = null )
        {
            var rockMemoryCache = RockMemoryCache.Default;
            if ( rockMemoryCache.IsRedisClusterEnabled && rockMemoryCache.IsRedisConnected )
            {
                rockMemoryCache.SendRedisCommand( string.Format( "REFRESH_AUTH_ENTITY,{0},{1}", entityTypeId, entityId ) );
            }
            else
            {
                RefreshEntity( entityTypeId, entityId, rockContext );
            }
        }

        /// <summary>
        /// Reloads the entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        internal static void RefreshEntity( int entityTypeId, int entityId, RockContext rockContext = null )
        {
            if ( !Load() )
            {
                lock ( _lock )
                {
                    if ( _authorizations != null && _authorizations.ContainsKey( entityTypeId ) )
                    {
                        var entityAuths = _authorizations[entityTypeId];
                        if ( entityAuths.ContainsKey( entityId ) )
                        {
                            entityAuths[entityId] = new Dictionary<string, List<AuthRule>>();
                        }
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

                var actions = auths.Select( a => a.Action ).Distinct().ToList();
                foreach ( string action in actions )
                {
                    var newAuthRules = new List<AuthRule>();

                    foreach ( Auth auth in auths.Where( a => a.Action == action ) )
                    {
                        newAuthRules.Add( new AuthRule( auth ) );
                    }
                    ResetAction( entityTypeId, entityId, action, newAuthRules );
                }

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
            var securityGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid(), rockContext );
            int securityGroupTypeId = securityGroupType != null ? securityGroupType.Id : 0;

            return new AuthService( rockContext )
                .Get( entityTypeId, entityId )
                .AsNoTracking()
                .Where( t =>
                    t.Group == null ||
                    ( t.Group.IsActive && ( t.Group.IsSecurityRole || t.Group.GroupTypeId == securityGroupTypeId ) )
                )
                .OrderBy( a => a.Order )
                .ToList();
        }

        /// <summary>
        /// Reloads the action.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="action">The action.</param>
        public static void ReloadAction( int entityTypeId, int entityId, string action )
        {
            var rockMemoryCache = RockMemoryCache.Default;
            if ( rockMemoryCache.IsRedisClusterEnabled && rockMemoryCache.IsRedisConnected )
            {
                rockMemoryCache.SendRedisCommand( string.Format( "REFRESH_AUTH_ACTION,{0},{1},{2}", entityTypeId, entityId, action ) );
            }
            else
            {
                RefreshAction( entityTypeId, entityId, action );
            }
        }

        /// <summary>
        /// Reloads the authorizations for the specified entity and action.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="action">The action.</param>
        internal static void RefreshAction( int entityTypeId, int entityId, string action )
        {
            // if the authorizations have already been loaded, update just the selected action
            if ( !Load() )
            {
                var newAuthRules = new List<AuthRule>();

                // Query database for the authorizations related to this entitytype, entity, and action
                using ( var rockContext = new RockContext() )
                {
                    var securityGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid(), rockContext );
                    int securityGroupTypeId = securityGroupType != null ? securityGroupType.Id : 0;

                    foreach ( Auth auth in new AuthService( rockContext )
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
        }

        /// <summary>
        /// Reloads the action.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="action">The action.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void ReloadAction( int entityTypeId, int entityId, string action, RockContext rockContext )
        {
            var rockMemoryCache = RockMemoryCache.Default;
            if ( rockMemoryCache.IsRedisClusterEnabled && rockMemoryCache.IsRedisConnected )
            {
                rockMemoryCache.SendRedisCommand( string.Format( "REFRESH_AUTH_ACTION,{0},{1},{2}", entityTypeId, entityId, action ) );
            }
            else
            {
                RefreshAction( entityTypeId, entityId, action, rockContext );
            }
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
            // if the authorizations have already been loaded, update just the selected action
            if ( !Load() )
            {
                var newAuthRules = new List<AuthRule>();

                var securityGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid(), rockContext );
                int securityGroupTypeId = securityGroupType != null ? securityGroupType.Id : 0;

                // Query database for the authorizations related to this entitytype, entity, and action
                foreach ( Auth auth in new AuthService( rockContext )
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
        public static bool Authorized( ISecured entity, string action, Rock.Model.Person person )
        {
            return ItemAuthorized( entity, action, person, true, true ) ?? entity.IsAllowedByDefault( action );
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
            bool isPrivate = false;

            if ( person != null )
            {
                Load();

                lock ( _lock )
                {
                    // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each 
                    // one to find the first one specific to the selected user or a role that the selected user belongs 
                    // to.  If a match is found return whether the user is allowed (true) or denied (false) access
                    if ( _authorizations != null &&
                        _authorizations.Keys.Contains( entity.TypeId ) &&
                        _authorizations[entity.TypeId].Keys.Contains( entity.Id ) &&
                        _authorizations[entity.TypeId][entity.Id].Keys.Contains( action ) &&
                        _authorizations[entity.TypeId][entity.Id][action].Count == 2 )
                    {
                        AuthRule firstRule = _authorizations[entity.TypeId][entity.Id][action][0];
                        AuthRule secondRule = _authorizations[entity.TypeId][entity.Id][action][1];

                        // If first rule allows current user, and second rule denies all other users then entity is private
                        if ( firstRule.AllowOrDeny == 'A' &&
                            firstRule.SpecialRole == SpecialRole.None &&
                            firstRule.PersonId == person.Id &&
                            secondRule.AllowOrDeny == 'D' &&
                            secondRule.SpecialRole == SpecialRole.AllUsers )
                        {
                            isPrivate = true;
                        }
                    }
                }
            }

            return isPrivate;
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
            List<AuthRule> rules = new List<AuthRule>();

            Load();

            lock ( _lock )
            {
                // Find the Authrules for the given entity type, entity id, and action
                if ( _authorizations != null && _authorizations.ContainsKey( entityTypeId ) )
                {
                    if ( _authorizations[entityTypeId].ContainsKey( entityId ) )
                    {
                        if ( _authorizations[entityTypeId][entityId].ContainsKey( action ) )
                        {
                            rules = _authorizations[entityTypeId][entityId][action];
                        }
                    }
                }
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
        /// <param name="action">Optional action (if ommitted or left blank, all actions will be copied).</param>
        /// <remarks>
        /// This method will save any previous changes made to the context
        /// </remarks>
        public static void CopyAuthorization( ISecured sourceEntity, ISecured targetEntity, RockContext rockContext, string action = "" )
        {
            Load();

            var sourceEntityTypeId = sourceEntity.TypeId;
            var targetEntityTypeId = targetEntity.TypeId;

            AuthService authService = new AuthService( rockContext );

            // Delete the current authorizations for the target entity
            foreach ( Auth auth in authService.Get( targetEntityTypeId, targetEntity.Id ).ToList() )
            {
                if ( string.IsNullOrWhiteSpace( action ) || auth.Action.Equals( action, StringComparison.OrdinalIgnoreCase ) )
                {
                    authService.Delete( auth );
                }
            }
            rockContext.SaveChanges();

            // Copy target auths to source auths
            int order = 0;
            foreach ( Auth sourceAuth in authService.Get( sourceEntityTypeId, sourceEntity.Id ).ToList() )
            {
                if ( ( string.IsNullOrWhiteSpace( action ) || sourceAuth.Action.Equals( action, StringComparison.OrdinalIgnoreCase ) ) &&
                    targetEntity.SupportedActions.ContainsKey( sourceAuth.Action ) )
                {
                    Auth auth = new Auth();
                    auth.EntityTypeId = targetEntityTypeId;
                    auth.EntityId = targetEntity.Id;
                    auth.Action = sourceAuth.Action;
                    auth.AllowOrDeny = sourceAuth.AllowOrDeny;
                    auth.GroupId = sourceAuth.GroupId;
                    auth.PersonAliasId = sourceAuth.PersonAliasId;
                    auth.SpecialRole = sourceAuth.SpecialRole;
                    auth.Order = order++;

                    authService.Add( auth );
                    rockContext.SaveChanges();
                }
            }

            ReloadEntity( targetEntityTypeId, targetEntity.Id, rockContext );

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
        public static void Flush()
        {
            var rockMemoryCache = RockMemoryCache.Default;
            if ( rockMemoryCache.IsRedisClusterEnabled && rockMemoryCache.IsRedisConnected )
            {
                rockMemoryCache.SendRedisCommand( "FLUSH_AUTH" );
            }
            else
            {
                FlushAuth();
            }
        }

        /// <summary>
        /// Clear the static Authorizations object
        /// </summary>
        internal static void FlushAuth()
        {
            lock ( _lock )
            {
                _authorizations = null;
            }
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
            byte[] b = Encoding.UTF8.GetBytes( assemblyQualifiedName );
            return Convert.ToBase64String( b );
        }

        /// <summary>
        /// Decodes the entity type name.
        /// </summary>
        /// <param name="encodedTypeName">Name of the encoded type.</param>
        /// <returns></returns>
        public static string DecodeEntityTypeName( string encodedTypeName )
        {
            byte[] b = Convert.FromBase64String( encodedTypeName );
            return Encoding.UTF8.GetString( b );
        }

        /// <summary>
        /// Sets the auth cookie.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="isPersisted">if set to <c>true</c> [is persisted].</param>
        /// <param name="IsImpersonated">if set to <c>true</c> [is impersonated].</param>
        public static void SetAuthCookie( string userName, bool isPersisted, bool IsImpersonated )
        {
            var ticket = new FormsAuthenticationTicket( 1, userName, RockDateTime.Now,
                RockDateTime.Now.Add( FormsAuthentication.Timeout ), isPersisted,
                IsImpersonated.ToString(), FormsAuthentication.FormsCookiePath );

            var authCookie = GetAuthCookie( GetCookieDomain(), FormsAuthentication.Encrypt( ticket ) );
            if ( ticket.IsPersistent )
            {
                authCookie.Expires = ticket.Expiration;
            }
            HttpContext.Current.Response.Cookies.Add( authCookie );

            // If cookie is for a more generic domain, we need to store that domain so that we can expire it correctly 
            // when the user signs out.
            if ( authCookie.Domain.IsNotNullOrWhitespace() )
            {
                var domainCookie = new HttpCookie( $"{FormsAuthentication.FormsCookieName}_DOMAIN", authCookie.Domain );
                domainCookie.HttpOnly = true;
                domainCookie.Domain = authCookie.Domain;
                domainCookie.Path = FormsAuthentication.FormsCookiePath;
                domainCookie.Secure = FormsAuthentication.RequireSSL;
                domainCookie.Expires = authCookie.Expires;
                HttpContext.Current.Response.Cookies.Add( domainCookie );
            }

        }

        /// <summary>
        /// Signs a user out of rock by deleting the appropriate forms authentication cookies
        /// </summary>
        public static void SignOut()
        {
            var domainCookie = HttpContext.Current.Request.Cookies[$"{FormsAuthentication.FormsCookieName}_DOMAIN"];
            if ( domainCookie != null )
            { 
                var authCookie = GetAuthCookie( domainCookie.Value, null );
                authCookie.Expires = DateTime.Now.AddDays( -1d );
                HttpContext.Current.Response.Cookies.Remove( FormsAuthentication.FormsCookieName );
                HttpContext.Current.Response.Cookies.Add( authCookie );

                domainCookie = new HttpCookie( $"{FormsAuthentication.FormsCookieName}_DOMAIN" );
                domainCookie.HttpOnly = true;
                domainCookie.Domain = authCookie.Domain;
                domainCookie.Path = FormsAuthentication.FormsCookiePath;
                domainCookie.Secure = FormsAuthentication.RequireSSL;
                domainCookie.Expires = DateTime.Now.AddDays( -1d );
                HttpContext.Current.Response.Cookies.Remove( $"{FormsAuthentication.FormsCookieName}_DOMAIN" );
                HttpContext.Current.Response.Cookies.Add( domainCookie );
            }
            else
            {
                FormsAuthentication.SignOut();
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
            var httpCookie = new HttpCookie( FormsAuthentication.FormsCookieName, value );
            httpCookie.Domain = domain.IsNotNullOrWhitespace() ? domain : FormsAuthentication.CookieDomain;
            httpCookie.HttpOnly = true;
            httpCookie.Path = FormsAuthentication.FormsCookiePath;
            httpCookie.Secure = FormsAuthentication.RequireSSL;
            return httpCookie;
        }


        /// <summary>
        /// Gets the domain for the forms authentication cookie. This is based on whether the current host name has an entry in the 'Domains Sharing Logins' defined type.
        /// </summary>
        /// <returns></returns>
        private static string GetCookieDomain()
        {
            // Get the domains that should be saving cookies as domain level cookies instead of the default of subdomain level.
            var dt = DefinedTypeCache.Read( SystemGuid.DefinedType.DOMAINS_SHARING_LOGINS.AsGuid() );
            var domains = dt != null ? dt.DefinedValues.Select( v => v.Value ).ToList() : new List<string>();

            // Get the first domain in the list that the current request's host name ends with
            string domain  = domains.FirstOrDefault( d => System.Web.HttpContext.Current.Request.Url.Host.ToLower().EndsWith( d.ToLower() ) );
            if ( domain.IsNotNullOrWhitespace() )
            {
                // Make sure domain name is prefixed with a '.'
                domain = domain.StartsWith(".") ? domain : $".{domain}";

                // Make sure there now at least two '.' characters (this is required for browser to store cookie).
                return domain.Count( c => c == '.' ) >= 2 ? domain : string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Checks to see if a person is authorized for entity
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static bool? AuthorizedForEntity( ISecured entity, string action, Rock.Model.Person person )
        {
            return ItemAuthorized( entity, action, person, true, false );
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
            Load();

            var entityTypeId = entity.TypeId;

            bool matchFound = false;
            bool authorized = false;

            lock ( _lock )
            {
                // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each 
                // one to find the first one specific to the selected user or a role that the selected user belongs 
                // to.  If a match is found return whether the user is allowed (true) or denied (false) access
                if ( _authorizations != null &&
                    _authorizations.Keys.Contains( entityTypeId ) &&
                    _authorizations[entityTypeId].Keys.Contains( entity.Id ) &&
                    _authorizations[entityTypeId][entity.Id].Keys.Contains( action ) )
                {
                    foreach ( AuthRule authRule in _authorizations[entityTypeId][entity.Id][action] )
                    {
                        if ( authRule.SpecialRole == specialRole )
                        {
                            matchFound = true;
                            authorized = authRule.AllowOrDeny == 'A';
                            break;
                        }
                    }
                }
            }

            if ( matchFound )
            {
                return authorized;
            }

            // If no match was found for the selected user on the current entity instance, check to see if the instance
            // has a parent authority defined and if so evaluate that entities authorization rules.  If there is no
            // parent authority return the defualt authorization
            bool? parentAuthorized = null;

            if ( checkParentAuthority )
            {
                if ( isRootEntity && entity.ParentAuthorityPre != null )
                {
                    parentAuthorized = ItemAuthorized( entity.ParentAuthorityPre, action, specialRole, false, false );
                }

                if ( !parentAuthorized.HasValue && entity.ParentAuthority != null )
                {
                    parentAuthorized = ItemAuthorized( entity.ParentAuthority, action, specialRole, false, true );
                }
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
        /// <returns></returns>
        private static bool? ItemAuthorized( ISecured entity, string action, Rock.Model.Person person, bool isRootEntity, bool checkParentAuthority )
        {
            Load();

            var entityTypeId = entity.TypeId;

            // check for infinite recursion
            var parentHistory = new List<ISecured>();
            parentHistory.Add( entity );
            foreach ( var parentAuth in new ISecured[] { entity.ParentAuthority, entity.ParentAuthorityPre } )
            {
                var parentAuthEntity = parentAuth;
                while ( parentAuthEntity != null )
                {
                    // check if the exact same instance of an entity is already a parent (indicating we are spinning around recursively)
                    if ( parentHistory.Any( a => a.TypeId == parentAuthEntity.TypeId && a.Id == parentAuthEntity.Id && parentAuthEntity.Id > 0 ) )
                    {
                        // infinite recursion situation, so treat as if no rules were found and return NULL
                        return null;
                    }
                    else
                    {
                        parentHistory.Add( parentAuthEntity );
                    }

                    parentAuthEntity = parentAuthEntity.ParentAuthority;
                }
            }

            bool matchFound = false;
            bool authorized = false;

            lock ( _lock )
            {
                // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each 
                // one to find the first one specific to the selected user or a role that the selected user belongs 
                // to.  If a match is found return whether the user is allowed (true) or denied (false) access
                if ( _authorizations != null &&
                    _authorizations.Keys.Contains( entityTypeId ) &&
                    _authorizations[entityTypeId].Keys.Contains( entity.Id ) &&
                    _authorizations[entityTypeId][entity.Id].Keys.Contains( action ) )
                {
                    Guid? personGuid = person != null ? person.Guid : ( Guid? ) null;

                    foreach ( AuthRule authRule in _authorizations[entityTypeId][entity.Id][action] )
                    {
                        // All Users
                        if ( authRule.SpecialRole == SpecialRole.AllUsers )
                        {
                            matchFound = true;
                            authorized = authRule.AllowOrDeny == 'A';
                            break;
                        }

                        // All Authenticated Users
                        if ( !matchFound && authRule.SpecialRole == SpecialRole.AllAuthenticatedUsers && personGuid.HasValue )
                        {
                            matchFound = true;
                            authorized = authRule.AllowOrDeny == 'A';
                            break;
                        }

                        // All Unauthenticated Users
                        if ( !matchFound && authRule.SpecialRole == SpecialRole.AllUnAuthenticatedUsers && !personGuid.HasValue )
                        {
                            matchFound = true;
                            authorized = authRule.AllowOrDeny == 'A';
                            break;
                        }

                        if ( !matchFound && authRule.SpecialRole == SpecialRole.None && person != null )
                        {
                            // See if person has been authorized to entity
                            if ( authRule.PersonId.HasValue &&
                                authRule.PersonId.Value == person.Id )
                            {
                                matchFound = true;
                                authorized = authRule.AllowOrDeny == 'A';
                                break;
                            }

                            // See if person is in role authorized
                            if ( !matchFound && authRule.GroupId.HasValue )
                            {
                                Role role = Role.Read( authRule.GroupId.Value );
                                if ( role != null && role.IsPersonInRole( personGuid ) )
                                {
                                    matchFound = true;
                                    authorized = authRule.AllowOrDeny == 'A';
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if ( matchFound )
            {
                return authorized;
            }

            // If no match was found for the selected user on the current entity instance, check to see if the instance
            // has a parent authority defined and if so evaluate that entities authorization rules.  If there is no
            // parent authority return the defualt authorization
            bool? parentAuthorized = null;

            if ( checkParentAuthority )
            {
                if ( isRootEntity && entity.ParentAuthorityPre != null )
                {
                    parentAuthorized = ItemAuthorized( entity.ParentAuthorityPre, action, person, false, false );
                }

                if ( !parentAuthorized.HasValue && entity.ParentAuthority != null )
                {
                    parentAuthorized = ItemAuthorized( entity.ParentAuthority, action, person, false, true );
                }
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
            foreach ( Auth auth in authService
                .GetAuths( entity.TypeId, entity.Id, action ) )
            {
                authService.Delete( auth );
            }

            rockContext.SaveChanges();

            // Create the rule in the database
            Auth auth1 = new Auth();
            auth1.EntityTypeId = entity.TypeId;
            auth1.EntityId = entity.Id;
            auth1.Order = 0;
            auth1.Action = action;
            auth1.AllowOrDeny = "A";
            auth1.SpecialRole = SpecialRole.AllUsers;
            authService.Add( auth1 );

            rockContext.SaveChanges();

            // Reload the static dictionary for this action
            ReloadAction( entity.TypeId, entity.Id, action, rockContext );
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
            if ( !IsPrivate( entity, action, person ) )
            {
                if ( person != null )
                {
                    var personAlias = new PersonAliasService( rockContext ).GetPrimaryAlias( person.Id );
                    if ( personAlias != null )
                    {
                        var authService = new AuthService( rockContext );

                        // Delete any existing rules in database
                        foreach ( Auth auth in authService
                            .GetAuths( entity.TypeId, entity.Id, action ) )
                        {
                            authService.Delete( auth );
                        }

                        rockContext.SaveChanges();

                        // Create the rules in the database
                        Auth auth1 = new Auth();
                        auth1.EntityTypeId = entity.TypeId;
                        auth1.EntityId = entity.Id;
                        auth1.Order = 0;
                        auth1.Action = action;
                        auth1.AllowOrDeny = "A";
                        auth1.SpecialRole = SpecialRole.None;
                        auth1.PersonAlias = personAlias;
                        auth1.PersonAliasId = personAlias.Id;
                        authService.Add( auth1 );

                        Auth auth2 = new Auth();
                        auth2.EntityTypeId = entity.TypeId;
                        auth2.EntityId = entity.Id;
                        auth2.Order = 1;
                        auth2.Action = action;
                        auth2.AllowOrDeny = "D";
                        auth2.SpecialRole = SpecialRole.AllUsers;
                        authService.Add( auth2 );

                        rockContext.SaveChanges();

                        // Reload the static dictionary for this action
                        ReloadAction( entity.TypeId, entity.Id, action, rockContext );
                    }
                }
            }
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
            if ( IsPrivate( entity, action, person ) )
            {
                var authService = new AuthService( rockContext );

                // Delete any existing rules in database
                foreach ( Auth auth in authService
                    .GetAuths( entity.TypeId, entity.Id, action ) )
                {
                    authService.Delete( auth );
                }

                // Reload the static dictionary for this action
                ReloadAction( entity.TypeId, entity.Id, action, rockContext );
            }
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

            if ( personAlias != null || group != null || specialRole != SpecialRole.None )
            {
                var authService = new AuthService( rockContext );

                // Update the order for any existing rules in database
                int order = 1;
                foreach ( Auth existingAuth in authService
                    .GetAuths( entity.TypeId, entity.Id, action ) )
                {
                    existingAuth.Order = order++;
                }

                // Add the new auth (with order of zero)
                Auth auth = new Auth();
                auth.EntityTypeId = entity.TypeId;
                auth.EntityId = entity.Id;
                auth.Order = 0;
                auth.Action = action;
                auth.AllowOrDeny = "A";
                auth.SpecialRole = specialRole;
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
                ReloadAction( entity.TypeId, entity.Id, action, rockContext );
            }
        }

        /// <summary>
        /// Resets the action.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="action">The action.</param>
        /// <param name="authRules">The authentication rules.</param>
        private static void ResetAction( int entityTypeId, int entityId, string action, List<AuthRule> authRules )
        {
            lock ( _lock )
            {
                if ( _authorizations != null )
                {
                    _authorizations.AddOrIgnore( entityTypeId, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
                    var entityAuths = _authorizations[entityTypeId];

                    entityAuths.AddOrIgnore( entityId, new Dictionary<string, List<AuthRule>>() );
                    var instanceAuths = entityAuths[entityId];

                    instanceAuths.AddOrReplace( action, new List<AuthRule>() );
                    var actionPermissions = instanceAuths[action];

                    // Update authorization dictionary with the new rules
                    foreach ( AuthRule authRule in authRules )
                    {
                        actionPermissions.Add( authRule );
                    }
                }
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
    public struct AuthRule
    {

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this is an allow or deny rule.  Acceptable values are "A" or "D".
        /// </summary>
        /// <value>
        /// The allow or deny.
        /// </value>
        public char AllowOrDeny { get; set; }

        /// <summary>
        /// Gets or sets the special role.
        /// </summary>
        /// <value>
        /// The special role.
        /// </value>
        public SpecialRole SpecialRole { get; set; }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        /// <value>
        /// The group id.
        /// </value>
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
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
                    case Model.SpecialRole.AllUsers:
                        return "All Users";

                    case Model.SpecialRole.AllAuthenticatedUsers:
                        return "All Authenticated Users";

                    case Model.SpecialRole.AllUnAuthenticatedUsers:
                        return "All Un-Authenticated Users";

                    default:

                        if ( PersonId.HasValue )
                        {
                            try
                            {
                                PersonService personService = new PersonService( new RockContext() );
                                Person person = personService.Get( PersonId.Value );
                                if ( person != null )
                                {
                                    return person.FullName + " <small>(User)</small>";
                                }
                            }
                            catch { }
                        }

                        if ( GroupId.HasValue )
                        {
                            try
                            {
                                var role = Role.Read( GroupId.Value );
                                if ( role != null )
                                {
                                    return ( role.IsSecurityTypeGroup ? "" : "GROUP - " ) +
                                        role.Name + " <small>(Role)</small>";
                                }
                                else
                                {
                                    GroupService groupService = new GroupService( new RockContext() );
                                    Group group = groupService.Get( GroupId.Value );
                                    if ( group != null )
                                    {
                                        return string.Format( "<span class='text-muted'>GROUP - {0} <small>(No longer a valid active role)</small></span>",
                                            group.Name );
                                        ;
                                    }
                                }
                            }
                            catch { }
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
            PersonId = auth.PersonAlias != null ? auth.PersonAlias.PersonId : ( int? ) null;
            PersonAliasId = auth.PersonAliasId;
            GroupId = auth.GroupId;
            Order = auth.Order;
        }
    }

    #endregion

}