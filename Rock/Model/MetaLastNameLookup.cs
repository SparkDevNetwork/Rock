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
    /// Represents a MetaLastNameLookup <see cref="Rock.Model.MetaLastNameLookup"/>. 
    /// </summary>
    [RockDomain( "Meta" )]
    [Table( "MetaLastNameLookup" )]
    [DataContract]
    public class MetaLastNameLookup : Entity<MetaLastNameLookup>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing last name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the Count.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing count.
        /// </value>
        [DataMember]
        public int? Count { get; set; }

        /// <summary>
        /// Gets or sets the Rank.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing Rank.
        /// </value>
        [DataMember]
        public int? Rank { get; set; }

        /// <summary>
        /// Gets or sets the count in 100k.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the count in 100k.
        /// </value>
        [DataMember]
        public decimal? CountIn100k { get; set; }

        #endregion

        #region Virtual Properties

        #endregion

        #region Public Methods

        #endregion
    }

    /// <summary>
    /// MetaLastNameLookup Configuration class.
    /// </summary>
    public partial class MetaLastNameLookupConfiguration : EntityTypeConfiguration<MetaLastNameLookup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaLastNameLookupConfiguration" /> class.
        /// </summary>
        public MetaLastNameLookupConfiguration()
        {

        }
    }
}
