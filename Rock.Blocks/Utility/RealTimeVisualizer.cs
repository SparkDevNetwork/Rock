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
using System.Security;
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
    /// <seealso cref="Rock.Blocks.RockObsidianDetailBlockType" />

    [DisplayName( "RealTime Visualizer" )]
    [Category( "Utility" )]
    [Description( "Displays RealTime events from Rock with custom formatting options." )]
    [IconCssClass( "fa fa-chart-bar" )]

    #region Block Attributes

    [KeyValueListField( "Channels",
        Description = "The list of topics and channels to subscribe to.",
        KeyPrompt = "Topic",
        ValuePrompt = "Channel",
        IsRequired = true,
        Order = 0,
        Key = AttributeKeys.Channels )]

    [BlockTemplateField( "Template",
        Description = "The lava template to use when processing the message for display. The 'Message' variable will contain the message name and 'Args' variable will be an array of the message arguments.",
        TemplateBlockValueGuid = "8ff590c7-84cf-4d17-83fc-61b61d05937a",
        DefaultValue = "",
        IsRequired = true,
        Order = 1,
        Key = AttributeKeys.Template )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 2 )]

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
    public class RealTimeVisualizer : RockObsidianDetailBlockType, IHasCustomActions
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

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

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
                settings.AddOrIgnore( kvp.Key, kvp.Value );
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

        const string ToastPageTemplate = @"<div class=""visualizer-container"">
</div>";
        const string ToastStyle = @".visualizer-container {
    display: flex;
    flex-direction: column;
    min-height: 400px;
    position: relative;
    background-color: black;
{% if Settings.fullscreen == ""true"" %}
    position: fixed;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
{% endif %}
}

.visualizer-container > canvas {
    position: absolute;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
}

.visualizer-container > .realtime-visualizer-item {
    height: 0px;
}

