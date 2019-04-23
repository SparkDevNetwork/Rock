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

namespace Rock.Communication
{
    /// <summary>
    /// Helper class used to edit communications
    /// </summary>
    /// <seealso cref="Rock.Communication.ICommunicationDetails" />
    [Serializable]
    public class CommunicationDetails: ICommunicationDetails
    {
        #region Email Fields

        /// <summary>
        /// Gets or sets the name of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the communication.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets from email.
        /// </summary>
        /// <value>
        /// From email.
        /// </value>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply to email.
        /// </summary>
        /// <value>
        /// The reply to email.
        /// </value>
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the cc emails.
        /// </summary>
        /// <value>
        /// The cc emails.
        /// </value>
        public string CCEmails { get; set; }

        /// <summary>
        /// Gets or sets the BCC emails.
        /// </summary>
        /// <value>
        /// The BCC emails.
        /// </value>
        public string BCCEmails { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message meta data.
        /// </summary>
        /// <value>
        /// The message meta data.
        /// </value>
        public string MessageMetaData { get; set; }

        /// <summary>
        /// Gets or sets the email attachment binary file ids.
        /// </summary>
        /// <value>
        /// The email attachment binary file ids.
        /// </value>
        public IEnumerable<int> EmailAttachmentBinaryFileIds { get; set; }

        #endregion

        #region SMS Properties

        /// <summary>
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        public int? SMSFromDefinedValueId { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string SMSMessage { get; set; }

        /// <summary>
        /// Gets or sets the SMS attachment binary file ids.
        /// </summary>
        /// <value>
        /// The SMS attachment binary file ids.
        /// </value>
        public IEnumerable<int> SMSAttachmentBinaryFileIds { get; set; }

        #endregion

        #region Push Notification Properties

        /// <summary>
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        public string PushSound { get; set; }

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public static void Copy( ICommunicationDetails source, ICommunicationDetails target )
        {
            target.FromName = source.FromName;
            target.FromEmail = source.FromEmail;
            target.ReplyToEmail = source.ReplyToEmail;
            target.CCEmails = source.CCEmails;
            target.BCCEmails = source.BCCEmails;
            target.Subject = source.Subject;
            target.Message = source.Message;
            target.MessageMetaData = source.MessageMetaData;

            target.PushTitle = source.PushTitle;
            target.PushMessage = source.PushMessage;
            target.PushSound = source.PushSound;

            target.SMSFromDefinedValueId = source.SMSFromDefinedValueId;
            target.SMSMessage = source.SMSMessage;
        }

        #endregion

    }
}
