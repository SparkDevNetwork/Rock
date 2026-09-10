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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonLeft
{
    /// <summary>
    /// Input to the SendSms block action of the Check-in Manager Person
    /// Profile (limited) block.
    /// </summary>
    public class PersonLeftSendSmsRequestBag
    {
        /// <summary>
        /// Gets or sets the SMS body text to send.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the optional BinaryFile attachment
        /// uploaded by the image uploader. Null when no attachment.
        /// </summary>
        public Guid? AttachmentGuid { get; set; }
    }
}
