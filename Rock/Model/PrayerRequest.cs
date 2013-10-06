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
    /// Represents a prayer request that a person has submitted. The PrayerRequest entity implements ICategorized which means that a prayer request can belong to a category.
    /// </summary>
    [Table( "PrayerRequest" )]
    [DataContract]
    public partial class PrayerRequest : Model<PrayerRequest>, ICategorized
    {

        /// <summary>
        /// Gets or sets the First Name of the person that this prayer request is about. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the first name of the person that this prayer request is about.
        /// </value>
        [Required]
        [MaxLength ( 50 ) ]
        [DataMember( IsRequired = true )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the Last Name of the person that this prayer request is about. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the last name of the person that this prayer request is about.  
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person requesting prayer.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the email address of the person requesting prayer.
        /// </value>
        [DataMember]
        [MaxLength( 254 )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is submitting the PrayerRequest
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of <see cref="Rock.Model.Person"/> submitting the PrayerRequest.
        /// </value>
        [DataMember]
        public int? RequestedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that the PrayerRequest belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> that the PrayerRequest belongs to.
        /// </value>
        [DataMember]
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
        [Column( TypeName = "Date" )]
        public DateTime EnteredDate { get; set; }

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
        public int? ApprovedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the date this prayer request was approved.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that this prayer request was approved.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? ApprovedOnDate { get; set; }

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> who made the prayer request.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> who made the prayer request.
        /// </value>
        [DataMember]
        public virtual Person RequestedByPerson { get; set; }

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
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> who approved the prayer request.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> who approved the prayer request.
        /// </value>
        [DataMember]
        public virtual Person ApprovedByPerson { get; set; }


        /// <summary>
        /// Gets  full name of the person for who the prayer request is about.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the full name of the person who this prayer request is about.
        /// </value>
        public virtual string FullName
        {
            get
            {
                return string.Format( "{0} {1}", FirstName, LastName );
            }
        }

        /// <summary>
        /// Gets the full name of the person who this prayer request is about in Last Name, First Name format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the full name of the person who this prayer request is about in last name first name format.
        /// </value>
        public virtual string FullNameLastFirst
        {
            get
            {
                return string.Format( "{0}, {1}", LastName, FirstName );
            }
        }

        /// <summary>
        /// Gets the name of the prayer request. The format for this is the EnteredDate - FullName. This is required to implement ICategorized
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of prayer request. 
        /// </value>
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
        /// Returns a <see cref="System.String" /> containing the text of the request that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the text of the request that represents this instance.
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