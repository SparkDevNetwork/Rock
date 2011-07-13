using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rock.Models.Cms;
using Rock.Services.Cms;

namespace Rock.Cms.Security
{
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
                    auth.AllowOrDeny,
                    auth.UserOrRole,
                    auth.UserOrRoleName ) );
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
        public static bool Authorized( ISecured entity, string action, System.Web.Security.MembershipUser user )
        {
            // If there's no Authorizations object, create it
            if ( Authorizations == null )
                Load();

            // If there's no entry in the Authorizations object for this entity type, return the default authorization
            if ( !Authorizations.Keys.Contains( entity.AuthEntity ) )
                return entity.DefaultAuthorization( action );

            // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each 
            // one to find the first one specific to the selected user or a role that the selected user belongs 
            // to.  If a match is found return whether the user is allowed (true) or denied (false) access
            if ( Authorizations[entity.AuthEntity].Keys.Contains( entity.Id ) &&
                Authorizations[entity.AuthEntity][entity.Id].Keys.Contains( action ) )
                foreach ( AuthRule authRule in Authorizations[entity.AuthEntity][entity.Id][action] )
                {
                    if ( authRule.UserOrRoleName == "*" )
                        return authRule.AllowOrDeny == "A";

                    if ( user != null )
                        if ( ( authRule.UserOrRole == "U" && user.UserName == authRule.UserOrRoleName ) ||
                        ( authRule.UserOrRole == "R" && System.Web.Security.Roles.IsUserInRole( user.UserName, authRule.UserOrRoleName ) ) )
                            return authRule.AllowOrDeny == "A";
                }

            // If not match was found for the selected user on the current entity instance, check to see if the instance
            // has a parent authority defined and if so evaluate that entities authorization rules.  If there is no
            // parent authority return the defualt authorization
            if ( entity.ParentAuthority != null )
                return Authorized( entity.ParentAuthority, action, user );
            else
                return entity.DefaultAuthorization( action );
        }

        public static void CopyAuthorization( ISecured sourceEntity, ISecured targetEntity, int? personId )
        {
            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                // If there's no Authorizations object, create it
                if ( Authorizations == null )
                    Load();

                AuthService authService = new AuthService();

                // Delete the current authorizations for the target entity
                foreach(Auth auth in authService.GetAuthsByEntityTypeAndEntityId(targetEntity.AuthEntity, targetEntity.Id))
                    authService.DeleteAuth(auth);

                Dictionary<string, List<AuthRule>> newActions = new Dictionary<string, List<AuthRule>>();

                int order = 0;
                foreach ( KeyValuePair<string, List<AuthRule>> action in Authorizations[sourceEntity.AuthEntity][sourceEntity.Id] )
                    if (targetEntity.SupportedActions.Contains(action.Key))
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
                            auth.UserOrRole = rule.UserOrRole;
                            auth.UserOrRoleName = rule.UserOrRoleName;

                            authService.AddAuth(auth);
                            authService.Save(auth, personId);

                            newActions[action.Key].Add( new AuthRule( rule.AllowOrDeny, rule.UserOrRole, rule.UserOrRoleName ) );

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
        public string AllowOrDeny { get; set; }
        public string UserOrRole { get; set; }
        public string UserOrRoleName { get; set; }

        public AuthRule( string allowOrDeny, string userOrRole, string userOrRoleName )
        {
            AllowOrDeny = allowOrDeny;
            UserOrRole = userOrRole;
            UserOrRoleName = userOrRoleName;
        }
    }

}