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
using System.Data.Entity;
using System.Linq;

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Global Attributes
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheGlobalAttributes instead" )]
    public class GlobalAttributesCache
    {
        #region Constructors

        /// <summary>
        /// Use Static Read() method to instantiate a new Global Attributes object
        /// </summary>
        private GlobalAttributesCache()
        {
        }

        private GlobalAttributesCache( CacheGlobalAttributes cacheGlobalAttributes )
        {
            CopyFromNewCache( cacheGlobalAttributes );
        }

        #endregion

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the attributes.  Used to iterate all values when merging possible merge fields
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public List<AttributeCache> Attributes
        {
            get
            {
                var attributes = new List<AttributeCache>();

                lock ( _obj )
                {
                    if ( attributeIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            attributeIds = new AttributeService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( t =>
                                    !t.EntityTypeId.HasValue &&
                                    ( t.EntityTypeQualifierColumn == null || t.EntityTypeQualifierColumn == string.Empty ) &&
                                    ( t.EntityTypeQualifierValue == null || t.EntityTypeQualifierValue == string.Empty ) )
                                .Select( t => t.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( var id in attributeIds )
                {
                    var attribute = AttributeCache.Read( id );
                    if ( attribute != null )
                    {
                        attributes.Add( attribute );
                    }
                }

                return attributes;
            }

            set
            {
                lock ( _obj )
                {
                    attributeIds = value?.Select( a => a.Id ).ToList();
                }
            }
        }
        private List<int> attributeIds;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheGlobalAttributes">The cache global attributes.</param>
        private void CopyFromNewCache( CacheGlobalAttributes cacheGlobalAttributes )
        {
            attributeIds = cacheGlobalAttributes.Attributes.Select( a => a.Id ).ToList();
        }

        /// <summary>
        /// Gets the Global Attribute values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public string GetValue( string key, RockContext rockContext = null )
        {
            return CacheGlobalAttributes.Value( key );
        }

        /// <summary>
        /// Gets the value formatted.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public string GetValueFormatted( string key, RockContext rockContext = null )
        {
            return CacheGlobalAttributes.Get().GetValueFormatted( key, rockContext );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="saveValue">if set to <c>true</c> [save value].</param>
        public void SetValue( string key, string value, bool saveValue )
        {
            CacheGlobalAttributes.Get().SetValue( key, value, saveValue );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="saveValue">if set to <c>true</c> [save value].</param>
        /// <param name="rockContext">The rock context.</param>
        public void SetValue( string key, string value, bool saveValue, RockContext rockContext )
        {
            CacheGlobalAttributes.Get().SetValue( key, value, saveValue, rockContext );
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the existing or a new item from cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public static GlobalAttributesCache GetOrAddExisting( string key, Func<GlobalAttributesCache> valueFactory )
        {
            return new GlobalAttributesCache( CacheGlobalAttributes.Get() );
        }

        /// <summary>
        /// Returns Global Attributes from cache.  If they are not already in cache, they
        /// will be read and added to cache
        /// </summary>
        /// <returns></returns>
        public static GlobalAttributesCache Read()
        {
            return Read( null );
        }

        /// <summary>
        /// Reads the specified rock context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static GlobalAttributesCache Read( RockContext rockContext )
        {
            return new GlobalAttributesCache( CacheGlobalAttributes.Get() );
        }

        /// <summary>
        /// Returns the global attribute value for the given key.
        /// </summary>
        /// <returns></returns>
        public static string Value( string key )
        {
            return CacheGlobalAttributes.Value( key );
        }

        /// <summary>
        /// Removes Global Attributes from cache
        /// </summary>
        public static void Flush()
        {
            CacheGlobalAttributes.Clear();
        }

        /// <summary>
        /// Gets the global attribute values as merge fields for dotLiquid merging.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        [Obsolete( "Use Rock.Lava.LavaHelper.GetCommonMergeFields instead" )]
        public static Dictionary<string, object> GetMergeFields( Person currentPerson )
        {
            return CacheGlobalAttributes.GetLegacyMergeFields( currentPerson );
        }

        /// <summary>
        /// Gets the legacy global attribute values as merge fields for dotLiquid merging.
        /// Note: You should use LavaHelper.GetCommonMergeFields instead of this
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        internal static Dictionary<string, object> GetLegacyMergeFields( Person currentPerson )
        {
            return CacheGlobalAttributes.GetLegacyMergeFields( currentPerson );
        }

        /// <summary>
        /// Gets the current graduation year based on grade transition date
        /// </summary>
        /// <value>
        /// Returns current year if transition month/day has not passed, otherwise will return next year
        /// </value>
        [Obsolete("Moved to RockDateTime.CurrentGraduationYear")]
        public int CurrentGraduationYear
        {
            get
            {
                return RockDateTime.CurrentGraduationYear;
            }
        }
        /// <summary>
        /// Gets the organization location (OrganizationAddress)
        /// </summary>
        /// <value>
        /// The organization location.
        /// </value>
        public Location OrganizationLocation => CacheGlobalAttributes.Get().OrganizationLocation;

        /// <summary>
        /// Gets the state of the organization.
        /// </summary>
        /// <value>
        /// The state of the organization.
        /// </value>
        public string OrganizationState => CacheGlobalAttributes.Get().OrganizationState;

        /// <summary>
        /// Gets the organization country.
        /// </summary>
        /// <value>
        /// The organization country.
        /// </value>
        public string OrganizationCountry => CacheGlobalAttributes.Get().OrganizationCountry;

        /// <summary>
        /// Gets the organization location formatted.
        /// </summary>
        /// <value>
        /// The organization location formatted.
        /// </value>
        public string OrganizationLocationFormatted => CacheGlobalAttributes.Get().OrganizationLocationFormatted;

        /// <summary>
        /// Gets the lava support level.
        /// </summary>
        /// <value>
        /// The lava support level.
        /// </value>
        public Lava.LavaSupportLevel LavaSupportLevel => CacheGlobalAttributes.Get().LavaSupportLevel;

        /// <summary>
        /// Gets a value indicating whether Envelope Number feature is enabled
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable giving envelope number]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableGivingEnvelopeNumber => CacheGlobalAttributes.Get().EnableGivingEnvelopeNumber;

        #endregion
    }
}