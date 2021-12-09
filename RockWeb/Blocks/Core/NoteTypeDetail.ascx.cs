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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Web.Cache;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Note Type Detail" )]
    [Category( "Core" )]
    [Description( "Block for managing a note type" )]
    public partial class NoteTypeDetail : RockBlock
    {
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

            var entityTypes = new EntityTypeService( new RockContext() ).Queryable().Where( a => a.IsEntity ).AsNoTracking().OrderBy( t => t.FriendlyName ).ToList();

            epEntityType.EntityTypes = entityTypes;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                int? noteTypeId = this.PageParameter( "NoteTypeId" ).AsIntegerOrNull();
                if ( noteTypeId.HasValue )
                {
                    ShowDetail( noteTypeId.Value );
                }
            }

            base.OnLoad( e );
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
            //
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbAllowsReplies control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbAllowsReplies_CheckedChanged( object sender, EventArgs e )
        {
            nbMaxReplyDepth.Visible = cbAllowsReplies.Checked;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbAllowsAttachments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbAllowsAttachments_CheckedChanged( object sender, EventArgs e )
        {
            bftpAttachmentType.Visible = cbAllowsAttachments.Checked;
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
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            int noteTypeId = hfNoteTypeId.Value.AsInteger();

            var rockContext = new RockContext();
            var service = new NoteTypeService( rockContext );
            NoteType noteType = null;

            if ( noteTypeId != 0 )
            {
                noteType = service.Get( noteTypeId );
            }

            int entityTypeId = epEntityType.SelectedEntityTypeId ?? 0;
            if ( noteType == null )
            {
                int? maxNoteTypeOrderForEntity = service.Queryable().Where( t => t.EntityTypeId == entityTypeId ).Max( a => ( int? ) a.Order );

                noteType = new NoteType();
                noteType.Order = ( maxNoteTypeOrderForEntity ?? 0 ) + 1;
                service.Add( noteType );
            }

            noteType.Name = tbName.Text;
            noteType.EntityTypeId = entityTypeId;
            noteType.EntityTypeQualifierColumn = string.Empty;
            noteType.EntityTypeQualifierValue = string.Empty;
            
            noteType.IconCssClass = tbIconCssClass.Text;
            noteType.BackgroundColor = cpBackgroundColor.Text;
            noteType.FontColor = cpFontColor.Text;
            noteType.BorderColor = cpBorderColor.Text;

            noteType.UserSelectable = cbUserSelectable.Checked;
            noteType.RequiresApprovals = cbRequiresApprovals.Checked;
            noteType.SendApprovalNotifications = cbSendApprovalNotifications.Checked;
            noteType.AllowsWatching = cbAllowsWatching.Checked;
            noteType.AutoWatchAuthors = cbAutoWatchAuthors.Checked;

            noteType.AllowsReplies = cbAllowsReplies.Checked;
            noteType.MaxReplyDepth = nbMaxReplyDepth.Text.AsIntegerOrNull();

            noteType.AllowsAttachments = cbAllowsAttachments.Checked;
            noteType.BinaryFileTypeId = noteType.AllowsAttachments ? bftpAttachmentType.SelectedValueAsId() : null;

            noteType.ApprovalUrlTemplate = ceApprovalUrlTemplate.Text;

            if ( noteType.IsValid )
            {
                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="noteTypeId">The note type identifier.</param>
        public void ShowDetail( int noteTypeId )
        {
            NoteType noteType = null;
            bool showEntityTypePicker = true;

            var rockContext = new RockContext();
            EntityTypeCache entityType = null;

            if ( noteTypeId > 0 )
            {
                noteType = new NoteTypeService( rockContext ).Get( noteTypeId );
                lActionTitle.Text = ActionTitle.Edit( NoteType.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( noteType, ResolveRockUrl( "~" ) );
                entityType = EntityTypeCache.Get( noteType.EntityTypeId );
            }

            if ( noteType == null )
            {
                noteType = new NoteType();
                lActionTitle.Text = ActionTitle.Add( NoteType.FriendlyTypeName ).FormatAsHtmlTitle();
                // if an entityTypeId was specified in the URL, set the new noteType's entityTypeId
                int? entityTypeId = this.PageParameter( "EntityTypeId" ).AsIntegerOrNull();

                if ( entityTypeId.HasValue )
                {
                    noteType.EntityTypeId = entityTypeId.Value;
                    entityType = EntityTypeCache.Get( entityTypeId.Value );
                    showEntityTypePicker = false;
                    lActionTitle.Text = ActionTitle.Add( entityType.FriendlyName + " " + NoteType.FriendlyTypeName ).FormatAsHtmlTitle();
                }

                // hide the panel drawer that shows created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfNoteTypeId.Value = noteTypeId.ToString();

            tbName.Text = noteType.Name;
            
            if ( noteType.IsSystem )
            {
                showEntityTypePicker = false;
            }
            else
            {
                if ( noteType.Id > 0 )
                {
                    bool hasNotes = new NoteService( rockContext ).Queryable().Any( a => a.NoteTypeId == noteType.Id );
                    if ( hasNotes )
                    {
                        showEntityTypePicker = true;
                    }
                }
            }

            lEntityTypeReadOnly.Visible = !showEntityTypePicker;
            epEntityType.Visible = showEntityTypePicker;

            epEntityType.SelectedEntityTypeId = noteType.EntityTypeId;
            
            lEntityTypeReadOnly.Text = entityType != null ? entityType.FriendlyName : string.Empty;
            
            tbIconCssClass.Text = noteType.IconCssClass;
            cpBackgroundColor.Text = noteType.BackgroundColor;
            cpFontColor.Text = noteType.FontColor;
            cpBorderColor.Text = noteType.BorderColor;

            cbUserSelectable.Checked = noteType.UserSelectable;
            cbRequiresApprovals.Checked = noteType.RequiresApprovals;
            cbSendApprovalNotifications.Checked = noteType.SendApprovalNotifications;
            cbAllowsWatching.Checked = noteType.AllowsWatching;
            cbAutoWatchAuthors.Checked = noteType.AutoWatchAuthors;

            cbAllowsAttachments.Checked = noteType.AllowsAttachments;
            bftpAttachmentType.SetValue( noteType.BinaryFileTypeId );
            bftpAttachmentType.Visible = cbAllowsAttachments.Checked;

            cbAllowsReplies.Checked = noteType.AllowsReplies;
            nbMaxReplyDepth.Text = noteType.MaxReplyDepth.ToString();
            ceApprovalUrlTemplate.Text = noteType.ApprovalUrlTemplate;

            cbAllowsReplies_CheckedChanged( null, null );
        }

        #endregion
    }
}