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
using System.Linq;
using Rock.Web.Cache;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="DotLiquid.Drop" />
    public class NoteOptions : DotLiquid.Drop
    {
        /// <summary>
        /// Gets or sets the note view lava template.
        /// </summary>
        /// <value>
        /// The note view lava template.
        /// </value>
        public string NoteViewLavaTemplate { get; set; } = "{% include '~~/Assets/Lava/NoteViewList.lava' %}";

        /// <summary>
        /// Gets or sets the note type ids.
        /// </summary>
        /// <value>
        /// The note type ids.
        /// </value>
        private List<int> _noteTypeIds { get; set; } = new List<int>();

        /// <summary>
        /// Sets the note types.
        /// </summary>
        /// <param name="noteTypeList">The note type list.</param>
        public void SetNoteTypes( List<NoteTypeCache> noteTypeList )
        {
            this.NoteTypes = noteTypeList.ToArray();
        }

        /// <summary>
        /// Gets or sets the note types.
        /// </summary>
        /// <value>
        /// The note types.
        /// </value>
        public NoteTypeCache[] NoteTypes
        {
            get
            {
                return _noteTypeIds.Select( a => NoteTypeCache.Get( a ) ).ToArray();
            }

            set
            {
                _noteTypeIds = value.Select( a => a.Id ).ToList();
                NoteTypesChange?.Invoke( this, new EventArgs() );
            }
        }

        /// <summary>
        /// Gets the viewable note types.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        public List<NoteTypeCache> GetViewableNoteTypes( Person currentPerson )
        {
            return this.NoteTypes?.Where( a => a.IsAuthorized( Security.Authorization.VIEW, currentPerson ) ).ToList();
        }

        /// <summary>
        /// Gets the editable note types.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        public List<NoteTypeCache> GetEditableNoteTypes( Person currentPerson )
        {
            return this.NoteTypes?.Where( a => a.UserSelectable && a.IsAuthorized( Security.Authorization.EDIT, currentPerson ) ).ToList();
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [display note type heading].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display note type heading]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayNoteTypeHeading { get; set; }

        /// <summary>
        /// Gets or sets the display type.
        /// </summary>
        /// <value>
        /// The display type.
        /// </value>
        public NoteDisplayType DisplayType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show alert CheckBox].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show alert CheckBox]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAlertCheckBox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show private CheckBox].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show private CheckBox]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPrivateCheckBox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show security button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show security button]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSecurityButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show create date input].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show create date input]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCreateDateInput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use person icon].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use person icon]; otherwise, <c>false</c>.
        /// </value>
        public bool UsePersonIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [add always visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [add always visible]; otherwise, <c>false</c>.
        /// </value>
        public bool AddAlwaysVisible { get; set; }

        /// <summary>
        /// Gets or sets the note label (used to be called 'Term')
        /// </summary>
        /// <value>
        /// The note label.
        /// </value>
        public string NoteLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [expand replies].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [expand replies]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpandReplies { get; set; }

        /// <summary>
        /// Occurs when [note types change].
        /// </summary>
        public event EventHandler NoteTypesChange;
    }
}