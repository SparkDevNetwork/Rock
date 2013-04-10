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

namespace Rock.Model
{
    /// <summary>
    /// Email Template POCO Entity.
    /// </summary>
    [Table( "EmailTemplate" )]
    [DataContract]
    public partial class EmailTemplate : Model<EmailTemplate>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        /// <value>
        /// Category.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Category { get; set; }
        
        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        /// <value>
        /// Title.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the From.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string From { get; set; }
        
        /// <summary>
        /// Gets or sets the To.
        /// </summary>
        /// <value>
        /// To.
        /// </value>
        [DataMember]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the Cc.
        /// </summary>
        /// <value>
        /// Cc.
        /// </value>
        [DataMember]
        public string Cc { get; set; }
        
        /// <summary>
        /// Gets or sets the Bcc.
        /// </summary>
        /// <value>
        /// Bcc.
        /// </value>
        [DataMember]
        public string Bcc { get; set; }
        
        /// <summary>
        /// Gets or sets the Subject.
        /// </summary>
        /// <value>
        /// Subject.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Subject { get; set; }
        
        /// <summary>
        /// Gets or sets the Body.
        /// </summary>
        /// <value>
        /// Body.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
        [DataMember]
        public virtual Person Person { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }
    }
    
    /// <summary>
    /// Email Template Configuration class.
    /// </summary>
    public partial class EmailTemplateConfiguration : EntityTypeConfiguration<EmailTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailTemplateConfiguration"/> class.
        /// </summary>
        public EmailTemplateConfiguration()
        {
            this.HasOptional( p => p.Person ).WithMany( p => p.EmailTemplates ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(true);
        }
    }
}
