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
using Rock.Web.UI;

namespace RockWeb.Plugins.com_centralaz.DpsMatch
{
    /// <summary>
    /// Block that takes a personId and text and sets up a new note of the configured type to be added to the person's record.
    /// </summary>
    [DisplayName( "DPS Simple Add Note To Person" )]
    [Category( "com_centralaz > DpsMatch" )]
    [Description( "Block that takes a personId and text and sets up a new note of the configured type to be added to the person's record." )]
    [TextField( "Note Text", "The text for the alert note that is placed on the person's timeline when you have a possible match that needs more data on the Rock person record.", true, "Same name to registered sex offender. Verify DOB, Address and photo before serving. See security director if you have questions." )]
    [NoteTypeField( "Alert Note Type", "The alert note type you use for noting a possible match that needs more data on the Rock person record. The last note of that type which is an Alert will be shown.", false, "Rock.Model.Person", defaultValue: "66A1B9D7-7EFA-40F3-9415-E54437977D60" )]
    public partial class SimpleAddNoteToPerson : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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

            DialogPage dialogPage = this.Page as DialogPage;
            if ( dialogPage != null )
            {
                dialogPage.OnSave += new EventHandler<EventArgs>( btnSave_Click );
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
                var personId = PageParameter( "personId" ).AsIntegerOrNull();
                var text = PageParameter( "text" ).ToStringSafe();
                if ( personId != null )
                {
                    var person = new PersonService( new RockContext() ).Get( personId.Value );
                    if ( person != null )
                    {
                        hlblPersonName.Text = person.FullNameFormal;
                    }
                }

                tbNoteText.Text = text;
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

        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var noteService = new NoteService( rockContext );
            var noteType = new NoteTypeService( rockContext ).Get( GetAttributeValue( "AlertNoteType" ).AsGuid() );
            if ( noteType != null )
            {
                Note note = new Note();
                note.NoteTypeId = noteType.Id;
                note.EntityId = PageParameter( "PersonId" ).AsInteger();
                note.IsAlert = true;
                note.Text = String.Format( tbNoteText.Text );
                noteService.Add( note );
                rockContext.SaveChanges();

                string script = "if (typeof window.parent.Rock.controls.modal.close === 'function') window.parent.Rock.controls.modal.close('PAGE_UPDATED');";
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
            }
        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
    }
}