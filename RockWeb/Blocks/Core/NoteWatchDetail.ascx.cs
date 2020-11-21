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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
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
    [DisplayName( "Note Watch Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a note watch." )]

    [EntityTypeField( "Entity Type",
        Description = "Set an Entity Type to limit this block to Note Types and Entities for a specific entity type.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EntityType )]

    [NoteTypeField( "Note Type",
        Description = "Set Note Type to limit this block to a specific note type",
        AllowMultiple = false,
        Order = 1,
        Key = AttributeKey.NoteType )]

    // Context Aware will limit the Watcher Option to the Person or Group context (when a new watch is added)
    [ContextAware( typeof( Rock.Model.Group ), typeof( Rock.Model.Person ) )]
    public partial class NoteWatchDetail : RockBlock, IDetailBlock
    {
        public static class AttributeKey
        {
            public const string EntityType = "EntityType";
            public const string NoteType = "NoteType";
        }

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
                LoadDropDowns();
                int? noteWatchId = PageParameter( "NoteWatchId" ).AsIntegerOrNull();
                if ( noteWatchId.HasValue )
                {
                    ShowDetail( noteWatchId.Value );
                }
                else
                {
                    this.Visible = false;
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
            // TODO
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            NoteWatch noteWatch;

            var rockContext = new RockContext();
            var noteWatchService = new NoteWatchService( rockContext );
            var noteWatchId = hfNoteWatchId.Value.AsInteger();

            if ( noteWatchId == 0 )
            {
                noteWatch = new NoteWatch();
                noteWatchService.Add( noteWatch );
            }
            else
            {
                noteWatch = noteWatchService.Get( noteWatchId );
            }

            noteWatch.NoteTypeId = ddlNoteType.SelectedValue.AsIntegerOrNull();
            noteWatch.EntityTypeId = etpEntityType.SelectedEntityTypeId;

            if ( noteWatch.EntityTypeId.HasValue )
            {
                if ( noteWatch.EntityTypeId.Value == EntityTypeCache.GetId<Rock.Model.Person>() )
                {
                    noteWatch.EntityId = ppWatchedPerson.PersonId;
                }
                else if ( noteWatch.EntityTypeId.Value == EntityTypeCache.GetId<Rock.Model.Group>() )
                {
                    noteWatch.EntityId = gpWatchedGroup.GroupId;
                }
                else
                {
                    noteWatch.EntityId = nbWatchedEntityId.Text.AsIntegerOrNull();
                }
            }

            noteWatch.WatcherPersonAliasId = ppWatcherPerson.PersonAliasId;
            noteWatch.WatcherGroupId = gpWatcherGroup.GroupId;
            noteWatch.IsWatching = cbIsWatching.Checked;
            noteWatch.AllowOverride = cbAllowOverride.Checked;

            // see if the Watcher parameters are valid
            if ( !noteWatch.IsValidWatcher )
            {
                nbWatcherMustBeSelectWarning.Visible = true;
                return;
            }

            // see if the Watch filters parameters are valid
            if ( !noteWatch.IsValidWatchFilter )
            {
                nbWatchFilterMustBeSeletedWarning.Visible = true;
                return;
            }

            if ( !noteWatch.IsValid )
            {
                return;
            }

            // See if there is a matching filter that doesn't allow overrides
            if ( noteWatch.IsWatching == false )
            {
                if ( !noteWatch.IsAbleToUnWatch( rockContext ) )
                {
                    var nonOverridableNoteWatch = noteWatch.GetNonOverridableNoteWatches( rockContext ).FirstOrDefault();
                    if ( nonOverridableNoteWatch != null )
                    {
                        string otherNoteWatchLink;
                        if ( nonOverridableNoteWatch.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                        {
                            var otherNoteWatchWatchPageReference = new Rock.Web.PageReference( this.CurrentPageReference );
                            otherNoteWatchWatchPageReference.QueryString = new System.Collections.Specialized.NameValueCollection( otherNoteWatchWatchPageReference.QueryString );
                            otherNoteWatchWatchPageReference.QueryString["NoteWatchId"] = nonOverridableNoteWatch.Id.ToString();
                            otherNoteWatchLink = string.Format( "<a href='{0}'>note watch</a>", otherNoteWatchWatchPageReference.BuildUrl() );
                        }
                        else
                        {
                            otherNoteWatchLink = "note watch";
                        }

                        nbUnableToOverride.Text = string.Format(
                            "Unable to set Watching to false. This would override another {0} that doesn't allow overrides.",
                            otherNoteWatchLink );

                        nbUnableToOverride.Visible = true;
                        return;
                    }
                }
            }

            // see if the NoteType allows following
            if ( noteWatch.NoteTypeId.HasValue )
            {
                var noteTypeCache = NoteTypeCache.Get( noteWatch.NoteTypeId.Value );
                if ( noteTypeCache != null )
                {
                    if ( noteTypeCache.AllowsWatching == false )
                    {
                        nbNoteTypeWarning.Visible = true;
                        return;
                    }
                }
            }

            rockContext.SaveChanges();
            NavigateToNoteWatchParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToNoteWatchParentPage();
        }

        /// <summary>
        /// Navigates to note watch parent page.
        /// </summary>
        private void NavigateToNoteWatchParentPage()
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            var contextPerson = ContextEntity<Person>();
            var contextGroup = ContextEntity<Group>();
            if ( contextPerson != null )
            {
                queryParams.Add( "PersonId", contextPerson.Id.ToString() );
            }

            if ( contextGroup != null )
            {
                queryParams.Add( "GroupId", contextGroup.Id.ToString() );
            }

            NavigateToParentPage( queryParams );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the etpEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void etpEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadNoteTypeDropDown( etpEntityType.SelectedEntityTypeId );
            ShowEntityPicker( etpEntityType.SelectedEntityTypeId );
        }

        /// <summary>
        /// Handles the SelectItem event of the gpWatchedGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpWatchedGroup_SelectItem( object sender, EventArgs e )
        {
            // enable the EntityType picker if a specific entity is no longer selected
            etpEntityType.Enabled = gpWatchedGroup.GroupId == null;
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppWatchedPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppWatchedPerson_SelectPerson( object sender, EventArgs e )
        {
            // enable the EntityType picker if a specific entity is no longer selected
            etpEntityType.Enabled = ppWatchedPerson.PersonId == null;
        }

        /// <summary>
        /// Handles the TextChanged event of the nbWatchedEntityId control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void nbWatchedEntityId_TextChanged( object sender, EventArgs e )
        {
            // enable the EntityType picker if a specific entity is no longer selected
            int? entityId = nbWatchedEntityId.Text.AsIntegerOrNull();
            etpEntityType.Enabled = entityId == null;
            lWatchedEntityName.Visible = entityId != null;
            if ( entityId.HasValue && etpEntityType.SelectedEntityTypeId.HasValue )
            {
                var watchedEntity = new EntityTypeService( new RockContext() ).GetEntity( etpEntityType.SelectedEntityTypeId.Value, entityId.Value );
                if ( watchedEntity != null )
                {
                    lWatchedEntityName.Text = watchedEntity.ToString();
                }
                else
                {
                    lWatchedEntityName.Text = string.Format( "<span class='label label-danger'>{0} with Id {1} not found</span>", EntityTypeCache.Get( etpEntityType.SelectedEntityTypeId.Value ).FriendlyName, entityId );
                }
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbIsWatching control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbIsWatching_CheckedChanged( object sender, EventArgs e )
        {
            cbAllowOverride.Visible = cbIsWatching.Checked;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            etpEntityType.EntityTypes = new EntityTypeService( new RockContext() ).GetEntities().OrderBy( a => a.FriendlyName ).ToList();
        }

        /// <summary>
        /// Loads the note type drop down.
        /// </summary>
        public void LoadNoteTypeDropDown( int? entityTypeId )
        {
            ddlNoteType.Items.Clear();
            ddlNoteType.Items.Add( new ListItem() );
            if ( entityTypeId.HasValue )
            {
                var entityNoteTypes = NoteTypeCache.GetByEntity( entityTypeId.Value, null, null, true );
                ddlNoteType.Items.AddRange( entityNoteTypes.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );
            }

            ddlNoteType.Visible = entityTypeId.HasValue;
        }

        /// <summary>
        /// Shows the entity picker.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        public void ShowEntityPicker( int? entityTypeId )
        {
            ppWatchedPerson.Visible = false;
            gpWatchedGroup.Visible = false;
            pnlWatchedEntityGeneric.Visible = false;
            if ( entityTypeId.HasValue )
            {
                if ( entityTypeId.Value == EntityTypeCache.GetId<Rock.Model.Person>() )
                {
                    ppWatchedPerson.Visible = true;
                }
                else if ( entityTypeId.Value == EntityTypeCache.GetId<Rock.Model.Group>() )
                {
                    gpWatchedGroup.Visible = true;
                }
                else
                {
                    pnlWatchedEntityGeneric.Visible = true;
                }
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="noteWatchId">The note watch identifier.</param>
        public void ShowDetail( int noteWatchId )
        {
            pnlView.Visible = true;
            var rockContext = new RockContext();

            // Load depending on Add(0) or Edit
            NoteWatch noteWatch = null;
            if ( noteWatchId > 0 )
            {
                noteWatch = new NoteWatchService( rockContext ).Get( noteWatchId );
                lActionTitle.Text = ActionTitle.Edit( NoteWatch.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( noteWatch, ResolveRockUrl( "~" ) );
            }

            pdAuditDetails.Visible = noteWatch != null;

            var contextPerson = ContextEntity<Person>();
            var contextGroup = ContextEntity<Group>();

            if ( noteWatch == null )
            {
                noteWatch = new NoteWatch { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( NoteWatch.FriendlyTypeName ).FormatAsHtmlTitle();

                if ( contextPerson != null )
                {
                    noteWatch.WatcherPersonAliasId = contextPerson.PrimaryAliasId;
                    noteWatch.WatcherPersonAlias = contextPerson.PrimaryAlias;
                }
                else if ( contextGroup != null )
                {
                    noteWatch.WatcherGroupId = contextGroup.Id;
                    noteWatch.WatcherGroup = contextGroup;
                }
            }

            if ( contextPerson != null )
            {
                ppWatcherPerson.Enabled = false;
                gpWatcherGroup.Visible = false;

                // make sure we are seeing details for a NoteWatch that the current person is watching
                if ( !noteWatch.WatcherPersonAliasId.HasValue || !contextPerson.Aliases.Any( a => a.Id == noteWatch.WatcherPersonAliasId.Value ) )
                {
                    // The NoteWatchId in the url isn't a NoteWatch for the PersonContext, so just hide the block
                    pnlView.Visible = false;
                }
            }
            else if ( contextGroup != null )
            {
                ppWatcherPerson.Visible = false;
                gpWatcherGroup.Enabled = false;

                // make sure we are seeing details for a NoteWatch that the current group context is watching
                if ( !noteWatch.WatcherGroupId.HasValue || !( contextGroup.Id != noteWatch.WatcherGroupId ) )
                {
                    // The NoteWatchId in the url isn't a NoteWatch for the GroupContext, so just hide the block
                    pnlView.Visible = false;
                }
            }

            hfNoteWatchId.Value = noteWatchId.ToString();

            etpEntityType.SetValue( noteWatch.EntityTypeId );
            LoadNoteTypeDropDown( noteWatch.EntityTypeId );

            ddlNoteType.SetValue( noteWatch.NoteTypeId );

            if ( noteWatch.WatcherPersonAlias != null )
            {
                ppWatcherPerson.SetValue( noteWatch.WatcherPersonAlias.Person );
            }
            else
            {
                ppWatcherPerson.SetValue( ( Person ) null );
            }

            gpWatcherGroup.SetValue( noteWatch.WatcherGroup );

            cbIsWatching.Checked = noteWatch.IsWatching;
            cbIsWatching_CheckedChanged( null, null );

            cbAllowOverride.Checked = noteWatch.AllowOverride;

            ShowEntityPicker( etpEntityType.SelectedEntityTypeId );

            etpEntityType.Enabled = true;
            if ( noteWatch.EntityTypeId.HasValue && noteWatch.EntityId.HasValue )
            {
                var watchedEntityTypeId = noteWatch.EntityTypeId;
                IEntity watchedEntity = new EntityTypeService( rockContext ).GetEntity( noteWatch.EntityTypeId.Value, noteWatch.EntityId.Value );

                if ( watchedEntity != null )
                {
                    if ( watchedEntity is Rock.Model.Person )
                    {
                        ppWatchedPerson.SetValue( watchedEntity as Rock.Model.Person );
                    }
                    else if ( watchedEntity is Rock.Model.Group )
                    {
                        gpWatchedGroup.SetValue( watchedEntity as Rock.Model.Group );
                    }
                    else
                    {
                        lWatchedEntityName.Text = watchedEntity.ToString();
                        nbWatchedEntityId.Text = watchedEntity.Id.ToString();
                    }

                    // Don't let the EntityType get changed if there is a specific Entity getting watched
                    etpEntityType.Enabled = false;
                }
            }

            lWatchedNote.Visible = false;
            if ( noteWatch.Note != null )
            {
                var mergefields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                mergefields.Add( "Note", noteWatch.Note );
                var lavaTemplate = this.GetAttributeValue( "WatchedNoteLavaTemplate" );

                lWatchedNote.Text = lavaTemplate.ResolveMergeFields( mergefields );
            }
        }

        #endregion
    }
}