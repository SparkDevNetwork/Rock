using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web;
using System.Web.Security;

using Rock.Services.Cms;
using Rock.Services.Groups;

namespace Rock.Cms.Security
{
    /// <summary>
    /// Membership role provider.  This provider is used if managing roles through the 
    /// cmsRole table and granting authority to each cmsUser login independently of any 
    /// Person object that they may be associated with.
    /// </summary>
    public class GroupRoleProvider : RoleProvider
    {
        private string applicationName;

        private class UserInfo
        {
            public int PersonId { get; set; }
            public List<string> Roles { get; set; }
            public UserInfo( int personId, List<string> roles )
            {
                PersonId = personId;
                Roles = roles;
            }
        }

        // Role / Users
        private Dictionary<string, List<string>> CachedRoles = null;

        // Application name / Username / Roles
        private Dictionary<string, Dictionary<string, UserInfo>> CachedUsers = null;

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
            throw new NotImplementedException( "Provider does not support AddUsersToRoles method" );
        }

        public override void CreateRole( string roleName )
        {
            throw new NotImplementedException( "Provider does not support CreateRole method" );
        }

        public override bool DeleteRole( string roleName, bool throwOnPopulatedRole )
        {
            throw new NotImplementedException( "Provider does not support DeleteRole method" );
        }

        public override string[] FindUsersInRole( string roleName, string usernameToMatch )
        {
            if ( string.IsNullOrEmpty( roleName ) )
                throw new ArgumentNullException( "roleName" );

            if ( string.IsNullOrEmpty( usernameToMatch ) )
                throw new ArgumentNullException( "usernameToMatch" );

            List<string> users = GetRole( roleName );
            if ( users == null )
                throw new ProviderException( "Role does not exist." );

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

            UserInfo user = GetUser( username );
            if ( user == null )
                throw new ProviderException( "user does not exist." );

            return user.Roles.ToArray();
        }

        public override string[] GetUsersInRole( string roleName )
        {
            if ( string.IsNullOrEmpty( roleName ) )
                throw new ArgumentNullException( "roleName" );

            List<string> users = GetRole( roleName );
            if ( users == null )
                throw new ProviderException( "Role does not exist." );

            return users.ToArray();
        }

        public override bool IsUserInRole( string username, string roleName )
        {
            if ( string.IsNullOrEmpty( username ) )
                throw new ArgumentNullException( "username" );

            if ( string.IsNullOrEmpty( roleName ) )
                throw new ArgumentNullException( "roleName" );

            if ( GetUser( username ) == null )
                throw new ProviderException( "user does not exist." );

            List<string> users = GetRole( roleName );
            if ( users != null )
                return users.Contains( username );
            else
                throw new ProviderException( "Role does not exist." );
        }

        public override void RemoveUsersFromRoles( string[] usernames, string[] roleNames )
        {
            throw new NotImplementedException( "Provider does not support RemoveUsersFromRoles method" );
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
                CachedRoles = new Dictionary<string, List<string>>();

            return CachedRoles;
        }

        private List<string> GetRole( string role )
        {
            Dictionary<string, List<string>> applicationRoles = GetRoles();

            if ( applicationRoles == null )
            {
                applicationRoles = new Dictionary<string, List<string>>();

                GroupService GroupService = new GroupService();

                Rock.Models.Groups.Group groupModel = GroupService.GetGroupByGuid( new Guid( role ) );
                if (groupModel != null)
                {
                    List<string> usernames = new List<string>();

                    foreach ( Rock.Models.Groups.Member member in groupModel.Members)
                        foreach ( Rock.Models.Cms.User userModel in member.Person.Users)
                            usernames.Add( userModel.Username );

                    applicationRoles.Add( groupModel.Name, usernames );
                }
            }

            if ( applicationRoles.ContainsKey( role ) )
                return applicationRoles[role];
            else
                return null;
        }

        private Dictionary<string, UserInfo> GetUsers()
        {
            if ( CachedUsers == null )
                CachedUsers = new Dictionary<string, Dictionary<string, UserInfo>>();

            if ( !CachedUsers.ContainsKey( applicationName ) )
                CachedUsers.Add( applicationName, null );

            return CachedUsers[applicationName];
        }

        private UserInfo GetUser( string username )
        {
            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                UserService UserService = new UserService();
                MemberService MemberService = new MemberService();

                Dictionary<string, UserInfo> applicationUsers = GetUsers();

                if ( applicationUsers == null )
                    applicationUsers = new Dictionary<string, UserInfo>();

                if ( !applicationUsers.ContainsKey( username ) )
                {
                    Rock.Models.Cms.User userModel = UserService.GetUserByApplicationNameAndUsername( applicationName, username );
                    if ( userModel != null && userModel.PersonId != null )
                    {
                        List<string> rolenames = new List<string>();

                        foreach ( Rock.Models.Groups.Member member in
                            MemberService.Queryable().Where( m => m.Group.IsSecurityRole && m.PersonId == userModel.PersonId.Value ) )
                            rolenames.Add( member.Group.Guid.ToString() );

                        applicationUsers.Add( userModel.Username, new UserInfo( userModel.PersonId.Value, rolenames ) );
                    }
                }

                if ( applicationUsers.ContainsKey( username ) )
                    return applicationUsers[username];
                else
                    return null;
            }
        }

        #endregion
    }
}