using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web;
using System.Web.Security;

using Rock.Framework.Properties;
using Rock.Services.Cms;

namespace Rock.Cms.Security
{
    /// <summary>
    /// Membership role provider.  This provider is used if managing roles through the 
    /// cmsRole table and granting authority to each cmsUser login independently of any 
    /// Person object that they may be associated with.
    /// </summary>
    public class UserRoleProvider : RoleProvider
    {
        private string applicationName;

        // Application name / Role / Users
        private Dictionary<string, Dictionary<string, List<string>>> CachedRoles = null;

        // Application name / Username / Roles
        private Dictionary<string, Dictionary<string, List<string>>> CachedUsers = null;

        #region Properties

        public override string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        #endregion

        public override void Initialize( string name, System.Collections.Specialized.NameValueCollection config )
        {
            if ( config == null )
                throw new ArgumentNullException( "config" );

            if ( name == null || name.Length == 0 )
            {
                name = "RockChMS";
            }

            if ( String.IsNullOrEmpty( config["description"] ) )
            {
                config.Remove( "description" );
                config.Add( "description", "Rock ChMS" );
            }

            base.Initialize( name, config );

            applicationName = GetConfigValue( config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath );
        }

        public override void AddUsersToRoles( string[] usernames, string[] roleNames )
        {
            foreach ( string roleName in roleNames )
                if ( GetRole( roleName ) == null )
                    throw new ProviderException( string.Format( "{0} {1}", roleName, ExceptionMessage.RoleDoesntExist ) );

            foreach ( string username in usernames )
                if ( GetUser( username ) == null )
                    throw new ProviderException( string.Format( "{0} {1}", username, ExceptionMessage.UserDoesntExist ) );

            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                UserService UserService = new UserService();
                RoleService RoleService = new RoleService();

                foreach ( string roleName in roleNames )
                {
                    List<string> users = GetRole(roleName);

                    foreach ( string username in usernames )
                        if ( !users.Contains( username ) )
                        {
                            Rock.Models.Cms.Role role = RoleService.GetRoleByApplicationNameAndName( applicationName, roleName );
                            if ( role == null )
                                throw new ProviderException( string.Format("{0} {1}", roleName, ExceptionMessage.RoleDoesntExist ) );  //Shouldn't happen

                            Rock.Models.Cms.User user = UserService.GetUserByApplicationNameAndUsername( applicationName, username );
                            if ( user == null )
                                throw new ProviderException( string.Format( "{0} {1}", username, ExceptionMessage.UserDoesntExist ) );  //Shouldn't happen

                            role.Users.Add( user );
                            RoleService.Save( role, CurrentPersonId() );

                            users.Add( username );
                        }

                    users.Sort();
                }
            }
        }

        public override void CreateRole( string roleName )
        {
            if ( string.IsNullOrEmpty( roleName ) )
                throw new ArgumentNullException( "roleName" );

            if (GetRole(roleName) != null)
                throw new ProviderException( ExceptionMessage.RoleDoesntExist );

            RoleService RoleService = new RoleService();

            Rock.Models.Cms.Role role = RoleService.GetRoleByApplicationNameAndName( applicationName, roleName );

            if (role == null)
            {
                role = new Rock.Models.Cms.Role();
                role.ApplicationName = applicationName;
                role.Name = roleName;

                try
                {
                    RoleService.AddRole( role );
                    RoleService.Save( role, CurrentPersonId() );
                }
                catch (SystemException ex)
                {
                    throw new ProviderException( ExceptionMessage.CreateRoleError, ex);
                }

                GetRoles().Add(roleName, new List<string>());
            }
            else
                throw new ProviderException( ExceptionMessage.RoleAlreadyExists );
        }

        public override bool DeleteRole( string roleName, bool throwOnPopulatedRole )
        {
            if ( string.IsNullOrEmpty( roleName ) )
                throw new ArgumentNullException( "roleName" );

            if ( GetRole( roleName ) == null )
                throw new ArgumentException( ExceptionMessage.RoleDoesntExist );

            RoleService RoleService = new RoleService();

            Rock.Models.Cms.Role role = RoleService.GetRoleByApplicationNameAndName( applicationName, roleName );
            
            if ( role != null )
            {
                if ( throwOnPopulatedRole && role.Users.Count > 0 )
                    throw new ProviderException( ExceptionMessage.RoleContainsUsers );

                RoleService.DeleteRole( role );

                GetRoles().Remove( roleName );

                return true;

            }
            else
                throw new ArgumentException( ExceptionMessage.RoleDoesntExist );
        }

        public override string[] FindUsersInRole( string roleName, string usernameToMatch )
        {
            if ( string.IsNullOrEmpty( roleName ) )
                throw new ArgumentNullException( "roleName" );

            if ( string.IsNullOrEmpty( usernameToMatch ) )
                throw new ArgumentNullException( "usernameToMatch" );

            List<string> users = GetRole(roleName);
            if (users == null)
                throw new ProviderException( ExceptionMessage.RoleDoesntExist );

            return users.ToArray();
        }

        public override string[] GetAllRoles()
        {
            Dictionary<string, List<string>> roles = GetRoles();

            string[] roleNames = new string[roles.Count];

            return roles.Keys.ToArray();
        }

        public override string[] GetRolesForUser( string username )
        {
            if ( string.IsNullOrEmpty( username ) )
                throw new ArgumentNullException( "username" );

            List<string> roles = GetUser( username );
            if (roles == null)
                throw new ProviderException( ExceptionMessage.UserDoesntExist );

            return roles.ToArray();
        }

