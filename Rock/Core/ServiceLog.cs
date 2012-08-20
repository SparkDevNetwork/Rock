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
        /// Gets a Data Transfer Object (lightweight) version of this object.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Core.DTO.ServiceLog"/> object.
        /// </value>
		public Rock.Core.DTO.ServiceLog DataTransferObject
		{
			get 
			{ 
				Rock.Core.DTO.ServiceLog dto = new Rock.Core.DTO.ServiceLog();
				dto.Id = this.Id;
				dto.Guid = this.Guid;
				dto.Time = this.Time;
				dto.Input = this.Input;
				dto.Type = this.Type;
				dto.Name = this.Name;
				dto.Result = this.Result;
				dto.Success = this.Success;
				return dto; 
			}
		}

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
}
