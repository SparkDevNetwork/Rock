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
    [Description( "Block for adding predefined notes to a person" )]

    [ContextAware]
    [DefinedTypeField( "DefinedType", "Defined Type to get the values from. Values can contain Lava fields.", true )]
    [BooleanField( "Show History", "Show list of notes", true )]
    [TextField( "History Title", "", false, "History" )]
    [BooleanField( "Enable Debug", "Show lava merge fields.", false, "" )]
    [NoteTypeField( "Note Type", "The Person Note Type", false, "Rock.Model.Person" )]
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

            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                if ( contextEntity is Rock.Model.GroupMember )
                {
                    contextEntity = ( contextEntity as Rock.Model.GroupMember ).Person;
                }

                var noteTypeGuid = this.GetAttributeValue( "NoteType" ).AsGuidOrNull();
                if ( noteTypeGuid.HasValue )
                {
                    var noteType = NoteTypeCache.Read( noteTypeGuid.Value );
                    if ( noteType != null )
                    {
                        hfNoteTypeId.Value = noteType.Id.ToString();
                        var noteTypeList = new List<NoteTypeCache>();
                        noteTypeList.Add( noteType );
                        noteList.NoteTypes = noteTypeList;
                    }
                }

                noteList.EntityId = contextEntity.Id;
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
            if ( entity is Rock.Model.GroupMember )
            {
                entity = ( entity as Rock.Model.GroupMember ).Person;
            }

            note.NoteTypeId = hfNoteTypeId.Value.AsInteger();
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