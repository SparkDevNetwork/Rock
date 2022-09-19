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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a content collection required to obtain fast access
    /// to the collection system.
    /// </summary>
    [Serializable]
    [DataContract]
    internal class ContentCollectionCache : ModelCache<ContentCollectionCache, ContentCollection>
    {
        #region Properties

        /// <inheritdoc cref="ContentCollection.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="ContentCollection.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="ContentCollection.CollectionKey"/>
        [DataMember]
        public string CollectionKey { get; private set; }

        /// <inheritdoc cref="ContentCollection.TrendingEnabled"/>
        [DataMember]
        public bool TrendingEnabled { get; private set; }

        /// <inheritdoc cref="ContentCollection.TrendingWindowDay"/>
        [DataMember]
        public int TrendingWindowDay { get; private set; }

        /// <inheritdoc cref="ContentCollection.TrendingMaxItems"/>
        [DataMember]
        public int TrendingMaxItems { get; private set; }

        /// <inheritdoc cref="ContentCollection.TrendingGravity"/>
        [DataMember]
        public decimal TrendingGravity { get; private set; }

        /// <inheritdoc cref="ContentCollection.EnableSegments"/>
        [DataMember]
        public bool EnableSegments { get; private set; }

        /// <inheritdoc cref="ContentCollection.EnableRequestFilters"/>
        [DataMember]
        public bool EnableRequestFilters { get; private set; }

        /// <inheritdoc cref="ContentCollection.FilterSettings"/>
        [DataMember]
        public ContentCollectionFilterSettingsBag FilterSettings { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is ContentCollection contentCollection ) )
            {
                return;
            }

            Name = contentCollection.Name;
            Description = contentCollection.Description;
            CollectionKey = contentCollection.CollectionKey;
            TrendingEnabled = contentCollection.TrendingEnabled;
            TrendingWindowDay = contentCollection.TrendingWindowDay;
            TrendingMaxItems = contentCollection.TrendingMaxItems;
            TrendingGravity = contentCollection.TrendingGravity;
            EnableSegments = contentCollection.EnableSegments;
            EnableRequestFilters = contentCollection.EnableRequestFilters;
            FilterSettings = contentCollection.FilterSettings.FromJsonOrNull<ContentCollectionFilterSettingsBag>() ?? new ContentCollectionFilterSettingsBag();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}