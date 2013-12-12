//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

using System.Collections.Generic;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Block that will prompt for name and email, create a person if they don't exist, and add them to a group.
    /// </summary>
    #region Block Attributes

    [GroupField( "Group", "The group to add people to", true )]
    [TextField( "Save Button Text", "The text to use for the Save button", false, "Save" )]
    [TextField( "Success Message", "The message to display when user is succesfully added to the group", false, "Please check your email to verify your registration" )]
    [EmailTemplateField( "Confirmation Email", "The email to send the person to confirm their registration.  If not specified, the user will not need to confirm their registration", false )]
    [LinkedPage( "Confirmation Page", "The page that user should be directed to to confirm their registration" )]

    #endregion

    public partial class GroupSimpleRegister : Rock.Web.UI.RockBlock
    {
        #region overridden control methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            btnSave.Text = GetAttributeValue( "SaveButtonText" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            nbError.Visible = false;
        }

        #endregion

        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( txtFirstName.Text ) ||
                string.IsNullOrWhiteSpace( txtLastName.Text ) ||
                string.IsNullOrWhiteSpace( txtEmail.Text ) )
            {
                ShowError( "Missing Information", "Please enter a value for First Name, Last Name, and Email" );
            }
            else
            {
                var person = GetPerson();
                if ( person != null )
                {
                    int groupId = int.MinValue;
                    if ( int.TryParse( GetAttributeValue( "Group" ), out groupId ) )
                    {
                        using ( new UnitOfWorkScope() )
                        {
                            var groupService = new GroupService();
                            var groupMemberService = new GroupMemberService();

                            var group = groupService.Get( groupId );
                            if ( group != null && group.GroupType.DefaultGroupRoleId.HasValue )
                            {
                                string linkedPage = GetAttributeValue( "ConfirmationPage" );
                                if ( !string.IsNullOrWhiteSpace( linkedPage ) )
                                {
                                    var member = group.Members.Where( m => m.PersonId == person.Id ).FirstOrDefault();

                                    // If person has not registered or confirmed their registration
                                    if ( member == null || member.GroupMemberStatus != GroupMemberStatus.Active )
                                    {
                                        Email email = null;

                                        Guid guid = Guid.Empty;
                                        if ( Guid.TryParse( GetAttributeValue( "ConfirmationEmail" ), out guid ) )
                                        {
                                            email = new Email( guid );
                                        }

                                        if ( member == null )
                                        {
                                            member = new GroupMember();
                                            member.GroupId = group.Id;
                                            member.PersonId = person.Id;
                                            member.GroupRoleId = group.GroupType.DefaultGroupRoleId.Value;

                                            // If a confirmation email is configured, set status to Pending otherwise set it to active
                                            member.GroupMemberStatus = email != null ? GroupMemberStatus.Pending : GroupMemberStatus.Active;

                                            groupMemberService.Add( member, CurrentPersonId );
                                            groupMemberService.Save( member, CurrentPersonId );
                                            member = groupMemberService.Get( member.Id );
                                        }

                                        // Send the confirmation
                                        if ( email != null )
                                        {
                                            var mergeObjects = new Dictionary<string, object>();
                                            mergeObjects.Add( "Member", member );

                                            var pageParams = new Dictionary<string, string>();
                                            pageParams.Add( "gm", member.UrlEncodedKey );
                                            var pageReference = new Rock.Web.PageReference( linkedPage, pageParams );
                                            mergeObjects.Add( "ConfirmationPage", pageReference.BuildUrl() );

                                            var recipients = new Dictionary<string, Dictionary<string, object>>();
                                            recipients.Add( person.Email, mergeObjects );
                                            email.Send( recipients );
                                        }

                                        ShowSuccess( GetAttributeValue( "SuccessMessage" ) );

                                    }
                                    else
                                    {
                                        var pageParams = new Dictionary<string, string>();
                                        pageParams.Add( "gm", member.UrlEncodedKey );
                                        var pageReference = new Rock.Web.PageReference( linkedPage, pageParams );
                                        Response.Redirect( pageReference.BuildUrl(), false );
                                    }
                                }
                                else
                                {
                                    ShowError( "Configuration Error", "Invalid Confirmation Page setting" );
                                }
                            }
                            else
                            {
                                ShowError( "Configuration Error",
                                    "The configured group does not exist, or it's group type does not have a default role configured." );
                            }
                        }
                    }
                    else
                    {
                        ShowError( "Configuration Error", "Invalid Group setting" );
                    }
                }
            }
        }

        private Person GetPerson()
        {
            using ( new UnitOfWorkScope() )
            {
                var personService = new PersonService();

                var personMatches = personService.GetByEmail( txtEmail.Text ).Where( p =>
                    p.LastName.Equals( txtLastName.Text, StringComparison.OrdinalIgnoreCase ) &&
                    ( ( p.GivenName != null && p.GivenName.Equals( txtFirstName.Text, StringComparison.OrdinalIgnoreCase ) ) ||
                    ( p.NickName != null && p.NickName.Equals( txtFirstName.Text, StringComparison.OrdinalIgnoreCase ) ) ) );

                if ( personMatches.Count() == 1 )
                {
                    return personMatches.FirstOrDefault();
                }
                else
                {
                    Person person = new Person();
                    person.GivenName = txtFirstName.Text;
                    person.LastName = txtLastName.Text;
                    person.Email = txtEmail.Text;

                    // Create Family Role
                    var groupMember = new GroupMember();
                    groupMember.Person = person;
                    groupMember.GroupRole = new GroupTypeRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) );

                    // Create Family
                    var group = new Group();
                    group.Members.Add( groupMember );
                    group.Name = person.LastName + " Family";
                    group.GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;

                    // Save person/family
                    var groupService = new GroupService();
                    groupService.Add( group, CurrentPersonId );
                    groupService.Save( group, CurrentPersonId );

                    return personService.Get( person.Id );
                }
            }
        }

        private void ShowError( string title, string text )
        {
            nbError.Title = title;
            nbError.Text = text;
            nbError.Visible = true;
        }

        private void ShowSuccess( string text )
        {
            pnlInputInfo.Visible = false;
            pnlSuccess.Visible = true;
            nbSuccess.Text = text;
        }
    }

}
