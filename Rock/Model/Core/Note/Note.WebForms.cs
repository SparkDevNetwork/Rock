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
using Rock.Data;
using Rock.Lava;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Note Properties that use System.Web.
    /// </summary>
    public partial class Note
    {
        #region Virtual Properties

        /// <summary>
        /// Gets the childs note that the current person is allowed to view
        /// </summary>
        /// <value>
        /// The viewable child notes.
        /// </value>
        [LavaVisible]
        [NotMapped]
        public virtual List<Note> ViewableChildNotes
        {
            get
            {
                // only get notes they have auth to VIEW ( note that VIEW has special rules based on approval status, etc. See Note.IsAuthorized for details )
                var currentPerson = System.Web.HttpContext.Current?.Items["CurrentPerson"] as Person;

                var viewableChildNotes = ChildNotes.ToList().Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) ).ToList();

                return viewableChildNotes;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the currently logged in person is watching this specific note
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is current person watching; otherwise, <c>false</c>.
        /// </value>
        [LavaVisible]
        public virtual bool IsCurrentPersonWatching
        {
            get
            {
                var currentPerson = System.Web.HttpContext.Current?.Items["CurrentPerson"] as Person;
                var currentPersonId = currentPerson?.Id;
                if ( currentPersonId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        bool isWatching = new NoteWatchService( rockContext ).Queryable()
                                .Where( a => a.NoteId == this.Id 
                                    && a.WatcherPersonAlias.PersonId == currentPersonId.Value 
                                    && a.IsWatching == true ).Any();

                        return isWatching;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the count of that are descendants (replies) of this note.
        /// </summary>
        /// <value>
        /// The viewable descendents count.
        /// </value>
        [LavaVisible]
        public virtual int ViewableDescendentsCount
        {
            get
            {
                if ( !_viewableDescendentsCount.HasValue )
                {
                    var currentPerson = System.Web.HttpContext.Current?.Items["CurrentPerson"] as Person;

                    using ( var rockContext = new RockContext() )
                    {
                        var noteDescendents = new NoteService( rockContext ).GetAllDescendents( this.Id ).ToList();
                        var viewableDescendents = noteDescendents.ToList().Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) ).ToList();
                        _viewableDescendentsCount = viewableDescendents.Count();
                    }
                }

                return _viewableDescendentsCount.Value;
            }
        }

        private int? _viewableDescendentsCount = null;

        #endregion Virtual Properties
    }
}
