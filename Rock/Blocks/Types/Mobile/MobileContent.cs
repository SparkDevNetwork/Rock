using System.Collections.Generic;
using System.ComponentModel;

using Rock.Attribute;

namespace Rock.Blocks.Types.Mobile
{
    [DisplayName( "Mobile Content" )]
    [Category( "Mobile" )]
    [Description( "Demo Mobile Block" )]
    [IconCssClass( "fa fa-align-center" )]

    #region Block Attributes

    [CodeEditorField( "Content",
        description: "The XAML to use when rendering the block. <span class='tip tip-lava'></span>",
        mode: Web.UI.Controls.CodeEditorMode.Xml,
        key: AttributeKeys.Content,
        order: 0 )]

    [LavaCommandsField( "Enabled Lava Commands",
        description: "The Lava commands that should be enabled for this block, only affects Lava rendered on the server.",
        required: false,
        key: AttributeKeys.EnabledLavaCommands,
        order: 1 )]
    
    [BooleanField( "Dynamic Content",
        "If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.",
        true,
        category: "custommobile",
        order: 0 )]

    [IntegerField( "Cache Duration",
        "The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.",
        false,
        86400,
        category: "custommobile",
        order: 1 )]

    [CustomDropdownListField( "Lava Render Location",
        "Specifies where to render the Lava",
        "On Server, On Device, Both",
        true,
        "On Server",
        category: "custommobile",
        order: 2 )]

    #endregion

    public class MobileContent : RockBlockType, IRockMobileBlockType
    {
        public static class AttributeKeys
        {
            public const string CacheDuration = "CacheDuration";

            public const string Content = "Content";

            public const string DynamicContent = "DynamicContent";

            public const string EnabledLavaCommands = "EnabledLavaCommands";

            public const string LavaRenderLocation = "LavaRenderLocation";
        }

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        int IRockMobileBlockType.RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        string IRockMobileBlockType.MobileBlockType => "Rock.Mobile.Blocks.XamlContent";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        object IRockMobileBlockType.GetMobileConfigurationValues()
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
            var content = GetAttributeValue( "Content" );
            var config = new Dictionary<string, object>();

            //
            // If we are rendering lava On Server or on Both, then render it.
            //
            if ( GetAttributeValue( AttributeKeys.LavaRenderLocation ) != "On Device" )
            {
                var mergeFields = RequestContext.GetCommonMergeFields();

                content = content.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );
            }

            config.Add( "Xaml", content );

            return config;
        }

        #endregion
    }
}
