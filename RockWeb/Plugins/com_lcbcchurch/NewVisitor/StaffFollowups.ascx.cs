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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Web.UI.WebControls;
using Rock.Web.Cache;
using Rock.Reporting;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_lcbcchurch.NewVisitor
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Staff Follow-ups" )]
    [Category( "LCBC > New Visitor" )]
    [Description( "This lists all the notes (for the configured type) of the past 16 weeks for the given staff person." )]

    #region Block Attributes
    [AttributeField( Rock.SystemGuid.EntityType.PERSON,
        "Engagement Begin Attribute",
        description: "Points to the attibute that holds the person's current engagement begin date.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKeys.EngagementBeginDateAttribute )]
    [KeyValueListField( "Connections",
        description: "The Key is the NoteTypeId and the Value is the icon to use for indicating completion.",
        keyPrompt: "NoteTypeId",
        valuePrompt: "Icon Css Class",
        IsRequired = true,
        Order = 1,
        Key = AttributeKeys.Connections )]
    # endregion Block Attributes
    public partial class StaffFollowups : Rock.Web.UI.RockBlock
    {
        #region Fields 

        private const int PERIOD_IN_DAYS = 112;

        #endregion Fields

        #region Attribute Keys

        protected static class AttributeKeys
        {
            public const string EngagementBeginDateAttribute = "EngagementBeginDateAttribute";
            public const string Connections = "Connections";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gNotes.DataKeyNames = new string[] { "Id" };
            gNotes.GridRebind += gNotes_GridRebind;
            gNotes.EntityTypeId = EntityTypeCache.Get( typeof( Note ) ).Id;
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

            if ( !IsPostBack )
            {
                bool isValid = IsBlockSettingValid();
                if ( isValid )
                {
                    int? personId = PageParameter( "PersonId" ).AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        hfPersonId.SetValue( personId.Value );
                        var person = new PersonService( new RockContext() ).Get( personId.Value );
                        if ( person != null )
                        {
                            lTitle.Text = person.FullName;
                        }
                        BindGrid();
                    }
                }
            }
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gNotes_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();

            var personId = hfPersonId.ValueAsInt();
            AttributeCache beginDateAttribute = AttributeCache.Get( GetAttributeValue( AttributeKeys.EngagementBeginDateAttribute ).AsGuid() );
            var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;
            var connections = GetKeyIconValues( AttributeKeys.Connections );

            var connectionIds = connections.Keys.Select( a => a.AsInteger() ).ToList();

            var notesQry = new NoteService( rockContext )
                .Queryable()
                .Where( a => connectionIds.Contains( a.NoteTypeId ) &&
                 a.CreatedByPersonAlias.PersonId == personId &&
                 a.EntityId.HasValue );

            var notes = notesQry.Select( a => new PersonNote
            {
                Id = a.Id,
                CreatedDateTime = a.CreatedDateTime,
                Text = a.Text,
                NoteTypeId = a.NoteTypeId,
                PersonId = a.EntityId.Value,
                NoteTypeName = a.NoteType.Name
            } ).ToList();

            var personIds = notes.Select( a => a.PersonId ).Distinct().ToList();
            var persons = new PersonService( rockContext ).GetByIds( personIds ).ToList();


            foreach ( var note in notes )
            {
                note.FullName = persons.Where( b => b.Id == note.PersonId ).Select( a => a.FullName ).FirstOrDefault();
                note.TypeIconCssClass = connections[note.NoteTypeId.ToString()];
            }

            SortProperty sortProperty = gNotes.SortProperty;
            if ( sortProperty != null )
            {
                gNotes.DataSource = notes.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gNotes.DataSource = notes
                    .OrderByDescending( r => r.CreatedDateTime )
                    .ToList();
            }
            gNotes.DataBind();
        }

        private bool IsBlockSettingValid()
        {
            bool isValid = false;

            var engagementBeginDateAttributeGuid = GetAttributeValue( AttributeKeys.EngagementBeginDateAttribute ).AsGuidOrNull();
            if ( !engagementBeginDateAttributeGuid.HasValue )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>block setting are not configured.</p>";
                nbWarningMessage.Visible = true;
                return isValid;
            }

            var engagementBeginDateAttribute = AttributeCache.Get( engagementBeginDateAttributeGuid.Value );
            if ( !( engagementBeginDateAttribute != null && engagementBeginDateAttribute.FieldTypeId == FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.DATE ) ).Id ) )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>Engagement begin date Attribute block setting expect attribute with date field type.</p>";
                nbWarningMessage.Visible = true;
                return isValid;
            }

            return true;
        }

        private Dictionary<string, string> GetKeyIconValues( string attributeKey )
        {
            var keyIconValues = new Dictionary<string, string>();
            var keyIconValuesString = GetAttributeValue( attributeKey );
            if ( !string.IsNullOrWhiteSpace( keyIconValuesString ) )
            {
                keyIconValuesString = keyIconValuesString.TrimEnd( '|' );
                foreach ( var keyVal in keyIconValuesString.Split( '|' )
                                .Select( s => s.Split( '^' ) )
                                .Where( s => s.Length == 2 ) )
                {
                    keyIconValues.AddOrIgnore( keyVal[0], keyVal[1] );
                }
            }

            return keyIconValues;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A class to store the person note
        /// </summary>
        public class PersonNote
        {
            /// <summary>
            /// Gets or sets the note identifier.
            /// </summary>
            /// <value>
            /// The note identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the NoteTypeId identifier.
            /// </summary>
            /// <value>
            /// The NoteTypeId identifier.
            /// </value>
            public int NoteTypeId { get; set; }

            /// <summary>
            /// Gets or sets the note type name.
            /// </summary>
            /// <value>
            /// The note type name.
            /// </value>
            public string NoteTypeName { get; set; }

            /// <summary>
            /// Gets or sets the Note Type Icon Css Class.
            /// </summary>
            /// <value>
            /// The Note Type Icon Css Class.
            /// </value>
            public string TypeIconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the created date time.
            /// </summary>
            /// <value>
            /// The created date time.
            /// </value>
            public DateTime? CreatedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the text/body of the note.
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> representing the text/body of the note.
            /// </value>
            public string Text { get; set; }

            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the fullname.
            /// </summary>
            /// <value>
            /// The fullname.
            /// </value>
            public string FullName { get; set; }
        }

        # endregion Helper Classes

    }
}