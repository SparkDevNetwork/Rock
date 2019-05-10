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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Event" )]
    [NotAudited]
    [Table( "AttendanceData" )]
    [DataContract]
    public partial class AttendanceData
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the value of the identifier.  This value is the primary field/key for the entity object.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [Key]
        [DataMember]
        [IncludeForReporting]
        public int Id { get; set; }

        //// ** NOTE:  We need [DataMember] on LabelData so that REST can GET and POST LabelData. 
        //// ** However, we don't have to worry about Liquid serializing this since Attendance.AttendanceData is not marked with [DataMember]
        //// ** So the only way you would get serialized Content if you intentionally requested to serializeLabelData

        /// <summary>
        /// Gets or sets the LabelData data/content of <see cref="Rock.Model.Attendance.AttendanceData"/>/>
        /// </summary>
        /// <value>
        /// A <see cref="System.Byte"/> array that contains the data/content of <see cref="Rock.Model.Attendance.AttendanceData"/>
        /// </value>
        [DataMember]
        [HideFromReporting]
        public string LabelData { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class AttendanceLabelDataConfiguration : EntityTypeConfiguration<AttendanceData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceLabelDataConfiguration"/> class.
        /// </summary>
        public AttendanceLabelDataConfiguration()
        {
        }
    }

    #endregion

}
