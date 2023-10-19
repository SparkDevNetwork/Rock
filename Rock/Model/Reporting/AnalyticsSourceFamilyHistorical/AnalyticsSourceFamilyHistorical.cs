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
    /// Represents the source record for the AnalyticsDimFamilyHistorical and AnalyticsDimFamilyCurrent views
    /// NOTE: Rock.Jobs.ProcessBIAnalytics dynamically adds additional columns to this table for any Attribute that is marked for Analytics
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourceFamilyHistorical" )]
    [DataContract]
    [HideFromReporting]
    [Rock.SystemGuid.EntityTypeGuid( "C9941E89-EC9D-41FF-A892-5016730F22C1")]
    public class AnalyticsSourceFamilyHistorical : AnalyticsSourceFamilyBase<AnalyticsSourceFamilyHistorical>
    {
        // intentionally blank.  See AnalyticsSourceFamilyBase.
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsSourceFamilyHistorical Configuration Class.
    /// </summary>
    public partial class AnalyticsSourceFamilyHistoricalConfiguration : EntityTypeConfiguration<AnalyticsSourceFamilyHistorical>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsSourceFamilyHistorical"/> class.
        /// </summary>
        public AnalyticsSourceFamilyHistoricalConfiguration()
        {
            // NOTE: When creating a migration for this, don't create the actual FK's in the database for any of these since they are views

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier birthdates 
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
        }
    }

    #endregion Entity Configuration
}
