using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.SystemGuid;

using Rock.Data;
using Rock.Model;

namespace church.ccv.Pastoral.Model
{
    /// <summary>
    /// Represents a pastoral care request that a person has submitted.
    /// </summary>
    [Table( "_church_ccv_Pastoral_CareRequest" )]
    [DataContract]
    public partial class CareRequest : Model<CareRequest>, IRockEntity
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the First Name of the person that this Care Request is about. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the first name of the person that this Care Request is about.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the Last Name of the person that this Care Request is about. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the last name of the person that this Care Request is about.  
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person requesting care.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the email address of the person requesting care.
        /// </value>
        [DataMember]
        [MaxLength( 254 )]
        [RegularExpression( @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+", ErrorMessage = "The Email address is invalid" )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the <see cref="Rock.Model.PersonAlias"/> who is submitting the Care Request
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonAliasId of <see cref="Rock.Model.PersonAlias"/> submitting the Care Request.
        /// </value>
        [DataMember]
        public int? RequestedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the text/content of the request.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the text/content of the request.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string RequestText { get; set; }

        /// <summary>
        /// Gets or sets the date that this care request was entered.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that this care request was entered.
        /// </value>
        [Required]
        [DataMember]
        public DateTime RequestDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Home Phone Number of the person who requested care.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the the Home Phone Number of the person who requested care.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public String HomePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Cell Phone Number of the person who requested care.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the the Cell Phone Number of the person who requested care.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public String CellPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Work Phone Number of the person who requested care.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the the Work Phone Number of the person who requested care.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public String WorkPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the <see cref="Rock.Model.PersonAlias"/> who is the worker for this request.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonAliasId of the <see cref="Rock.Model.PersonAlias"/> who is the worker for this benevolence request.
        /// </value>
        [DataMember]
        public int? WorkerPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the summary of the request result.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the summary of the request result.
        /// </value>
        [DataMember]
        public string ResultSummary { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Defined Value <see cref="Rock.Model.DefinedValue"/> representing the connection status of the Requester.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the connection status of the Requester.
        /// </value>
        [DataMember]
        [DefinedValue( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS )]
        public int? ConnectionStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Location"/> that is associated with this care request.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the <see cref="Rock.Model.Location"/> that is associated with this care request. 
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the provided next steps.
        /// </summary>
        /// <value>
        /// The provided next steps.
        /// </value>
        [DataMember]
        public string ProvidedNextSteps { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [HideFromReporting]
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.CAMPUS )]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the Type of the Care Request.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Type of the Care Request. 
        /// </value>
        [DataMember]
        public Types Type { get; set; }

        /// <summary>
        /// Enum of Care Request Types
        /// </summary>
        /// <value>
        /// An <see cref="System.Enum"/> of the Care Request types. 
        /// </value>
        public enum Types
        {
            /// <summary>
            /// General Pastoral Care Request
            /// </summary>
            Care = 0,

            /// <summary>
            /// Counseling Request
            /// </summary>
            Counseling = 1
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        public CareRequest()
            : base()
        {
            _results = new Collection<CareResult>();
            _documents = new Collection<CareDocument>();
        }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the requested by person alias.
        /// </summary>
        /// <value>
        /// The requested by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias RequestedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the approved by person alias.
        /// </summary>
        /// <value>
        /// The approved by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias WorkerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Requester's connection status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the Requester's connection status.
        /// </value>
        [DataMember]
        public virtual Rock.Model.DefinedValue ConnectionStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> that is associated with this Care Request.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Location"/> that is associated with this Care Request.
        /// </value>
        [DataMember]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that this Care Request is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that this Care Request is associated with.
        /// </value>
        [DataMember]
        public virtual Rock.Model.Campus Campus { get; set; }

        /// <summary>
        /// Gets  full name of the person for who the Care Request is about.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the full name of the person who this Care Request is about.
        /// </value>
        public virtual string FullName
        {
            get
            {
                return string.Format( "{0} {1}", FirstName, LastName );
            }
        }

        /// <summary>
        /// Gets the full name of the person who this Care Request is about in Last Name, First Name format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the full name of the person who this Care Request is about in last name first name format.
        /// </value>
        public virtual string FullNameReversed
        {
            get
            {
                return string.Format( "{0}, {1}", LastName, FirstName );
            }
        }

        /// <summary>
        /// Gets the total cost amount of care given.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> containing the total cost amount of care given.
        /// </value>
        public virtual decimal TotalAmount
        {
            get
            {
                decimal totalAmount = 0;
                foreach ( CareResult result in CareResults )
                {
                    if ( result.Amount != null )
                    {
                        totalAmount += result.Amount.Value;
                    }
                }
                return totalAmount;
            }
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="Pastoral.CareResult">CareResults</see>
        /// </summary>
        /// <value>
        /// A collection of <see cref="Pastoral.CareResult"/> entities representing the results of the Care Request.
        /// </value>
        [DataMember]
        public virtual ICollection<CareResult> CareResults
        {
            get { return _results; }
            set { _results = value; }
        }
        private ICollection<CareResult> _results;


        /// <summary>
        /// Gets or sets the documents.
        /// </summary>
        /// <value>
        /// The documents.
        /// </value>
        [DataMember]
        public virtual ICollection<CareDocument> Documents
        {
            get { return _documents ?? (_documents = new Collection<CareDocument>()); }
            set { _documents = value; }
        }
        private ICollection<CareDocument> _documents;

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
            return this.RequestText;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// CareRequest Configuration class.
    /// </summary>
    public partial class CareRequestConfiguration : EntityTypeConfiguration<CareRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareRequestConfiguration" /> class.
        /// </summary>
        public CareRequestConfiguration()
        {
            this.HasEntitySetName( "CareRequest" );

            this.HasOptional( p => p.RequestedByPersonAlias ).WithMany().HasForeignKey( p => p.RequestedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.WorkerPersonAlias ).WithMany().HasForeignKey( p => p.WorkerPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ConnectionStatusValue ).WithMany().HasForeignKey( p => p.ConnectionStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Location ).WithMany().HasForeignKey( p => p.LocationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}