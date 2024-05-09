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
using Rock.Data;
using Rock.Utility;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents the source record for the AnalyticDimCampus view in Rock.
    /// NOTE: Rock.Jobs.ProcessBIAnalytics dynamically adds additional columns to this table for any Attribute that is marked for Analytics
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourceCampus" )]
    [DataContract]
    [HideFromReporting]
    [CodeGenExclude( CodeGenFeature.ViewModelFile )]
    [Rock.SystemGuid.EntityTypeGuid( "9DE61413-6D38-4F14-AE1B-DB927E07CE56")]
    public class AnalyticsSourceCampus : AnalyticsSourceCampusBase<AnalyticsSourceCampus>
    {
        // intentionally blank.  See AnalyticsSourceCampusBase.
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsSourceCampus Configuration Class
    /// </summary>
    public partial class AnalyticsSourceCampusConfiguration : EntityTypeConfiguration<AnalyticsSourceCampus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsSourceCampusConfiguration"/> class.
        /// </summary>
        public AnalyticsSourceCampusConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
