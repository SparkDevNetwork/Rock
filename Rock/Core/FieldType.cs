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
    /// Field Type POCO Entity.
    /// </summary>
    [Table( "coreFieldType" )]
    public partial class FieldType : Model<FieldType>, IAuditable
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		[Required]
		[DataMember]
		public bool IsSystem { get; set; }
		
		/// <summary>
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		[DataMember]
		public string Description { get; set; }
		
		/// <summary>
		/// Gets or sets the Assembly.
		/// </summary>
		/// <value>
		/// Assembly.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Assembly { get; set; }
		
		/// <summary>
		/// Gets or sets the Class.
		/// </summary>
		/// <value>
		/// Class.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Class { get; set; }
		
		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified Date Time.
		/// </summary>
		/// <value>
		/// Modified Date Time.
		/// </value>
		[DataMember]
		public DateTime? ModifiedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified By Person Id.
		/// </summary>
		/// <value>
		/// Modified By Person Id.
		/// </value>
		[DataMember]
		public int? ModifiedByPersonId { get; set; }
		
		/// <summary>
        /// Gets a Data Transfer Object (lightweight) version of this object.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Core.DTO.FieldType"/> object.
        /// </value>
		public Rock.Core.DTO.FieldType DataTransferObject
		{
			get 
			{ 
				Rock.Core.DTO.FieldType dto = new Rock.Core.DTO.FieldType();
				dto.Id = this.Id;
				dto.Guid = this.Guid;
				dto.IsSystem = this.IsSystem;
				dto.Name = this.Name;
				dto.Description = this.Description;
				dto.Assembly = this.Assembly;
				dto.Class = this.Class;
				dto.CreatedDateTime = this.CreatedDateTime;
				dto.ModifiedDateTime = this.ModifiedDateTime;
				dto.CreatedByPersonId = this.CreatedByPersonId;
				dto.ModifiedByPersonId = this.ModifiedByPersonId;
				return dto; 
			}
		}

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "Core.FieldType"; } }
        
		/// <summary>
        /// Gets or sets the Attributes.
        /// </summary>
        /// <value>
        /// Collection of Attributes.
        /// </value>
		public virtual ICollection<Attribute> Attributes { get; set; }
        
		/// <summary>
        /// Gets or sets the Defined Types.
        /// </summary>
        /// <value>
        /// Collection of Defined Types.
        /// </value>
		public virtual ICollection<DefinedType> DefinedTypes { get; set; }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person ModifiedByPerson { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

    }
    /// <summary>
    /// Field Type Configuration class.
    /// </summary>
    public partial class FieldTypeConfiguration : EntityTypeConfiguration<FieldType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldTypeConfiguration"/> class.
        /// </summary>
        public FieldTypeConfiguration()
        {
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }
}
