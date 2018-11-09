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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.Note"/> entity objects.
    /// </summary>
    public partial class NoteService : Service<Note>
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Note">Notes</see> for the specified <see cref="Rock.Model.NoteType"/> and entity.
        /// </summary>
        /// <param name="noteTypeId">A <see cref="System.Int32" /> representing the Id of the <see cref="Rock.Model.NoteType"/>.</param>
        /// <param name="entityId">TA <see cref="System.Int32"/> representing the Id of the entity that the note belongs to.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Note">Notes</see> for the specified <see cref="Rock.Model.NoteType"/> and entity. </returns>
        public IQueryable<Note> Get( int noteTypeId, int entityId )
        {
            return Queryable( "CreatedByPersonAlias.Person" )
                .Where( n =>
                    n.NoteTypeId == noteTypeId &&
                    n.EntityId == entityId )
                .OrderByDescending( n => n.IsAlert )
                .ThenByDescending( n => n.CreatedDateTime );
        }

        /// <summary>
        /// Returns all of the <see cref="Rock.Model.Note">Notes</see> for the specified note type.
        /// </summary>
        /// <param name="noteTypeId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.NoteType"/>.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Note">Notes</see> by <see cref="Rock.Model.NoteType"/>.</returns>
        public IQueryable<Note> GetByNoteTypeId( int noteTypeId )
        {
            return Queryable( "CreatedByPersonAlias.Person" )
                .Where( n =>
                    n.NoteTypeId == noteTypeId )
                .OrderByDescending( n => n.IsAlert )
                .ThenByDescending( n => n.CreatedDateTime );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Note">Notes</see> that are descendants (replies) of a specified note.
        /// </summary>
        /// <param name="parentNoteId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Note"/> to retrieve descendants for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Note">Notes</see> that are descendants of referenced note.</returns>
        public IEnumerable<Note> GetAllDescendents( int parentNoteId )
        {
            return this.ExecuteQuery(
                @"
                with CTE as (
                select * from [Note] where [ParentNoteId]={0}
                union all
                select [a].* from [Note] [a]
                inner join CTE pcte on pcte.Id = [a].[ParentNoteId]
                )
                select * from CTE
                ", parentNoteId );
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="deleteChildNotes">if set to <c>true</c> [delete child notes].</param>
        /// <returns></returns>
        public bool Delete( Note item, bool deleteChildNotes )
        {
            if ( deleteChildNotes )
            {
                var childNotes = GetAllDescendents( item.Id );
                if ( childNotes.Any() )
                {
                    base.DeleteRange( childNotes );
                }
            }

            return base.Delete( item );
        }

        /// <summary>
        /// Determines whether this instance [can delete child notes] the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can delete child notes] the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDeleteChildNotes( Note item, Person currentPerson, out string errorMessage )
        {
            errorMessage = string.Empty;

            var childNotes = this.GetAllDescendents( item.Id );
            if ( childNotes.Any() )
            {
                foreach ( var childNote in childNotes )
                {
                    if ( !childNote.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson ) )
                    {
                        errorMessage = "This note contains one or more note replies that cannot be deleted.";
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
