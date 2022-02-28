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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Person Signal that the organization uses to flag "special care" individuals.  
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "PersonSignal" )]
    [DataContract]
    public partial class PersonSignal : Model<PersonSignal>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> that is represented by the PersonSignal. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who is represented by the PersonSignal.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.SignalType"/> that is represented by the PersonSignal. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.SignalType"/> that is represented by the PersonSignal.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int SignalTypeId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier of the individual that reported this signal.
        /// </summary>
        /// <value>
        /// The person alias identifier of the individual that reported this signal.
        /// </value>
        [Required]
        [DataMember]
        public int OwnerPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the note applied to this signal.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the date this signal expires.
        /// </summary>
        /// <value>
        /// The date this signal expires.
        /// </value>
        [DataMember]
        public System.DateTime? ExpirationDate { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> representing the person who has the signal applied to them.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> representing the person who has the signal applied to them.
        /// </value>
        [DataMember]
        public virtual Model.Person Person { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.SignalType"/> representing the signal that has been applied.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.SignalType"/> representing the signal tha thas been applied.
        /// </value>
        [DataMember]
        public virtual Model.SignalType SignalType { get; set; }

        /// <summary>
        /// Gets or sets the person alias of the individual that reported this signal.
        /// </summary>
        /// <value>
        /// The person alias of the individual that reported this signal.
        /// </value>
        [DataMember]
        public virtual PersonAlias OwnerPersonAlias { get; set; }

        /// <summary>
        /// Gets the parent security authority of this PersonSignal. Where security is inherited from.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                var signalType = SignalTypeCache.Get( this.SignalTypeId );
                return signalType ?? base.ParentAuthority;
            }
        }

        #endregion

        #region Public Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// PersonSignal Configuration class.
    /// </summary>
    public partial class PersonSignalConfiguration : EntityTypeConfiguration<PersonSignal>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonSignalConfiguration"/> class.
        /// </summary>
        public PersonSignalConfiguration()
        {
            this.HasRequired( p => p.Person ).WithMany( p => p.Signals ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.SignalType ).WithMany().HasForeignKey( p => p.SignalTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.OwnerPersonAlias ).WithMany().HasForeignKey( p => p.OwnerPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
