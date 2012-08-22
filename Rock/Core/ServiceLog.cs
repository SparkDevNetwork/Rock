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
    /// Service Log POCO Entity.
    /// </summary>
    [Table( "coreServiceLog" )]
    public partial class ServiceLog : ModelWithAttributes<ServiceLog>
    {
		/// <summary>
		/// Gets or sets the Time.
		/// </summary>
		/// <value>
		/// Time.
		/// </value>
		[DataMember]
		public DateTime? Time { get; set; }
		
		/// <summary>
		/// Gets or sets the Input.
		/// </summary>
		/// <value>
		/// Input.
		/// </value>
		[DataMember]
		public string Input { get; set; }
		
		/// <summary>
		/// Gets or sets the Type.
		/// </summary>
		/// <value>
		/// Type.
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string Type { get; set; }
		
		/// <summary>
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the Result.
		/// </summary>
		/// <value>
		/// Result.
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string Result { get; set; }
		
		/// <summary>
		/// Gets or sets the Success.
		/// </summary>
		/// <value>
		/// Success.
		/// </value>
		[Required]
		[DataMember]
		public bool Success { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "Core.ServiceLog"; } }

    }

    /// <summary>
    /// Service Log Configuration class.
    /// </summary>
    public partial class ServiceLogConfiguration : EntityTypeConfiguration<ServiceLog>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLogConfiguration"/> class.
        /// </summary>
        public ServiceLogConfiguration()
        {
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class ServiceLogDTO : DTO<ServiceLog>
    {
		/// <summary>
		/// Gets or sets the Time.
		/// </summary>
		/// <value>
		/// Time.
		/// </value>
		public DateTime? Time { get; set; }

		/// <summary>
		/// Gets or sets the Input.
		/// </summary>
		/// <value>
		/// Input.
		/// </value>
		public string Input { get; set; }

		/// <summary>
		/// Gets or sets the Type.
		/// </summary>
		/// <value>
		/// Type.
		/// </value>
		public string Type { get; set; }

		/// <summary>
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Result.
		/// </summary>
		/// <value>
		/// Result.
		/// </value>
		public string Result { get; set; }

		/// <summary>
		/// Gets or sets the Success.
		/// </summary>
		/// <value>
		/// Success.
		/// </value>
		public bool Success { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public ServiceLogDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public ServiceLogDTO( ServiceLog serviceLog )
        {
            CopyFromModel( serviceLog );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="serviceLog"></param>
        public override void CopyFromModel( ServiceLog serviceLog )
        {
            this.Id = serviceLog.Id;
            this.Guid = serviceLog.Guid;
            this.Time = serviceLog.Time;
            this.Input = serviceLog.Input;
            this.Type = serviceLog.Type;
            this.Name = serviceLog.Name;
            this.Result = serviceLog.Result;
            this.Success = serviceLog.Success;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="serviceLog"></param>
        public override void CopyToModel( ServiceLog serviceLog )
        {
            serviceLog.Id = this.Id;
            serviceLog.Guid = this.Guid;
            serviceLog.Time = this.Time;
            serviceLog.Input = this.Input;
            serviceLog.Type = this.Type;
            serviceLog.Name = this.Name;
            serviceLog.Result = this.Result;
            serviceLog.Success = this.Success;
        }
    }
}
