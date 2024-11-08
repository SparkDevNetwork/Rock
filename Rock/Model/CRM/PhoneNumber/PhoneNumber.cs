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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Phone Number POCO Entity.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "PhoneNumber" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "AD7E27CB-036D-40C6-B352-81B38BFAE798")]
    public partial class PhoneNumber : Model<PhoneNumber>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if the PhoneNumber is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean" /> value that is <c>true</c> if the PhoneNumber is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person" /> that the PhoneNumber belongs to. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> that the PhoneNumber belongs to.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        /// <value>
        /// The country code.
        /// </value>
        [MaxLength( 3 )]
        [DataMember]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the phone number without country code. The number is stored without any string formatting. (i.e. (502) 555-1212 will be stored as 5025551212). This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the phone number without string formatting.
        /// </value>
        [Required]
        [MaxLength( 20 )]
        [DataMember( IsRequired = true )]
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the formatted number. Note: value is recalculated on every add/modify of entity during context's default SaveChanges() method.
        /// </summary>
        /// <value>
        /// The number formatted.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string NumberFormatted { get; set; }

        /// <summary>
        /// Gets or sets the phone number reversed. This is the fastest way to search by phone number ending in xxxx.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the phone number without string formatting.
        /// </value>
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        public string NumberReversed { get; set; }

        /// <summary>
        /// Gets or sets the extension (if any) that would need to be dialed to contact the owner. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the extensions that would need to be dialed to contact the owner. If no extension is required, this property will be null. 
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public string Extension { get; set; }

        /// <summary>
        /// Gets the Phone Number's Number Type <see cref="Rock.Model.DefinedValue"/> Id.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Number Type <see cref="Rock.Model.DefinedValue"/> Id. If unknown, this value will be null.
        /// </value>
        [DataMember]
        public int? NumberTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the number has been opted in for SMS
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if the phone number has opted in for SMS messaging; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the PhoneNumber is unlisted or not.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the PhoneNumber is unlisted; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsUnlisted { get; set; }

        /// <summary>
        /// Gets or sets an optional description of the PhoneNumber.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing an optional description of the PhoneNumber.
        /// </value>
        [DataMember]
        [LavaHidden]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the phone number has opted out of messages through the SMS provider.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the PhoneNumber has opted out of messaging; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsMessagingOptedOut { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the phone number opted out of messaging.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time when the phone number opted out of messaging. If the opted out date is not known or the phone number has not opted out this value will be null.
        /// </value>
        [DataMember]
        public DateTime? MessagingOptedOutDateTime { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Phone Number's NumberType <see cref="Rock.Model.DefinedValue"/>
        /// </summary>
        /// <value>
        /// The Number Type <see cref="Rock.Model.DefinedValue"/> of the phone number.
        /// </value>
        [DataMember]
        public virtual Model.DefinedValue NumberTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> who the PhoneNumber belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> that the phone number belongs to.
        /// </value>
        [LavaVisible]
        public virtual Person Person { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Phone Number Configuration class.
    /// </summary>
    public partial class PhoneNumberConfiguration : EntityTypeConfiguration<PhoneNumber>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberConfiguration"/> class.
        /// </summary>
        public PhoneNumberConfiguration()
        {
            this.HasRequired( p => p.Person ).WithMany( p => p.PhoneNumbers ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.NumberTypeValue ).WithMany().HasForeignKey( p => p.NumberTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
