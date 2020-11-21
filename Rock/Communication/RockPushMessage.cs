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
using System.Collections.Generic;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Rock Push Message
    /// </summary>
    /// <seealso cref="Rock.Communication.RockMessage" />
    public class RockPushMessage : RockMessage
    {
        /// <summary>
        /// Gets the medium entity type identifier.
        /// </summary>
        /// <value>
        /// The medium entity type identifier.
        /// </value>
        public override int MediumEntityTypeId
        {
            get
            {
                return EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() ).Id;
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the sound.
        /// </summary>
        /// <value>
        /// The sound.
        /// </value>
        public string Sound { get; set; }

        /// <summary>
        /// Gets or sets the push image binary file identifier.
        /// </summary>
        /// <value>
        /// The push image binary file identifier.
        /// </value>
        public int? ImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the push open action.
        /// </summary>
        /// <value>
        /// The push open action.
        /// </value>
        public PushOpenAction? OpenAction { get; set; }

        /// <summary>
        /// Gets or sets the push open message.
        /// </summary>
        /// <value>
        /// The push open message.
        /// </value>
        public string OpenMessage { get; set; }

        /// <summary>
        /// Gets or sets the push data.
        /// </summary>
        /// <value>
        /// The push data.
        /// </value>
        public PushData Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockPushMessage"/> class.
        /// </summary>
        public RockPushMessage() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockPushMessage"/> class.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        public RockPushMessage( SystemCommunication systemCommunication ) : this()
        {
            InitializePushMessage( systemCommunication );
        }

        private void InitializePushMessage( SystemCommunication systemCommunication )
        {
            if ( systemCommunication == null )
            {
                return;
            }

            Message = systemCommunication.PushMessage;
            Title = systemCommunication.PushTitle;
            Sound = systemCommunication.PushSound;
            ImageBinaryFileId = systemCommunication.PushImageBinaryFileId;
            OpenAction = systemCommunication.PushOpenAction;
            OpenMessage = systemCommunication.PushOpenMessage;
            Data = Newtonsoft.Json.JsonConvert.DeserializeObject<PushData>( systemCommunication.PushData );
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        public void SetRecipients( List<RockPushMessageRecipient> recipients )
        {
            this.Recipients = new List<RockMessageRecipient>();
            this.Recipients.AddRange( recipients );
        }
    }
}
