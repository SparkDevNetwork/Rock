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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class PersistedDatasetCache : EntityCache<PersistedDatasetCache, PersistedDataset>
    {
        /// <summary>
        /// Gets or sets the unique key to use to access this persisted dataset
        /// </summary>
        /// <value>
        /// The access key.
        /// </value>
        [DataMember]
        public string AccessKey { get; private set; }

        /// <summary>
        /// Gets or sets the Name of the PesistedDataset.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the PesistedDataset.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a user defined description of the PesistedDataset.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the PesistedDataset.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the refresh interval minutes
        /// </summary>
        /// <value>
        /// The refresh interval minutes.
        /// </value>
        [DataMember]
        public int? RefreshIntervalMinutes { get; private set; }

        /// <summary>
        /// Gets or sets a comma-delimited list of enabled LavaCommands
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        [DataMember]
        public string EnabledLavaCommands { get; private set; }

        /// <summary>
        /// Gets or sets the persisted last refresh date time.
        /// </summary>
        /// <value>
        /// The persisted last refresh date time.
        /// </value>
        [DataMember]
        public DateTime? LastRefreshDateTime { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow manual refresh].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow manual refresh]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowManualRefresh { get; private set; } = true;

        /// <summary>
        /// Gets or sets the serialized data of the dataset. See <seealso cref="ResultFormat"/>
        /// </summary>
        /// <value>
        /// The result data.
        /// </value>
        [DataMember]
        public string ResultData { get; private set; }

        /// <summary>
        /// returns an <see cref="System.Dynamic.ExpandoObject"/> or a list of <see cref="System.Dynamic.ExpandoObject"/>.  If <see cref="ResultData"/> can't be deserialized, returns null
        /// </summary>
        /// <value>
        /// The result data object.
        /// </value>
        public dynamic ResultDataObject
        {
            get
            {
                object resultDataObject;

                Func<PersistedDatasetValueCache> itemFactory = () =>
                {
                    object itemFactoryResultObject;
                    switch ( this.ResultFormat )
                    {
                        case PersistedDatasetDataFormat.JSON:
                            itemFactoryResultObject = this.ResultData.FromJsonDynamicOrNull();
                            break;

                        default:
                            throw new DataFormatException( this.ResultFormat );
                    }

                    // Append entity type id and entity id if appropriate
                    itemFactoryResultObject = AppendEntityInformation( itemFactoryResultObject );

                    return new PersistedDatasetValueCache( itemFactoryResultObject, MemoryCacheDurationMS.HasValue ?
                        TimeSpan.FromMilliseconds( MemoryCacheDurationMS.Value ) :
                        (TimeSpan?) null );
                };

                if ( this.MemoryCacheDurationMS.HasValue )
                {
                    PersistedDatasetValueCache persistedDatasetValueCache = ItemCache<PersistedDatasetValueCache>.GetOrAddExisting(
                        this.Id,
                        itemFactory );

                    resultDataObject = persistedDatasetValueCache?.ResultDataObjectValue;
                }
                else
                {
                    PersistedDatasetValueCache persistedDatasetValueCache = itemFactory();

                    resultDataObject = persistedDatasetValueCache?.ResultDataObjectValue;
                }

                return resultDataObject;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="System.FormatException" />
        public class DataFormatException : FormatException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DataFormatException" /> class.
            /// </summary>
            /// <param name="dataFormat">The data format.</param>
            public DataFormatException( PersistedDatasetDataFormat dataFormat )
                : base( $"Unexpected ResultFormat: {dataFormat.ConvertToString()}" )
            {
            }
        }

        /// <summary>
        /// Gets or sets the result format.
        /// </summary>
        /// <value>
        /// The result format.
        /// </value>
        [DataMember]
        public PersistedDatasetDataFormat ResultFormat { get; set; } = PersistedDatasetDataFormat.JSON;

        /// <summary>
        /// Gets or sets the memory cache duration ms.
        /// </summary>
        /// <value>
        /// The memory cache duration ms.
        /// </value>
        [DataMember]
        public int? MemoryCacheDurationMS { get; set; }

        /// <summary>
        /// Gets or sets the build script. See <seealso cref="BuildScriptType"/>
        /// </summary>
        /// <value>
        /// The build script.
        /// </value>
        [DataMember]
        public string BuildScript { get; set; }

        /// <summary>
        /// Gets or sets the type of the build script.
        /// </summary>
        /// <value>
        /// The type of the build script.
        /// </value>
        [DataMember]
        public PersistedDatasetScriptType BuildScriptType { get; set; } = PersistedDatasetScriptType.Lava;

        /// <summary>
        /// Gets or sets a flag indicating that this PesistedDataset is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this PesistedDataset is part of the core system/framework, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The amount of time that it took to persist the <see cref="ResultData"/>
        /// </summary>
        /// <value>
        /// The time to build ms.
        /// </value>
        [DataMember]
        public double? TimeToBuildMS { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Web.Cache.EntityTypeCache"/> (<see cref="Rock.Model.EntityType"/>) that this PesistedDataset is used for.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EntityType"/>
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// The DateTime when to stop updating the <see cref="ResultData"/>
        /// </summary>
        /// <value>
        /// The expire date time.
        /// </value>
        [DataMember]
        public DateTime? ExpireDateTime { get; set; }

        /// <summary>
        /// Lookup the Id for a PersistedDataSet from AccessKey
        /// </summary>
        private static readonly ConcurrentDictionary<string, int> AccessKeyIdLookup = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Gets the <see cref="PersistedDatasetCache.ResultDataObject" /> using the specified <see cref="PersistedDataset.AccessKey" />
        /// </summary>
        /// <param name="accessKey">The access key.</param>
        /// <returns></returns>
        public static dynamic GetObjectFromAccessKey( string accessKey )
        {
            return GetFromAccessKey( accessKey )?.ResultDataObject;
        }

        /// <summary>
        /// Gets the PersistedDatasetCache using the specified <see cref="PersistedDataset.AccessKey" />
        /// </summary>
        /// <param name="accessKey">The access key.</param>
        /// <returns></returns>
        public static PersistedDatasetCache GetFromAccessKey( string accessKey )
        {
            int persistedDatasetId;
            if ( AccessKeyIdLookup.TryGetValue( accessKey, out persistedDatasetId ) )
            {
                return Get( persistedDatasetId );
            }

            PersistedDatasetCache persistedDatasetCache = null;

            using ( var rockContext = new RockContext() )
            {
                var persistedDataset = new PersistedDatasetService( rockContext ).GetFromAccessKeyNoTracking( accessKey );
                if ( persistedDataset != null )
                {
                    persistedDatasetCache = Get( persistedDataset.Id );
                }
            }

            if ( persistedDatasetCache != null )
            {
                AccessKeyIdLookup.AddOrUpdate( persistedDatasetCache.AccessKey, persistedDatasetCache.Id, ( k, v ) => persistedDatasetCache.Id );
            }

            return persistedDatasetCache;
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var persistedDataset = entity as PersistedDataset;
            if ( persistedDataset == null )
            {
                return;
            }

            AccessKey = persistedDataset.AccessKey;
            Name = persistedDataset.Name;
            Description = persistedDataset.Description;
            RefreshIntervalMinutes = persistedDataset.RefreshIntervalMinutes;
            LastRefreshDateTime = persistedDataset.LastRefreshDateTime;
            EnabledLavaCommands = persistedDataset.EnabledLavaCommands;
            AllowManualRefresh = persistedDataset.AllowManualRefresh;
            ResultData = persistedDataset.ResultData;
            ResultFormat = persistedDataset.ResultFormat;
            MemoryCacheDurationMS = persistedDataset.MemoryCacheDurationMS;
            BuildScript = persistedDataset.BuildScript;
            BuildScriptType = persistedDataset.BuildScriptType;
            IsSystem = persistedDataset.IsSystem;
            IsActive = persistedDataset.IsActive;
            TimeToBuildMS = persistedDataset.TimeToBuildMS;
            EntityTypeId = persistedDataset.EntityTypeId;
            ExpireDateTime = persistedDataset.ExpireDateTime;

            // the ResultDataObject is cached in PersistedDatasetValueCache (with a cache expiration), so need to flush that when re-loading
            PersistedDatasetValueCache.FlushItem( this.Id );

            AccessKeyIdLookup.AddOrUpdate( persistedDataset.AccessKey, persistedDataset.Id, ( k, v ) => persistedDataset.Id );
        }

        #region Private Methods

        /// <summary>
        /// Appends the entity information.
        /// </summary>
        /// <param name="persistedDataset">The persisted dataset.</param>
        /// <returns></returns>
        private dynamic AppendEntityInformation( dynamic persistedDataset )
        {
            if ( !this.EntityTypeId.HasValue )
            {
                return persistedDataset;
            }

            if ( persistedDataset is IEnumerable<ExpandoObject> )
            {
                foreach ( dynamic datasetItem in ( IEnumerable ) persistedDataset )
                {
                    datasetItem.EntityTypeId = this.EntityTypeId.Value;
                    datasetItem.EntityId = ( int? ) datasetItem.Id;
                }
            }
            else
            {
                persistedDataset.EntityTypeId = this.EntityTypeId.Value;
                persistedDataset.EntityId = ( int? ) persistedDataset.Id;
            }

            return persistedDataset;
        }

        #endregion
    }
}