/* IN transition initial states. */
.visualizer-container > .realtime-visualizer-item.left-in {
    transform: translateX(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.top-in {
    transform: translateY(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.right-in {
    transform: translateX(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.bottom-in {
    transform: translateY(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.fade-in {
    opacity: 0;
}

/* IN transition final states. */
.visualizer-container > .realtime-visualizer-item.left-in.in,
.visualizer-container > .realtime-visualizer-item.top-in.in,
.visualizer-container > .realtime-visualizer-item.right-in.in,
.visualizer-container > .realtime-visualizer-item.bottom-in.in {
    transform: initial;
}

.visualizer-container > .realtime-visualizer-item.fade-in.in {
    opacity: 1;
}

/* OUT transition final states. */
.visualizer-container > .realtime-visualizer-item.left-out.out {
    transform: translateX(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.top-out.out {
    transform: translateY(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.right-out.out {
    transform: translateX(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.bottom-out.out {
    transform: translateY(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.fade-out.out {
    opacity: 0;
    overflow-y: initial;
}

/* Transition Timings. */
.visualizer-container > .realtime-visualizer-item.in {
    transition: height var(--animationDuration) ease-out, transform var(--animationDuration) ease-out, opacity var(--animationDuration) ease-out;
}

.visualizer-container > .realtime-visualizer-item.out {
    transition: opacity var(--animationDuration) ease-in, transform var(--animationDuration) ease-in, height var(--animationDuration) ease-in;
}
";
        const string ToastScript = @"let helper = undefined;

// Called one time to initialize everything before any
// calls to showItem() are made.
async function setup(container, settings) {
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    const fingerprint = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString();

    const Helper = (await import(`/Scripts/Rock/UI/realtimevisualizer/common.js?v=${fingerprint}`)).Helper;
    helper = new Helper(itemContainer);
}

// Shows a single visual item from the RealTime system.
async function showItem(content, container, settings) {
    if (!content) {
        return;
    }
    
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    
    // Configure the item and it's content.
    const item = document.createElement(""div"");
    item.classList.add(""realtime-visualizer-item"");
    item.innerHTML = content;
    itemContainer.prepend(item);

    // Configure all the animation classes.
    if (settings.fade === ""true"") {
        item.classList.add(""fade-in"", ""fade-out"");
    }

    if (settings.slideInDirection) {
        item.classList.add(`${settings.slideInDirection}-in`);
    }

    if (settings.slideOutDirection) {
        item.classList.add(`${settings.slideOutDirection}-out`);
    }

    // Show the item.
    helper.setItemHeight(item);
    item.classList.add(""in"");

    // Start up all the extras.
    if (settings.playAudio) {
        helper.playAudio(item, settings.defautlAudioUrl);
    }
    
    if (settings.confetti) {
        helper.showConfetti();
    }
    
    if (settings.fireworks) {
        helper.startFireworks();
    }
    
    // Wait until this item should be removed and then start
    // the removal process.
    setTimeout(() => {
        item.classList.add(""out"");
        item.style.height = ""0px"";

        item.addEventListener(""transitionend"", () => {
            if (item.parentElement) {
                item.remove();

                if (settings.fireworks) {
                    helper.stopFireworks();
                }
            }
        });
    }, parseInt(settings.duration) || 5000);
}
";
        const string ToastHelpContent = @"<p>
    The Toast theme displays the item at the top of the area for a short period of time
    and then removes it. If additional items are displayed before previous ones are
    removed then they are inserted at the top.
</p>

<div><strong>Presentation Settings</strong></div>

<p>
    There are a few properties that define how an item is presented that can be
    set to customize how this theme looks and behaves.
</p>

<ul>
    <li><strong>animationDuration:</strong> The number of seconds the item will spend transitioning onto or off of the screen. (Default: <strong>0.5s</strong>)</li>
    <li><strong>duration:</strong> The number of milliseconds the item will stay on screen, this does not include the animationDuration. (Default: <strong>5000</strong>)</li>
    <li><strong>fade:</strong> Determines if the item should fade in and out. Valid values are ""true"" and ""false"". (Default: <strong>true</strong>)</li>
    <li><strong>fullscreen:</strong> Determines if the theme should render itself full-screen. You can turn this off to use CSS to customize what part of the screen to fill. (Default: <strong>true</strong>)</li>
    <li><strong>slideAmount:</strong> The number of pixels to slide the item when slideInDirection or slideOutDirection are specified. (Default: <strong>15px</strong>)</li>
    <li><strong>slideInDirection:</strong> If set to a value this will determine what direction the item will slide in from. Valid values are ""left"", ""top"", ""right"" and ""bottom"". (No default)</li>
    <li><strong>slideOutDirection: </strong>If set to a value this will determine what direction the item will slide out towards. Valid values are ""left"", ""top"", ""right"" and ""bottom"". (No default)</li>
</ul>

<div><strong>Advanced Settings</strong></div>

<p>
    There are some other settings you can use to customize the behavior of the theme.
</p>

<ul>
    <li><strong>confetti:</strong> If turned on, a burst of confetti will appear from both sides of the screen when an item is displayed. Valid values are ""true"" and ""false"". (Default: <strong>false</strong>)</li>
    <li><strong>fireworks:</strong> If turned on, fireworks will be displayed during the entire duration that any item is displayed. Valid values are ""true"" and ""false"". (Default: <strong>false</strong>)</li>
    <li><strong>defaultAudioUrl:</strong> When playAudio is enabled, this provides the default audio file to use if none is specified in the item template. (No default)</li>
    <li><strong>playAudio:</strong> If turned on, an audio file will be played when an item appears on screen. If the item includes a ""data-audio-url"" attribute then it will be used as the URL of the audio file to play. Otherwise any value in defaultAudioUrl will be used. (Default: <strong>false</strong>)</li>
</ul>
";
        // fullscreen = true
        // slideAmount = 15px
        // animationDuration = 0.5s
        // duration = 5000
        // fireworks = false
        // confetti = false
        // playAudio = false
        // defaultAudioUrl =
        // fade = true
        // slideInDirection =
        // slideOutDirection =


        const string SwapPageTemplate = @"<div class=""visualizer-container"">
</div>";
        const string SwapStyle = @".visualizer-container {
    display: flex;
    flex-direction: column;
    min-height: 400px;
    position: relative;
    background-color: black;
{% if Settings.fullscreen == ""true"" %}
    position: fixed;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
{% endif %}
}

.visualizer-container > canvas {
    position: absolute;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
}

.visualizer-item.in {
  animation: 1.5s incoming both;
}
::view-transition-old(outgoing) {
  animation: 1.5s outgoing both;
}

@keyframes outgoing {
  0% {
    opacity: 1;
  }
  100% {
    opacity: 0;
  }
}

@keyframes incoming {
  0% {
    opacity: 0;
  }
  100% {
    opacity: 1;
  }
}
";
        const string SwapScript = @"let helper = undefined;
let itemCount = 0;

// Called one time to initialize everything before any
// calls to showItem() are made.
async function setup(container, settings) {
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    const fingerprint = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString();

    const Helper = (await import(`/Scripts/Rock/UI/realtimevisualizer/common.js?v=${fingerprint}`)).Helper;
    helper = new Helper(itemContainer);
}

// Shows a single visual item from the RealTime system.
async function showItem(content, container, settings) {
    if (!content) {
        return;
    }
    
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    
    // Configure the item and it's content.
    const item = document.createElement(""div"");
    item.classList.add(""visualizer-item"", ""in"");
    item.style.viewTransitionName = `visualizer-item-${itemCount++}`;
    item.innerHTML = content;

    // Prepare old items for removal.
    const oldItems = itemContainer.querySelectorAll("".visualizer-item"");
    for (let i = 0; i < oldItems.length; i++) {
        oldItems[i].classList.remove(""in"");
        oldItems[i].style.viewTransitionName = ""outgoing"";
    }

    if (document.startViewTransition) {
        document.startViewTransition(() => {
            itemContainer.prepend(item);
            for (let i = 0; i < oldItems.length; i++) {
                oldItems[i].remove();
            }
        });
    }
    else {
        itemContainer.prepend(item);
        for (let i = 0; i < oldItems.length; i++) {
            oldItems[i].remove();
        }
    }

    // Start up all the extras.
    if (settings.playAudio) {
        helper.playAudio(item, settings.defautlAudioUrl);
    }
    
    if (settings.confetti) {
        helper.showConfetti();
    }
}
";
        const string SwapHelpContent = @"";
        // fullscreen = true
        // confetti = false
        // playAudio = false
        // defaultAudioUrl =
    }
}