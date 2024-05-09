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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.RealTime;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Utility.RealTimeVisualizer;
using Rock.ViewModels.Cms;
using Rock.Web.Cache;

namespace Rock.Blocks.Utility
{
    /// <summary>
    /// Displays RealTime events from Rock with custom formatting options.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "RealTime Visualizer" )]
    [Category( "Utility" )]
    [Description( "Displays RealTime events from Rock with custom formatting options." )]
    [IconCssClass( "fa fa-chart-bar" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [KeyValueListField( "Channels",
        Description = "The list of topics and channels to subscribe to.",
        KeyPrompt = "Topic",
        ValuePrompt = "Channel",
        IsRequired = true,
        Category = "CustomSetting",
        Key = AttributeKeys.Channels )]

    [BlockTemplateField( "Template",
        Description = "The lava template to use when processing the message for display. The 'Message' variable will contain the message name and 'Args' variable will be an array of the message arguments.",
        TemplateBlockValueGuid = "74ae7a3d-7335-439c-8c06-ae30b033a82b",
        DefaultValue = "",
        IsRequired = true,
        Order = 0,
        Key = AttributeKeys.Template )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 1 )]

    [TextField( "Theme",
        Description = "The theme of the visualizer when rendering items.",
        DefaultValue = "",
        IsRequired = true,
        Category = "CustomSetting",
        Key = AttributeKeys.Theme )]

    [TextField( "Theme Settings",
        Description = "The custom settings for the selected theme.",
        DefaultValue = "",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKeys.ThemeSettings )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "77f4ea4a-ce87-4309-a7a0-2a1a75ab61cd" )]
    [Rock.SystemGuid.BlockTypeGuid( "ce185083-df13-48f9-8c97-83eda1ca65c2" )]
    public class RealTimeVisualizer : RockDetailBlockType, IHasCustomActions
    {
        #region Keys

        private static class AttributeKeys
        {
            public const string Channels = "Channels";
            public const string Template = "Template";
            public const string EnabledLavaCommands = "EnabledLavaCommands";

            public const string Theme = "Theme";
            public const string ThemeSettings = "ThemeSettings";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the lava template to use when displaying messages.
        /// </summary>
        /// <value>
        /// The lava template to use when displaying messages.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var theme = GetThemeTemplate();
            var settings = GetCurrentSettings( theme );
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.Add( "Settings", settings );

            return new
            {
                HasItemTemplate = Template.IsNotNullOrWhiteSpace(),
                Topics = GetTopicsAndChannels().Select( t => t.Topic ).Distinct().ToList(),
                PageTemplate = theme.PageTemplate?.ResolveMergeFields( mergeFields ) ?? string.Empty,
                Script = theme.Script ?? string.Empty,
                Style = theme.Style?.ResolveMergeFields( mergeFields ) ?? string.Empty,
                Settings = GetCurrentSettings( theme )
            };
        }

