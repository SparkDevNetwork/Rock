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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Displays a structured content channel item for the user to view and fill out.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Structured Content View" )]
    [Category( "Mobile > Cms" )]
    [Description( "Displays a structured content channel item for the user to view and fill out." )]
    [IconCssClass( "fa fa-list-alt" )]

    #region Block Attributes

    // TODO: We probably need an attribute for content channel(s) we are allowed
    // to load data from otherwise it _could_ be a security concern in the future
    // to allow the user to access any content channel item structured content just
    // by changing the Id number. -dsh

    // TODO: We probably need an attribute for note type to specify what note type
    // is used to save the user's fill-in-the-blank reveals. Unless that is going
    // to be a hard coded note type somewhere. -dsh

    #endregion

    public class StructuredContentView : RockMobileBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the Structured Content View block.
        /// </summary>
        public static class AttributeKeys
        {
        }

        #region Attribute Properties

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Cms.StructuredContentView";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            var config = new
            {
            };

            return config;
        }

        #endregion

        #region Methods

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the structured content for the given item.
        /// </summary>
        /// <param name="itemId">The content channel item identifier.</param>
        /// <returns>
        /// The structured content for the item.
        /// </returns>
        [BlockAction]
        public object GetStructuredContent( int itemId )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemService = new ContentChannelItemService( rockContext );

                var item = contentChannelItemService.Get( itemId );

                return new
                {
                    DocumentJson = item.StructuredContent
                };
            }
        }

        #endregion
    }
}
