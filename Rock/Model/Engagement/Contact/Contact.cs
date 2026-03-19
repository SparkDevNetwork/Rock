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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Engagement;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a contact.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "Contact" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )]
    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.CONTACT )]
    public partial class Contact : Model<Contact>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the owner person alias identifier.
        /// </summary>
        [DataMember]
        public int OwnerPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        [MaxLength( 50 )]
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        ///</summary>
        [MaxLength( 50 )]
        [DataMember]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        [DataMember]
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the photo identifier.
        /// </summary>
        [DataMember]
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets ro sets the birth day.
        /// </summary>
        [DataMember]
        public int? BirthDay { get; set; }

        /// <summary>
        /// Gets or sets the birth month.
        /// </summary>
        [DataMember]
        public int? BirthMonth { get; set; }

        /// <summary>
        /// Gets or sets the birth year of the individual.
        /// </summary>
        [DataMember]
        public int? BirthYear { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [MaxLength( 75 )]
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        [MaxLength( 20 )]
        [DataMember]
        public string MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the relationship strength.
        /// </summary>
        [DataMember]
        public RelationshipStrength RelationshipStrength { get; set; }

        /// <summary>
        /// Gets or sets the wedding Day.
        /// </summary>
        [DataMember]
        public int? WeddingDay { get; set; }

        /// <summary>
        /// Gets or sets the wedding month.
        /// </summary>
        [DataMember]
        public int? WeddingMonth { get; set; }

        /// <summary>
        /// Gets or sets the wedding year.
        /// </summary>
        [DataMember]
        public int? WeddingYear { get; set; }

        /// <summary>
        /// Gets or sets the prayer cadence.
        /// </summary>
        [DataMember]
        public OutreachCadence PrayerCadence { get; set; }

        /// <summary>
        /// Gets or sets the connection cadence.
        /// </summary>
        [DataMember]
        public OutreachCadence ConnectionCadence { get; set; }

        /// <summary>
        /// Gets or sets the relationship focus.
        /// </summary>
        [DataMember]
        public RelationshipFocus RelationshipFocus { get; set; }

        /// <summary>
        /// Gets or sets the prayer note.
        /// </summary>
        [MaxLength( 500 )]
        [DataMember]
        public string ContactNote { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this contact has accepted Jesus as their personal savior.
        /// </summary>
        [DataMember]
        public bool? HasAcceptedJesus { get; set; }

        /// <summary>
        /// Gets or sets the day of the salvation day.
        /// </summary>
        [DataMember]
        public int? SalvationDay { get; set; }

        /// <summary>
        /// Gets or sets the month of the salvation month.
        /// </summary>
        [DataMember]
        public int? SalvationMonth { get; set; }

        /// <summary>
        /// Gets or sets the year of the salvation year.
        /// </summary>
        [DataMember]
        public int? SalvationYear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this contact has been baptized.
        /// </summary>
        [DataMember]
        public bool? HasBeenBaptized { get; set; }

        /// <summary>
        /// Gets or sets the baptism day.
        /// </summary>
        [DataMember]
        public int? BaptismDay { get; set; }

        /// <summary>
        /// Gets or sets the baptism month.
        /// </summary>
        [DataMember]
        public int? BaptismMonth { get; set; }

        /// <summary>
        /// Gets or sets the baptism year.
        /// </summary>
        [DataMember]
        public int? BaptismYear { get; set; }

        /// <summary>
        /// Gets or sets the Instagram profile URL.
        /// </summary>
        [MaxLength( 75 )]
        [DataMember]
        public string InstagramProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets the Facebook profile URL.
        /// </summary>
        [MaxLength( 75 )]
        [DataMember]
        public string FacebookProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets the LinkedIn profile URL.
        /// </summary>
        [MaxLength( 75 )]
        [DataMember]
        public string LinkedInProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets the X (formerly Twitter) profile URL.
        /// </summary>
        [MaxLength( 75 )]
        [DataMember]
        public string XProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets the TikTok profile URL.
        /// </summary>
        [MaxLength( 75 )]
        [DataMember]
        public string TikTokProfileUrl { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the owner <see cref="PersonAlias"/>.
        /// </summary>
        [DataMember]
        public virtual PersonAlias OwnerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the photo <see cref="BinaryFile"/>.
        /// </summary>
        [DataMember]
        public virtual BinaryFile Photo { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Contact Configuration class.
    /// </summary>
    public partial class ContactConfiguration : EntityTypeConfiguration<Contact>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactConfiguration"/> class.
        /// </summary>
        public ContactConfiguration()
        {
            HasRequired( c => c.OwnerPersonAlias ).WithMany().HasForeignKey( c => c.OwnerPersonAliasId ).WillCascadeOnDelete( false );
            HasOptional( c => c.Photo ).WithMany().HasForeignKey( c => c.PhotoId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
