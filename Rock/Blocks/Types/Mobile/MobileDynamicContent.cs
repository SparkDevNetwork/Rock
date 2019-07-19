using System.Collections.Generic;
using System.ComponentModel;

using Rock.Attribute;

namespace Rock.Blocks.Types.Mobile
{
    /// <summary>
    /// Displays XAML content that can respond to user interaction.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="Rock.Blocks.IRockMobileBlockType" />

    [DisplayName( "Mobile Dynamic Content" )]
    [Category( "Mobile" )]
    [Description( "Displays XAML content that can respond to user interaction." )]
    [IconCssClass( "fa fa-magic" )]

    #region Block Attributes

    [CodeEditorField( "Content",
        Description = "The XAML to use when rendering the block. <span class='tip tip-lava'></span>",
        EditorMode = Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKeys.Content,
        Order = 0 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 1 )]

    [CustomDropdownListField( "Initial Content",
        description: "If the initial content should be static or dynamic.",
        listSource: "Static,Dynamic",
        IsRequired = true,
        DefaultValue = "Static",
        Key = AttributeKeys.InitialContent,
        Order = 2 )]

    #endregion

    public class MobileDynamicContent : RockBlockType, IRockMobileBlockType
    {
        /// <summary>
        /// Defines the block setting attribute keys for the MobileDynamicContent block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The content key
            /// </summary>
            public const string Content = "Content";

            /// <summary>
            /// The enabled lava commands key
            /// </summary>
            public const string EnabledLavaCommands = "EnabledLavaCommands";

            /// <summary>
            /// The initial content key
            /// </summary>
            public const string InitialContent = "InitialContent";
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
        string IRockMobileBlockType.MobileBlockType => "Rock.Mobile.Blocks.DynamicContent";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        object IRockMobileBlockType.GetMobileConfigurationValues()
        {
            var content = GetAttributeValue( AttributeKeys.InitialContent ) == "Dynamic" ? null : GetStartupContent();

            return new {
                InitialContent = content
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the startup XAML content that should be rendered.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public string GetStartupContent()
        {
            var content = GetAttributeValue( AttributeKeys.Content );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Action", string.Empty );
            mergeFields.Add( "Parameters", new Dictionary<string, object>() );

            return content.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );
        }

        /// <summary>
        /// Gets the dynamic XAML content that should be rendered based upon the request.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        [BlockAction]
        public string GetDynamicContent( string action, Dictionary<string, object> parameters )
        {
            var content = GetAttributeValue( AttributeKeys.Content );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Parameters", parameters );

            return content.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );
        }

        #endregion
    }
}
