// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    [Table( "BenevolenceRequest" )]
    [DataContract]
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
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person requesting benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the email address of the person requesting benevolence.
        /// </value>
        [DataMember]
        [MaxLength( 254 )]
        [RegularExpression( @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+", ErrorMessage = "The Email address is invalid" )]
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
        /// A <see cref="System.Int32"/> representing the the Home Phone Number of the person who requested benevolence.
        /// </value>
        [DataMember]
        public String HomePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Cell Phone Number of the person who requested benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the the Cell Phone Number of the person who requested benevolence.
        /// </value>
        [DataMember]
        public String CellPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Work Phone Number of the person who requested benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the the Work Phone Number of the person who requested benevolence.
        /// </value>
        [DataMember]
        public String WorkPhoneNumber { get; set; }

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
        /// A <see cref="System.Int32"/> representing the the GovernmentId of the person who requested benevolence.
        /// </value>
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
        [DataMember]
        public int? LocationId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        public BenevolenceRequest()
            : base()
        {
            _results = new Collection<BenevolenceResult>();
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
        /// Gets  full name of the person for who the benevolence request is about.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the full name of the person who this benevolence request is about.
        /// </value>
        public virtual string FullName
        {
            get
            {
                return string.Format( "{0} {1}", FirstName, LastName );
            }
        }

        /// <summary>
        /// Gets the full name of the person who this benevolence request is about in Last Name, First Name format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the full name of the person who this benevolence request is about in last name first name format.
        /// </value>
        public virtual string FullNameReversed
        {
            get
            {
                return string.Format( "{0}, {1}", LastName, FirstName );
            }
        }

        /// <summary>
        /// Gets the total amount of benevolence given.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> containing the total amount of benevolence given.
        /// </value>
        public virtual decimal TotalAmount
        {
            get
            {
                decimal totalAmount = 0;
                foreach ( BenevolenceResult result in BenevolenceResults )
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
            this.HasOptional( p => p.Location ).WithMany().HasForeignKey( p => p.LocationId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.RequestStatusValue ).WithMany().HasForeignKey( p => p.RequestStatusValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}