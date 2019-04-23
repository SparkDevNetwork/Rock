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
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.AlertNote
{

    /// <summary>
    /// 
    /// </summary>
    [Description( "Displays if the person has an alert note." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Alert Note" )]

    [NoteTypeField( "Note Types", "The note types you want to look for alerts on.", true, "Rock.Model.Person", "", "", true, Rock.SystemGuid.NoteType.PERSON_TIMELINE_NOTE, order: 0 )]
    [CodeEditorField( "Badge Content", "", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "<span class='label label-danger'>Alert Note Exists</span>", order: 1 )]
    public class AlertNote : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            List<Guid> noteTypes = new List<Guid>();

            if ( !string.IsNullOrEmpty( GetAttributeValue( badge, "NoteTypes" ) ) )
            {
                noteTypes = Array.ConvertAll( GetAttributeValue( badge, "NoteTypes" ).Split( ',' ), s => new Guid( s ) ).ToList();
            }

            var currentUser = UserLoginService.GetCurrentUser();
            int? currentPersonId = currentUser != null ? currentUser.PersonId : null;

            // check for alert note
            var alertNotesExist = new NoteService( new RockContext() ).Queryable().AsNoTracking()
                                .Where( n => noteTypes.Contains( n.NoteType.Guid )
                                        && n.EntityId.Value == Person.Id
                                        && n.IsAlert == true
                                        && ( !n.IsPrivateNote || n.CreatedByPersonAlias.PersonId == currentPersonId )
                                        )
                                .Any();

            if ( alertNotesExist )
            {
                writer.Write( GetAttributeValue( badge, "BadgeContent" ) );
            }
        }
    }
}
