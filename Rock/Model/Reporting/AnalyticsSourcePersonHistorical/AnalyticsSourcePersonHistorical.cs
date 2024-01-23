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
    /// Represents the source record for the AnalyticsDimPersonHistorical and AnalyticsDimPersonCurrent views.
    /// NOTE: Rock.Jobs.ProcessBIAnalytics dynamically adds additional columns to this table for any Attribute that is marked for Analytics
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourcePersonHistorical" )]
    [DataContract]
    [HideFromReporting]
    [Rock.SystemGuid.EntityTypeGuid( "FC84E469-7E8F-4202-89C3-F27DD41BC132")]
    public class AnalyticsSourcePersonHistorical : AnalyticsSourcePersonBase<AnalyticsSourcePersonHistorical>
    {
        // intentionally blank
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsSourcePersonHistorical Configuration Class
    /// </summary>
    public partial class AnalyticsSourcePersonHistoricalConfiguration : EntityTypeConfiguration<AnalyticsSourcePersonHistorical>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsSourcePersonHistoricalConfiguration"/> class.
        /// </summary>
        public AnalyticsSourcePersonHistoricalConfiguration()
        {
            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier birthdates 
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasOptional( t => t.BirthDateDim ).WithMany().HasForeignKey( t => t.BirthDateKey ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