        private List<KeyValuePair<string, string>> GetKeyValueList( string dataValue )
        {
            var valuePairs = dataValue?.Split( '|' ) ?? new string[0];
            var values = new List<KeyValuePair<string, string>>();

            foreach ( string valuePair in valuePairs )
            {
                var keyAndValue = valuePair.Split( new char[] { '^' } );

                // url decode array items just in case they were UrlEncoded (in the KeyValueList controls)
                keyAndValue = keyAndValue.Select( s => s.GetFullyUrlDecodedValue() ).ToArray();

                if ( keyAndValue.Length != 2 )
                {
                    continue;
                }

                var key = keyAndValue[0];
                var value = keyAndValue[1];

                if ( key.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                values.Add( new KeyValuePair<string, string>( key, value ) );
            }

            return values;
        }

        private string ToKeyValueList( IEnumerable<KeyValuePair<string, string>> items )
        {
            return items
                .Select( item => $"{item.Key.UrlEncode()}^{item.Value.UrlEncode()}" )
                .JoinStrings( "|" );
        }

        private List<TopicAndChannelBag> GetTopicsAndChannels()
        {
            return GetKeyValueList( GetAttributeValue( AttributeKeys.Channels ) )
                .Where( kv => kv.Value.IsNotNullOrWhiteSpace() )
                .Select( kv => new TopicAndChannelBag
                {
                    Topic = kv.Key,
                    Channel = kv.Value
                } )
                .ToList();
        }

        private ThemeTemplate GetThemeTemplate()
        {
            var themeValue = GetAttributeValue( AttributeKeys.Theme );

            if ( Guid.TryParse( themeValue, out var themeGuid ) )
            {
                var dv = DefinedValueCache.Get( themeGuid );

                if ( dv == null )
                {
                    return new ThemeTemplate();
                }

                var settings = new Dictionary<string, string>();

                foreach ( var setting in GetKeyValueList( dv.GetAttributeValue( "Settings" ) ) )
                {
                    settings.AddOrReplace( setting.Key, setting.Value );
                }

                return new ThemeTemplate
                {
                    PageTemplate = dv.GetAttributeValue( "PageTemplate" ),
                    Script = dv.GetAttributeValue( "Script" ),
                    Style = dv.GetAttributeValue( "Style" ),
                    Settings = settings
                };
            }

            return themeValue.FromJsonOrNull<ThemeTemplate>() ?? new ThemeTemplate();
        }

        private Dictionary<string, string> GetCurrentSettings( ThemeTemplate theme )
        {
            var settings = GetAttributeValue( AttributeKeys.ThemeSettings )
                .FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();

            foreach ( var kvp in theme.Settings )
            {
                settings.TryAdd( kvp.Key, kvp.Value );
            }

            return settings;
        }

        private List<ThemeListItemBag> GetThemeBags()
        {
            return DefinedTypeCache.Get( Guid.Parse( "b8a57dfe-827a-40c1-b8de-f6ea0c50b864" ) )
                .DefinedValues
                .Select( dv => new ThemeListItemBag
                {
                    Value = dv.Guid.ToString(),
                    Text = dv.Value,
                    HelpContent = dv.GetAttributeValue( "HelpContent" ),
                    Settings = GetKeyValueList( dv.GetAttributeValue( "Settings" ) )
                        .Select( kvp => kvp.Key )
                        .ToList()
                } )
                .ToList();
        }

        private List<string> GetAvailableTopics()
        {
            return RealTimeHelper.Engine.GetTopicConfigurations()
                .Select( tc => tc.TopicIdentifier )
                .ToList();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant()
                .ToToken();
        }

        #endregion

        #region IHasCustomActions

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "fa fa-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/Utility/realTimeVisualizerCustomSettings.obs"
                } );
            }

            return actions;
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public async Task<BlockActionResult> Subscribe( string connectionId )
        {
            foreach ( var topicAndChannel in GetTopicsAndChannels() )
            {
                var topic = RealTimeHelper.Engine.GetTopicConfigurations()
                    .Where( c => c.TopicIdentifier == topicAndChannel.Topic )
                    .FirstOrDefault();

                if ( topic == null )
                {
                    return ActionBadRequest( $"Invalid topic '{topicAndChannel.Topic}'." );
                }

                var getTopicContextGeneric = typeof( RealTimeHelper )
                    .GetMethod( nameof( RealTimeHelper.GetTopicContext ), BindingFlags.Public | BindingFlags.Static );

                if ( getTopicContextGeneric == null )
                {
                    return ActionBadRequest( "Unable to resolve GetTopicContext method." );
                }

                var getTopicContext = getTopicContextGeneric.MakeGenericMethod( topic.ClientInterfaceType );

                var context = ( ITopic ) getTopicContext.Invoke( null, Array.Empty<object>() );

                await context.Channels.AddToChannelAsync( connectionId, topicAndChannel.Channel );
            }

            return ActionOk();
        }

        [BlockAction]
        public BlockActionResult Resolve( string topicIdentifier, string message, object[] arguments )
        {
            var enabledLavaCommands = GetAttributeValue( AttributeKeys.EnabledLavaCommands );
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.AddOrReplace( "Topic", topicIdentifier );
            mergeFields.AddOrReplace( "Message", message );
            mergeFields.AddOrReplace( "Args", LavaHelper.JavaScriptObjectToLavaObject( arguments ) );

            var result = Template.ResolveMergeFields( mergeFields, null, enabledLavaCommands ).Trim();

            return ActionOk( result );
        }

        /// <summary>
        /// Gets the values and all other required details that will be needed
        /// to display the custom settings modal.
        /// </summary>
        /// <returns>A box that contains the custom settings values and additional data.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit block settings." );
                }

                var options = new CustomSettingsOptionsBag
                {
                    Themes = GetThemeBags(),
                    Topics = GetAvailableTopics()
                };

                var settings = new CustomSettingsBag
                {
                    TopicConfiguration = GetTopicsAndChannels()
                };

                var themeGuid = GetAttributeValue( AttributeKeys.Theme ).AsGuidOrNull();
                var theme = GetThemeTemplate();

                if ( themeGuid.HasValue )
                {
                    settings.ThemeGuid = themeGuid.Value;
                    settings.ThemeSettings = GetAttributeValue( AttributeKeys.ThemeSettings )
                        .FromJsonOrNull<Dictionary<string, string>>()
                        ?? new Dictionary<string, string>();
                }
                else
                {
                    settings.PageTemplate = theme.PageTemplate;
                    settings.Script = theme.Script;
                    settings.Style = theme.Style;
                    settings.ThemeSettings = new Dictionary<string, string>();
                }

                return ActionOk( new CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag>
                {
                    Settings = settings,
                    Options = options,
                    SecurityGrantToken = GetSecurityGrantToken()
                } );
            }
        }

        /// <summary>
        /// Saves the updates to the custom setting values for this block.
        /// </summary>
        /// <param name="box">The box that contains the setting values.</param>
        /// <returns>A response that indicates if the save was successful or not.</returns>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit block settings." );
                }

                var block = new BlockService( rockContext ).Get( BlockId );
                block.LoadAttributes( rockContext );

                if ( box.IsValidProperty( nameof( box.Settings.ThemeGuid ) ) )
                {
                    if ( box.Settings.ThemeGuid.HasValue )
                    {
                        if ( !box.IsValidProperty( nameof( box.Settings.ThemeSettings ) ) )
                        {
                            return ActionBadRequest( $"{nameof( box.Settings.ThemeSettings )} is required if {nameof( box.Settings.ThemeGuid )} has a value." );
                        }

                        block.SetAttributeValue( AttributeKeys.Theme, box.Settings.ThemeGuid.ToString() );
                        block.SetAttributeValue( AttributeKeys.ThemeSettings, box.Settings.ThemeSettings.ToJson() );
                    }
                    else
                    {
                        var theme = block.GetAttributeValue( AttributeKeys.Theme ).FromJsonOrNull<ThemeTemplate>() ?? new ThemeTemplate();

                        box.IfValidProperty( nameof( box.Settings.PageTemplate ),
                            () => theme.PageTemplate = box.Settings.PageTemplate );

                        box.IfValidProperty( nameof( box.Settings.Script ),
                            () => theme.Script = box.Settings.Script );

                        box.IfValidProperty( nameof( box.Settings.Style ),
                            () => theme.Style = box.Settings.Style );

                        block.SetAttributeValue( AttributeKeys.Theme, theme.ToJson() );
                        block.SetAttributeValue( AttributeKeys.ThemeSettings, "" );
                    }
                }

                box.IfValidProperty( nameof( box.Settings.TopicConfiguration ), () =>
                {
                    var topicsAndChannels = box.Settings.TopicConfiguration
                        .Select( tc => new KeyValuePair<string, string>( tc.Topic, tc.Channel ) );

                    block.SetAttributeValue( AttributeKeys.Channels, ToKeyValueList( topicsAndChannels ) );
                } );

                block.SaveAttributeValues( rockContext );

                return ActionOk();
            }
        }

        #endregion

        #region Support Classes

        private class ThemeTemplate
        {
            public string PageTemplate { get; set; } = string.Empty;

            public string Style { get; set; } = string.Empty;

            public string Script { get; set; } = string.Empty;

            public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
        }

        #endregion
    }
}