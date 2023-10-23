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

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PersonPreference
    {
        /// <summary>
        /// Save hook implementation for <see cref="PersonPreference"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<PersonPreference>
        {
            /// <summary>
            /// The anonymous visitor person identifier.
            /// </summary>
            private static readonly Lazy<int> AnonymousVisitorPersonId = new Lazy<int>( () => new PersonService( new RockContext() ).GetOrCreateAnonymousVisitorPersonId() );

            /// <inheritdoc/>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Added || State == EntityContextState.Modified || State == EntityContextState.Deleted )
                {
                    var updateOptions = RockContext.GetOrCreateOptions<CacheUpdateOptions>();
                    var personAlias = Entity.PersonAlias
                        ?? new PersonAliasService( RockContext ).Get( Entity.PersonAliasId );

                    if ( personAlias.PersonId == AnonymousVisitorPersonId.Value )
                    {
                        updateOptions.VisitorIds.Add( personAlias.Id );
                    }
                    else
                    {
                        updateOptions.PersonIds.Add( personAlias.PersonId );
                    }

                    RockContext.ExecuteAfterCommit( () => UpdateCache() );
                }

                base.PreSave();
            }

            /// <summary>
            /// Updates the cache for the given people or visitors. This happens
            /// after the save has completed. Duplicate person identifiers will
            /// only be flushed once.
            /// </summary>
            private void UpdateCache()
            {
                var updateOptions = RockContext.GetOptions<CacheUpdateOptions>();

                if ( updateOptions == null )
                {
                    return;
                }

                foreach ( var personId in updateOptions.PersonIds )
                {
                    PersonPreferenceCache.FlushPerson( personId );
                }

                foreach ( var personAliasId in updateOptions.VisitorIds )
                {
                    PersonPreferenceCache.FlushVisitor( personAliasId );
                }

                updateOptions.Clear();
            }

            /// <summary>
            /// Custom options object to store the unique set of identifiers that
            /// need to be flushed from the cache.
            /// </summary>
            private class CacheUpdateOptions
            {
                /// <summary>
                /// Gets the person ids to be flushed from the cache.
                /// </summary>
                /// <value>The person ids to be flushed from the cache.</value>
                public HashSet<int> PersonIds { get; } = new HashSet<int>();

                /// <summary>
                /// Gets the visitor alias ids to be flushed from the cache.
                /// </summary>
                /// <value>The visitor alias ids to be flushed from the cache.</value>
                public HashSet<int> VisitorIds { get; } = new HashSet<int>();

                /// <summary>
                /// Clears all the identifiers from this instance.
                /// </summary>
                public void Clear()
                {
                    PersonIds.Clear();
                    VisitorIds.Clear();
                }
            }
        }
    }
}
