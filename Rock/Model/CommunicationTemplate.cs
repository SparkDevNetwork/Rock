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

using Rock.Communication;
using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents a communication Template in Rock (i.e. email, SMS message, etc.).
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationTemplate" )]
    [DataContract]
    public partial class CommunicationTemplate : Model<CommunicationTemplate>, ICommunicationDetails
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
        /// Gets or sets a flag indicating if this PageContext is a part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the PageContext is part of the core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active communication template. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this schedule is active, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets a value indicating whether [CSS inlining enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [CSS inlining enabled]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CssInliningEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is the sender of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person" /> who is the sender of the Communication.
        /// </value>
        [DataMember]
        public int? SenderPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the image file identifier for the Template Preview Image
        /// </summary>
        /// <value>
        /// The image file identifier.
        /// </value>
        [DataMember]
        public int? ImageFileId { get; set; }

        /// <summary>
        /// Gets or sets the logo binary file identifier that email messages using this template can use for the logo in the message content
        /// </summary>
        /// <value>
        /// The logo binary file identifier.
        /// </value>
        [DataMember]
        public int? LogoBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets a Json formatted string containing the Medium specific data.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> that contains any Medium specific data.
        /// </value>
        [RockObsolete( "1.7" )]
        [Obsolete( "MediumDataJson is no longer used.", true )]
        public string MediumDataJson { get; set; }

        #region Email Fields

        /// <summary>
        /// Gets or sets the name of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the communication.
        /// </value>
        [DataMember]
        [MaxLength( 1000 )]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets from email.
        /// </summary>
        /// <value>
        /// From email.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply to email.
        /// </summary>
        /// <value>
        /// The reply to email.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the cc emails.
        /// </summary>
        /// <value>
        /// The cc emails.
        /// </value>
        [DataMember]
        public string CCEmails { get; set; }

        /// <summary>
        /// Gets or sets the BCC emails.
        /// </summary>
        /// <value>
        /// The BCC emails.
        /// </value>
        [DataMember]
        public string BCCEmails { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message meta data.
        /// </summary>
        /// <value>
        /// The message meta data.
        /// </value>
        [DataMember]
        public string MessageMetaData { get; set; }

        /// <summary>
        /// The internal storage for <see cref="CommunicationTemplate.LavaFields"/>
        /// </summary>
        /// <value>
        /// The lava fields json
        /// </value>
        [DataMember]
        public string LavaFieldsJson
        {
            get
            {
                return LavaFields.ToJson( Formatting.None );
            }

            set
            {
                LavaFields = value.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            }
        }

        #endregion

        #region SMS Properties

        /// <summary>
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        [DataMember]
        public int? SMSFromDefinedValueId { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string SMSMessage { get; set; }

        #endregion

        #region Push Notification Properties

        /// <summary>
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PushSound { get; set; }

        #endregion

        #endregion

        #region Virtual Properties

        /// <summary>
        /// A Dictionary of Key,DefaultValue for Lava MergeFields that can be used when processing Lava in the CommunicationTemplate
        /// By convention, a Key with a 'Color' suffix will indicate that the Value is selected using a ColorPicker. Otherwise,it is just text
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        [DataMember]
        public virtual Dictionary<string, string> LavaFields { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the attachments.
        /// NOTE: In most cases, you should use GetAttachments( CommunicationType ) instead.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationTemplateAttachment> Attachments
        {
            get { return _attachments ?? ( _attachments = new Collection<CommunicationTemplateAttachment>() ); }
            set { _attachments = value; }
        }
        private ICollection<CommunicationTemplateAttachment> _attachments;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> of the Communication's sender.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PersonAlias"/> that represents the Communication's sender.
        /// </value>
        [DataMember]
        public virtual PersonAlias SenderPersonAlias { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is personal (has a SenderPersonAliasId value) or not
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is personal; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public virtual bool IsPersonal => SenderPersonAliasId.HasValue;

        /// <summary>
        /// Gets or sets the logo binary file that email messages using this template can use for the logo in the message content
        /// </summary>
        /// <value>
        /// The logo binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile LogoBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets the <see cref="Rock.Communication.MediumComponent"/> for the communication medium that is being used.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Communication.MediumComponent"/> for the communication medium that is being used.
        /// </value>
        [NotMapped]
        public virtual List<MediumComponent> Mediums
        {
            get
            {
                var mediums = new List<MediumComponent>();

                foreach ( var serviceEntry in MediumContainer.Instance.Components )
                {
                    var component = serviceEntry.Value.Value;
                    if ( component.IsActive )
                    {
                        mediums.Add( component );
                    }
                }

                return mediums;
            }
        }

        /// <summary>
        /// Gets or sets the data used by the selected communication medium.
        /// </summary>
        /// <value>
        /// A <see cref="System.Collections.Generic.Dictionary{String,String}"/> of key value pairs that contain medium specific data.
        /// </value>
        [DataMember]
        [NotMapped]
        [RockObsolete( "1.7" )]
        [Obsolete( "MediumData is no longer used. Communication Template now has specific properties for medium data.", true )]
        public virtual Dictionary<string, string> MediumData
        {
            get
            {
                var mediumData = new Dictionary<string, string>();

                if ( SMSFromDefinedValueId.HasValue )
                {
                    mediumData.AddIfNotBlank( "FromValue", SMSFromDefinedValueId.Value.ToString() );
                    mediumData.AddIfNotBlank( "Subject", Subject );
                    mediumData.AddIfNotBlank( "Message", SMSMessage );
                }
                else if ( PushMessage.IsNotNullOrWhiteSpace() )
                {
                    mediumData.AddIfNotBlank( "Title", PushTitle );
                    mediumData.AddIfNotBlank( "Message", PushMessage );
                    mediumData.AddIfNotBlank( "Sound", PushSound );
                }
                else
                {
                    mediumData.AddIfNotBlank( "FromName", FromName );
                    mediumData.AddIfNotBlank( "FromAddress", FromEmail );
                    mediumData.AddIfNotBlank( "ReplyTo", ReplyToEmail );
                    mediumData.AddIfNotBlank( "CC", CCEmails );
                    mediumData.AddIfNotBlank( "BCC", BCCEmails );
                    mediumData.AddIfNotBlank( "Subject", Subject );
                    mediumData.AddIfNotBlank( "HtmlMessage", Message );
                    mediumData.AddIfNotBlank( "Attachments", AttachmentBinaryFileIds.ToList().AsDelimited( "," ) );
                }

                return mediumData;
            }

            set { }
        }

        /// <summary>
        /// Gets or sets the SMS from defined value.
        /// </summary>
        /// <value>
        /// The SMS from defined value.
        /// </value>
        [DataMember]
        public virtual DefinedValue SMSFromDefinedValue { get; set; }

        /// <summary>
        /// Gets or sets a list of binary file ids
        /// </summary>
        /// <value>
        /// The attachment binary file ids
        /// </value>
        [NotMapped]
        [RockObsolete( "1.7" )]
        [Obsolete( "Use EmailAttachmentBinaryFileIds or SMSAttachmentBinaryFileIds", true )]
        public virtual IEnumerable<int> AttachmentBinaryFileIds
        {
            get
            {
                return this.Attachments.Select( a => a.BinaryFileId ).ToList();
            }
        }

        /// <summary>
        /// Gets or sets a list of email binary file ids
        /// </summary>
        /// <value>
        /// The attachment binary file ids
        /// </value>
        [NotMapped]
        public virtual IEnumerable<int> EmailAttachmentBinaryFileIds
        {
            get
            {
                return this.Attachments.Where( a => a.CommunicationType == CommunicationType.Email ).Select( a => a.BinaryFileId ).ToList();
            }
        }

        /// <summary>
        /// Gets or sets a list of sms binary file ids
        /// </summary>
        /// <value>
        /// The attachment binary file ids
        /// </value>
        [NotMapped]
        public virtual IEnumerable<int> SMSAttachmentBinaryFileIds
        {
            get
            {
                return this.Attachments.Where( a => a.CommunicationType == CommunicationType.SMS ).Select( a => a.BinaryFileId ).ToList();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the attachment.
        /// Specify CommunicationType.Email for Email and CommunicationType.SMS for SMS
        /// </summary>
        /// <param name="communicationTemplateAttachment">The communication template attachment.</param>
        /// <param name="communicationType">Type of the communication.</param>
        public void AddAttachment( CommunicationTemplateAttachment communicationTemplateAttachment, CommunicationType communicationType )
        {
            communicationTemplateAttachment.CommunicationType = communicationType;
            this.Attachments.Add( communicationTemplateAttachment );
        }

        /// <summary>
        /// Gets the attachments.
        /// Specify CommunicationType.Email to get the attachments for Email and CommunicationType.SMS to get the Attachment(s) for SMS
        /// </summary>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns></returns>
        public IEnumerable<CommunicationTemplateAttachment> GetAttachments( CommunicationType communicationType )
        {
            return this.Attachments.Where( a => a.CommunicationType == communicationType );
        }

        /// <summary>
        /// Returns a medium data value.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> containing the key associated with the value to retrieve. </param>
        /// <returns>A <see cref="System.String"/> representing the value that is linked with the specified key.</returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "MediumData is no longer used", true )]
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
        [RockObsolete( "1.7" )]
        [Obsolete( "MediumData is no longer used", true )]
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
            return this.Name ?? this.Subject ?? base.ToString();
        }

        /// <summary>
        /// Returns true if this Communication Template has an Email Template that supports the New Communication Wizard
        /// </summary>
        /// <returns></returns>
        public bool SupportsEmailWizard()
        {
            string templateHtml = this.Message;
            if ( string.IsNullOrWhiteSpace( templateHtml ) )
            {
                return false;
            }

            templateHtml = templateHtml.ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields( null ) );

            HtmlAgilityPack.HtmlDocument templateDoc = new HtmlAgilityPack.HtmlDocument();
            templateDoc.LoadHtml( templateHtml );

            // see if there is at least one 'dropzone' div
            var hasDropzones = templateDoc.DocumentNode.Descendants( "div" ).Where( a => a.GetAttributeValue( "class", string.Empty ).Split( new char[] { ' ' } ).Any( c => c == "dropzone" ) ).Any();

            return hasDropzones;
        }

        /// <summary>
        /// Returns true if this Communication Template has text for an SMS Template
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has SMS template]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasSMSTemplate()
        {
            return !string.IsNullOrWhiteSpace( this.SMSMessage );
        }

        /// <summary>
        /// When checking for security, if a template does not have specific rules, first check the category it belongs to, but then check the default entity security for templates.
        /// </summary>
        public override ISecured ParentAuthorityPre => this.Category ?? base.ParentAuthority;

        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Communication Template Configuration class.
    /// </summary>
    public partial class CommunicationTemplateConfiguration : EntityTypeConfiguration<CommunicationTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationTemplateConfiguration"/> class.
        /// </summary>
        public CommunicationTemplateConfiguration()
        {
            this.HasOptional( c => c.Category ).WithMany().HasForeignKey( c => c.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.LogoBinaryFile ).WithMany().HasForeignKey( c => c.LogoBinaryFileId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.SenderPersonAlias ).WithMany().HasForeignKey( c => c.SenderPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.SMSFromDefinedValue ).WithMany().HasForeignKey( c => c.SMSFromDefinedValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}