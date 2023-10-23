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
using System.Data.Entity;

using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class NoteType
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return NoteTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            NoteTypeCache.UpdateCachedEntity( this.Id, entityState );
            NoteTypeCache.RemoveEntityNoteTypes();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the legacy colors. This can be removed when the legacy
        /// properties are removed.
        /// </summary>
        internal void UpdateLegacyColors()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if ( Color.IsNullOrWhiteSpace() )
            {
                BackgroundColor = string.Empty;
                FontColor = string.Empty;
                BorderColor = string.Empty;

                return;
            }

            try
            {
                var color = new RockColor( Color );
                var pair = RockColor.CalculateColorPair( color );

                BackgroundColor = pair.BackgroundColor.ToRGBA();
                FontColor = pair.ForegroundColor.ToRGBA();
                BorderColor = pair.ForegroundColor.ToRGBA();
            }
            catch
            {
                BackgroundColor = string.Empty;
                FontColor = string.Empty;
                BorderColor = string.Empty;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        #endregion
    }
}
