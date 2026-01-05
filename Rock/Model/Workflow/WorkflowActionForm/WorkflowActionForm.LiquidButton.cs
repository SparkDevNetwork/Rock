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
using Rock.Lava;

namespace Rock.Model
{
    public partial class WorkflowActionForm
    {
        /// <summary>
        /// Special class for adding a button field to liquid properties
        /// </summary>
        [LavaType( "Name", "Html", "EmailHtml" )]        
        public class LiquidButton
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the HTML.
            /// </summary>
            /// <value>
            /// The HTML.
            /// </value>
            public string Html { get; set; }

            /// <summary>
            /// Gets or sets the Email HTML.
            /// </summary>
            /// <value>
            /// The Email HTML.
            /// </value>
            public string EmailHtml { get; set; }
        }
    }
}