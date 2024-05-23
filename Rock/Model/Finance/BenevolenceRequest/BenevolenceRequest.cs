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
    /// Represents a benevolence request that a person has submitted.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "BenevolenceRequest" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.BENEVOLENCE_REQUEST )]
    public partial class BenevolenceRequest : Model<BenevolenceRequest>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the First Name of the person that this benevolence request is about. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the first name of the person that this benevolence request is about.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        [Previewable]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the Last Name of the person that this benevolence request is about. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the last name of the person that this benevolence request is about.  
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        [Previewable]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person requesting benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the email address of the person requesting benevolence.
        /// </value>
        [DataMember]
        [MaxLength( 254 )]
        [Previewable]
        [EmailAddressValidation]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the <see cref="Rock.Model.PersonAlias"/> who is submitting the BenevolenceRequest
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonAliasId of <see cref="Rock.Model.PersonAlias"/> submitting the BenevolenceRequest.
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
        /// Gets or sets the date that this benevolence request was entered.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that this benevolence request was entered.
        /// </value>
        [Required]
        [DataMember]
        public DateTime RequestDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Home Phone Number of the person who requested benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Home Phone Number of the person who requested benevolence.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public string HomePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Cell Phone Number of the person who requested benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Cell Phone Number of the person who requested benevolence.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public string CellPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Work Phone Number of the person who requested benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Work Phone Number of the person who requested benevolence.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public string WorkPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the <see cref="Rock.Model.PersonAlias"/> who is the case worker for this request.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonAliasId of the <see cref="Rock.Model.PersonAlias"/> who is the case worker for this benevolence request.
        /// </value>
        [DataMember]
        public int? CaseWorkerPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the GovernmentId of the person who requested benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the GovernmentId of the person who requested benevolence.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string GovernmentId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Defined Value <see cref="Rock.Model.DefinedValue"/> representing the status of the Benevolence Request.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the status of the Benevolence Request.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [DefinedValue( SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS )]
        public int? RequestStatusValueId { get; set; }

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
        [DefinedValue( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS )]
        public int? ConnectionStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Location"/> that is associated with this BenevolenceRequest.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the <see cref="Rock.Model.Location"/> that is associated with this BenevolenceRequest. 
        /// </value>
        [HideFromReporting]
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
        /// Gets or sets the benevolence type identifier.
        /// </summary>
        /// <value>
        /// The benevolence type identifier.
        /// </value>
        [HideFromReporting]
        [DataMember( IsRequired = true )]
        public int BenevolenceTypeId { get; set; }

        /// <summary>
        /// Gets the request date key.
        /// </summary>
        /// <value>
        /// The request date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int RequestDateKey
        {
            get => RequestDateTime.ToString( "yyyyMMdd" ).AsInteger();
            private set { }
        }

        #endregion Entity Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        public BenevolenceRequest()
            : base()
        {
            _results = new Collection<BenevolenceResult>();
            _documents = new Collection<BenevolenceRequestDocument>();
        }

        #endregion Constructors

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the requested by <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The requested by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias RequestedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the case worker <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The case worker person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias CaseWorkerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Requester's connection status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the Requester's connection status.
        /// </value>
        [DataMember]
        public virtual DefinedValue ConnectionStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Benevolence Request's status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the Benevolence Request's status.
        /// </value>
        [DataMember]
        public virtual DefinedValue RequestStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> that is associated with this Benevolence Request.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Location"/> that is associated with this Benevolence Request.
        /// </value>
        [DataMember]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that this Benevolence Request is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that this Benevolence Request is associated with.
        /// </value>
        [DataMember]
        public virtual Rock.Model.Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.BenevolenceResult">BenevolenceResults</see>
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.BenevolenceResult"/> entities representing the results of the Benevolence Request.
        /// </value>
        [DataMember]
        public virtual ICollection<BenevolenceResult> BenevolenceResults
        {
            get { return _results; }
            set { _results = value; }
        }

        private ICollection<BenevolenceResult> _results;

        /// <summary>
        /// Gets or sets the request source date.
        /// </summary>
        /// <value>
        /// The request source date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate RequestSourceDate { get; set; }

        /// <summary>
        /// Gets or sets the benevolence type.
        /// </summary>
        /// <value>
        /// The benevolence type.
        /// </value>
        [DataMember]
        public virtual Rock.Model.BenevolenceType BenevolenceType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BenevolenceRequestDocument">documents</see>.
        /// </summary>
        /// <value>
        /// The documents.
        /// </value>
        [DataMember]
        public virtual ICollection<BenevolenceRequestDocument> Documents
        {
            get { return _documents ?? ( _documents = new Collection<BenevolenceRequestDocument>() ); }
            set { _documents = value; }
        }

        private ICollection<BenevolenceRequestDocument> _documents;

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
            return this.RequestText;
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// BenevolenceRequest Configuration class.
    /// </summary>
    public partial class BenevolenceRequestConfiguration : EntityTypeConfiguration<BenevolenceRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BenevolenceRequestConfiguration" /> class.
        /// </summary>
        public BenevolenceRequestConfiguration()
        {
            this.HasOptional( p => p.RequestedByPersonAlias ).WithMany().HasForeignKey( p => p.RequestedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.CaseWorkerPersonAlias ).WithMany().HasForeignKey( p => p.CaseWorkerPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ConnectionStatusValue ).WithMany().HasForeignKey( p => p.ConnectionStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Location ).WithMany().HasForeignKey( p => p.LocationId ).WillCascadeOnDelete( false );

            this.HasRequired( p => p.BenevolenceType ).WithMany( p => p.BenevolenceRequests ).HasForeignKey( p => p.BenevolenceTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.RequestStatusValue ).WithMany().HasForeignKey( p => p.RequestStatusValueId ).WillCascadeOnDelete( false );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier RequestSourceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasRequired( r => r.RequestSourceDate ).WithMany().HasForeignKey( r => r.RequestDateKey ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}