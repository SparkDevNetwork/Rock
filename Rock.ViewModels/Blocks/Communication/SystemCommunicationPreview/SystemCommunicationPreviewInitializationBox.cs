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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.SystemCommunicationPreview
{
    /// <summary>
    /// 
    /// </summary>
    public class SystemCommunicationPreviewInitializationBox : EntityBagBase
    {
        /// <summary>
        ///Gets or sets the ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Get or set the title.
        /// </summary>
        public string Title { get; set; }

        ///<summary>
        /// Get or set the email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Get or set the from.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Get or set the from name.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Get or set the subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Get or set the body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Get or set the date.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Get or set the HasSendDate property.
        /// </summary>
        public bool HasSendDate { get; set; }

        /// <summary>
        /// Get or set the Person ID
        /// </summary>
        public int TargetPersonId { get; set; }

        /// <summary>
        /// Get or set the publication date
        /// </summary>
        public string PublicationDate { get; set; } 
    }
}
