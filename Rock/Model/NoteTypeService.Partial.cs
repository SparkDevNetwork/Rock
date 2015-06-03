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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/Service class for entities of the <see cref="Rock.Model.NoteType"/>
    /// </summary>
    public partial class NoteTypeService : Service<NoteType>
    {
        /// <summary>
        /// Gets the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="create">if set to <c>true</c> [create].</param>
        /// <returns></returns>
        public NoteType Get( int entityTypeId, string name, bool create = true )
        {
            var noteTypes = Get( entityTypeId, string.Empty, string.Empty ).ToList();
            var noteType = noteTypes.Where( t => t.Name == name ).FirstOrDefault();

            if ( noteType == null && create )
            {
                noteType = new NoteType();
                noteType.IsSystem = false;
                noteType.EntityTypeId = entityTypeId;
                noteType.EntityTypeQualifierColumn = string.Empty;
                noteType.EntityTypeQualifierValue = string.Empty;
                noteType.Name = name;
                noteType.UserSelectable = true;
                noteType.IconCssClass = string.Empty;
                noteType.CssClass = string.Empty;
                noteType.Order = noteTypes.Any() ? noteTypes.Max( t => t.Order ) + 1 : 0;

                // Create a new context/service so that save does not affect calling method's context
                using ( var rockContext = new RockContext() )
                {
                    var noteTypeService = new NoteTypeService( rockContext );
                    noteTypeService.Add( noteType );
                    rockContext.SaveChanges();
                }

                // requery using calling context
                noteType = Get( noteType.Id );
            }

            return noteType;
        }

        /// <summary>
        /// Gets the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <returns></returns>
        public IQueryable<NoteType> Get( int entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            return Queryable()
                .Where( n =>
                    n.EntityTypeId == entityTypeId &&
                    ( n.EntityTypeQualifierColumn ?? "" ) == ( entityTypeQualifierColumn ?? "" ) &&
                    ( n.EntityTypeQualifierValue ?? "" ) == ( entityTypeQualifierValue ?? "" ) );
        }

    }
}
