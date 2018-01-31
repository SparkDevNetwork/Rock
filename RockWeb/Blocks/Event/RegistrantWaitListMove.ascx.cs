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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data.Entity;
using Humanizer;
using Rock.Communication;
using System.Text;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Moves the person from the wait list to be a full registrant.
    /// </summary>
    [DisplayName( "Registrant Wait List Move" )]
    [Category( "Event" )]
    [Description( "Moves the person from the wait list to be a full registrant." )]
    public partial class RegistrantWaitListMove : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private RockContext _rockContext = new RockContext();
        private List<RegistrationRegistrant> _registrants = null;
        private Registration _firstRegistration = null;
        private RegistrationTemplate _template = null;

        #endregion

        #region Base Control Methods

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

            var entitySetId = PageParameter( "WaitListSetId" ).AsIntegerOrNull();
            if ( entitySetId.HasValue )
            {
                // get the registrant Ids
                var registrantIds = new EntitySetItemService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( i => i.EntitySetId == entitySetId )
                    .Select( i => i.EntityId )
                    .ToList();

                // get the registrants
                _registrants = new RegistrationRegistrantService( _rockContext )
                    .Queryable()
                    .Where( r => registrantIds.Contains( r.Id ) )
                    .ToList();

                // get the first registration
                _firstRegistration = _registrants
                    .Where( r => r.Registration != null )
                    .Select( r => r.Registration )
                    .FirstOrDefault();

                // get the template
                _template = _registrants
                    .Where( r =>
                        r.Registration != null &&
                        r.Registration.RegistrationInstance != null &&
                        r.Registration.RegistrationInstance.RegistrationTemplate != null )
                    .Select( r => r.Registration.RegistrationInstance.RegistrationTemplate )
                    .FirstOrDefault();

                // Bind the grid
                rptRecipients.DataSource = _registrants
                    .GroupBy( r => r.Registration )
                    .Select( r => new RegistrationSummary {
                        Registration = r.Key,
                        Registrants = r.Key.Registrants.Where( g => registrantIds.Contains( g.Id ) ).ToList() } );
                rptRecipients.DataBind();
            }
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
                if ( _registrants != null )
                {
                    foreach ( var registrant in _registrants )
                    {
                        int? groupMemberId = AddRegistrantToGroup( registrant );
                        if ( groupMemberId.HasValue )
                        {
                            registrant.GroupMemberId = groupMemberId.Value;
                        }
                        registrant.OnWaitList = false;

                        _rockContext.SaveChanges();
                    }

                    nbUpdate.Text = string.Format( "<strong>Wait List Updated</strong> {0} {1} moved from the wait list.", 
                        "individuals".ToQuantity( _registrants.Count ), _registrants.Count == 1 ? "was" : "were" );

                    if ( _template != null )
                    {
                        Dictionary<string, object> mergeObjects = GetMergeObjects( _firstRegistration );

                        tbFromSubject.Text = _template.WaitListTransitionSubject.ResolveMergeFields( mergeObjects );
                        tbFromName.Text = _template.WaitListTransitionFromName.ResolveMergeFields( mergeObjects );
                        tbFromEmail.Text = _template.WaitListTransitionFromEmail.ResolveMergeFields( mergeObjects );
                        ceEmailMessage.Text = _template.WaitListTransitionEmailTemplate;
                        ifEmailPreview.Attributes["srcdoc"] = ceEmailMessage.Text.ResolveMergeFields( mergeObjects );

                        // needed to work in IE
                        ifEmailPreview.Src = "javascript: window.frameElement.getAttribute('srcdoc');";
                    }
                }
                else
                {
                    nbUpdate.NotificationBoxType = NotificationBoxType.Warning;
                    nbUpdate.Text = "Could not find an entity set in the query string.";
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglEmailBodyView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglEmailBodyView_CheckedChanged( object sender, EventArgs e )
        {
            if ( tglEmailBodyView.Checked )
            {
                ceEmailMessage.Visible = false;
                ifEmailPreview.Visible = true;

                if ( _firstRegistration != null )
                {
                    Dictionary<string, object> mergeObjects = GetMergeObjects( _firstRegistration );
                    ifEmailPreview.Attributes["srcdoc"] = ceEmailMessage.Text.ResolveMergeFields( mergeObjects );

                    // needed to work in IE
                    ifEmailPreview.Src = "javascript: window.frameElement.getAttribute('srcdoc');";
                }
            }
            else
            {
                ceEmailMessage.Visible = true;
                ifEmailPreview.Visible = false;
            }
        }

        protected void cbShowEmail_CheckedChanged( object sender, EventArgs e )
        {
            pnlEmail.Visible = cbShowEmail.Checked;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptRecipients_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var registrationSummary = (RegistrationSummary)e.Item.DataItem;

                StringBuilder recipientText = new StringBuilder();
                recipientText.Append( string.Format( "{0} {1} <small>({2})</small><br />"
                    , registrationSummary.Registration.FirstName
                    , registrationSummary.Registration.LastName
                    , registrationSummary.Registration.ConfirmationEmail ) );

                recipientText.Append( string.Format( "<small>Registrants: {0}</small>"
                    , string.Join( ", ", registrationSummary.Registrants.Select( r => r.Person.FullName ) ) ) );

                CheckBox cbEmailRecipient = (CheckBox)e.Item.FindControl( "cbEmailRecipient" );

                if ( cbEmailRecipient != null )
                {
                    cbEmailRecipient.Text = recipientText.ToString();
                    cbEmailRecipient.ClientIDMode = ClientIDMode.Static;
                    cbEmailRecipient.Attributes.Add( "Id", registrationSummary.Registration.Id.ToString() );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSendEmail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendEmail_Click( object sender, EventArgs e )
        {
            int sendCount = 0;

            foreach ( RepeaterItem repeaterItem in rptRecipients.Items )
            {
                CheckBox cbEmailRecipient = (CheckBox)repeaterItem.FindControl( "cbEmailRecipient" );
                if ( cbEmailRecipient != null && cbEmailRecipient.Checked )
                {
                    int? registrationId = cbEmailRecipient.Attributes["Id"].AsIntegerOrNull();
                    if ( registrationId.HasValue )
                    {
                        var registration = _registrants.Where( r => r.RegistrationId == registrationId ).Select( r => r.Registration ).FirstOrDefault();
                        var mergeObjects = GetMergeObjects( registration );

                        var emailMessage = new RockEmailMessage();
                        emailMessage.AdditionalMergeFields = mergeObjects;
                        emailMessage.FromEmail = tbFromEmail.Text;
                        emailMessage.FromName = tbFromName.Text;
                        emailMessage.Subject = tbFromSubject.Text;
                        emailMessage.AddRecipient( new RecipientData( registration.ConfirmationEmail, mergeObjects ) );
                        emailMessage.Message = ceEmailMessage.Text;
                        emailMessage.AppRoot = ResolveRockUrl( "~/" );
                        emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                        emailMessage.Send();

                        sendCount++;
                    }
                }
            }

            pnlSend.Visible = false;
            pnlComplete.Visible = true;
            nbResult.Text = string.Format( "Wait List Transition emails have been sent to {0}.", "individuals".ToQuantity( sendCount ) );
        }

        #endregion

        #region Methods

        private int? AddRegistrantToGroup( RegistrationRegistrant registrant )
        {
            if ( registrant.PersonAliasId.HasValue &&
                registrant.Registration != null &&
                registrant.Registration.Group != null &&
                registrant.Registration.Group.GroupType != null && _template != null )
            {
                var group = registrant.Registration.Group;

                var groupService = new GroupService( _rockContext );
                var personAliasService = new PersonAliasService( _rockContext );
                var groupMemberService = new GroupMemberService( _rockContext );

                var personAlias = personAliasService.Get( registrant.PersonAliasId.Value );
                GroupMember groupMember = group.Members.Where( m => m.PersonId == personAlias.PersonId ).FirstOrDefault();
                if ( groupMember == null )
                {
                    groupMember = new GroupMember();
                    groupMemberService.Add( groupMember );
                    groupMember.GroupId = group.Id;
                    groupMember.PersonId = personAlias.PersonId;

                    if ( _template.GroupTypeId.HasValue &&
                        _template.GroupTypeId == group.GroupTypeId &&
                        _template.GroupMemberRoleId.HasValue )
                    {
                        groupMember.GroupRoleId = _template.GroupMemberRoleId.Value;
                        groupMember.GroupMemberStatus = _template.GroupMemberStatus;
                    }
                    else
                    {
                        if ( group.GroupType.DefaultGroupRoleId.HasValue )
                        {
                            groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId.Value;
                        }
                        else
                        {
                            groupMember.GroupRoleId = group.GroupType.Roles.Select( r => r.Id ).FirstOrDefault();
                        }
                    }
                }

                groupMember.GroupMemberStatus = _template.GroupMemberStatus;

                _rockContext.SaveChanges();

                return groupMember.Id;
            }

            return (int?)null;
        }

        private Dictionary<string, object> GetMergeObjects( Registration registration )
        {
            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "RegistrationInstance", registration.RegistrationInstance );
            mergeObjects.Add( "Registration", registration );
            mergeObjects.Add( "TransitionedRegistrants", _registrants.Where( r => r.RegistrationId == registration.Id ).ToList() );

            bool additionalFieldsNeeded = false;

            foreach ( var forms in registration.RegistrationInstance.RegistrationTemplate.Forms )
            {
                foreach ( var field in forms.Fields )
                {
                    if ( !field.ShowOnWaitlist )
                    {
                        additionalFieldsNeeded = true;
                        break;
                    }
                }
            }

            mergeObjects.Add( "AdditionalFieldsNeeded", additionalFieldsNeeded );

            return mergeObjects;
        }

        #endregion


        /// <summary>
        /// Summary Class for Registrants
        /// </summary>
        public class RegistrationSummary
        {
            /// <summary>
            /// Gets or sets the registration.
            /// </summary>
            /// <value>
            /// The registration.
            /// </value>
            public Registration Registration { get; set; }

            /// <summary>
            /// Gets or sets the registrants.
            /// </summary>
            /// <value>
            /// The registrants.
            /// </value>
            public List<RegistrationRegistrant> Registrants { get; set; }
        }
    }
}