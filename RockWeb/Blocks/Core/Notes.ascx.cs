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
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Context aware block for adding notes to an entity.
    /// </summary>
    [DisplayName( "Notes" )]
    [Category( "Core" )]
    [Description( "Context aware block for adding notes to an entity." )]

    [ContextAware]
    [TextField( "Note Type", "The note type name associated with the context entity to use (If it doesn't exist it will be created).", false, "Notes", "", 0 )]
    [TextField( "Heading", "The text to display as the heading.  If left blank, the Note Type name will be used.", false, "", "", 1 )]
    [TextField( "Heading Icon CSS Class", "The css class name to use for the heading icon. ", false, "fa fa-calendar", "", 2, "HeadingIcon" )]
    [TextField( "Note Term", "The term to use for note (i.e. 'Note', 'Comment').", false, "Note", "", 3 )]
    [CustomDropdownListField( "Display Type", "The format to use for displaying notes.", "Full,Light", true, "Full", "", 4 )]
    [BooleanField( "Use Person Icon", "", false, "", 5 )]
    [BooleanField( "Show Alert Checkbox", "", true, "", 6 )]
    [BooleanField( "Show Private Checkbox", "", true, "", 7 )]
    [BooleanField( "Show Security Button", "", true, "", 8 )]
    [BooleanField( "Allow Anonymous", "", false, "", 9 )]
    [BooleanField( "Add Always Visible", "Should the add entry screen always be visible (vs. having to click Add button to display the entry screen).", false, "", 10 )]
    [CustomDropdownListField( "Display Order", "Descending will render with entry field at top and most recent note at top.  Ascending will render with entry field at bottom and most recent note at the end.  Ascending will also disable the more option", "Ascending,Descending", true, "Descending", "", 11 )]
    public partial class Notes : RockBlock, ISecondaryBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                upNotes.Visible = true;

                string noteTypeName = GetAttributeValue( "NoteType" );

                using ( var rockContext = new RockContext() )
                {
                    var service = new NoteTypeService( rockContext );
                    var noteType = service.Get( contextEntity.TypeId, noteTypeName );

                    notesTimeline.NoteTypeId = noteType.Id;
                    notesTimeline.EntityId = contextEntity.Id;
                    notesTimeline.Title = GetAttributeValue( "Heading" );
                    if ( string.IsNullOrWhiteSpace( notesTimeline.Title ) )
                    {
                        notesTimeline.Title = noteType.Name;
                    }
                    notesTimeline.TitleIconCssClass = GetAttributeValue( "HeadingIcon" );
                    notesTimeline.Term = GetAttributeValue( "NoteTerm" );
                    notesTimeline.DisplayType = GetAttributeValue( "DisplayType" ) == "Light" ? NoteDisplayType.Light : NoteDisplayType.Full;
                    notesTimeline.UsePersonIcon = GetAttributeValue( "UsePersonIcon" ).AsBoolean();
                    notesTimeline.ShowAlertCheckBox = GetAttributeValue( "ShowAlertCheckbox" ).AsBoolean();
                    notesTimeline.ShowPrivateCheckBox = GetAttributeValue( "ShowPrivateCheckbox" ).AsBoolean();
                    notesTimeline.ShowSecurityButton = GetAttributeValue( "ShowSecurityButton" ).AsBoolean();
                    notesTimeline.AllowAnonymousEntry = GetAttributeValue( "Allow Anonymous" ).AsBoolean();
                    notesTimeline.AddAlwaysVisible = GetAttributeValue( "AddAlwaysVisible" ).AsBoolean();
                    notesTimeline.SortDirection = GetAttributeValue( "DisplayOrder" ) == "Ascending" ? ListSortDirection.Ascending : ListSortDirection.Descending;
                }
            }
            else
            {
                upNotes.Visible = false;
            }
        }

        #endregion

        #region Methods

        public void SetVisible( bool visible )
        {
            notesTimeline.Visible = visible;
        }

        #endregion

    }
}