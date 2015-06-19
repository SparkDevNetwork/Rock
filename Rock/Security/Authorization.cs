// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Text;
using Rock.Data;
using Rock.Model;

namespace Rock.Security
{
    /// <summary>
    /// Static class for managing authorizations
    /// </summary>
    public static class Authorization
    {
        /// <summary>
        /// Authorization to view object
        /// </summary>
        public const string VIEW = "View";

        /// <summary>
        /// Authorization to edit object ( add, set properties, delete, etc )
        /// </summary>
        public const string EDIT = "Edit";

        /// <summary>
        /// Authorization to administer object ( add child object, set security, etc)
        /// </summary>
        public const string ADMINISTRATE = "Administrate";

        /// <summary>
        /// Authorization to approve object (html, prayer, ads, etc)
        /// </summary>
        public const string APPROVE = "Approve";

        /// <summary>
        /// Sets the auth cookie.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="isPersisted">if set to <c>true</c> [is persisted].</param>
        /// <param name="IsImpersonated">if set to <c>true</c> [is impersonated].</param>
        public static void SetAuthCookie( string userName, bool isPersisted, bool IsImpersonated )
        {
            var ticket = new System.Web.Security.FormsAuthenticationTicket( 1, userName, RockDateTime.Now,
                RockDateTime.Now.Add( System.Web.Security.FormsAuthentication.Timeout ), isPersisted,
                IsImpersonated.ToString(), System.Web.Security.FormsAuthentication.FormsCookiePath );

            var encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt( ticket );

            var httpCookie = new System.Web.HttpCookie( System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket );
            httpCookie.HttpOnly = true;
            httpCookie.Path = System.Web.Security.FormsAuthentication.FormsCookiePath;
            httpCookie.Secure = System.Web.Security.FormsAuthentication.RequireSSL;
            if ( System.Web.Security.FormsAuthentication.CookieDomain != null )
                httpCookie.Domain = System.Web.Security.FormsAuthentication.CookieDomain;
            if ( ticket.IsPersistent )
                httpCookie.Expires = ticket.Expiration;

            System.Web.HttpContext.Current.Response.Cookies.Add( httpCookie );
        }


        /// <summary>
        /// Authorizations is a static variable for storing all authorizations.  It uses multiple Dictionary objects similiar 
        /// to a multi-dimensional array to store information.  The first dimension is the entity type id, second is the entity
        /// ID, third is the action, and the fourth dimension is a list of AuthRules for the action.
        /// </summary>
        public static Dictionary<int, Dictionary<int, Dictionary<string, List<AuthRule>>>> Authorizations { get; set; }

