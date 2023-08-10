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
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Tv.AppleTvAppDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class AppleTvAppBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the application styles.
        /// </summary>
        /// <value>
        /// The application styles.
        /// </value>
        public string ApplicationStyles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable page views].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable page views]; otherwise, <c>false</c>.
        /// </value>
        public bool EnablePageViews { get; set; }

        /// <summary>
        /// Gets or sets the page view retention period.
        /// </summary>
        /// <value>
        /// The page view retention period.
        /// </value>
        public int? PageViewRetentionPeriod { get; set; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.EntityType of this item.
        /// </summary>
        public PageRouteValueBag LoginPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the application javascript.
        /// </summary>
        /// <value>
        /// The application javascript.
        /// </value>
        public string ApplicationJavascript { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show application javascript].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show application javascript]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowApplicationJavascript { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable page view geo tracking].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable page view geo tracking]; otherwise, <c>false</c>.
        /// </value>
        public bool EnablePageViewGeoTracking { get; set; }
    }
}
