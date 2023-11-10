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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cache of a <see cref="Rock.Model.PersonalLinkSection.IsShared">shared</see> <see cref="Rock.Model.PersonalLinkSection"/>
    /// </summary>
    [Serializable]
    [DataContract]
    public class SharedPersonalLinkSectionCache : ModelCache<SharedPersonalLinkSectionCache, Rock.Model.PersonalLinkSection>
    {
        /// <inheritdoc cref="Rock.Model.PersonalLinkSection.Name"/>
        public string Name { get; private set; }

        /// <inheritdoc cref="Rock.Model.PersonalLinkSection.IsShared"/>
        public bool IsShared { get; private set; }

        /// <inheritdoc cref="Rock.Model.PersonalLinkSection.IconCssClass"/>
        public string IconCssClass { get; private set; }

        private SharedPersonalLinkCache[] _links = null;

        private readonly object _populateLinksLock = new object();

        /// <inheritdoc cref="Rock.Model.PersonalLinkSection.PersonalLinks"/>
        public SharedPersonalLinkCache[] Links
        {
            get
            {
                if ( _links == null )
                {
                    lock ( _populateLinksLock )
                    {
                        // check again if _links is null now that we are in the lock
                        if ( _links == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                _links = new PersonalLinkService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( r => r.SectionId == Id )
                                    .OrderBy( r => r.Order )
                                    .AsNoTracking()
                                    .ToList()
                                    .Select( a => new SharedPersonalLinkCache( a ) )
                                    .ToArray();
                            }
                        }
                    }
                }

                return _links;
            }
        }

        /// <summary>
        /// The amount of time that this item will live in the cache before expiring. If null, then the
        /// default lifespan is used.
        /// </summary>
        public override TimeSpan? Lifespan
        {
            get
            {
                if ( !IsShared )
                {
                    // just in case this isn't a shared PersonalLinkSection, expire after 10 minutes
                    return new TimeSpan( 0, 10, 0 );
                }

                return base.Lifespan;
            }
        }

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var personalLinkSection = entity as PersonalLinkSection;

            if ( personalLinkSection == null )
            {
                return;
            }

            this.Name = personalLinkSection.Name;
            this.IsShared = personalLinkSection.IsShared;
            this.IconCssClass = personalLinkSection.IconCssClass;
        }

        /// <summary>
        /// The last update date time
        /// </summary>
        private static DateTime _lastUpdateDateTime = default;

        /// <summary>
        /// The DateTime (in <see cref="RockDateTime"/>) of the most recently modified <see cref="Rock.Model.PersonalLinkSection.IsShared">shared</see> <see cref="Rock.Model.PersonalLinkSection"/>
        /// or <see cref="Rock.Model.PersonalLinkSection.IsShared">shared</see> <see cref="Rock.Model.PersonalLink"/>
        /// </summary>
        /// <value>The last update date time.</value>
        public static DateTime LastModifiedDateTime
        {
            get
            {
                if ( _lastUpdateDateTime != default )
                {
                    return _lastUpdateDateTime;
                }

                var rockContext = new RockContext();

                // get the date of the most recently modified of Shared Section and Shared Links
                var sharedSectionLastModifiedDateTimeQuery = new PersonalLinkSectionService( rockContext )
                    .Queryable()
                    .Where( a => a.IsShared && a.ModifiedDateTime.HasValue )
                    .Select( a => a.ModifiedDateTime );

                var sharedLinkLastModifiedDateTimeQuery = new PersonalLinkService( rockContext )
                    .Queryable()
                    .Where( a => a.ModifiedDateTime.HasValue && a.Section.IsShared )
                    .Select( a => a.ModifiedDateTime );

                var lastModifiedDateTime = sharedSectionLastModifiedDateTimeQuery.Union( sharedLinkLastModifiedDateTimeQuery ).Max( a => a );

                // if there aren't any, use Today at MidNight at the last modified time
                // that way we know we've checked, but it'll stay the same if nothing is added today;
                _lastUpdateDateTime = lastModifiedDateTime ?? RockDateTime.Today;

                return _lastUpdateDateTime;
            }
        }

        /// <summary>
        /// Flushes the last modified date time.
        /// </summary>
        internal static void FlushLastModifiedDateTime()
        {
            _lastUpdateDateTime = default;
        }

        /// <summary>
        /// Returns all <see cref="Rock.Model.PersonalLinkSection.IsShared">shared</see> <see cref="Rock.Model.PersonalLinkSection"/> records
        /// </summary>
        /// <returns>List&lt;SharedPersonalLinkSectionCache&gt;.</returns>
        public static new List<SharedPersonalLinkSectionCache> All()
        {
            return All( null );
        }

        /// <summary>
        /// Returns all <see cref="Rock.Model.PersonalLinkSection.IsShared">shared</see> <see cref="Rock.Model.PersonalLinkSection"/> records
        /// </summary>
        /// <returns>List&lt;SharedPersonalLinkSectionCache&gt;.</returns>
        public static new List<SharedPersonalLinkSectionCache> All( RockContext rockContext )
        {
            var cachedKeys = GetOrAddKeys( () => QueryDbForAllIds( rockContext ) );
            if ( cachedKeys == null )
            {
                return new List<SharedPersonalLinkSectionCache>();
            }

            var allValues = new List<SharedPersonalLinkSectionCache>();
            foreach ( var key in cachedKeys.ToList() )
            {
                var value = Get( key.AsInteger(), rockContext );
                if ( value != null )
                {
                    allValues.Add( value );
                }
            }

            return allValues;
        }

        /// <summary>
        /// Queries the database for all ids with context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static List<string> QueryDbForAllIds( RockContext rockContext )
        {
            var service = new PersonalLinkSectionService( rockContext );
            return service.Queryable()
                .Where( a => a.IsShared )
                .Select( i => i.Id )
                .ToList()
                .ConvertAll( i => i.ToString() );
        }

        /// <summary>
        /// SharedPersonalLinkCache.
        /// </summary>
        public class SharedPersonalLinkCache
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SharedPersonalLinkCache"/> class.
            /// </summary>
            /// <param name="personalLink">The personal link.</param>
            internal SharedPersonalLinkCache( PersonalLink personalLink )
            {
                this.Id = personalLink.Id;
                this.Name = personalLink.Name;
                this.Url = personalLink.Url;
                this.Order = personalLink.Order;
                this.SectionId = this.SectionId;
            }

            /// <inheritdoc cref="IEntity.Id"/>
            public int Id { get; private set; }

            /// <inheritdoc cref="PersonalLink.Name"/>
            public string Name { get; private set; }

            /// <inheritdoc cref="PersonalLink.Url"/>
            public string Url { get; private set; }

            /// <inheritdoc cref="PersonalLink.Order"/>
            public int Order { get; private set; }

            /// <inheritdoc cref="PersonalLink.SectionId"/>
            public int SectionId { get; private set; }
        }
    }
}