        /// <summary>
        /// Load the static Authorizations object
        /// </summary>
        public static void Load()
        {
            Authorizations = new Dictionary<int, Dictionary<int, Dictionary<string, List<AuthRule>>>>();

            using ( var rockContext = new RockContext() )
            {
                AuthService authService = new AuthService( rockContext );

                foreach ( Auth auth in authService.Queryable( "PersonAlias.Person" ).
                    OrderBy( A => A.EntityTypeId ).ThenBy( A => A.EntityId ).ThenBy( A => A.Action ).ThenBy( A => A.Order ) )
                {
                    if ( !Authorizations.ContainsKey( auth.EntityTypeId ) )
                        Authorizations.Add( auth.EntityTypeId, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
                    Dictionary<int, Dictionary<string, List<AuthRule>>> entityAuths = Authorizations[auth.EntityTypeId];

                    if ( !entityAuths.ContainsKey( auth.EntityId ?? 0 ) )
                        entityAuths.Add( auth.EntityId ?? 0, new Dictionary<string, List<AuthRule>>() );
                    Dictionary<string, List<AuthRule>> instanceAuths = entityAuths[auth.EntityId ?? 0];

                    if ( !instanceAuths.ContainsKey( auth.Action ) )
                        instanceAuths.Add( auth.Action, new List<AuthRule>() );
                    List<AuthRule> actionPermissions = instanceAuths[auth.Action];

                    actionPermissions.Add( new AuthRule( auth ) );
                }
            }
        }

        /// <summary>
        /// Load the static Authorizations object
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        [Obsolete("Use the Load() method instead (without RockContext parameter).")]
        public static void Load( RockContext rockContext)
        {
            Load();
        }

        /// <summary>
        /// Reloads the authorizations for the specified entity and action.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="action">The action.</param>
        public static void ReloadAction( int entityTypeId, int entityId, string action )
        {
            // If there's no Authorizations object, create it
            if ( Authorizations == null )
            {
                Load();
            }
            else
            {
                // Delete the current authorizations
                if ( Authorizations.ContainsKey( entityTypeId ) )
                    if ( Authorizations[entityTypeId].ContainsKey( entityId ) )
                        if ( Authorizations[entityTypeId][entityId].ContainsKey( action ) )
                            Authorizations[entityTypeId][entityId][action] = new List<AuthRule>();

                // Find the Authrules for the given entity type, entity id, and action
                using ( var rockContext = new RockContext() )
                {
                    AuthService authService = new AuthService( rockContext );
                    foreach ( Auth auth in authService.GetAuths( entityTypeId, entityId, action ) )
                    {
                        if ( !Authorizations.ContainsKey( auth.EntityTypeId ) )
                            Authorizations.Add( auth.EntityTypeId, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
                        Dictionary<int, Dictionary<string, List<AuthRule>>> entityAuths = Authorizations[auth.EntityTypeId];

                        if ( !entityAuths.ContainsKey( auth.EntityId ?? 0 ) )
                            entityAuths.Add( auth.EntityId ?? 0, new Dictionary<string, List<AuthRule>>() );
                        Dictionary<string, List<AuthRule>> instanceAuths = entityAuths[auth.EntityId ?? 0];

                        if ( !instanceAuths.ContainsKey( auth.Action ) )
                            instanceAuths.Add( auth.Action, new List<AuthRule>() );
                        List<AuthRule> actionPermissions = instanceAuths[auth.Action];

                        actionPermissions.Add( new AuthRule( auth ) );
                    }
                }
            }
        }

        /// <summary>
        /// Clear the static Authorizations object
        /// </summary>
        public static void Flush()
        {
            Authorizations = null;
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
            return ItemAuthorized( entity, action, specialRole ) ?? entity.IsAllowedByDefault( action );
        }

        private static bool? ItemAuthorized( ISecured entity, string action, SpecialRole specialRole )
        {
            // If there's no Authorizations object, create it
            if ( Authorizations == null )
            {
                Load();
            }

            var entityTypeId = entity.TypeId;

            // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each 
            // one to find the first one specific to the selected user or a role that the selected user belongs 
            // to.  If a match is found return whether the user is allowed (true) or denied (false) access
            if ( Authorizations.Keys.Contains( entityTypeId ) &&
                Authorizations[entityTypeId].Keys.Contains( entity.Id ) &&
                Authorizations[entityTypeId][entity.Id].Keys.Contains( action ) )
            {
                foreach ( AuthRule authRule in Authorizations[entityTypeId][entity.Id][action] )
                {
                    if ( authRule.SpecialRole == specialRole )
                    {
                        return authRule.AllowOrDeny == "A";
                    }
                }
            }

            // If no match was found for the selected user on the current entity instance, check to see if the instance
            // has a parent authority defined and if so evaluate that entities authorization rules.  If there is no
            // parent authority return the defualt authorization
            bool? parentAuthorized = null;

            if ( entity.ParentAuthorityPre != null )
            {
                parentAuthorized = ItemAuthorized( entity.ParentAuthorityPre, action, specialRole );
            }

            if ( !parentAuthorized.HasValue && entity.ParentAuthority != null )
            {
                parentAuthorized = ItemAuthorized( entity.ParentAuthority, action, specialRole );
            }

            return parentAuthorized;
        }

        /// <summary>
        /// Evaluates whether a selected person is allowed to perform the selected action on the selected
        /// entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [Obsolete( "Use Authorized( ISecured, action, Person) instead. RockContext parameter is no longer needed." )]
        public static bool Authorized( ISecured entity, string action, Rock.Model.Person person, RockContext rockContext )
        {
            return Authorized( entity, action, person );
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
            return ItemAuthorized( entity, action, person, null ) ?? entity.IsAllowedByDefault( action );
        }

        private static bool? ItemAuthorized( ISecured entity, string action, Rock.Model.Person person, List<ISecured> parentHistory )
        {
            // If there's no Authorizations object, create it
            if ( Authorizations == null )
            {
                Load();
            }

            var entityTypeId = entity.TypeId;

            parentHistory = parentHistory ?? new List<ISecured>();
            if ( parentHistory.Contains( entity ) )
            {
                // infinite recursion situation, so threat as if no rules were found and return NULL
                return null;
            }
            else
            {
                parentHistory.Add( entity );
            }

            // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each 
                // one to find the first one specific to the selected user or a role that the selected user belongs 
                // to.  If a match is found return whether the user is allowed (true) or denied (false) access
            if ( Authorizations.Keys.Contains( entityTypeId ) &&
                Authorizations[entityTypeId].Keys.Contains( entity.Id ) &&
                Authorizations[entityTypeId][entity.Id].Keys.Contains( action ) )
            {
                string userName = person != null ? person.Guid.ToString() : string.Empty;

                foreach ( AuthRule authRule in Authorizations[entityTypeId][entity.Id][action] )
                {
                    // All Users
                    if ( authRule.SpecialRole == SpecialRole.AllUsers )
                    {
                        return authRule.AllowOrDeny == "A";
                    }

                    // All Authenticated Users
                    if ( authRule.SpecialRole == SpecialRole.AllAuthenticatedUsers && userName.Trim() != string.Empty )
                    {
                        return authRule.AllowOrDeny == "A";
                    }

                    // All Unauthenticated Users
                    if ( authRule.SpecialRole == SpecialRole.AllUnAuthenticatedUsers && userName.Trim() == string.Empty )
                    {
                        return authRule.AllowOrDeny == "A";
                    }

                    if ( authRule.SpecialRole == SpecialRole.None && person != null )
                    {
                        // See if person has been authorized to entity
                        if ( authRule.PersonId.HasValue &&
                            authRule.PersonId.Value == person.Id )
                        {
                            return authRule.AllowOrDeny == "A";
                        }

                        // See if person is in role authorized
                        if ( authRule.GroupId.HasValue )
                        {
                            Role role = Role.Read( authRule.GroupId.Value );
                            if ( role != null && role.IsUserInRole( userName ) )
                            {
                                return authRule.AllowOrDeny == "A";
                            }
                        }
                    }
                }
            }

            // If no match was found for the selected user on the current entity instance, check to see if the instance
            // has a parent authority defined and if so evaluate that entities authorization rules.  If there is no
            // parent authority return the defualt authorization
            bool? parentAuthorized = null;

            if ( entity.ParentAuthorityPre != null )
            {
                parentAuthorized = ItemAuthorized( entity.ParentAuthorityPre, action, person, parentHistory );
            }

            if ( !parentAuthorized.HasValue && entity.ParentAuthority != null )
            {
                parentAuthorized = ItemAuthorized( entity.ParentAuthority, action, person, parentHistory );
            }

            return parentAuthorized;
        }

        /// <summary>
        /// Determines whether the specified entity is private. Entity is considered private if only the current user
        /// has access.  In this scenario, the first rule would give current user access, and second rule would deny
        /// all users.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <c>true</c> if the specified entity is private; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("Use IsPrivate( ISecured, string, Person ) instead. RockContext is no longer needed.")]
        public static bool IsPrivate( ISecured entity, string action, Person person, RockContext rockContext )
        {
            return IsPrivate( entity, action, person );
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
            if ( person != null )
            {
                // If there's no Authorizations object, create it
                if ( Authorizations == null )
                {
                    Load();
                }

                // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each 
                // one to find the first one specific to the selected user or a role that the selected user belongs 
                // to.  If a match is found return whether the user is allowed (true) or denied (false) access
                if ( Authorizations.Keys.Contains( entity.TypeId ) &&
                    Authorizations[entity.TypeId].Keys.Contains( entity.Id ) &&
                    Authorizations[entity.TypeId][entity.Id].Keys.Contains( action ) &&
                    Authorizations[entity.TypeId][entity.Id][action].Count == 2 )
                {
                    AuthRule firstRule = Authorizations[entity.TypeId][entity.Id][action][0];
                    AuthRule secondRule = Authorizations[entity.TypeId][entity.Id][action][1];

                    // If first rule allows current user, and second rule denies all other users then entity is private
                    if ( firstRule.AllowOrDeny == "A" &&
                        firstRule.SpecialRole == SpecialRole.None &&
                        firstRule.PersonId == person.Id &&
                        secondRule.AllowOrDeny == "D" &&
                        secondRule.SpecialRole == SpecialRole.AllUsers )
                    {
                        return true;
                    }
                }
            }

            return false;
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

        private static void MyMakePrivate( ISecured entity, string action, Person person, RockContext rockContext )
        {
            if ( !IsPrivate( entity, action, person ) )
            {
                if ( person != null )
                {
                    var personAlias = new PersonAliasService( rockContext ).GetPrimaryAlias( person.Id );
                    if ( personAlias != null )
                    {

                        // If there's no Authorizations object, create it
                        if ( Authorizations == null )
                        {
                            Load();
                        }

                        var authService = new AuthService( rockContext );

                        // If there are not entries in the Authorizations object for this entity type and entity instance, create
                        // the dictionary entries
                        if ( !Authorizations.Keys.Contains( entity.TypeId ) )
                        {
                            Authorizations.Add( entity.TypeId, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
                        }

                        if ( !Authorizations[entity.TypeId].Keys.Contains( entity.Id ) )
                        {
                            Authorizations[entity.TypeId].Add( entity.Id, new Dictionary<string, List<AuthRule>>() );
                        }

                        if ( !Authorizations[entity.TypeId][entity.Id].Keys.Contains( action ) )
                        {
                            Authorizations[entity.TypeId][entity.Id].Add( action, new List<AuthRule>() );
                        }
                        else
                        {
                            // If existing rules exist, delete them.
                            foreach ( AuthRule authRule in Authorizations[entity.TypeId][entity.Id][action] )
                            {
                                var oldAuth = authService.Get( authRule.Id );
                                authService.Delete( oldAuth );
                            }
                        }

                        var rules = new List<AuthRule>();

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

                        rules.Add( new AuthRule( auth1 ) );
                        rules.Add( new AuthRule( auth2 ) );

                        Authorizations[entity.TypeId][entity.Id][action] = rules;
                    }
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
        
        private static void MyMakeUnPrivate( ISecured entity, string action, Person person, RockContext rockContext )
        {
            if ( IsPrivate( entity, action, person ) )
            {
                var authService = new AuthService( rockContext );

                // if is private, then there are only two rules for this action that should be deleted
                foreach ( AuthRule authRule in Authorizations[entity.TypeId][entity.Id][action] )
                {
                    var oldAuth = authService.Get( authRule.Id );
                    authService.Delete( oldAuth );
                }

                rockContext.SaveChanges();

                Authorizations[entity.TypeId][entity.Id][action] = new List<AuthRule>();
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
                MyAllowPerson( entity, action, person, rockContext );
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    MyAllowPerson( entity, action, person, myRockContext );
                }
            }
        }

        private static void MyAllowPerson( ISecured entity, string action, Person person, RockContext rockContext )
        {
            if ( person != null )
            {
                rockContext = rockContext ?? new RockContext();

                var personAlias = new PersonAliasService( rockContext ).GetPrimaryAlias( person.Id );
                if ( personAlias != null )
                {                
                    
                    // If there's no Authorizations object, create it
                    if ( Authorizations == null )
                    {
                        Load();
                    }

                    var authService = new AuthService( rockContext );

                    // If there are not entries in the Authorizations object for this entity type and entity instance, create
                    // the dictionary entries
                    if ( !Authorizations.Keys.Contains( entity.TypeId ) )
                    {
                        Authorizations.Add( entity.TypeId, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
                    }

                    if ( !Authorizations[entity.TypeId].Keys.Contains( entity.Id ) )
                    {
                        Authorizations[entity.TypeId].Add( entity.Id, new Dictionary<string, List<AuthRule>>() );
                    }

                    List<AuthRule> rules = null;
                    if ( Authorizations[entity.TypeId][entity.Id].Keys.Contains( action ) )
                    {
                        rules = Authorizations[entity.TypeId][entity.Id][action];
                    }
                    else
                    {
                        rules = new List<AuthRule>();
                        Authorizations[entity.TypeId][entity.Id].Add( action, rules );
                    }

                    int order = 0;

                    Auth auth = new Auth();
                    auth.EntityTypeId = entity.TypeId;
                    auth.EntityId = entity.Id;
                    auth.Order = order++;
                    auth.Action = action;
                    auth.AllowOrDeny = "A";
                    auth.SpecialRole = SpecialRole.None;
                    auth.PersonAlias = personAlias;
                    auth.PersonAliasId = personAlias.Id;
                    authService.Add( auth );

                    foreach ( var rule in rules )
                    {
                        var existingAuth = authService.Get( rule.Id );
                        existingAuth.Order = order++;
                    }

                    rockContext.SaveChanges();

                    rules.Insert( 0, new AuthRule( auth ) );
                }
            }
        }

        /// <summary>
        /// Returns the authorization rules for the specified entity and action.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="action">The action.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [Obsolete("Use AuthRules( int, int, string ) instead. RockContext is no longer needed.")]
        public static List<AuthRule> AuthRules( int entityTypeId, int entityId, string action, RockContext rockContext )
        {
            return AuthRules( entityTypeId, entityId, action );
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

            // If there's no Authorizations object, create it
            if ( Authorizations == null )
            {
                Load();
            }

            // Find the Authrules for the given entity type, entity id, and action
            if ( Authorizations.ContainsKey( entityTypeId ) )
            {
                if ( Authorizations[entityTypeId].ContainsKey( entityId ) )
                {
                    if ( Authorizations[entityTypeId][entityId].ContainsKey( action ) )
                    {
                        rules = Authorizations[entityTypeId][entityId][action];
                    }
                }
            }

            return rules;
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
        /// <remarks>
        /// This method will save any previous changes made to the context
        /// </remarks>
        public static void CopyAuthorization( ISecured sourceEntity, ISecured targetEntity, RockContext rockContext )
        {
            // If there's no Authorizations object, create it
            if ( Authorizations == null )
            {
                Load();
            }

            var sourceEntityTypeId = sourceEntity.TypeId;
            var targetEntityTypeId = targetEntity.TypeId;

            AuthService authService = new AuthService( rockContext );

            // Delete the current authorizations for the target entity
            foreach ( Auth auth in authService.Get( targetEntityTypeId, targetEntity.Id ) )
            {
                authService.Delete( auth );
            }

            Dictionary<string, List<AuthRule>> newActions = new Dictionary<string, List<AuthRule>>();

            int order = 0;
            if ( Authorizations.ContainsKey( sourceEntityTypeId ) && Authorizations[sourceEntityTypeId].ContainsKey( sourceEntity.Id ) )
            {
                foreach ( KeyValuePair<string, List<AuthRule>> action in Authorizations[sourceEntityTypeId][sourceEntity.Id] )
                {
                    if ( targetEntity.SupportedActions.ContainsKey( action.Key ) )
                    {
                        newActions.Add( action.Key, new List<AuthRule>() );

                        foreach ( AuthRule rule in action.Value )
                        {
                            Auth auth = new Auth();
                            auth.EntityTypeId = targetEntityTypeId;
                            auth.EntityId = targetEntity.Id;
                            auth.Order = order;
                            auth.Action = action.Key;
                            auth.AllowOrDeny = rule.AllowOrDeny;
                            auth.SpecialRole = rule.SpecialRole;
                            auth.PersonAliasId = rule.PersonAliasId;
                            auth.GroupId = rule.GroupId;

                            authService.Add( auth );

                            newActions[action.Key].Add( new AuthRule( rule.Id, targetEntity.Id, rule.AllowOrDeny, rule.SpecialRole, rule.PersonId, rule.PersonAliasId, rule.GroupId, rule.Order ) );

                            order++;
                        }
                    }
                }
            }

            rockContext.SaveChanges();

            if ( !Authorizations.ContainsKey( targetEntityTypeId ) )
            {
                Authorizations.Add( targetEntityTypeId, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
            }

            Dictionary<int, Dictionary<string, List<AuthRule>>> entityType = Authorizations[targetEntityTypeId];

            if ( !entityType.ContainsKey( targetEntity.Id ) )
            {
                entityType.Add( targetEntity.Id, new Dictionary<string, List<AuthRule>>() );
            }

            entityType[targetEntity.Id] = newActions;
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
    }

    /// <summary>
    /// Lightweight class to store if a particular user or role is allowed or denied access
    /// </summary>
    public class AuthRule
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
        public string AllowOrDeny { get; set; }

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
        public AuthRule( int id, int? entityId, string allowOrDeny, SpecialRole specialRole, int? personId, int? personAliasId, int? groupId, int order )
        {
            Id = id;
            EntityId = entityId;
            AllowOrDeny = allowOrDeny;
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
        public AuthRule( Auth auth )
        {
            Id = auth.Id;
            EntityId = auth.EntityId;
            AllowOrDeny = auth.AllowOrDeny;
            SpecialRole = auth.SpecialRole;
            PersonId = auth.PersonAlias != null ? auth.PersonAlias.PersonId : (int?)null;
            PersonAliasId = auth.PersonAliasId;
            GroupId = auth.GroupId;
            Order = auth.Order;
        }
    }
}