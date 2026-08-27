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
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Lava;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents an entry in the Exception Log. It is a record of an exception that was was thrown and logged by the Rock system/framework.
    /// These exceptions can include several status codes, the one most often seen is 500, but can also include 404 (when configured in Global Attributes) 
    /// and other status codes. These entities are not tracked by the <see cref="Rock.Model.Audit"/> model.
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "ExceptionLog" )]
    [DataContract]
    [CodeGenerateRest( Enums.CodeGenerateRestEndpoint.ReadOnly, DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "F61A9F8A-6DA5-49C6-BC8E-5545C5EEDA21")]
    public partial class ExceptionLog : Model<ExceptionLog>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the parent/outer ExceptionLog entity (if it exists). ExceptionLog entities are hierarchical.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the parent ExceptionId. If this ExceptionLog entity does not have a parent exception,
        /// will be null.
        /// </value>
        [DataMember]
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Site"/> that the exception occurred on. If this did not occur on a site (i.e. a job) this value will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of <see cref="Rock.Model.Site"/> that this exception occurred on.
        /// </value>
        [DataMember]
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Page"/> that the exception occurred on.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Page"/> that the exception occurred on. 
        /// If this exception did not occur on a <see cref="Rock.Model.Page"/> this value will be null.
        /// </value>
        [DataMember]
        public int? PageId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this exception has a child/inner exception. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that will be <c>true</c> if the exception has an inner exception otherwise <c>false</c> or null.
        /// </value>
        //// TODO: Consider making non-nullable when reviewing migration flattening.
        [DataMember]
        public bool? HasInnerException { get; set; }

        /// <summary>
        /// Gets or sets the StatusCode that was returned and describes the type of error.  
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> value representing the StatusCode that was returned as part of this exception. If a StatusCode was returned
        /// this value will be null.
        /// </value>
        [MaxLength( 10 )]
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the type (exception class) of the exception that occurred. i.e. System.Data.SqlClient.SqlException
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the type name of the exception that occurred. 
        /// </value>
        [MaxLength( 150 )]
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets a message that describes the exception.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the exception.
        /// </value>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the application or the object that causes the error.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the class type name/application that threw the exception.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string Source { get; set; }

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the StackTrace of the exception that occurred.
        /// </value>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the relative URL of the page that the exception occurred on.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the URL of the <see cref="Rock.Model.Page"/> that the exception occurred on. 
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets a table of the ServerVariables at the time that the exception occurred.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing a table of the ServerVariables at the time the exception occurred.
        /// </value>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string ServerVariables { get; set; }

        /// <summary>
        /// Gets or sets the full query string from the page that the exception occurred on.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the URL Query String from the page that threw the exception.
        /// </value>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string QueryString { get; set; }

        /// <summary>
        /// Gets or sets a table containing all the form items from the page request where the exception occurred.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a table containing the value of the form items posted during the page request.
        /// </value>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string Form { get; set; }

        /// <summary>
        /// Gets or sets a table containing the session cookies from the client when the exception occurred.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the session cooks from the client when the exception occurred
        /// </value>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string Cookies { get; set; }

        /// <summary>
        /// Gets the key used to group related exceptions: the <see cref="ExceptionType"/>, a pipe and the first
        /// <see cref="ExceptionLogService.DescriptionGroupingPrefixLength"/> characters of the <see cref="Description"/>,
        /// e.g. "System.NullReferenceException|Object reference not set to an instance of an object.".
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the exception group key. This is a non-persisted computed column
        /// that SQL Server derives from the other columns, so it is never set from code and is only populated on
        /// entities that were loaded from the database.
        /// </value>
        /*
            8/26/26 - MSE

            The Exception List block groups exceptions by this key in SQL, and the filtered index that covers that
            query INCLUDEs this column in place of the unbounded [Description] column. Deriving the key in SQL Server
            means every insert path (EF, raw SQL in migrations, imports) is covered with nothing to backfill, and the
            size of the index is bounded by the schema instead of by how long an install's exception messages happen
            to be. The column is added by the AddExceptionLogExceptionGroupKey migration and indexed by the
            PostV201UpdateExceptionListIndex job.

            Reason: SQL-derived grouping key so exceptions can be grouped in SQL against a bounded covering index.
        */
        [DataMember]
        [MaxLength( 406 )]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaHidden]
        [RockInternal( "20.1", true )]
        public string ExceptionGroupKey { get; private set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Site"/> that the exception occurred on.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Site"/> that the exception occurred on. If this did not occur on a site, this value will be null.
        /// </value>
        [LavaVisible]
        public virtual Rock.Model.Site Site { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Page"/> that the exception occurred on.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Page"/> that the exception occurred on. If this exception was not thrown on a <see cref="Rock.Model.Page"/>
        /// this value will be null.
        /// </value>
        [LavaVisible]
        public virtual Rock.Model.Page Page { get; set; }

        #endregion Navigation Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Exception's description that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Exception's description that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Description;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Exception Log Configuration class.
    /// </summary>
    public partial class ExceptionLogConfiguration : EntityTypeConfiguration<ExceptionLog>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLogConfiguration"/> class.
        /// </summary>
        public ExceptionLogConfiguration()
        {
            this.HasOptional( s => s.Site ).WithMany().HasForeignKey( s => s.SiteId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Page ).WithMany().HasForeignKey( p => p.PageId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}