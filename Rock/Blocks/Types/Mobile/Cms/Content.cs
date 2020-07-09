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
using Rock.Common.Mobile.Blocks.Content;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Displays custom XAML content on the page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Content" )]
    [Category( "Mobile > Cms" )]
    [Description( "Displays custom XAML content on the page." )]
    [IconCssClass( "fa fa-align-center" )]

    #region Block Attributes

    [CodeEditorField( "Content",
        Description = "The XAML to use when rendering the block. <span class='tip tip-lava'></span>",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKeys.Content,
        Order = 0 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block, only affects Lava rendered on the server.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 1 )]
    
    [BooleanField( "Dynamic Content",
        Description = "If enabled then the client will download fresh content from the server on each page (taking cache duration into account), otherwise the content will remain static.",
        IsRequired = true,
        Key = AttributeKeys.DynamicContent,
        Order = 2 )]

    [CodeEditorField( "Callback Logic",
        Description = "If you provided any callback commands in your Content then you can specify the Lava logic for handling those commands here. <span class='tip tip-laval'></span>",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
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
            var additionalSettings = GetAdditionalSettings();
            var content = GetAttributeValue( AttributeKeys.Content );

            //
            // Check if we need to render lava on the server.
            //
            if ( additionalSettings.ProcessLavaOnServer )
            {
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.Add("CurrentPage", this.PageCache);

                content = content.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );
            }

            return new Rock.Common.Mobile.Blocks.Content.Configuration
            {
                Content = content,
                ProcessLava = additionalSettings.ProcessLavaOnClient,
                CacheDuration = additionalSettings.CacheDuration,
                DynamicContent = GetAttributeValue( AttributeKeys.DynamicContent ).AsBoolean()
            };
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the initial content for this block.
        /// </summary>
        /// <returns>The initial content.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            var additionalSettings = GetAdditionalSettings();
            var content = GetAttributeValue( AttributeKeys.Content );

            //
            // Check if we need to render lava on the server.
            //
            if ( additionalSettings.ProcessLavaOnServer )
            {
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.Add("CurrentPage", this.PageCache);

                content = content.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );
            }

            return new CallbackResponse
            {
                Content = content
            };
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
            mergeFields.Add("CurrentPage", this.PageCache);

            var xaml = content.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );

            return new
            {
                Content = xaml
            };
        }

        #endregion
    }
}
