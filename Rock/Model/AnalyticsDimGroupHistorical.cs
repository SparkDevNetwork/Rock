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
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsDimGroupHistorical is SQL View based on AnalyticsSourceGroupHistorical
    /// and represents the historic and current records from AnalyticsSourceGroupHistorical
    /// </summary>
    [Table( "AnalyticsDimGroupHistorical" )]
    [DataContract]
    public class AnalyticsDimGroupHistorical : AnalyticsDimGroupBase<AnalyticsDimGroupHistorical>
    {
        // intentionally blank. See AnalyticsDimGroupBase, etc for the fields
    }

    /// <summary>
    /// *Another* Abstract Layer since AnalyticDimGroupHistorical and AnalyticsDimGroupCurrent share all the same fields
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Rock.Model.AnalyticsSourceGroupBase{T}" />
    public abstract class AnalyticsDimGroupBase<T> : AnalyticsSourceGroupBase<T>
        where T : AnalyticsDimGroupBase<T>, new()
    {
        #region Denormalized Lookup Values

        /// <summary>
        /// Gets or sets the name of the group type.
        /// </summary>
        /// <value>
        /// The name of the group type.
        /// </value>
        [DataMember]
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the campus.
        /// </summary>
        /// <value>
        /// The name of the campus.
        /// </value>
        [DataMember]
        public string CampusName { get; set; }

        #endregion
    }
}
