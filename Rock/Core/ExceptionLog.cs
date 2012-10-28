//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Core
{
    /// <summary>
    /// Exception Log POCO Entity.
    /// </summary>
    [NotAudited]
    [Table( "coreExceptionLog" )]
    public partial class ExceptionLog : Model<ExceptionLog>
    {
        /// <summary>
        /// Gets or sets the Parent Id.
        /// </summary>
        /// <value>
        /// Parent Id of the exeption, used for linking inner exceptions..
        /// </value>
        [DataMember]
        public int? ParentId { get; set; }
        
        /// <summary>
        /// Gets or sets the Site Id.
        /// </summary>
        /// <value>
        /// Site Id that the exception occurred on..
        /// </value>
        [DataMember]
        public int? SiteId { get; set; }
        
        /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id that the exception occurred on..
        /// </value>
        [DataMember]
        public int? PageId { get; set; }
        
        /// <summary>
        /// Gets or sets the Exception Date.
        /// </summary>
        /// <value>
        /// Date / time that the exception occurred..
        /// </value>
        [Required]
        [DataMember]
        public DateTime ExceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the Created By Person Id.
        /// </summary>
        /// <value>
        /// Created By Person Id.
        /// </value>
        [DataMember]
        public int? CreatedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the created by person.
        /// </summary>
        /// <value>
        /// The created by person.
        /// </value>
        public virtual Rock.Crm.Person CreatedByPerson { get; set; }

        /// <summary>
        /// Gets or sets the Has Inner Exception.
        /// </summary>
        /// <value>
        /// Whether the exception has an inner exception..
        /// </value>
        [DataMember]
        public bool? HasInnerException { get; set; }
        
        /// <summary>
        /// Gets or sets the Status Code.
        /// </summary>
        /// <value>
        /// Status code that would have been thrown (404, 500, etc).
        /// </value>
        [MaxLength( 10 )]
        [DataMember]
        public string StatusCode { get; set; }
        
        /// <summary>
        /// Gets or sets the Exception Type.
        /// </summary>
        /// <value>
        /// Name of the exception.
        /// </value>
        [MaxLength( 150 )]
        [DataMember]
        public string ExceptionType { get; set; }
        
        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// The exception message..
        /// </value>
        [DataMember]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        /// <value>
        /// What assembly the exception occurred in..
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Source { get; set; }
        
        /// <summary>
        /// Gets or sets the Stack Trace.
        /// </summary>
        /// <value>
        /// The stack trace that was produced..
        /// </value>
        [DataMember]
        public string StackTrace { get; set; }
        
        /// <summary>
        /// Gets or sets the Page Url.
        /// </summary>
        /// <value>
        /// The URL of the page that generated the exception..
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string PageUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the Server Variables.
        /// </summary>
        /// <value>
        /// Server variables at the time of the exception..
        /// </value>
        [DataMember]
        public string ServerVariables { get; set; }
        
        /// <summary>
        /// Gets or sets the Query String.
        /// </summary>
        /// <value>
        /// Full query string..
        /// </value>
        [DataMember]
        public string QueryString { get; set; }
        
        /// <summary>
        /// Gets or sets the Form.
        /// </summary>
        /// <value>
        /// Form items at the time of the exception..
        /// </value>
        [DataMember]
        public string Form { get; set; }
        
        /// <summary>
        /// Gets or sets the Cookies.
        /// </summary>
        /// <value>
        /// Cookies at the time of the exception..
        /// </value>
        [DataMember]
        public string Cookies { get; set; }

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
        [NotMapped]
        public string DeprecatedEntityTypeName { get { return "Core.ExceptionLog"; } }
        
        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static ExceptionLog Read( int id )
        {
            return Read<ExceptionLog>( id );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Description;
        }
    }

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
            this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete( true );
        }
    }
}
