//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rock.CMS;

namespace Rock.Security
{
    /// <summary>
    /// Static class for managing authorizations
    /// </summary>
    public static class Authorization
    {
        /// <summary>
        /// Authorizations is a static variable for storing all authorizations.  It uses multiple Dictionary objects similiar 
        /// to a multi-dimensional array to store information.  The first dimension is the entity type, second is the entity
        /// ID, third is the action, and the fourth dimension is a list of AuthRules for the action.
        /// </summary>
        public static Dictionary<string, Dictionary<int, Dictionary<string, List<AuthRule>>>> Authorizations { get; set; }
        
        /// <summary>
        /// Load the static Authorizations object
        /// </summary>
        public static void Load()
        {
            Authorizations = new Dictionary<string, Dictionary<int, Dictionary<string, List<AuthRule>>>>();

            AuthService authService = new AuthService();

            foreach ( Auth auth in authService.Queryable().
                OrderBy( A => A.EntityType ).ThenBy( A => A.EntityId ).ThenBy( A => A.Action ).ThenBy( A => A.Order ) )
            {
                if ( !Authorizations.ContainsKey( auth.EntityType ) )
                    Authorizations.Add( auth.EntityType, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
                Dictionary<int, Dictionary<string, List<AuthRule>>> entityAuths = Authorizations[auth.EntityType];

                if ( !entityAuths.ContainsKey( auth.EntityId ?? 0 ) )
                    entityAuths.Add( auth.EntityId ?? 0, new Dictionary<string, List<AuthRule>>() );
                Dictionary<string, List<AuthRule>> instanceAuths = entityAuths[auth.EntityId ?? 0];

                if ( !instanceAuths.ContainsKey( auth.Action ) )
                    instanceAuths.Add( auth.Action, new List<AuthRule>() );
                List<AuthRule> actionPermissions = instanceAuths[auth.Action];

                actionPermissions.Add( new AuthRule(
                    auth.Id,
                    auth.AllowOrDeny,
                    auth.SpecialRole,
                    auth.PersonId,
                    auth.GroupId,
                    auth.Order) );
            }
        }

        /// <summary>
        /// Reloads the authorizations for the specified entity and action.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="action">The action.</param>
        public static void ReloadAction( string entityType, int entityId, string action )
        {
            // If there's no Authorizations object, create it
            if ( Authorizations == null )
                Load();
            else
            {
                // Delete the current authorizations
                if ( Authorizations.ContainsKey( entityType ) )
                    if ( Authorizations[entityType].ContainsKey( entityId ) )
                        if ( Authorizations[entityType][entityId].ContainsKey( action ) )
                            Authorizations[entityType][entityId][action] = new List<AuthRule>();

                // Find the Authrules for the given entity type, entity id, and action
                AuthService authService = new AuthService();
                foreach ( Auth auth in authService.GetAuths(entityType, entityId, action))
                {
                    if ( !Authorizations.ContainsKey( auth.EntityType ) )
                        Authorizations.Add( auth.EntityType, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );
                    Dictionary<int, Dictionary<string, List<AuthRule>>> entityAuths = Authorizations[auth.EntityType];

                    if ( !entityAuths.ContainsKey( auth.EntityId ?? 0 ) )
                        entityAuths.Add( auth.EntityId ?? 0, new Dictionary<string, List<AuthRule>>() );
                    Dictionary<string, List<AuthRule>> instanceAuths = entityAuths[auth.EntityId ?? 0];

                    if ( !instanceAuths.ContainsKey( auth.Action ) )
                        instanceAuths.Add( auth.Action, new List<AuthRule>() );
                    List<AuthRule> actionPermissions = instanceAuths[auth.Action];

                    actionPermissions.Add( new AuthRule(
                        auth.Id,
                        auth.AllowOrDeny,
                        auth.SpecialRole,
                        auth.PersonId,
                        auth.GroupId,
                        auth.Order ) );
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
        /// <param name="entity"></param>
        /// <param name="action"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool Authorized( ISecured entity, string action, Rock.CMS.User user )
        {
        //    return Authorized( entity, action, user != null ? user.Person.Guid.ToString() : string.Empty );
        //}

        ///// <summary>
        ///// Evaluates whether a selected user is allowed to perform the selected action on the selected
        ///// entity.
        ///// </summary>
        ///// <param name="entity">The entity.</param>
        ///// <param name="action">The action.</param>
        ///// <param name="userName">Name of the user.</param>
        ///// <returns></returns>
        //private static bool Authorized( ISecured entity, string action, string userName )
        //{
            // If there's no Authorizations object, create it
            if ( Authorizations == null )
                Load();

            // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each 
            // one to find the first one specific to the selected user or a role that the selected user belongs 
            // to.  If a match is found return whether the user is allowed (true) or denied (false) access
            if ( Authorizations.Keys.Contains( entity.AuthEntity ) &&
                Authorizations[entity.AuthEntity].Keys.Contains( entity.Id ) &&
                Authorizations[entity.AuthEntity][entity.Id].Keys.Contains( action ) )
            {

                string userName = user != null ? user.Person.Guid.ToString() : string.Empty;

                foreach ( AuthRule authRule in Authorizations[entity.AuthEntity][entity.Id][action] )
                {
                    // All Users
                    if ( authRule.SpecialRole == SpecialRole.AllUsers )
                        return authRule.AllowOrDeny == "A";

                    // All Authenticated Users
                    if (authRule.SpecialRole == SpecialRole.AllAuthenticatedUsers && userName.Trim() != string.Empty)
                        return authRule.AllowOrDeny == "A";

                    // All Unauthenticated Users
                    if ( authRule.SpecialRole == SpecialRole.AllUnAuthenticatedUsers && userName.Trim() == string.Empty )
                        return authRule.AllowOrDeny == "A";

                    if ( authRule.SpecialRole == SpecialRole.None && userName != string.Empty )
                    {
                        // See if person has been authorized to entity
                        if ( authRule.PersonId.HasValue && 
                            user.PersonId.HasValue && 
                            authRule.PersonId.Value == user.PersonId.Value )
                            return authRule.AllowOrDeny == "A";

                        // See if person is in role authorized
                        if (authRule.GroupId.HasValue)
                        {
                            Role role = Role.Read( authRule.GroupId.Value );
                            if ( role != null && role.UserInRole( userName ) )
                                return authRule.AllowOrDeny == "A";
                        }
                    }
                }
            }

            // If not match was found for the selected user on the current entity instance, check to see if the instance
            // has a parent authority defined and if so evaluate that entities authorization rules.  If there is no
            // parent authority return the defualt authorization
            if ( entity.ParentAuthority != null )
                return Authorized( entity.ParentAuthority, action, user );
            else
                return entity.DefaultAuthorization( action );

        }

        /// <summary>
        /// Returns the authorization rules for the specified entity and action.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static List<AuthRule> AuthRules( string entityType, int entityId, string action )
        {
            List<AuthRule> rules = new List<AuthRule>();

            // If there's no Authorizations object, create it
            if ( Authorizations == null )
                Load();

            // Find the Authrules for the given entity type, entity id, and action
            if (Authorizations.ContainsKey(entityType))
                if ( Authorizations[entityType].ContainsKey( entityId ) )
                    if (Authorizations[entityType][entityId].ContainsKey(action))
                        rules = Authorizations[entityType][entityId][action];

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
        /// Copies the authorizations from one <see cref="ISecured"/> object to another
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="targetEntity">The target entity.</param>
        /// <param name="personId">The person id.</param>
        public static void CopyAuthorization( ISecured sourceEntity, ISecured targetEntity, int? personId )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                // If there's no Authorizations object, create it
                if ( Authorizations == null )
                    Load();

                AuthService authService = new AuthService();

                // Delete the current authorizations for the target entity
                foreach ( Auth auth in authService.GetByEntityTypeAndEntityId( targetEntity.AuthEntity, targetEntity.Id ) )
                    authService.Delete( auth, personId );

                Dictionary<string, List<AuthRule>> newActions = new Dictionary<string, List<AuthRule>>();

                int order = 0;
                if ( Authorizations.ContainsKey( sourceEntity.AuthEntity ) && Authorizations[sourceEntity.AuthEntity].ContainsKey( sourceEntity.Id ) )
                    foreach ( KeyValuePair<string, List<AuthRule>> action in Authorizations[sourceEntity.AuthEntity][sourceEntity.Id] )
                        if ( targetEntity.SupportedActions.Contains( action.Key ) )
                        {
                            newActions.Add( action.Key, new List<AuthRule>() );

                            foreach ( AuthRule rule in action.Value )
                            {
                                Auth auth = new Auth();
                                auth.EntityType = targetEntity.AuthEntity;
                                auth.EntityId = targetEntity.Id;
                                auth.Order = order;
                                auth.Action = action.Key;
                                auth.AllowOrDeny = rule.AllowOrDeny;
                                auth.SpecialRole = rule.SpecialRole;
                                auth.PersonId = rule.PersonId;
                                auth.GroupId = rule.GroupId;

                                authService.Add( auth, personId );
                                authService.Save( auth, personId );

                                newActions[action.Key].Add( new AuthRule( rule.Id, rule.AllowOrDeny, rule.SpecialRole, rule.PersonId, rule.GroupId, rule.Order ) );

                                order++;
                            }
                        }

                if ( !Authorizations.ContainsKey( targetEntity.AuthEntity ) )
                    Authorizations.Add( targetEntity.AuthEntity, new Dictionary<int, Dictionary<string, List<AuthRule>>>() );

                Dictionary<int, Dictionary<string, List<AuthRule>>> entityType = Authorizations[targetEntity.AuthEntity];

                if ( !entityType.ContainsKey( targetEntity.Id ) )
                    entityType.Add( targetEntity.Id, new Dictionary<string, List<AuthRule>>() );

                entityType[targetEntity.Id] = newActions;
            }
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
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int? PersonId { get; set; }

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
                switch (SpecialRole)
                {
                    case CMS.SpecialRole.AllUsers:
                        return "All Users";

                    case CMS.SpecialRole.AllAuthenticatedUsers:
                        return "All Authenticated Users";

                    case CMS.SpecialRole.AllUnAuthenticatedUsers:
                        return "All Un-Authenticated Users";

                    default:

                        if (PersonId.HasValue)
                        {
                            try
                            {
                                Rock.CRM.PersonService personService = new CRM.PersonService();
                                Rock.CRM.Person person = personService.Get( PersonId.Value );
                                if (person != null)
                                    return person.FullName + " (User)";
                            }
                            catch {}
                        }

                        if (GroupId.HasValue)
                        {
                            try 
                            {
                                var role = Role.Read(GroupId.Value);
                                if (role != null)
                                    return role.Name + " (Role)";
                            }
                            catch {}
                        }

                        return "*** Unknown User/Role ***";
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthRule"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="allowOrDeny">Allow or Deny ("A" or "D").</param>
        /// <param name="userOrRole">User or Role ("U" or "R").</param>
        /// <param name="userOrRoleName">Name of the user or role.</param>
        /// <param name="order">The order.</param>
        public AuthRule( int id, string allowOrDeny, SpecialRole specialRole, int? personId, int? groupId, int order )
        {
            Id = id;
            AllowOrDeny = allowOrDeny;
            SpecialRole = specialRole;
            PersonId = personId;
            GroupId = groupId;
            Order = order;
        }
    }

}