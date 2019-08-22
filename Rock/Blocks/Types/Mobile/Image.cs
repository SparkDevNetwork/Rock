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
using System.ComponentModel;

using Rock.Attribute;

namespace Rock.Blocks.Types.Mobile
{
    /// <summary>
    /// Display an image on the mobile device screen.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Image" )]
    [Category( "Mobile" )]
    [Description( "Display an image on the mobile device screen." )]
    [IconCssClass( "fa fa-image" )]

    #region Block Attributes

    [CodeEditorField( "Image Url",
        Description = "The URL to use for displaying the image. <span class='tip tip-lava'></span>",
        EditorMode = Web.UI.Controls.CodeEditorMode.Lava,
        IsRequired = true,
        Key = AttributeKeys.ImageUrl,
        Order = 0 )]

    [BooleanField( "Dynamic Content",
        Description = "If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.",
        IsRequired = true,
        Category = "custommobile",
        Key = AttributeKeys.DynamicContent,
        Order = 0 )]

    [IntegerField( "Cache Duration",
        Description = "The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.",
        IsRequired = false,
        DefaultIntegerValue = 86400,
        Category = "custommobile",
        Key = AttributeKeys.CacheDuration,
        Order = 1 )]

    #endregion

    public class Image : RockMobileBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the MobileContent block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The cache duration key
            /// </summary>
            public const string CacheDuration = "CacheDuration";

            /// <summary>
            /// The dynamic content key
            /// </summary>
            public const string DynamicContent = "DynamicContent";

            /// <summary>
            /// The image url key
            /// </summary>
            public const string ImageUrl = "ImageUrl";
        }

        private static string Xaml = @"<Rock:RockImage ImageUrl=""{Binding ConfigurationValues[Url]}"" />";

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
        public override string MobileBlockType => "Rock.Mobile.Blocks.Content";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            //
            // Since we are such as simple block, we don't have any additional configuration
            // to provide other than the basic content that will be returned on expired cache
            // reload requests.
            //
            return GetCurrentConfig();
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the current configuration for this block.
        /// </summary>
        /// <returns>A collection of string/string pairs.</returns>
        [BlockAction]
        public object GetCurrentConfig()
        {
            var mergeFields = RequestContext.GetCommonMergeFields();

            return new Dictionary<string, object>
            {
                { "Xaml", Xaml },
                { "Url", GetAttributeValue( "ImageUrl" ).ResolveMergeFields( mergeFields, null ) },
                { "DynamicContent", GetAttributeValue( AttributeKeys.DynamicContent ).AsBoolean() },
                { "ProcessLava", false },
                { "CacheDuration", GetAttributeValue( AttributeKeys.CacheDuration ).AsInteger() }
            };
        }

        #endregion
    }
}
