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
    [RockDomain( "Reporting" )]
    [Table( "DataViewPersistedValue" )]
    [DataContract]
    [HideFromReporting]
    public class DataViewPersistedValue
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the data view identifier.
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        [DataMember]
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public int DataViewId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public int EntityId { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the data view.
        /// </summary>
        /// <value>
        /// The data view.
        /// </value>
        [DataMember]
        public virtual DataView DataView { get; set; }

        #endregion Virtual Properties
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class DataViewPersistedValueConfiguration : EntityTypeConfiguration<DataViewPersistedValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceConfiguration"/> class.
        /// </summary>
        public DataViewPersistedValueConfiguration()
        {
            this.HasKey( k => new { k.DataViewId, k.EntityId } );
            this.HasRequired( a => a.DataView ).WithMany().HasForeignKey( p => p.DataViewId ).WillCascadeOnDelete( true );
        }
    }
}
