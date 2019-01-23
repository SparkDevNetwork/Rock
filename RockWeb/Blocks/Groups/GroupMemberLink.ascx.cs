// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Block that will add/update a person into a group as a group member and
    /// any given query string name-value parameters that are group member attributes
    /// will be updated on their group member record.
    /// Their status will be set as per the block setting and their role will 
    /// be the group type's default role.
    /// The person must be given via the "Person" parameter with a value of a UrlEncodedKey.
    /// </summary>
    [DisplayName( "Group Member Link" )]
    [Category( "Groups" )]
    [Description( "Block adds or updates a person into the configured group with the configured status and role, and sets group member attribute values that are given as name-value pairs in the querystring." )]
    [GroupField("Group", "The group this block will be adding or updating people into.", true )]
    [EnumField( "Group Member Status", "The group member status you want to set for the person.", typeof(GroupMemberStatus), true, "2" )]
    [CodeEditorField( "Success Message", "The text (HTML) to display when a person is successfully added to the group.", CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, @"<h1>You're in!</h1>
<p>You have been added to the group.</p>" )]
    [LinkedPage( "Success Page", "The page to redirect to if the person was registered successfully. (If set, this overrides the Success Message setting.)", false )]
    [TextField("Error Message", "The text to display when a valid person key is NOT provided", false, "There was a problem with your registration.  Please try to register again.")]
    public partial class GroupMemberLink : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                var groupGuid = GetAttributeValue( "Group" ).AsGuidOrNull();
                string personKey = PageParameter( "Person" );

                if ( !groupGuid.HasValue )
                {
                    ShowConfigError( "The 'Group' configuration for this block has not been set." );
                }
                else if ( string.IsNullOrWhiteSpace( personKey ) )
                {
                    ShowError( "Missing Parameter Value" );
                }
                else
                {
                    RockContext rockContext = new RockContext();
                    Person targetPerson = null;
                    targetPerson = new PersonService( rockContext ).GetByUrlEncodedKey( personKey );

                    if ( targetPerson == null )
                    {
                        ShowError( "We can't find a record in our system for you." );
                    }
                    else
                    {
                        GroupService groupService = new GroupService( rockContext );
                        Group group = groupService.Get( groupGuid.Value );
                        if ( group == null )
                        {
                            ShowConfigError( "The 'Group' configuration for this block is incorrect." );
                        }
                        else
                        {
                            AddOrUpdatePersonInGroup( targetPerson, group, rockContext );
                        }
                    }
                }
            }
            catch ( System.FormatException )
            {
                ShowError( "Something is wrong with that link.  Are you sure you copied it correctly?" );
            }
            catch ( SystemException ex )
            {
                ShowError( ex.Message );
            }
        }

        #endregion

        #region  Methods

        /// <summary>
        /// Adds or updates the given person into the given group.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        private void AddOrUpdatePersonInGroup( Person person, Group group, RockContext rockContext )
        {
            try
            {
                var groupMember = group.Members.Where( m => m.PersonId == person.Id ).FirstOrDefault();
                if ( groupMember == null )
                {
                    groupMember = new GroupMember();
                    groupMember.GroupId = group.Id;
                    groupMember.PersonId = person.Id;
                    groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId ?? 0;
                    group.Members.Add( groupMember );
                }
                var groupMemberStatus = GetAttributeValue( "GroupMemberStatus" );
                groupMember.GroupMemberStatus = (GroupMemberStatus)groupMemberStatus.AsInteger();
                rockContext.SaveChanges();

                // Now update any member attributes for the person...
                AddOrUpdateGroupMemberAttributes( person, group, rockContext );

                string successPage = GetAttributeValue( "SuccessPage" );
                if ( ! string.IsNullOrWhiteSpace( successPage ) )
                {
                    var pageReference = new Rock.Web.PageReference( successPage );
                    Response.Redirect( pageReference.BuildUrl(), false );
                    // this remaining stuff prevents .NET from quietly throwing ThreadAbortException
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
                else
                {
                    lSuccess.Visible = true;
                    lSuccess.Text = GetAttributeValue( "SuccessMessage" );
                }
            }
            catch ( Exception ex )
            {
                LogException( ex );
                nbMessage.Visible = true;
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Text = "Something went wrong and we could not save your request. If it happens again please contact our office at the number below.";
            }
        }

        /// <summary>
        /// Adds or update group member's attributes using any page parameters that are attributes of the group.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        private void AddOrUpdateGroupMemberAttributes( Person person, Group group, RockContext rockContext )
        {
            var groupMember = group.Members.Where( m => m.PersonId == person.Id ).FirstOrDefault();
            AttributeService attributeService = new AttributeService( rockContext );

            // Load all the group member attributes for comparison below.
            var attributes = attributeService.GetGroupMemberAttributesCombined( group.Id, group.GroupTypeId, true ).ToCacheAttributeList();

            // In order to add attributes to the person, you have to first load them all
            groupMember.LoadAttributes( rockContext );

            foreach ( var entry in PageParameters() )
            {
                // skip the parameter if the group's group type doesn't have that one
                var attribute = attributes.Where( a => a.Key.Equals( entry.Key, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
                if ( attribute == null )
                {
                    continue;
                }
                //attribute.SetAttributeValue
                groupMember.SetAttributeValue( entry.Key, (string) entry.Value );
                    
            }
            groupMember.SaveAttributeValues( rockContext );
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="errorDetail">The error detail.</param>
        private void ShowConfigError( string errorDetail = "" )
        {
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Title = "Configuration Error";

            if ( string.IsNullOrWhiteSpace( errorDetail ) )
            {
                nbMessage.Text = "This block is not ready to use yet. There is a problem with the configuration.";
            }
            else
            {
                nbMessage.Text = errorDetail;
            }
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="errorDetail">The error detail.</param>
        private void ShowError( string errorDetail = "" )
        {
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Title = "Sorry";

            if ( string.IsNullOrWhiteSpace( errorDetail ) )
            {
                nbMessage.Text = GetAttributeValue( "ErrorMessage" );
            }
            else
            {
                nbMessage.Text = string.Format( "{0} [{1}]", GetAttributeValue( "ErrorMessage" ), errorDetail );
            }
        }

        #endregion
    }
}
