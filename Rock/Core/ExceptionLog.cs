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
    [Table( "coreExceptionLog" )]
    public partial class ExceptionLog : ModelWithAttributes<ExceptionLog>
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
		/// Gets or sets the Has Inner Exception.
		/// </summary>
		/// <value>
		/// Whether the exception has an inner exception..
		/// </value>
		[DataMember]
		public bool? HasInnerException { get; set; }
		
		/// <summary>
		/// Gets or sets the Person Id.
		/// </summary>
		/// <value>
		/// Person Id of the logged in person who experienced the exception..
		/// </value>
		[DataMember]
		public int? PersonId { get; set; }
		
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
		public override string AuthEntity { get { return "Core.ExceptionLog"; } }
        
		/// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person Person { get; set; }

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
			this.HasOptional( p => p.Person ).WithMany().HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class ExceptionLogDTO : DTO<ExceptionLog>
    {
        /// <summary>
        /// Gets or sets the Parent Id.
        /// </summary>
        /// <value>
        /// Parent Id.
        /// </value>
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the Site Id.
        /// </summary>
        /// <value>
        /// Site Id.
        /// </value>
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        public int? PageId { get; set; }

        /// <summary>
        /// Gets or sets the Exception Date.
        /// </summary>
        /// <value>
        /// Exception Date.
        /// </value>
        public DateTime ExceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the Has Inner Exception.
        /// </summary>
        /// <value>
        /// Has Inner Exception.
        /// </value>
        public bool? HasInnerException { get; set; }

        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Status Code.
        /// </summary>
        /// <value>
        /// Status Code.
        /// </value>
        public string StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the Exception Type.
        /// </summary>
        /// <value>
        /// Exception Type.
        /// </value>
        public string ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        /// <value>
        /// Source.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the Stack Trace.
        /// </summary>
        /// <value>
        /// Stack Trace.
        /// </value>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the Page Url.
        /// </summary>
        /// <value>
        /// Page Url.
        /// </value>
        public string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets the Server Variables.
        /// </summary>
        /// <value>
        /// Server Variables.
        /// </value>
        public string ServerVariables { get; set; }

        /// <summary>
        /// Gets or sets the Query String.
        /// </summary>
        /// <value>
        /// Query String.
        /// </value>
        public string QueryString { get; set; }

        /// <summary>
        /// Gets or sets the Form.
        /// </summary>
        /// <value>
        /// Form.
        /// </value>
        public string Form { get; set; }

        /// <summary>
        /// Gets or sets the Cookies.
        /// </summary>
        /// <value>
        /// Cookies.
        /// </value>
        public string Cookies { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public ExceptionLogDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public ExceptionLogDTO( ExceptionLog exceptionLog )
        {
            CopyFromModel( exceptionLog );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="exceptionLog"></param>
        public override void CopyFromModel( ExceptionLog exceptionLog )
        {
            this.Id = exceptionLog.Id;
            this.Guid = exceptionLog.Guid;
            this.ParentId = exceptionLog.ParentId;
            this.SiteId = exceptionLog.SiteId;
            this.PageId = exceptionLog.PageId;
            this.ExceptionDate = exceptionLog.ExceptionDate;
            this.HasInnerException = exceptionLog.HasInnerException;
            this.PersonId = exceptionLog.PersonId;
            this.StatusCode = exceptionLog.StatusCode;
            this.ExceptionType = exceptionLog.ExceptionType;
            this.Description = exceptionLog.Description;
            this.Source = exceptionLog.Source;
            this.StackTrace = exceptionLog.StackTrace;
            this.PageUrl = exceptionLog.PageUrl;
            this.ServerVariables = exceptionLog.ServerVariables;
            this.QueryString = exceptionLog.QueryString;
            this.Form = exceptionLog.Form;
            this.Cookies = exceptionLog.Cookies;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="exceptionLog"></param>
        public override void CopyToModel( ExceptionLog exceptionLog )
        {
            exceptionLog.Id = this.Id;
            exceptionLog.Guid = this.Guid;
            exceptionLog.ParentId = this.ParentId;
            exceptionLog.SiteId = this.SiteId;
            exceptionLog.PageId = this.PageId;
            exceptionLog.ExceptionDate = this.ExceptionDate;
            exceptionLog.HasInnerException = this.HasInnerException;
            exceptionLog.PersonId = this.PersonId;
            exceptionLog.StatusCode = this.StatusCode;
            exceptionLog.ExceptionType = this.ExceptionType;
            exceptionLog.Description = this.Description;
            exceptionLog.Source = this.Source;
            exceptionLog.StackTrace = this.StackTrace;
            exceptionLog.PageUrl = this.PageUrl;
            exceptionLog.ServerVariables = this.ServerVariables;
            exceptionLog.QueryString = this.QueryString;
            exceptionLog.Form = this.Form;
            exceptionLog.Cookies = this.Cookies;
        }
    }
}
