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

using System.ComponentModel;
using System.Net;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Model;

namespace Rock.Obsidian.Blocks.Utility
{
    /// <summary>
    /// An example block that uses the Vue Composition API.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

    [DisplayName( "Stark Detail - Vue Composition API" )]
    [Category( "Obsidian > Utility" )]
    [Description( "An example block that uses the Vue Composition API." )]
    [IconCssClass( "fa fa-star" )]

    #region Block Attributes

    [BooleanField(
        "Show Email Address",
        Key = AttributeKey.ShowEmailAddress,
        Description = "Should the email address be shown?",
        DefaultBooleanValue = true,
        Order = 1 )]

    [EmailField(
        "Email",
        Key = AttributeKey.Email,
        Description = "The Email address to show.",
        DefaultValue = "ted@rocksolidchurchdemo.com",
        Order = 2 )]

    #endregion Block Attributes

    public class StarkDetailComposition : ObsidianBlockType
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowEmailAddress = "ShowEmailAddress";
            public const string Email = "Email";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string StarkId = "StarkId";
        }

        #endregion PageParameterKeys

        #region Base Overloads

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianConfigurationValues()
        {
            return new
            {
                Message = "This is a value that came from the server before the Vue component mounted."
            };
        }

        #endregion Base Overloads

        #region Block Actions

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <param name="paramFromClient">The parameter from client.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetMessage( string paramFromClient )
        {
            var message = $"This is a value that came from the server via AJAX after the Vue component mounted. The client had sent: {paramFromClient}";

            return new BlockActionResult( HttpStatusCode.OK, new
            {
                Message = message
            } );
        }

        #endregion Block Actions
    }
}
