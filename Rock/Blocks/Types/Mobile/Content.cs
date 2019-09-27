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
    /// Displays custom XAML content on the page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Content" )]
    [Category( "Mobile" )]
    [Description( "Displays custom XAML content on the page." )]
    [IconCssClass( "fa fa-align-center" )]

    #region Block Attributes

    [CodeEditorField( "Content",
        Description = "The XAML to use when rendering the block. <span class='tip tip-lava'></span>",
        EditorMode = Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKeys.Content,
        Order = 0 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block, only affects Lava rendered on the server.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 1 )]
    
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

    [CustomDropdownListField( "Lava Render Location",
        "Specifies where to render the Lava",
        "On Server, On Device, Both",
        IsRequired = true,
        DefaultValue = "On Server",
        Category = "custommobile",
        Key = AttributeKeys.LavaRenderLocation,
        Order = 2 )]

    [CodeEditorField( "Callback Logic",
        Description = "If you provided any callback commands in your Content then you can specify the Lava logic for handling those commands here. <span class='tip tip-laval'></span>",
        IsRequired = false,
        EditorMode = Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKeys.CallbackLogic,
        Category = "advanced",
        Order = 0 )]

    #endregion

    public class Content : RockMobileBlockType
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
            /// The content key
            /// </summary>
            public const string Content = "Content";

            /// <summary>
            /// The dynamic content key
            /// </summary>
            public const string DynamicContent = "DynamicContent";

            /// <summary>
            /// The enabled lava commands key
            /// </summary>
            public const string EnabledLavaCommands = "EnabledLavaCommands";

            /// <summary>
            /// The lava render location key
            /// </summary>
            public const string LavaRenderLocation = "LavaRenderLocation";

            /// <summary>
            /// The callback logic key
            /// </summary>
            public const string CallbackLogic = "CallbackLogic";
        }

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
            var content = GetAttributeValue( AttributeKeys.Content );
            var config = new Dictionary<string, object>();

            //
            // If we are rendering lava On Server or on Both, then render it.
            //
            if ( GetAttributeValue( AttributeKeys.LavaRenderLocation ) != "On Device" )
            {
                var mergeFields = RequestContext.GetCommonMergeFields();

                content = content.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );
            }

            // TODO: Change this to Content after next Rock.Mobile update.
            config.Add( "Xaml", content );
            config.Add( "ProcessLava", GetAttributeValue( AttributeKeys.LavaRenderLocation ) != "On Server" );
            config.Add( "CacheDuration", GetAttributeValue( AttributeKeys.CacheDuration ).AsInteger() );
            config.Add( "DynamicContent", GetAttributeValue( AttributeKeys.DynamicContent ).AsBoolean() );

            return config;
        }

        /// <summary>
        /// Gets the dynamic XAML content that should be rendered based upon the request.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        [BlockAction]
        public object GetCallbackContent( string command, Dictionary<string, object> parameters )
        {
            var content = GetAttributeValue( AttributeKeys.CallbackLogic );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Command", command );
            mergeFields.Add( "Parameters", parameters );

            var xaml = content.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );

            return new
            {
                Content = xaml
            };
        }

        #endregion
    }
}
