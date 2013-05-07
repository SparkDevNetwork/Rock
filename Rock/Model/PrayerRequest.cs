//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// PrayerRequest POCO class.
    /// </summary>
    [Table( "PrayerRequest" )]
    [DataContract]
    public partial class PrayerRequest : Model<PrayerRequest>, ICategorized
    {

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [Required]
        [MaxLength ( 50 ) ]
        [DataMember( IsRequired = true )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person for who the prayer request is about.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        [DataMember]
        [MaxLength( 254 )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the requester's person id.
        /// </summary>
        /// <value>
        /// The requester's person id.
        /// </value>
        [DataMember]
        public int? RequestedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the answer for the prayer request.
        /// </summary>
        /// <value>
        /// The prayer's answer.
        /// </value>
        [DataMember]
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the date this request was entered.
        /// </summary>
        /// <value>
        /// The date the request was entered.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime EnteredDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        /// <value>
        /// The expiration date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        /// <value>
        /// The group id.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets whether or not comments can be made against the request.
        /// </summary>
        /// <value>
        /// True if comments are allowed.
        /// </value>
        [DataMember]
        public bool? AllowComments { get; set; }

        /// <summary>
        /// Gets or sets the urgent flag.
        /// </summary>
        /// <value>
        /// True if urgent.
        /// </value>
        [DataMember]
        public bool? IsUrgent { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether or not the request is public.
        /// </summary>
        /// <value>
        /// True if request is public.
        /// </value>
        [DataMember]
        public bool? IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the active flag.
        /// </summary>
        /// <value>
        /// True if active.
        /// </value>
        [DataMember]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the approved flag.
        /// </summary>
        /// <value>
        /// True if approved.
        /// </value>
        [DataMember]
        public bool? IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the number of times this request has been flagged.
        /// </summary>
        /// <value>
        /// Number of times this request has been flagged.
        /// </value>
        [DataMember]
        public int? FlagCount { get; set; }

        /// <summary>
        /// Gets or sets the prayer count.
        /// </summary>
        /// <value>
        /// The prayer count.
        /// </value>
        [DataMember]
        public int? PrayerCount { get; set; }

        /// <summary>
        /// Gets or sets the approver's person id.
        /// </summary>
        /// <value>
        /// The approver's person id.
        /// </value>
        [DataMember]
        public int? ApprovedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the date this request was approved.
        /// </summary>
        /// <value>
        /// The date the request was approved.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? ApprovedOnDate { get; set; }

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the requested by person.
        /// </summary>
        /// <value>
        /// The requested by person.
        /// </value>
        [DataMember]
        public virtual Person RequestedByPerson { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The request's group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the person who approved the request.
        /// </summary>
        /// <value>
        /// The person who approved the request.
        /// </value>
        [DataMember]
        public virtual Person ApprovedByPerson { get; set; }


        /// <summary>
        /// Gets or sets the full name of the person for who the prayer request is about.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public virtual string FullName
        {
            get
            {
                return string.Format( "{0} {1}", FirstName, LastName );
            }
        }

        /// <summary>
        /// Gets the full name (Last, First)
        /// </summary>
        public virtual string FullNameLastFirst
        {
            get
            {
                return string.Format( "{0}, {1}", LastName, FirstName );
            }
        }

        /// <summary>
        /// Name of the prayer request (required for ICategorized)
        /// </summary>
        public virtual string Name
        {
            get
            {
                return string.Format( "{0} - {1:MM/dd/yy}", FullName, EnteredDate );
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Text;
        }

        #endregion
    }

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
        }
    }
}