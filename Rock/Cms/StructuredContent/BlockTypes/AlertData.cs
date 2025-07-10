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
using System.Runtime.Serialization;

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// The data used by the <see cref="AlertRenderer"/> block type.
    /// </summary>
    [DataContract]
    public class AlertData
    {
        /// <summary>
        /// The alignment of the text. Will be one of 'left', 'center', or 'right'.
        /// </summary>
        [DataMember( Name = "align" )]
        public string Align { get; set; }

        /// <summary>
        /// Gets or sets the content message.
        /// </summary>
        [DataMember( Name = "message" )]
        public string Message { get; set; }

        /// <summary>
        /// The type of alert to display. Will be one of 'success', 'info',
        /// 'warning', 'danger'.
        /// </summary>
        [DataMember( Name = "type" )]
        public string Type { get; set; }
    }
}
