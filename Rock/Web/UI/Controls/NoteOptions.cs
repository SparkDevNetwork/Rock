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
using System.Web.UI;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="DotLiquid.Drop" />
    public class NoteOptions : DotLiquid.Drop
    {
        private StateBag _containerViewState;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOptions"/> class.
        /// </summary>
        [RockObsolete( "1.8" )]
        [Obsolete( " Use NoteOptions( NoteContainer noteContainer ) instead" )]
        public NoteOptions() : this( null )
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOptions"/> class.
        /// </summary>
        /// <param name="noteContainer">The note container.</param>
        public NoteOptions( NoteContainer noteContainer )
        {
            _containerViewState = noteContainer?.ContainerViewState ?? new StateBag();
            _noteTypeIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets the note view lava template.
        /// </summary>
        /// <value>
        /// The note view lava template.
        /// </value>
        public string NoteViewLavaTemplate
        {
            get
            {
                return _containerViewState["NoteOptions.NoteViewLavaTemplate"] as string ?? "{% include '~~/Assets/Lava/NoteViewList.lava' %}";
            }

            set
            {
                _containerViewState["NoteOptions.NoteViewLavaTemplate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the note type ids.
        /// </summary>
        /// <value>
        /// The note type ids.
        /// </value>
        private List<int> _noteTypeIds
        {
            get
            {
                return _containerViewState["NoteOptions._noteTypeIds"] as List<int>;
            }

            set
            {
                _containerViewState["NoteOptions._noteTypeIds"] = value;
            }
        }

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
        /// Gets the editable note types sorted by order,name
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        public List<NoteTypeCache> GetEditableNoteTypes( Person currentPerson )
        {
            return this.NoteTypes?.Where( a => a.UserSelectable && a.IsAuthorized( Security.Authorization.EDIT, currentPerson ) ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId
        {
            get
            {
                return _containerViewState["NoteOptions.EntityId"] as int?;
            }

            set
            {
                _containerViewState["NoteOptions.EntityId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display note type heading].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display note type heading]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayNoteTypeHeading
        {
            get
            {
                return _containerViewState["NoteOptions.DisplayNoteTypeHeading"] as bool? ?? false;
            }

            set
            {
                _containerViewState["NoteOptions.DisplayNoteTypeHeading"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the display type.
        /// </summary>
        /// <value>
        /// The display type.
        /// </value>
        public NoteDisplayType DisplayType
        {
            get
            {
                return _containerViewState["NoteOptions.DisplayType"] as NoteDisplayType? ?? NoteDisplayType.Full;
            }

            set
            {
                _containerViewState["NoteOptions.DisplayType"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show alert CheckBox].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show alert CheckBox]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAlertCheckBox
        {
            get
            {
                return _containerViewState["NoteOptions.ShowAlertCheckBox"] as bool? ?? false;
            }

            set
            {
                _containerViewState["NoteOptions.ShowAlertCheckBox"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show private CheckBox].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show private CheckBox]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPrivateCheckBox
        {
            get
            {
                return _containerViewState["NoteOptions.ShowPrivateCheckBox"] as bool? ?? false;
            }

            set
            {
                _containerViewState["NoteOptions.ShowPrivateCheckBox"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show security button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show security button]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSecurityButton
        {
            get
            {
                return _containerViewState["NoteOptions.ShowSecurityButton"] as bool? ?? false;
            }

            set
            {
                _containerViewState["NoteOptions.ShowSecurityButton"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show create date input].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show create date input]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCreateDateInput
        {
            get
            {
                return _containerViewState["NoteOptions.ShowCreateDateInput"] as bool? ?? false;
            }

            set
            {
                _containerViewState["NoteOptions.ShowCreateDateInput"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use person icon].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use person icon]; otherwise, <c>false</c>.
        /// </value>
        public bool UsePersonIcon
        {
            get
            {
                return _containerViewState["NoteOptions.UsePersonIcon"] as bool? ?? false;
            }

            set
            {
                _containerViewState["NoteOptions.UsePersonIcon"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [add always visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [add always visible]; otherwise, <c>false</c>.
        /// </value>
        public bool AddAlwaysVisible
        {
            get
            {
                return _containerViewState["NoteOptions.AddAlwaysVisible"] as bool? ?? false;
            }

            set
            {
                _containerViewState["NoteOptions.AddAlwaysVisible"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the note label (used to be called 'Term')
        /// </summary>
        /// <value>
        /// The note label.
        /// </value>
        public string NoteLabel
        {
            get
            {
                return _containerViewState["NoteOptions.NoteLabel"] as string;
            }

            set
            {
                _containerViewState["NoteOptions.NoteLabel"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [expand replies].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [expand replies]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpandReplies
        {
            get
            {
                return _containerViewState["NoteOptions.ExpandReplies"] as bool? ?? false;
            }

            set
            {
                _containerViewState["NoteOptions.ExpandReplies"] = value;
            }
        }

        /// <summary>
        /// Occurs when [note types change].
        /// </summary>
        public event EventHandler NoteTypesChange;
    }
}