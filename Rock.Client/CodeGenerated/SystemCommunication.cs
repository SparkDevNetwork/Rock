//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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


namespace Rock.Client
{
    /// <summary>
    /// Base client model for SystemCommunication that only includes the non-virtual fields. Use this for PUT/POSTs
    /// </summary>
    public partial class SystemCommunicationEntity
    {
        /// <summary />
        public int Id { get; set; }

        /// <summary />
        public string Bcc { get; set; }

        /// <summary />
        public string Body { get; set; }

        /// <summary />
        public int? CategoryId { get; set; }

        /// <summary />
        public string Cc { get; set; }

        /// <summary />
        public bool CssInliningEnabled { get; set; } = true;

        /// <summary />
        public Guid? ForeignGuid { get; set; }

        /// <summary />
        public string ForeignKey { get; set; }

        /// <summary />
        public string From { get; set; }

        /// <summary />
        public string FromName { get; set; }

        /// <summary />
        public bool? IsActive { get; set; }

        /// <summary />
        public bool IsSystem { get; set; }

        /// <summary />
        public string LavaFieldsJson { get; set; } = @"{}";

        /// <summary>
        /// If the ModifiedByPersonAliasId is being set manually and should not be overwritten with current user when saved, set this value to true
        /// </summary>
        public bool ModifiedAuditValuesAlreadyUpdated { get; set; }

        /// <summary />
        public string PushData { get; set; }

        /// <summary />
        public int? PushImageBinaryFileId { get; set; }

        /// <summary />
        public string PushMessage { get; set; }

        /// <summary />
        public int /* PushOpenAction*/? PushOpenAction { get; set; }

        /// <summary />
        public string PushOpenMessage { get; set; }

        /// <summary />
        public string PushOpenMessageJson { get; set; }

        /// <summary />
        public string PushSound { get; set; }

        /// <summary />
        public string PushTitle { get; set; }

        /// <summary />
        // Made Obsolete in Rock "1.15"
        [Obsolete( "Use SmsFromSystemPhoneNumberId instead.", false )]
        public int? SMSFromDefinedValueId { get; set; }

        /// <summary />
        public int? SmsFromSystemPhoneNumberId { get; set; }

        /// <summary />
        public string SMSMessage { get; set; }

        /// <summary />
        public string Subject { get; set; }

        /// <summary />
        public string Title { get; set; }

        /// <summary />
        public string To { get; set; }

        /// <summary>
        /// Leave this as NULL to let Rock set this
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// This does not need to be set or changed. Rock will always set this to the current date/time when saved to the database.
        /// </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Leave this as NULL to let Rock set this
        /// </summary>
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// If you need to set this manually, set ModifiedAuditValuesAlreadyUpdated=True to prevent Rock from setting it
        /// </summary>
        public int? ModifiedByPersonAliasId { get; set; }

        /// <summary />
        public Guid Guid { get; set; }

        /// <summary />
        public int? ForeignId { get; set; }

        /// <summary>
        /// Copies the base properties from a source SystemCommunication object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( SystemCommunication source )
        {
            this.Id = source.Id;
            this.Bcc = source.Bcc;
            this.Body = source.Body;
            this.CategoryId = source.CategoryId;
            this.Cc = source.Cc;
            this.CssInliningEnabled = source.CssInliningEnabled;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.From = source.From;
            this.FromName = source.FromName;
            this.IsActive = source.IsActive;
            this.IsSystem = source.IsSystem;
            this.LavaFieldsJson = source.LavaFieldsJson;
            this.ModifiedAuditValuesAlreadyUpdated = source.ModifiedAuditValuesAlreadyUpdated;
            this.PushData = source.PushData;
            this.PushImageBinaryFileId = source.PushImageBinaryFileId;
            this.PushMessage = source.PushMessage;
            this.PushOpenAction = source.PushOpenAction;
            this.PushOpenMessage = source.PushOpenMessage;
            this.PushOpenMessageJson = source.PushOpenMessageJson;
            this.PushSound = source.PushSound;
            this.PushTitle = source.PushTitle;
            #pragma warning disable 612, 618
            this.SMSFromDefinedValueId = source.SMSFromDefinedValueId;
            #pragma warning restore 612, 618
            this.SmsFromSystemPhoneNumberId = source.SmsFromSystemPhoneNumberId;
            this.SMSMessage = source.SMSMessage;
            this.Subject = source.Subject;
            this.Title = source.Title;
            this.To = source.To;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;

        }
    }

    /// <summary>
    /// Client model for SystemCommunication that includes all the fields that are available for GETs. Use this for GETs (use SystemCommunicationEntity for POST/PUTs)
    /// </summary>
    public partial class SystemCommunication : SystemCommunicationEntity
    {
        /// <summary />
        public Category Category { get; set; }

        /// <summary />
        public Dictionary<string, string> LavaFields { get; set; }

        /// <summary />
        public SystemPhoneNumber SmsFromSystemPhoneNumber { get; set; }

        /// <summary>
        /// NOTE: Attributes are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' 
        /// </summary>
        public Dictionary<string, Rock.Client.Attribute> Attributes { get; set; }

        /// <summary>
        /// NOTE: AttributeValues are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' 
        /// </summary>
        public Dictionary<string, Rock.Client.AttributeValue> AttributeValues { get; set; }
    }
}
