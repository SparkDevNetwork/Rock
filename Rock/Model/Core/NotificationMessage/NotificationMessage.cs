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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// A single notification that will be displayed to an individual.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "NotificationMessage" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "239ADD2E-2DBF-46A7-BD28-4A2A201D4E7B" )]
    public partial class NotificationMessage : Entity<NotificationMessage>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the identifier of the <see cref="Rock.Model.NotificationMessageType"/>
        /// that handles logic for this instance.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the Id of the <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int NotificationMessageTypeId { get; set; }

        /// <summary>
        /// Gets or sets the title of the message. This should be a very short
        /// string, such as only a few words.
        /// </summary>
        /// <value>The title of the message.</value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the message. This should be a
        /// somewhat short string, such as a couple of sentences.
        /// </summary>
        /// <value>The description of the message.</value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies this instance to the component.
        /// The key allows components to look up existing messages so they can
        /// be updated. <c>null</c> values are allowed.
        /// </summary>
        /// <remarks>
        /// This property is indexed in the database so randomly generated values
        /// must not be used.
        /// </remarks>
        /// <value>
        /// A <see cref="string"/> that identifies this instance.
        /// </value>
        [MaxLength( 200 )]
        [Index]
        [DataMember]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which point the message will be
        /// shown to the individual. By default the current date and time will
        /// be used, but setting to a future date is allowed.
        /// </summary>
        /// <value>The date and time at which point the message will become visible.</value>
        [DataMember]
        public DateTime MessageDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// Gets or sets the date and time the message will automatically expire
        /// and be removed.
        /// </summary>
        /// <value>The date and time the message will be removed.</value>
        [DataMember]
        public DateTime ExpireDateTime { get; set; } = RockDateTime.Now.AddDays( 90 );

        /// <summary>
        /// Gets or sets the count of the message. This value will be summed
        /// for all visible messages and used as the total number of messages.
        /// It will also usually be displayed as a badge on the message itself.
        /// </summary>
        /// <value>The count of the message.</value>
        [DataMember]
        public int Count { get; set; } = 1;

        /// <summary>
        /// Gets or sets the person alias identifier of the individual this
        /// message should be displayed to.
        /// </summary>
        /// <value>The person alias identifier.</value>
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this message has been read.
        /// </summary>
        /// <value><c>true</c> if this message has been read; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets the component data json. This data is only understood
        /// by the component itself and should not be modified elsewhere.
        /// </summary>
        /// <value>The component data json.</value>
        [DataMember]
        public string ComponentDataJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.NotificationMessageType"/>
        /// that this instance belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.NotificationMessageType"/> of this instance.
        /// </value>
        [DataMember]
        public virtual NotificationMessageType NotificationMessageType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> of the individual
        /// this message should be displayed to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.PersonAlias"/> to display this message to.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Title;
        }

        #endregion
    }

    #region Entity Configuration    

    /// <summary>
    /// Notification Message Configuration class.
    /// </summary>
    public partial class NotificationMessageConfiguration : EntityTypeConfiguration<NotificationMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationMessageConfiguration"/> class.
        /// </summary>
        public NotificationMessageConfiguration()
        {
            this.HasRequired( nm => nm.NotificationMessageType ).WithMany( nmt => nmt.NotificationMessages ).HasForeignKey( nm => nm.NotificationMessageTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( nm => nm.PersonAlias ).WithMany().HasForeignKey( nm => nm.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
