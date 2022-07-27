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
    /// Information about a content library required to obtain fast access
    /// to the library system.
    /// </summary>
    [Serializable]
    [DataContract]
    internal class ContentLibraryCache : ModelCache<ContentLibraryCache, ContentLibrary>
    {
        #region Properties

        /// <inheritdoc cref="ContentLibrary.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="ContentLibrary.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="ContentLibrary.LibraryKey"/>
        [DataMember]
        public string LibraryKey { get; private set; }

        /// <inheritdoc cref="ContentLibrary.TrendingEnabled"/>
        [DataMember]
        public bool TrendingEnabled { get; private set; }

        /// <inheritdoc cref="ContentLibrary.TrendingWindowDay"/>
        [DataMember]
        public int TrendingWindowDay { get; private set; }

        /// <inheritdoc cref="ContentLibrary.TrendingMaxItems"/>
        [DataMember]
        public int TrendingMaxItems { get; private set; }

        /// <inheritdoc cref="ContentLibrary.TrendingGravity"/>
        [DataMember]
        public decimal TrendingGravity { get; private set; }

        /// <inheritdoc cref="ContentLibrary.EnableSegments"/>
        [DataMember]
        public bool EnableSegments { get; private set; }

        /// <inheritdoc cref="ContentLibrary.EnableRequestFilters"/>
        [DataMember]
        public bool EnableRequestFilters { get; private set; }

        /// <inheritdoc cref="ContentLibrary.FilterSettings"/>
        [DataMember]
        public ContentLibraryFilterSettingsBag FilterSettings { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is ContentLibrary contentLibrary ) )
            {
                return;
            }

            Name = contentLibrary.Name;
            Description = contentLibrary.Description;
            LibraryKey = contentLibrary.LibraryKey;
            TrendingEnabled = contentLibrary.TrendingEnabled;
            TrendingWindowDay = contentLibrary.TrendingWindowDay;
            TrendingMaxItems = contentLibrary.TrendingMaxItems;
            TrendingGravity = contentLibrary.TrendingGravity;
            EnableSegments = contentLibrary.EnableSegments;
            EnableRequestFilters = contentLibrary.EnableRequestFilters;
            FilterSettings = contentLibrary.FilterSettings.FromJsonOrNull<ContentLibraryFilterSettingsBag>() ?? new ContentLibraryFilterSettingsBag();
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