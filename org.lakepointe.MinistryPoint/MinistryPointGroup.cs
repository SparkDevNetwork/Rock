using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;


namespace org.lakepointe.MinistryPoint
{
    public class MinistryPointGroup
    {

        public const string RegistrationTemplateGuid = "1FF94512-350E-498D-956A-6CAA359E35DC";
        public const string MinistryPointGroupTypeGuid = "23AB2BEA-9182-486B-A9EB-9961DDC8D6CE";
        public const string AccountContactRoleGuid = "6C1893A7-E0CB-4DE8-8290-9729F2CC178F";

        public int RegistrationId { get; set; }
        public int OrganizationGroupId { get; set; }
        public Group OrganizationGroup { get; set; }
        public string OrganizationName { get; set; }
        public int? AccountContactPersonAliasId { get; set; }




        #region Constructor
        public MinistryPointGroup()
        { }

        public MinistryPointGroup( int groupId )
        {
            LoadOrganizationGroup( groupId );
        }
        #endregion



        public int CreateNewGroup( int parentGroupId )
        {
            using ( var rockContext = new RockContext() )
            {
                var registration = new RegistrationService( rockContext ).Get( RegistrationId );

                if ( registration == null )
                {
                    throw new Exception( "Registration is required to create a Ministry Point Group." );
                }

                if ( OrganizationName.IsNullOrWhiteSpace() )
                {
                    throw new Exception( "Organization Name is requried to create a Ministry Point Group." );
                }

                var groupType = GroupTypeCache.Get( MinistryPointGroupTypeGuid.AsGuid() );
                var accountContactGuid = AccountContactRoleGuid.AsGuid();
                var contactUserRole = groupType.Roles.Where( r => r.Guid == accountContactGuid ).SingleOrDefault();

                //todo: add logic to check for existing group and throw error if it exists.

                var groupService = new GroupService( rockContext );
                var group = new Group();
                group.GroupTypeId = groupType.Id;
                group.Name = OrganizationName;
                group.ParentGroupId = parentGroupId;
                group.IsActive = true;
                group.IsPublic = false;

                if ( AccountContactPersonAliasId.HasValue )
                {
                    var accountContactPerson = new PersonAliasService( rockContext ).GetPerson( AccountContactPersonAliasId.Value );
                    var accountContactGM = new GroupMember();
                    accountContactGM.PersonId = accountContactPerson.Id;
                    accountContactGM.GroupRoleId = contactUserRole.Id;
                    accountContactGM.GroupMemberStatus = GroupMemberStatus.Active;
                    accountContactGM.IsArchived = false;
                    group.Members.Add( accountContactGM );
                }

                groupService.Add( group );
                rockContext.SaveChanges();

                group.LoadAttributes();
                group.SetAttributeValue( "Registration", registration != null ? registration.Guid.ToString() : String.Empty);
                group.SetAttributeValue( "OrganizationName", OrganizationName );
                group.SetAttributeValue( "ExpandedContent", bool.FalseString );
                group.SaveAttributeValues( rockContext );

                if ( registration != null )
                {
                    registration.LoadAttributes();
                    registration.SetAttributeValue( "OrganizationUserGroup", group.Guid );
                    registration.SaveAttributeValue( "OrganizationUserGroup", rockContext );
                }

                OrganizationGroup = group;
                OrganizationGroupId = group.Id;
                return group.Id;
            }
        }


        public int AddMember( int personId, int? roleId = null )
        {
            return AddMember( OrganizationGroupId, personId, roleId );
        }

        public void SendWelcomeEmail( Person person, string userName, string password, string appRoot, string themeRoot)
        {
            if(person.Email.IsNullOrWhiteSpace())
            {
                return;
            }

            var welcomeEmailGuid = "F9C0C6B7-1D48-479A-B136-C0B9AE483A1C".AsGuid();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Person", person );
            mergeFields.Add( "UserName", userName );
            mergeFields.Add( "Password", password );
            mergeFields.Add( "OrganizationName", OrganizationName );
            mergeFields.Add( "Group", this );

            var emailMessage = new Rock.Communication.RockEmailMessage( welcomeEmailGuid );
            emailMessage.AddRecipient( new Rock.Communication.RockEmailMessageRecipient( person, mergeFields ) );
            emailMessage.AppRoot = appRoot;
            emailMessage.ThemeRoot = themeRoot;
            emailMessage.CreateCommunicationRecord = false;
            emailMessage.Send();
        }

        private int AddMember( int groupId, int personId, int? roleId )
        {
            using ( var rockContext = new RockContext() )
            {

                if ( !roleId.HasValue )
                {
                    roleId = GroupTypeCache.Get( MinistryPointGroupTypeGuid.AsGuid() ).DefaultGroupRoleId;

                    if ( !roleId.HasValue )
                    {
                        throw new Exception( "Group Member Role is required." );
                    }
                }



                var groupMemberService = new GroupMemberService( rockContext );

                var groupMember = new GroupMember();
                groupMember.GroupId = groupId;
                groupMember.PersonId = personId;
                groupMember.GroupRoleId = roleId.Value;
                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                groupMember.IsArchived = false;

                groupMemberService.Add( groupMember );
                rockContext.SaveChanges();

                return groupMember.Id;
            }
        }


        private void LoadOrganizationGroup( int groupId )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( groupId );
                if ( group == null )
                {
                    RegistrationId = 0;
                    OrganizationGroupId = 0;
                    OrganizationGroup = null;
                    return;
                }

                group.LoadAttributes( rockContext );

                OrganizationGroupId = group.Id;
                OrganizationGroup = group;
                OrganizationName = group.GetAttributeValue( "OrganizationName" );

                var accountContactRole = GroupTypeCache.Get( MinistryPointGroupTypeGuid.AsGuid() )
                    .Roles.Where( r => r.Guid == AccountContactRoleGuid.AsGuid() )
                    .SingleOrDefault();

                var accountContactPerson = group.Members
                    .Where( m => m.GroupRoleId == accountContactRole.Id )
                    .Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive )
                    .Where( m => !m.IsArchived )
                    .Select( m => m.Person )
                    .FirstOrDefault();

                if ( accountContactPerson != null )
                {
                    AccountContactPersonAliasId = accountContactPerson.PrimaryAliasId;
                }
                else
                {
                    AccountContactPersonAliasId = null;
                }

                var registrationGuid = group.GetAttributeValue( "Registration" ).AsGuid();
                var registration = new RegistrationService( rockContext ).Get( registrationGuid );
                if ( registration != null )
                {
                    RegistrationId = registration.Id;
                }


            }
        }
    }
}
