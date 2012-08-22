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

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class FieldTypeDTO : DTO<FieldType>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Assembly.
        /// </summary>
        /// <value>
        /// Assembly.
        /// </value>
        public string Assembly { get; set; }

        /// <summary>
        /// Gets or sets the Class.
        /// </summary>
        /// <value>
        /// Class.
        /// </value>
        public string Class { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public FieldTypeDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public FieldTypeDTO( FieldType fieldType )
        {
            CopyFromModel( fieldType );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="fieldType"></param>
        public override void CopyFromModel( FieldType fieldType )
        {
            this.Id = fieldType.Id;
            this.Guid = fieldType.Guid;
            this.IsSystem = fieldType.IsSystem;
            this.Name = fieldType.Name;
            this.Description = fieldType.Description;
            this.Assembly = fieldType.Assembly;
            this.Class = fieldType.Class;
            this.CreatedDateTime = fieldType.CreatedDateTime;
            this.ModifiedDateTime = fieldType.ModifiedDateTime;
            this.CreatedByPersonId = fieldType.CreatedByPersonId;
            this.ModifiedByPersonId = fieldType.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="fieldType"></param>
        public override void CopyToModel( FieldType fieldType )
        {
            fieldType.Id = this.Id;
            fieldType.Guid = this.Guid;
            fieldType.IsSystem = this.IsSystem;
            fieldType.Name = this.Name;
            fieldType.Description = this.Description;
            fieldType.Assembly = this.Assembly;
            fieldType.Class = this.Class;
            fieldType.CreatedDateTime = this.CreatedDateTime;
            fieldType.ModifiedDateTime = this.ModifiedDateTime;
            fieldType.CreatedByPersonId = this.CreatedByPersonId;
            fieldType.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
