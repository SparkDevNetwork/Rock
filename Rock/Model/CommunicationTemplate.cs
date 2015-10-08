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
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the Communication Medium that is being used for this Communication.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the Communication Medium that is being used for this Communication. 
        /// </value>
        [DataMember]
        public int? MediumEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a Json formatted string containing the Medium specific data.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> that contains any Medium specific data.
        /// </value>
        public string MediumDataJson
        {
            get
            {
                return MediumData.ToJson();
            }

            set
            {
                MediumData = value.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();
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
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the communications Medium that is being used by this Communication.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the communications Medium that is being used by this Communication.
        /// </value>
        [DataMember]
        public virtual EntityType MediumEntityType { get; set; }

        /// <summary>
        /// Gets the <see cref="Rock.Communication.MediumComponent"/> for the communication medium that is being used.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Communication.MediumComponent"/> for the communication medium that is being used.
        /// </value>
        public virtual MediumComponent Medium
        {
            get
            {
                if ( this.MediumEntityType != null || this.MediumEntityTypeId.HasValue )
                {
                    foreach ( var serviceEntry in MediumContainer.Instance.Components )
                    {
                        var component = serviceEntry.Value.Value;

                        if ( this.MediumEntityTypeId.HasValue &&
                            this.MediumEntityTypeId == component.EntityType.Id )
                        {
                            return component;
                        }

                        string componentName = component.GetType().FullName;
                        if ( this.MediumEntityType != null &&
                            this.MediumEntityType.Name == componentName)
                        {
                            return component;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets or sets the data used by the selected communication medium.
        /// </summary>
        /// <value>
        /// A <see cref="System.Collections.Generic.Dictionary{String,String}"/> of key value pairs that contain medium specific data.
        /// </value>
        [DataMember]
        public virtual Dictionary<string, string> MediumData
        {
            get { return _mediumData; }
            set { _mediumData = value; }
        }
        private Dictionary<string, string> _mediumData = new Dictionary<string, string>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a medium data value.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> containing the key associated with the value to retrieve. </param>
        /// <returns>A <see cref="System.String"/> representing the value that is linked with the specified key.</returns>
        public string GetMediumDataValue( string key )
        {
            if ( MediumData.ContainsKey( key ) )
            {
                return MediumData[key];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets a medium data value. If the key exists, the value will be replaced with the new value, otherwise a new key value pair will be added to dictionary.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the key.</param>
        /// <param name="value">A <see cref="System.String"/> representing the value.</param>
        public void SetMediumDataValue( string key, string value )
        {
            if ( MediumData.ContainsKey( key ) )
            {
                MediumData[key] = value;
            }
            else
            {
                MediumData.Add( key, value );
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
            this.HasOptional( c => c.MediumEntityType ).WithMany().HasForeignKey( c => c.MediumEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion


}