        public override string[] GetUsersInRole( string roleName )
        {
            if ( string.IsNullOrEmpty( roleName ) )
                throw new ArgumentNullException( "roleName" );

            List<string> users = GetRole( roleName );
            if (users == null)
                throw new ProviderException( ExceptionMessage.RoleDoesntExist );

            return users.ToArray();
        }

        public override bool IsUserInRole( string username, string roleName )
        {
            if ( string.IsNullOrEmpty( username ) )
                throw new ArgumentNullException( "username" );

            if ( string.IsNullOrEmpty( roleName ) )
                throw new ArgumentNullException( "roleName" );

            if (GetUser(username) == null)
                throw new ProviderException( ExceptionMessage.UserDoesntExist );

            List<string> users = GetRole( roleName );
            if (users != null)
                return users.Contains(username);
            else
                throw new ProviderException( ExceptionMessage.RoleDoesntExist );
       }

        public override void RemoveUsersFromRoles( string[] usernames, string[] roleNames )
        {
            foreach ( string roleName in roleNames )
                if ( GetRole( roleName ) == null )
                    throw new ProviderException( string.Format( "{0} {1}", roleName, ExceptionMessage.RoleDoesntExist ) );

            foreach ( string username in usernames )
                if ( GetUser( username ) == null )
                    throw new ProviderException( string.Format( "{0} {1}", username, ExceptionMessage.UserDoesntExist ) );

            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                UserService UserService = new UserService();
                RoleService RoleService = new RoleService();

                List<Rock.Models.Cms.Role> roles = new List<Rock.Models.Cms.Role>();
                List<Rock.Models.Cms.User> users = new List<Rock.Models.Cms.User>();

                foreach ( Rock.Models.Cms.Role role in roles )
                {
                    foreach ( Rock.Models.Cms.User user in users )
                    {
                        role.Users.Remove( user );
                        GetRole( role.Name ).Remove( user.Username );
                    }
                    RoleService.Save( role, CurrentPersonId() );
                }
            }
        }

        public override bool RoleExists( string roleName )
        {
            if ( string.IsNullOrEmpty( roleName ) )
                throw new ArgumentNullException( "roleName" );

            return GetRole( roleName ) != null;
        }

        #region Utility Methods

        private string GetConfigValue( string configValue, string defaultValue )
        {
            if ( String.IsNullOrEmpty( configValue ) )
            {
                return defaultValue;
            }

            return configValue;
        }

        private Dictionary<string, List<string>> GetRoles()
        {
            if ( CachedRoles == null )
                CachedRoles = new Dictionary<string, Dictionary<string, List<string>>>();

            if ( !CachedRoles.ContainsKey( applicationName ) )
                CachedRoles.Add( applicationName, null );

            return CachedRoles[applicationName];
        }

        private List<string> GetRole( string role )
        {
            Dictionary<string, List<string>> applicationRoles = GetRoles();

            if ( applicationRoles == null )
            {
                applicationRoles = new Dictionary<string, List<string>>();

                RoleService RoleService = new RoleService();

                foreach ( Rock.Models.Cms.Role roleModel in RoleService.Queryable().Where( r => r.ApplicationName == applicationName ).OrderBy( r => r.Name ) )
                {
                    List<string> usernames = new List<string>();

                    foreach ( Rock.Models.Cms.User userModel in roleModel.Users.OrderBy( u => u.Username ) )
                        usernames.Add( userModel.Username );

                    applicationRoles.Add( roleModel.Name, usernames );
                }
            }

            if ( applicationRoles.ContainsKey( role ) )
                return applicationRoles[role];
            else
                return null;
        }

        private Dictionary<string, List<string>> GetUsers()
        {
            if ( CachedUsers == null )
                CachedUsers = new Dictionary<string, Dictionary<string, List<string>>>();

            if ( !CachedUsers.ContainsKey( applicationName ) )
                CachedUsers.Add( applicationName, null );

            return CachedUsers[applicationName];
        }

        private List<string> GetUser( string username )
        {
            UserService UserService = new UserService();

            Dictionary<string, List<string>> applicationUsers = GetUsers();

            if ( applicationUsers == null )
            {
                applicationUsers = new Dictionary<string, List<string>>();

                foreach ( Rock.Models.Cms.User userModel in UserService.Queryable().Where( r => r.ApplicationName == applicationName ).OrderBy( u => u.Username ) )
                {
                    List<string> rolenames = new List<string>();
                    foreach ( Rock.Models.Cms.Role roleModel in userModel.Roles.OrderBy( r => r.Name ) )
                        rolenames.Add( roleModel.Name );

                    applicationUsers.Add( userModel.Username, rolenames );
                }
            }
            else
            {
                if ( !applicationUsers.ContainsKey( username ) )
                {
                    Rock.Models.Cms.User userModel = UserService.GetUserByApplicationNameAndUsername( applicationName, username );
                    if ( userModel != null )
                    {
                        List<string> rolenames = new List<string>();
                        foreach ( Rock.Models.Cms.Role roleModel in userModel.Roles.OrderBy( r => r.Name ) )
                            rolenames.Add( roleModel.Name );

                        applicationUsers.Add( userModel.Username, rolenames );
                    }
                }
            }

            if ( applicationUsers.ContainsKey( username ) )
                return applicationUsers[username];
            else
                return null;
        }

        private int? CurrentPersonId()
        {
            MembershipUser user = Membership.GetUser();
            if ( user != null )
            {
                if ( user.ProviderUserKey != null )
                    return ( int )user.ProviderUserKey;
                else
                    return null;
            }
            return null;
        }

        #endregion
    }
}