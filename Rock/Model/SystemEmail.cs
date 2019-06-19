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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Rock email template.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "SystemEmail" )]
    [DataContract]
    public partial class SystemEmail : Model<SystemEmail>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if the email template is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the EmailTemplate is part of the Rock core system/framework otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }
        
        /// <summary>
        /// Gets or sets the Title of the EmailTemplate 
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the Title of the EmailTemplate.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the From email address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the from email address.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the To email addresses that emails using this template should be delivered to.  If there is not a predetermined distribution list, this property can 
        /// remain empty.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a list of email addresses that the message should be delivered to. If there is not a predetermined email list, this property will 
        /// be null.
        /// </value>
        [DataMember]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the email addresses that should be sent a CC or carbon copy of an email using this template. If there is not a predetermined distribution list, this property
        /// can remain empty.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a list of email addresses that should be sent a CC or carbon copy of an email that uses this template. If there is not a predetermined
        /// distribution list, this property will be null.
        /// </value>
        [DataMember]
        public string Cc { get; set; }
        
        /// <summary>
        /// Gets or sets the email addresses that should be sent a BCC or blind carbon copy of an email using this template. If there is not a predetermined distribution list; this property 
        /// can remain empty.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing a list of email addresses that should be sent a BCC or blind carbon copy of an email that uses this template. If there is not a predetermined
        /// distribution list this property will remain null.
        /// </value>
        [DataMember]
        public string Bcc { get; set; }
        
        /// <summary>
        /// Gets or sets the subject of an email that uses this template.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the subject of an email that uses this template.
        /// </value>
        [Required]
        [MaxLength( 1000 )]
        [DataMember( IsRequired = true )]
        public string Subject { get; set; }
        
        /// <summary>
        /// Gets or sets the Body template that is used for emails that use this template.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the body template for emails that use this template.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string Body { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }
        
        #endregion

        #region Methods

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

        #endregion

    }

    #region Entity Configuration
        
    /// <summary>
    /// Email Template Configuration class.
    /// </summary>
    public partial class SystemEmailConfiguration : EntityTypeConfiguration<SystemEmail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEmailConfiguration"/> class.
        /// </summary>
        public SystemEmailConfiguration()
        {
            this.HasOptional( t => t.Category ).WithMany().HasForeignKey( t => t.CategoryId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
