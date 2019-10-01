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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "PersonSearchKey" )]
    [NotAudited]
    [DataContract]
    public class PersonSearchKey : Model<PersonSearchKey>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the search Type <see cref="Rock.Model.DefinedValue" /> representing search type key.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.DefinedValue"/> identifying the search type key.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_SEARCH_KEYS )]
        public int SearchTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the search value.
        /// </summary>
        /// <value>
        /// The search value.
        /// </value>
        [DataMember]
        public string SearchValue { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the person search key type.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> object representing the person search key type.
        /// </value>
        [DataMember]
        public virtual DefinedValue SearchTypeValue { get; set; }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class PersonSearchKeyConfiguration : EntityTypeConfiguration<PersonSearchKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonSearchKeyConfiguration"/> class.
        /// </summary>
        public PersonSearchKeyConfiguration()
        {
            this.HasRequired( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.SearchTypeValue ).WithMany().HasForeignKey( p => p.SearchTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
