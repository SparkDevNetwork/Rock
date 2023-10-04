using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_lakepointe.Security
{
    [DisplayName("Group Member User Tools")]
    [Category("LPC > Security")]
    [Description("Allows an administrator tools to bulk add User Logins for GroupMembers")]
    [BooleanField("Show Generate User Logins", "True/False flag indicating if the Show Generate User Logins command should be visible", true, Order = 0 )]
    [BooleanField("Show Convert Arena Logins", "True/False flag indicating if the Show Convert Arena Logins command should be visible.",true, Order = 1)]
    public partial class GroupMemberUsernameTools : RockBlock
    {
        public int? GroupId { get; set; }
        #region Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            Server.ScriptTimeout = 900;
            ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 900;

            if ( !CheckSettings() )
            {
                pnlCommands.Visible = false;
            }
            else
            {
                pnlCommands.Visible = true;
                nbMessage.Visible = false;
                lbGenerateUserLogin.Visible = GetAttributeValue( "ShowGenerateUserLogins" ).AsBoolean();
                lbConvertArenaLogin.Visible = GetAttributeValue( "ShowConvertArenaLogins" ).AsBoolean();
            }
            
        }
        #endregion

        protected void lbGenerateUserLogin_Click( object sender, EventArgs e )
        {
 
            GenerateUserLogins();
        }
        protected void lbConvertArenaLogin_Click( object sender, EventArgs e )
        {
            ConvertArenaLogins();
        }


        private bool CheckSettings()
        {
            GroupId = PageParameter( "GroupId" ).AsIntegerOrNull();

            if ( !GroupId.HasValue )
            {
                return false;
            }

            var count = new GroupMemberService( new RockContext() )
                .GetByGroupId( GroupId.Value )
                .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                .Count();

            if ( count == 0 )
            {
                return false;
            }

            return true;

        }

        private void ConvertArenaLogins( bool includeUsedLogins = false )
        {
            var rockContext = new RockContext();
            int arenaLoginCount = 0;

            var groupmemberSvc = new GroupMemberService( rockContext );
            var arenaAuthEntityType = EntityTypeCache.Get( "A2B1F985-FA3E-4D98-8065-C06D5621533A".AsGuid() );
            var databaseEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id;
            if ( arenaAuthEntityType == null || arenaAuthEntityType.Id <= 0 )
            {
                nbMessage.Title = "Arena User Conversion Canceled";
                nbMessage.Text = "Arena Authentication is not installed.";
                nbMessage.NotificationBoxType = NotificationBoxType.Warning;
                nbMessage.Visible = true;
                return;
            }

            var persons = groupmemberSvc.GetByGroupId( GroupId.Value )
                .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                .Select( m => m.Person )
                .ToList();

            foreach ( var person in persons )
            {
                using ( var personContext = new RockContext() )
                {
                    bool loginConverted = false;
                    var userLogins = new UserLoginService( personContext ).GetByPersonId( person.Id )
                        .Where( u => u.EntityTypeId == arenaAuthEntityType.Id )
                        .Where( u => u.LastActivityDateTime == null );

                    foreach ( var login in userLogins )
                    {
                        login.EntityTypeId = databaseEntityTypeId;
                        arenaLoginCount++;
                        loginConverted = true;
                    }

                    if ( loginConverted )
                    {
                        personContext.SaveChanges();
                    }
                }
            }

            nbMessage.NotificationBoxType = NotificationBoxType.Success;
            nbMessage.Title = "Arena User Conversion complete";
            if ( arenaLoginCount > 0 )
            {
                nbMessage.Text = string.Format( "{0} {1} successfully converted.", arenaLoginCount, "Arena Login".PluralizeIf( arenaLoginCount > 1 ) );
            }
            else
            {
                nbMessage.Text = "No Arena Logins found to convert.";
            }
            nbMessage.Visible = true;
        }

        private void GenerateUserLogins()
        {
            var rockContext = new RockContext();
            var groupMemberSvc = new GroupMemberService( rockContext );
            int loginsCreated = 0;

            int databaseLoginEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id;

            var memberQry = groupMemberSvc.GetByGroupId( GroupId.Value )
                .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active );

            foreach ( var person in memberQry.Select(m => m.Person).ToList() )
            {
                using ( var memberContext = new RockContext() )
                {
                    if ( !person.Users.Any() && !String.IsNullOrWhiteSpace( person.NickName ) && !String.IsNullOrWhiteSpace( person.LastName ) )
                    {
                        string newPassword = System.Web.Security.Membership.GeneratePassword( 9, 1 );
                        string userName = Rock.Security.Authentication.Database.GenerateUsername( person.NickName, person.LastName );

                        UserLogin login = UserLoginService.Create(
                            memberContext,
                            person,
                            AuthenticationServiceType.Internal,
                            databaseLoginEntityTypeId,
                            userName,
                            newPassword,
                            true,
                            false );

                        loginsCreated++;

                    }
                }
            }

            nbMessage.NotificationBoxType = NotificationBoxType.Success;
            nbMessage.Title = "User Logins Process Complete";
            if ( loginsCreated > 0 )
            {
                nbMessage.Text = string.Format( "{0} {1} successfully created.", loginsCreated, "User Login".PluralizeIf( loginsCreated > 1 ) );
            }
            else
            {
                nbMessage.Text = "No User Logins Created. All Group Members previously had logins.";
            }
            nbMessage.Visible = true;
        }


    }
}