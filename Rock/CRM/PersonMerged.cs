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

namespace Rock.CRM
{
    /// <summary>
    /// Person Trail POCO Entity.
    /// </summary>
    [Table( "crmPersonMerged" )]
    public partial class PersonMerged : ModelWithAttributes<PersonMerged>
    {
		/// <summary>
		/// Gets or sets the Current Id.
		/// </summary>
		/// <value>
		/// Current Id.
		/// </value>
		[Required]
		[DataMember]
		public int CurrentId { get; set; }
		
		/// <summary>
		/// Gets or sets the Current Guid.
		/// </summary>
		/// <value>
		/// Current Guid.
		/// </value>
		[Required]
		[DataMember]
		public Guid CurrentGuid { get; set; }
		
		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "CRM.PersonMerged"; } }

        /// <summary>
        /// Gets a publicly viewable unique key for the model.
        /// </summary>
        [NotMapped]
        public string CurrentPublicKey
        {
            get
            {
                string identifier = this.CurrentId.ToString() + ">" + this.CurrentGuid.ToString();
                return Rock.Security.Encryption.EncryptString( identifier );
            }
        }
    }

    /// <summary>
    /// Person Trail Configuration class.
    /// </summary>
    public partial class PersonMergedConfiguration : EntityTypeConfiguration<PersonMerged>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonMergedConfiguration"/> class.
        /// </summary>
        public PersonMergedConfiguration()
        {
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class PersonMergedDTO : DTO<PersonMerged>
    {
        /// <summary>
        /// Gets or sets the Current Id.
        /// </summary>
        /// <value>
        /// Current Id.
        /// </value>
        public int CurrentId { get; set; }

        /// <summary>
        /// Gets or sets the Current Guid.
        /// </summary>
        /// <value>
        /// Current Guid.
        /// </value>
        public Guid CurrentGuid { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public PersonMergedDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public PersonMergedDTO( PersonMerged personMerged )
        {
            CopyFromModel( personMerged );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="personMerged"></param>
        public override void CopyFromModel( PersonMerged personMerged )
        {
            this.Id = personMerged.Id;
            this.Guid = personMerged.Guid;
            this.CurrentId = personMerged.CurrentId;
            this.CurrentGuid = personMerged.CurrentGuid;
            this.CreatedDateTime = personMerged.CreatedDateTime;
            this.CreatedByPersonId = personMerged.CreatedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="personMerged"></param>
        public override void CopyToModel( PersonMerged personMerged )
        {
            personMerged.Id = this.Id;
            personMerged.Guid = this.Guid;
            personMerged.CurrentId = this.CurrentId;
            personMerged.CurrentGuid = this.CurrentGuid;
            personMerged.CreatedDateTime = this.CreatedDateTime;
            personMerged.CreatedByPersonId = this.CreatedByPersonId;
        }
    }
}
