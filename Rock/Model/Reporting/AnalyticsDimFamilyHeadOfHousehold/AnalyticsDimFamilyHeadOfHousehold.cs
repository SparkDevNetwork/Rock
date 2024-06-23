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
    /// AnalyticsDimFamilyHeadOfHousehold is straight SQL View off of AnalyticsDimPersonCurrent
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsDimFamilyHeadOfHousehold" )]
    [DataContract]
    [HideFromReporting]
    [CodeGenExclude( CodeGenFeature.ViewModelFile )]
    [Rock.SystemGuid.EntityTypeGuid( "89730008-FD3F-49BE-9084-6CC5EA4DC4B3")]
    public class AnalyticsDimFamilyHeadOfHousehold : AnalyticsDimPersonBase<AnalyticsDimFamilyHeadOfHousehold>
    {
        // intentionally blank. See AnalyticsDimPersonBase, etc for the fields
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsDimFamilyHeadOfHousehold Configuration Class
    /// </summary>
    public partial class AnalyticsDimFamilyHeadOfHouseholdConfiguration : EntityTypeConfiguration<AnalyticsDimFamilyHeadOfHousehold>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsDimFamilyHeadOfHouseholdConfiguration"/> class.
        /// </summary>
        public AnalyticsDimFamilyHeadOfHouseholdConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
