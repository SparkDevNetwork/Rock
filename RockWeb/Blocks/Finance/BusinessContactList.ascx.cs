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
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Business Contact List" )]
    [Category( "Finance" )]
    [Description( "Displays the list of contacts for a business." )]

    [LinkedPage( "Person Profile Page",
        Description = "The page used to view the details of a business contact",
        Key = AttributeKey.PersonProfilePage,
        Order = 0 )]
    [Rock.SystemGuid.BlockTypeGuid( "E8F41C21-7D0F-41AC-B5D7-2BA3FA016CB4" )]
    public partial class BusinessContactList : RockBlock, ISecondaryBlock
    {
        private static class AttributeKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
        }

        #region Control Methods

        /// <inheritdoc />
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += BusinessDetail_BlockUpdated;

            bool canEdit = IsUserAuthorized( Authorization.EDIT );
            gContactList.DataKeyNames = new string[] { "Id" };
            gContactList.Actions.ShowAdd = canEdit;
            gContactList.Actions.AddClick += gContactList_AddClick;
            gContactList.GridRebind += gContactList_GridRebind;
            gContactList.IsDeleteEnabled = canEdit;

            mdAddContact.SaveClick += mdAddContact_SaveClick;
            mdAddContact.OnCancelScript = string.Format( "$('#{0}').val('');", hfModalOpen.ClientID );
        }

        /// <inheritdoc />
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                hfBusinessId.Value = PageParameter( "BusinessId" );
                BindContactListGrid();
            }

            if ( !string.IsNullOrWhiteSpace( hfModalOpen.Value ) )
            {
                mdAddContact.Show();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the PublicProfileEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BusinessDetail_BlockUpdated( object sender, EventArgs e )
        {
            BindContactListGrid();
        }

        /// <inheritdoc />
        public void SetVisible( bool visible )
        {
            pnlContactList.Visible = visible;
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the AddClick event of the gContactList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gContactList_AddClick( object sender, EventArgs e )
        {
            ppContact.SetValue( null );
            hfModalOpen.Value = "Yes";
            mdAddContact.Show();
        }

        /// <summary>
        /// Handles the Delete event of the gContactList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gContactList_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            int? businessId = hfBusinessId.Value.AsIntegerOrNull();
            if ( businessId.HasValue )
            {
                var businessContactId = e.RowKeyId;

                var rockContext = new RockContext();
                var groupMemberService = new GroupMemberService( rockContext );

                Guid businessContact = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT.AsGuid();
                Guid business = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS.AsGuid();
                Guid ownerGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();
                var groupMembers = groupMemberService
                    .Queryable()
                    .Where( m =>
                    (
                        // The contact person in the business's known relationships
                        m.PersonId == businessContactId &&
                        m.GroupRole.Guid.Equals( businessContact ) &&
                        m.Group.Members.Any( o => o.PersonId == businessId && o.GroupRole.Guid.Equals( ownerGuid ) )
                    ) ||
                    (
                        // The business in the person's know relationships
                        m.PersonId == businessId &&
                        m.GroupRole.Guid.Equals( business ) &&
                        m.Group.Members.Any( o => o.PersonId == businessContactId && o.GroupRole.Guid.Equals( ownerGuid ) )
                    ) );

                foreach ( var groupMember in groupMembers )
                {
                    groupMemberService.Delete( groupMember );
                }

                rockContext.SaveChanges();

                BindContactListGrid();
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gContactList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gContactList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queryParams = new Dictionary<string, string>
            {
                { "PersonId", e.RowKeyId.ToString() }
            };

            NavigateToLinkedPage( AttributeKey.PersonProfilePage, queryParams );
        }

        /// <summary>
        /// Handles the GridRebind event of the gContactList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gContactList_GridRebind( object sender, EventArgs e )
        {
            BindContactListGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddContact control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void mdAddContact_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var groupService = new GroupService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var business = personService.Get( int.Parse( hfBusinessId.Value ) );
            int? contactId = ppContact.PersonId;
            
            if ( contactId.HasValue && contactId.Value > 0 )
            {
                personService.AddContactToBusiness( business.Id, contactId.Value );
                rockContext.SaveChanges();
            }

            mdAddContact.Hide();
            hfModalOpen.Value = string.Empty;
            BindContactListGrid();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Binds the contact list grid.
        /// </summary>
        /// <param name="business">The business.</param>
        private void BindContactListGrid()
        {
            var businessId = hfBusinessId.ValueAsInt();
            Guid groupRoleBusinessGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS.AsGuid();
            Guid groupRoleOwnerGuidGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

            var personList = new GroupMemberService( new RockContext() )
                .Queryable()
                .Where( g => g.GroupRole.Guid.Equals( groupRoleBusinessGuid ) && g.PersonId == businessId )
                .SelectMany( g => g.Group.Members.Where( m => m.GroupRole.Guid.Equals( groupRoleOwnerGuidGuid ) ).Select( m => m.Person ) )
                .ToList();

            gContactList.DataSource = personList;
            gContactList.DataBind();
        }

        #endregion Internal Methods
    }
}