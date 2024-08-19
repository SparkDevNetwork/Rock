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
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a prayer request that a person has submitted. The PrayerRequest entity implements ICategorized which means that a prayer request can belong to a category.
    /// </summary>
    [RockDomain( "Prayer" )]
    [Table( "PrayerRequest" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "F13C8FD2-7702-4C79-A6A9-86440DD5DE13")]
    public partial class PrayerRequest : Model<PrayerRequest>, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the First Name of the person that this prayer request is about. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the first name of the person that this prayer request is about.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        [Previewable]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the Last Name of the person that this prayer request is about. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the last name of the person that this prayer request is about.  
        /// </value>
        [MaxLength( 50 )]
        [DataMember( IsRequired = false )]
        [Previewable]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person requesting prayer.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the email address of the person requesting prayer.
        /// </value>
        [DataMember]
        [MaxLength( 254 )]
        [Previewable]
        [EmailAddressValidation]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is submitting the PrayerRequest
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of <see cref="Rock.Model.Person"/> submitting the PrayerRequest.
        /// </value>
        [DataMember]
        public int? RequestedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that the PrayerRequest belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> that the PrayerRequest belongs to.
        /// </value>
        [DataMember]
        [IncludeForReporting]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the text/content of the request.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the text/content of the request.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a description of the way that God has answered the prayer.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains a description of how God answered the prayer request.
        /// </value>
        [DataMember]
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the date that this prayer request was entered.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that this prayer request was entered.
        /// </value>
        [DataMember]
        public DateTime EnteredDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// Gets or sets the date that the prayer request expires. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that the prayer request expires.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// TODO: GET CLARIFICATION AND DOCUMENT
        /// Gets or sets the group id.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing a <see cref="Rock.Model.Group">Group's</see> GroupId.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating  whether or not comments can be made against the request.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if comments are allowed; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? AllowComments { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an urgent prayer request.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this prayer request is urgent; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsUrgent { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether or not the request is public.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the prayer request is public; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsPublic { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this prayer request is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the prayer request is active; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the prayer request has been approved. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this prayer request has been approved; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the number of times this request has been flagged.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of times that this prayer request has been flagged.
        /// </value>
        [DataMember]
        public int? FlagCount { get; set; }

        /// <summary>
        /// Gets or sets the number of times that this prayer request has been prayed for.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of times that this prayer request has been prayed for.
        /// </value>
        [DataMember]
        public int? PrayerCount { get; set; }

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who approved this prayer request.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who approved this prayer request.
        /// </value>
        [DataMember]
        public int? ApprovedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the date this prayer request was approved.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that this prayer request was approved.
        /// </value>
        [DataMember]
        public DateTime? ApprovedOnDateTime { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the <see cref="Rock.Model.DefinedValue"/> that represents the Language for this prayer request.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing DefinedValueId of the Language's <see cref="Rock.Model.DefinedValue"/> for this prayer request.
        /// </value>
        [DataMember]
        public int? LanguageValueId { get; set; }

        /// <summary>
        /// Gets or sets the Original Text of the PrayerRequest.
        /// </summary>
        public string OriginalRequest { get; set; }

        /// <summary>
        /// Gets or sets the identified emotional sentiment DefinedValueId (if any).
        /// </summary>
        public int? SentimentEmotionValueId { get; set; }

        /// <summary>
        /// Gets or sets the bitmask of moderation flags for the PrayerRequest.
        /// </summary>
        public long ModerationFlags { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the requested by person alias.
        /// </summary>
        /// <value>
        /// The requested by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias RequestedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> that this prayer request belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Category"/> that this prayer request belongs to.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// TODO: GET CONFIRMATION AND DOCUMENT -CSF
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The request's group.
        /// </value>
        [LavaVisible]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the approved by person alias.
        /// </summary>
        /// <value>
        /// The approved by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ApprovedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [LavaVisible]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the Language <see cref="Rock.Model.DefinedValue"/> for this prayer request.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DefinedValue"/> that represents the Language for this prayer request.
        /// </value>
        [DataMember]
        public virtual DefinedValue LanguageValue { get; set; }

        #endregion Navigation Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the text of the request that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the text of the request that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Text;
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// PrayerRequest Configuration class.
    /// </summary>
    public partial class PrayerRequestConfiguration : EntityTypeConfiguration<PrayerRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrayerRequestConfiguration"/> class.
        /// </summary>
        public PrayerRequestConfiguration()
        {
            this.HasOptional( p => p.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Category ).WithMany().HasForeignKey( p => p.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RequestedByPersonAlias ).WithMany().HasForeignKey( p => p.RequestedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ApprovedByPersonAlias ).WithMany().HasForeignKey( p => p.ApprovedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.LanguageValue ).WithMany().HasForeignKey( a => a.LanguageValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}