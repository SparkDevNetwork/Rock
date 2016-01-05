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
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Core
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Note Entry from Defined Type " )]
    [Category( "CCV > Core" )]
    [Description( "Context aware block for adding predefined notes to an entity" )]

    [ContextAware]
    [DefinedTypeField( "DefinedType", "Defined Type to get the values from. Values can contain Lava fields.", true )]
    [BooleanField( "Show History", "Show list of notes", true )]
    [TextField( "History Title", "", false, "History" )]
    [BooleanField( "Enable Debug", "Show lava merge fields.", false, "" )]
    public partial class NoteFromDefinedType : Rock.Web.UI.RockBlock
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

            noteList.Visible = this.GetAttributeValue( "ShowHistory" ).AsBooleanOrNull() ?? true;
            noteList.Title = this.GetAttributeValue( "HistoryTitle" );
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
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            if ( CurrentPerson != null )
            {
                mergeFields.Add( "CurrentPerson", CurrentPerson );
            }

            ddlNoteType.Items.Clear();
            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                var noteTypes = NoteTypeCache.GetByEntity( contextEntity.TypeId, string.Empty, string.Empty, true );
                foreach ( var noteType in noteTypes.OrderBy( a => a.Name ) )
                {
                    ddlNoteType.Items.Add( new ListItem( noteType.Name, noteType.Id.ToString() ) );
                }

                noteList.NoteTypes = noteTypes;
                noteList.EntityId = contextEntity.Id;

                ddlNoteType.Visible = ddlNoteType.Items.Count > 1;

                mergeFields.Add( "Context", contextEntity );
            }

            var definedTypeGuid = this.GetAttributeValue( "DefinedType" ).AsGuidOrNull();
            if ( definedTypeGuid.HasValue )
            {
                var definedType = DefinedTypeCache.Read( definedTypeGuid.Value );
                foreach ( var definedValue in definedType.DefinedValues )
                {
                    ddlNoteValueList.Items.Add( new ListItem( definedValue.Value.ResolveMergeFields( mergeFields ), definedValue.Id.ToString() ) );
                }
            }

            ddlNoteValueList.Items.Add( new ListItem( "Other" ) );

            ddlNoteValueList_SelectedIndexChanged( null, null );

            if ( this.GetAttributeValue( "EnableDebug" ).AsBoolean() )
            {
                lLavaDebug.Text = mergeFields.lavaDebugInfo();
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
            LoadDropDowns();
        }

        /// <summary>
        /// Handles the Click event of the btnAddNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddNote_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var noteService = new NoteService( rockContext );
            var note = new Note();

            if ( ddlNoteValueList.SelectedValue != "Other" )
            {
                note.Text = ddlNoteValueList.SelectedItem.Text;
            }
            else
            {
                note.Text = tbOtherText.Text;
            }

            var entity = this.ContextEntity();

            note.NoteTypeId = ddlNoteType.SelectedValue.AsInteger();
            note.EntityId = entity.Id;
            noteService.Add( note );
            rockContext.SaveChanges();

            noteList.RebuildNotes( false );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlNoteValueList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlNoteValueList_SelectedIndexChanged( object sender, EventArgs e )
        {
            tbOtherText.Visible = ddlNoteValueList.SelectedValue == "Other";
        }

        #endregion
    }
}