using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.VersionInfo;
using Rock.Web.Cache;

using com.SimpleDonation.Constants;
using com.SimpleDonation.Services;

namespace Plugins.com_simpledonation
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Setup Simple Donation" )]
    [Category( "Simple Donation" )]
    [Description( "Configure your Simple Donation integration" )]
    public partial class SimpleDonationSetup : Rock.Web.UI.RockBlock
    {
        private const string DEFAULT_USER_NAME = "simpledonation";
        private const string DEFAULT_PASSWORD = "secret";

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowView();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        protected void btnCreateUser_Click( object sender, EventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                PersonService personService = new PersonService( rockContext );
                Person person = personService.Get( SystemGuid.SIMPLE_DONATION_USER_PERSON_GUID.AsGuid() );

                if ( person == null )
                {
                    person = CreatePerson( rockContext );
                    SetupSecurityRole( rockContext, person );
                    CreateLogin( rockContext, person );
                    NotifySimpleDonation();
                }
            }

            ShowView();
        }

        private Person CreatePerson( RockContext rockContext )
        {  
            DefinedValueCache connectionStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_MEMBER );
            DefinedValueCache recordStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );
            DefinedValueCache recordType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );

            Person person = new Person
            {
                ConnectionStatusValueId = connectionStatus.Id,
                Email = "info@simpledonation.com",
                EmailPreference = EmailPreference.DoNotEmail,
                FirstName = "Simple",
                Guid = SystemGuid.SIMPLE_DONATION_USER_PERSON_GUID.AsGuid(),
                IsEmailActive = true,
                LastName = "Donation",
                RecordStatusValueId = recordStatus.Id,
                RecordTypeValueId = recordType.Id,
            };

            GroupTypeCache familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            GroupTypeRoleCache adultRole = familyGroupType.Roles
                    .FirstOrDefault( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );

            GroupMember groupMember = new GroupMember
            {
                GroupMemberStatus = GroupMemberStatus.Active,
                Person = person,
                GroupRoleId = adultRole.Id,
            };

            GroupService.SaveNewFamily( rockContext, new List<GroupMember> { groupMember }, null, true );

            return person;
        }

        private void ShowView()
        {
            using ( RockContext context = new RockContext() )
            {
                PersonService personService = new PersonService( context );
                Person person = personService.Get( SystemGuid.SIMPLE_DONATION_USER_PERSON_GUID.AsGuid() );
                bool hasPerson = person != null;

                divCreatePerson.Visible = !hasPerson;
                divPersonExists.Visible = hasPerson;
            }
        }

        private void SetupSecurityRole( RockContext rockContext, Person person )
        {
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            GroupService groupService = new GroupService( rockContext );
            Group administratorGroup = groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() );
            GroupTypeCache groupType = GroupTypeCache.GetSecurityRoleGroupType();
            GroupTypeRoleCache groupTypeRole = groupType.Roles.FirstOrDefault();

            GroupMember groupMember = new GroupMember
            {
                GroupMemberStatus = GroupMemberStatus.Active,
                Person = person,
                GroupId = administratorGroup.Id,
                GroupRoleId = groupTypeRole.Id,
            };

            groupMemberService.Add( groupMember );
            rockContext.SaveChanges();
        }

        private void CreateLogin( RockContext rockContext, Person person )
        {
            EntityTypeCache authEntity = EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE );
            UserLoginService userLoginService = new UserLoginService( rockContext );

            UserLogin userLogin = new UserLogin
            {
                EntityTypeId = authEntity.Id,
                IsConfirmed = true,
                IsLockedOut = false,
                IsPasswordChangeRequired = true,
                LastPasswordChangedDateTime = RockDateTime.Now,
                PersonId = person.Id,
                UserName = DEFAULT_USER_NAME,
            };

            AuthenticationComponent authenticationComponent = AuthenticationContainer.GetComponent( authEntity.Name );
            userLogin.Password = authenticationComponent.EncodePassword( userLogin, DEFAULT_PASSWORD );

            userLoginService.Add( userLogin );
            rockContext.SaveChanges();
        }

        private void NotifySimpleDonation()
        {
            SimpleDonationAccountService accountService = new SimpleDonationAccountService();

            accountService.NotifySimpleDonation(
                NotificationKind.SIMPLE_DONATION_USER_CREATED,
                GlobalAttributesCache.Value( "OrganizationName" ).EscapeQuotes(),
                GlobalAttributesCache.Value( "PublicApplicationRoot" ),
                VersionInfo.GetRockSemanticVersionNumber() );
        }
    }
}
