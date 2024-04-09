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
    /// AnalyticsDimFamilyHistorical is SQL View based on AnalyticsSourceFamilyHistorical
    /// and represents the historic and current records from AnalyticsSourceFamilyHistorical
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsDimFamilyHistorical" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.ViewModelFile )]
    [Rock.SystemGuid.EntityTypeGuid( "D906B981-9603-4B5F-9009-31F6EDDE9DC3")]
    public class AnalyticsDimFamilyHistorical : AnalyticsDimFamilyBase<AnalyticsDimFamilyHistorical>
    {
        // intentionally blank. See AnalyticsDimFamilyBase, etc for the fields
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsDimFamilyHistorical Configuration Class
    /// </summary>
    public partial class AnalyticsDimFamilyHistoricalConfiguration : EntityTypeConfiguration<AnalyticsDimFamilyHistorical>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsDimFamilyHistoricalConfiguration"/> class.
        /// </summary>
        public AnalyticsDimFamilyHistoricalConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
