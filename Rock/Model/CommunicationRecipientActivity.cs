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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Communication Recipient Activity POCO Entity.
    /// </summary>
    [Table( "CommunicationRecipientActivity" )]
    [DataContract]
    public partial class CommunicationRecipientActivity : Model<CommunicationRecipientActivity>
    {

        #region Constants

        /// <summary>
        /// User clicked link
        /// </summary>
        public const string ACTIVITY_TYPE_CLICK = "Click";

        #endregion

        #region Entity Properties

        /// <summary>
        /// Gets or sets the the CommunicationRecipientId of the <see cref="Rock.Model.CommunicationRecipient"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CommunicationRecipientId of the <see cref="Rock.Model.CommunicationRecipient"/>.
        /// </value>
        [DataMember]
        public int CommunicationRecipientId { get; set; }

        /// <summary>
        /// Gets or sets the activity date time.
        /// </summary>
        /// <value>
        /// The activity date time.
        /// </value>
        [DataMember]
        public DateTime ActivityDateTime { get; set; }

        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        [DataMember]
        [MaxLength(20)]
        public string ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the activity detail.
        /// </summary>
        /// <value>
        /// The activity detail.
        /// </value>
        [DataMember]
        [MaxLength(2200)]
        public string ActivityDetail { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.CommunicationRecipient"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.CommunicationRecipient"/>
        /// </value>
        public virtual CommunicationRecipient CommunicationRecipient { get; set; }

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
            return this.ActivityDetail;
        }

        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Communication Recipient Configuration class.
    /// </summary>
    public partial class CommunicationRecipientActivityConfiguration : EntityTypeConfiguration<CommunicationRecipientActivity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationRecipientActivityConfiguration"/> class.
        /// </summary>
        public CommunicationRecipientActivityConfiguration()
        {
            this.HasRequired( a => a.CommunicationRecipient ).WithMany( r => r.Activities ).HasForeignKey( a => a.CommunicationRecipientId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

