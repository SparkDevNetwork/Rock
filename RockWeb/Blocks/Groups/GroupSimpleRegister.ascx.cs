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
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Block that will prompt for name and email, create a person if they don't exist, and add them to a group.
    /// </summary>
    [DisplayName( "Group Simple Register" )]
    [Category( "Groups" )]
    [Description( "Prompts for name and email, creates a person record if none exists, and adds the person to a group." )]

    [GroupField( "Group", "The group to add people to", true )]
    [TextField( "Save Button Text", "The text to use for the Save button", false, "Save" )]
    [TextField( "Success Message", "The message to display when user is succesfully added to the group", false, "Please check your email to verify your registration" )]
    [SystemEmailField( "Confirmation Email", "The email to send the person to confirm their registration.  If not specified, the user will not need to confirm their registration", false )]
    [LinkedPage( "Confirmation Page", "The page that user should be directed to to confirm their registration" )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2" )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49" )]
    public partial class GroupSimpleRegister : RockBlock
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

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
                var rockContext = new RockContext();
                var person = GetPerson( rockContext );
                if ( person != null )
                {
                    Guid? groupGuid = GetAttributeValue( "Group" ).AsGuidOrNull();

                    if ( groupGuid.HasValue )
                    {
                        var groupService = new GroupService( rockContext );
                        var groupMemberService = new GroupMemberService( rockContext );

                        var group = groupService.Get( groupGuid.Value );
                        if ( group != null && group.GroupType.DefaultGroupRoleId.HasValue )
                        {
                            string linkedPage = GetAttributeValue( "ConfirmationPage" );
                            if ( !string.IsNullOrWhiteSpace( linkedPage ) )
                            {
                                var member = group.Members.Where( m => m.PersonId == person.Id ).FirstOrDefault();

                                // If person has not registered or confirmed their registration
                                if ( member == null || member.GroupMemberStatus != GroupMemberStatus.Active )
                                {
                                    Guid confirmationEmailTemplateGuid = Guid.Empty;
                                    if ( !Guid.TryParse( GetAttributeValue( "ConfirmationEmail" ), out confirmationEmailTemplateGuid ) )
                                    {
                                        confirmationEmailTemplateGuid = Guid.Empty;
                                    }

                                    if ( member == null )
                                    {
                                        member = new GroupMember();
                                        member.GroupId = group.Id;
                                        member.PersonId = person.Id;
                                        member.GroupRoleId = group.GroupType.DefaultGroupRoleId.Value;

                                        // If a confirmation email is configured, set status to Pending otherwise set it to active
                                        member.GroupMemberStatus = confirmationEmailTemplateGuid != Guid.Empty ? GroupMemberStatus.Pending : GroupMemberStatus.Active;

                                        groupMemberService.Add( member );
                                        rockContext.SaveChanges();

                                        member = groupMemberService.Get( member.Id );
                                    }

                                    // Send the confirmation
                                    if ( confirmationEmailTemplateGuid != Guid.Empty )
                                    {
                                        var mergeObjects = GlobalAttributesCache.GetMergeFields( null );
                                        mergeObjects.Add( "Member", member );

                                        var pageParams = new Dictionary<string, string>();
                                        pageParams.Add( "gm", member.UrlEncodedKey );
                                        var pageReference = new Rock.Web.PageReference( linkedPage, pageParams );
                                        mergeObjects.Add( "ConfirmationPage", pageReference.BuildUrl() );

                                        var recipients = new List<RecipientData>();
                                        recipients.Add( new RecipientData( person.Email, mergeObjects ) );
                                        Email.Send( confirmationEmailTemplateGuid, recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );
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
                            ShowError( "Configuration Error", "The configured group does not exist, or it's group type does not have a default role configured." );
                        }
                    }
                    else
                    {
                        ShowError( "Configuration Error", "Invalid Group setting" );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Person GetPerson( RockContext rockContext )
        {
            var personService = new PersonService( rockContext );

            var personMatches = personService.GetByEmail( txtEmail.Text )
                .Where( p =>
                    p.LastName.Equals( txtLastName.Text, StringComparison.OrdinalIgnoreCase ) &&
                    ( ( p.FirstName != null && p.FirstName.Equals( txtFirstName.Text, StringComparison.OrdinalIgnoreCase ) ) ||
                        ( p.NickName != null && p.NickName.Equals( txtFirstName.Text, StringComparison.OrdinalIgnoreCase ) ) ) )
                .ToList();
            if ( personMatches.Count() == 1 )
            {
                return personMatches.FirstOrDefault();
            }
            else
            {
                DefinedValueCache dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                DefinedValueCache dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );

                Person person = new Person();
                person.FirstName = txtFirstName.Text;
                person.LastName = txtLastName.Text;
                person.Email = txtEmail.Text;
                person.IsEmailActive = true;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                if ( dvcConnectionStatus != null )
                {
                    person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                }

                if ( dvcRecordStatus != null )
                {
                    person.RecordStatusValueId = dvcRecordStatus.Id;
                }

                PersonService.SaveNewPerson( person, rockContext, null, false );

                return personService.Get( person.Id );
            }
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        private void ShowError( string title, string text )
        {
            nbError.Title = title;
            nbError.Text = text;
            nbError.Visible = true;
        }

        /// <summary>
        /// Shows the success.
        /// </summary>
        /// <param name="text">The text.</param>
        private void ShowSuccess( string text )
        {
            pnlInputInfo.Visible = false;
            pnlSuccess.Visible = true;
            nbSuccess.Text = text;
        }
    }
}
