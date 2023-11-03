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
#if REVIEW_NET5_0_OR_GREATER
using System.Data.Entity.ModelConfiguration;
#endif
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsDimPersonHistorical is SQL View based on AnalyticsSourcePersonHistorical
    /// and represents the historic and current records from AnalyticsSourcePersonHistorical
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsDimPersonHistorical" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "050AAA2B-43EA-4952-936C-70638D3BCC0D")]
    public class AnalyticsDimPersonHistorical : AnalyticsDimPersonBase<AnalyticsDimPersonHistorical>
    {
        // intentionally blank. See AnalyticsDimPersonBase, etc for the fields
    }
#if REVIEW_NET5_0_OR_GREATER
    public partial class AnalyticsDimPersonHistoricalConfiguration : EntityTypeConfiguration<AnalyticsDimPersonHistorical>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsSourcePersonHistoricalConfiguration"/> class.
        /// </summary>
        public AnalyticsDimPersonHistoricalConfiguration()
        {
            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier birthdates 
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            Builder.HasOne( t => t.BirthDateDim )
                .WithMany()
                .HasForeignKey( t => t.BirthDateKey )
                .OnDelete( Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict );
        }
    }
#endif
}
