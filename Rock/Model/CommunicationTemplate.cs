// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Communication;

namespace Rock.Model
{
    /// <summary>
    /// Represents a communication Template in Rock (i.e. email, SMS message, etc.).
    /// </summary>
    [Table( "CommunicationTemplate" )]
    [DataContract]
    public partial class CommunicationTemplate : Model<CommunicationTemplate>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the Communication Template
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the communication template
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is the sender of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person" /> who is the sender of the Communication.
        /// </value>
        [DataMember]
        public int? SenderPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Subject of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Subject of the communication.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the Communication Channel that is being used for this Communication.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the Communication Channel that is being used for this Communication. 
        /// </value>
        [DataMember]
        public int? ChannelEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a Json formatted string containing the Channel specific data.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> that contains any Channel specific data.
        /// </value>
        public string ChannelDataJson
        {
            get
            {
                return ChannelData.ToJson();
            }

            set
            {
                if ( string.IsNullOrWhiteSpace( value ) )
                {
                    ChannelData = new Dictionary<string, string>();
                }
                else
                {
                    ChannelData = JsonConvert.DeserializeObject<Dictionary<string, string>>( value );
                }
            }
        }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> of the Communication's sender.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PersonAlias"/> that represents the Communication's sender.
        /// </value>
        [DataMember]
        public virtual PersonAlias SenderPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the communications Channel that is being used by this Communication.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the communications Channel that is being used by this Communication.
        /// </value>
        [DataMember]
        public virtual EntityType ChannelEntityType { get; set; }

        /// <summary>
        /// Gets the <see cref="Rock.Communication.ChannelComponent"/> for the communication channel that is being used.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Communication.ChannelComponent"/> for the communication channel that is being used.
        /// </value>
        public virtual ChannelComponent Channel
        {
            get
            {
                if ( this.ChannelEntityType != null || this.ChannelEntityTypeId.HasValue )
                {
                    foreach ( var serviceEntry in ChannelContainer.Instance.Components )
                    {
                        var component = serviceEntry.Value.Value;

                        if ( this.ChannelEntityTypeId.HasValue &&
                            this.ChannelEntityTypeId == component.EntityType.Id )
                        {
                            return component;
                        }

                        string componentName = component.GetType().FullName;
                        if ( this.ChannelEntityType != null &&
                            this.ChannelEntityType.Name == componentName)
                        {
                            return component;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets or sets the data used by the selected communication channel.
        /// </summary>
        /// <value>
        /// A <see cref="System.Collections.Generic.Dictionary{String,String}"/> of key value pairs that contain channel specific data.
        /// </value>
        [DataMember]
        public virtual Dictionary<string, string> ChannelData
        {
            get { return _channelData; }
            set { _channelData = value; }
        }
        private Dictionary<string, string> _channelData = new Dictionary<string, string>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a channel data value.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> containing the key associated with the value to retrieve. </param>
        /// <returns>A <see cref="System.String"/> representing the value that is linked with the specified key.</returns>
        public string GetChannelDataValue( string key )
        {
            if ( ChannelData.ContainsKey( key ) )
            {
                return ChannelData[key];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets a channel data value. If the key exists, the value will be replaced with the new value, otherwise a new key value pair will be added to dictionary.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the key.</param>
        /// <param name="value">A <see cref="System.String"/> representing the value.</param>
        public void SetChannelDataValue( string key, string value )
        {
            if ( ChannelData.ContainsKey( key ) )
            {
                ChannelData[key] = value;
            }
            else
            {
                ChannelData.Add( key, value );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Subject;
        }

        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Communication Configuration class.
    /// </summary>
    public partial class CommunicationTemplateConfiguration : EntityTypeConfiguration<CommunicationTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationTemplateConfiguration"/> class.
        /// </summary>
        public CommunicationTemplateConfiguration()
        {
            this.HasOptional( c => c.SenderPersonAlias ).WithMany().HasForeignKey( c => c.SenderPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.ChannelEntityType ).WithMany().HasForeignKey( c => c.ChannelEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion


}

