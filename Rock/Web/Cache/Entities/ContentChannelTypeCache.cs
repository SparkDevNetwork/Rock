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

namespace Rock.Web.Cache
{
    /// <summary>
    /// Content Channel Type Cache
    /// </summary>
    [Serializable]
    [DataContract]
    public class ContentChannelTypeCache : ModelCache<ContentChannelTypeCache, ContentChannelType>
    {
        #region Properties

        /// <summary>
        /// Gets or sets a flag indicating if this ContentType is part of the Rock core system/framework. 
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> flag that is <c>true</c> if this ContentChannelType is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the name of the ContentType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the name of the ContentType.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets an <see cref="ContentChannelDateType"/> enumeration that represents the type of date range that this DateRangeTypeEnum supports.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.ContentChannelDateType"/> that represents the type of DateRangeTypeEnum is supported. When <c>DateRangeTypeEnum.SingleDate</c> a single date 
        /// will be supported; when <c>DateRangeTypeEnum.DateRange</c> a date range will be supported.
        /// </value>
        [DataMember]
        public ContentChannelDateType DateRangeType { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether time should be included with the single or date range values
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include time]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IncludeTime { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable priority].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable priority]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool DisablePriority { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable content field].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable content field]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool DisableContentField { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable status].
        /// If this is set to True, all of the ContentChannelItems are "Approved"
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable status]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool DisableStatus { get; private set; }

        /// <summary>
        /// A flag indicating if a <see cref="Rock.Model.ContentChannel"/> of this ContentChannelType will be shown in the content channel list.
        /// When false, it means any 'Channel Types Include' settings MUST specifically include in order to show it.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if a <see cref="Rock.Model.Group"/> of this Content Channel Type will be shown in the Channel list; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowInChannelList { get; private set; }

        /// <summary>
        /// Gets the content channels of this type.
        /// </summary>
        /// <value>
        /// The content channels.
        /// </value>
        public List<ContentChannelCache> ContentChannels => ContentChannelCache.All().Where( cc => cc.ContentChannelTypeId == Id ).ToList();

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );
            var contentChannelType = entity as ContentChannelType;

            if ( contentChannelType == null )
            {
                return;
            }

            IsSystem = contentChannelType.IsSystem;
            Name = contentChannelType.Name;
            DateRangeType = contentChannelType.DateRangeType;
            IncludeTime = contentChannelType.IncludeTime;
            DisablePriority = contentChannelType.DisablePriority;
            DisableContentField = contentChannelType.DisableContentField;
            DisableStatus = contentChannelType.DisableStatus;
            ShowInChannelList = contentChannelType.ShowInChannelList;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}