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
using Rock.Constants;
using Rock.Data;
using Rock.Follow;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Follow
{
    /// <summary>
    /// Block for editing the following suggestion types.
    /// </summary>
    [DisplayName( "Suggestion Detail" )]
    [Category( "Follow" )]
    [Description( "Block for editing the following suggestion types." )]
    public partial class SuggestionDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        public int SuggestionId
        {
            get { return ViewState["SuggestionId"] as int? ?? 0; }
            set { ViewState["SuggestionId"] = value; }
        }

        public int? SuggestionEntityTypeId
        {
            get { return ViewState["SuggestionEntityTypeId"] as int?; }
            set { ViewState["SuggestionEntityTypeId"] = value; }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var followingSuggestion = new FollowingSuggestionType { Id = SuggestionId, EntityTypeId = SuggestionEntityTypeId };
            BuildDynamicControls( followingSuggestion, false );
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
                ShowDetail( PageParameter( "eventId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                FollowingSuggestionType followingSuggestion = null;

                var eventService = new Rock.Model.FollowingSuggestionTypeService( rockContext );

                if ( SuggestionId != 0 )
                {
                    followingSuggestion = eventService.Get( SuggestionId );
                }

                if ( followingSuggestion == null )
                {
                    followingSuggestion = new Rock.Model.FollowingSuggestionType();
                    eventService.Add( followingSuggestion );
                }

                followingSuggestion.Name = tbName.Text;
                followingSuggestion.IsActive = cbIsActive.Checked;
                followingSuggestion.Description = tbDescription.Text;
                followingSuggestion.EntityTypeId = cpSuggestionType.SelectedEntityTypeId;
                followingSuggestion.ReasonNote = tbReasonNote.Text;
                followingSuggestion.ReminderDays = nbReminderDays.Text.AsIntegerOrNull();
                followingSuggestion.EntityNotificationFormatLava = ceNotificationFormat.Text;

                rockContext.SaveChanges();

                followingSuggestion.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, followingSuggestion );
                followingSuggestion.SaveAttributeValues( rockContext );
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpSuggestionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpSuggestionType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var followingSuggestion = new FollowingSuggestionType { Id = SuggestionId, EntityTypeId = cpSuggestionType.SelectedEntityTypeId };
            BuildDynamicControls( followingSuggestion, true );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        public void ShowDetail( int eventId )
        {
            FollowingSuggestionType followingSuggestion = null;

            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            if ( !eventId.Equals( 0 ) )
            {
                followingSuggestion = new FollowingSuggestionTypeService( new RockContext() ).Get( eventId );
                editAllowed = editAllowed || followingSuggestion.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }

            if ( followingSuggestion == null )
            {
                followingSuggestion = new FollowingSuggestionType { Id = 0, IsActive = true };
            }

            SuggestionId = followingSuggestion.Id;

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FollowingSuggestionType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( followingSuggestion );
            }
            else
            {
                ShowEditDetails( followingSuggestion );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="event">The event.</param>
        private void ShowEditDetails( FollowingSuggestionType followingSuggestion )
        {
            if ( followingSuggestion.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( FollowingSuggestionType.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = followingSuggestion.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !followingSuggestion.IsActive;

            SetEditMode( true );

            tbName.Text = followingSuggestion.Name;
            cbIsActive.Checked = followingSuggestion.IsActive;
            tbDescription.Text = followingSuggestion.Description;
            cpSuggestionType.SetValue( followingSuggestion.EntityType != null ? followingSuggestion.EntityType.Guid.ToString().ToUpper() : string.Empty );
            tbReasonNote.Text = followingSuggestion.ReasonNote;
            nbReminderDays.Text = followingSuggestion.ReminderDays.HasValue ? followingSuggestion.ReminderDays.Value.ToString() : "";
            ceNotificationFormat.Text = followingSuggestion.EntityNotificationFormatLava;

            BuildDynamicControls( followingSuggestion, true );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="event">The event.</param>
        private void ShowReadonlyDetails( FollowingSuggestionType followingSuggestion )
        {
            SetEditMode( false );

            lActionTitle.Text = followingSuggestion.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !followingSuggestion.IsActive;
            lSuggestionDescription.Text = followingSuggestion.Description;

            DescriptionList descriptionList = new DescriptionList();

            if ( followingSuggestion.EntityType != null )
            {
                descriptionList.Add( "Suggestion Type", followingSuggestion.EntityType.Name );
            }

            descriptionList.Add( "Reason Note", followingSuggestion.ReasonNote );

            lblMainDetails.Text = descriptionList.Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        private void BuildDynamicControls( FollowingSuggestionType followingSuggestion, bool SetValues )
        {
            SuggestionEntityTypeId = followingSuggestion.EntityTypeId;
            if ( followingSuggestion.EntityTypeId.HasValue )
            {
                var SuggestionComponentEntityType = EntityTypeCache.Read( followingSuggestion.EntityTypeId.Value );
                var SuggestionEntityType = EntityTypeCache.Read( "Rock.Model.FollowingSuggestionType" );
                if ( SuggestionComponentEntityType != null && SuggestionEntityType != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        Rock.Attribute.Helper.UpdateAttributes( SuggestionComponentEntityType.GetEntityType(), SuggestionEntityType.Id, "EntityTypeId", SuggestionComponentEntityType.Id.ToString(), rockContext );
                        followingSuggestion.LoadAttributes( rockContext );
                    }
                }
            }

            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( followingSuggestion, phAttributes, SetValues, BlockValidationGroup, new List<string> { "Active", "Order" } );
        }

        #endregion
    }
}