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
    /// AnalyticsDimFamilyCurrent is SQL View off of AnalyticsDimFamilyHistorical
    /// and represents the CurrentRow record from AnalyticsDimFamilyHistorical
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsDimFamilyCurrent" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.ViewModelFile )]
    [Rock.SystemGuid.EntityTypeGuid( "B78878C9-4EB7-4EE4-BB85-D00CCA83BCEA")]
    public class AnalyticsDimFamilyCurrent : AnalyticsDimFamilyBase<AnalyticsDimFamilyCurrent>
    {
        // intentionally blank. See AnalyticsDimFamilyBase, etc for the fields
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsDimFamilyCurrent Configuration Class
    /// </summary>
    public partial class AnalyticsDimFamilyCurrentConfiguration : EntityTypeConfiguration<AnalyticsDimFamilyCurrent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsDimFamilyCurrentConfiguration"/> class.
        /// </summary>
        public AnalyticsDimFamilyCurrentConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
