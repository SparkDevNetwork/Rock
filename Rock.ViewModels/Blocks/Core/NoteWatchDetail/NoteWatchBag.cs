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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.NoteWatchDetail
{
    /// <summary>
    /// Contains the NoteWatch details.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class NoteWatchBag : EntityBagBase
    {
        /// <summary>
        /// Set AllowOverride to False to prevent people from adding an IsWatching=False on NoteWatch with the same filter that is marked as IsWatching=True
        /// In other words, if a group is configured a NoteWatch, an individual shouldn't be able to add an un-watch if AllowOverride=False (and any un-watches that may have been already added would be ignored)
        /// </summary>
        public bool AllowOverride { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Set IsWatching to False to disable this NoteWatch (or specifically don't watch based on the notewatch criteria)
        /// </summary>
        public bool IsWatching { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        public ListItemBag Note { get; set; }

        /// <summary>
        /// Gets or sets the type of the note.
        /// </summary>
        public ListItemBag NoteType { get; set; }

        /// <summary>
        /// Gets or sets the group that is watching this note watch
        /// </summary>
        public ListItemBag WatcherGroup { get; set; }

        /// <summary>
        /// Gets or sets the person alias of the person watching this note watch
        /// </summary>
        public ListItemBag WatcherPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the watch entity.
        /// </summary>
        /// <value>
        /// The watch entity.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        /// <value>
        /// The name of the entity.
        /// </value>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the watched entity.
        /// </summary>
        /// <value>
        /// The watched entity.
        /// </value>
        public ListItemBag WatchedEntity { get; set; }

        /// <summary>
        /// Gets or sets the watched note text.
        /// </summary>
        /// <value>
        /// The watched note text.
        /// </value>
        public string WatchedNoteText { get; set; }
    }
}
