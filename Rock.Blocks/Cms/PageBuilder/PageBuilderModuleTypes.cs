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
using System.Linq;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// The module types the Page Builder offers and the Canvas block renders.
    /// </summary>
    internal static class PageBuilderModuleTypes
    {
        /// <summary>
        /// Gets all module types, in the order the Page Builder lists them.
        /// </summary>
        public static IReadOnlyList<PageBuilderModuleType> All { get; } = new List<PageBuilderModuleType>
        {
            new PageBuilderModuleType
            {
                Key = "accordion",
                Name = "Accordion",
                IconCssClass = "ti ti-layout-navbar-collapse",
                Settings = new List<PageBuilderModuleSetting>
                {
                    new PageBuilderModuleSetting { Key = "Title", Name = "Title", DefaultValue = "Accordion" }
                },
                WebLavaTemplate = @"<h3>{{ Settings.Title | Escape }}</h3>
<details><summary>An accordion title goes here?</summary><p>Accordion content goes here.</p></details>
<details><summary>An accordion title goes here?</summary><p>Accordion content goes here.</p></details>
<details><summary>An accordion title goes here?</summary><p>Accordion content goes here.</p></details>"
            },
            new PageBuilderModuleType
            {
                Key = "billboard",
                Name = "Billboard",
                IconCssClass = "ti ti-presentation",
                Settings = new List<PageBuilderModuleSetting>
                {
                    new PageBuilderModuleSetting { Key = "Title", Name = "Title", DefaultValue = "Billboard" },
                    new PageBuilderModuleSetting { Key = "Text", Name = "Text", DefaultValue = "A large banner that introduces the page." }
                },
                WebLavaTemplate = @"<div class=""jumbotron""><h1>{{ Settings.Title | Escape }}</h1><p>{{ Settings.Text | Escape }}</p></div>"
            },
            new PageBuilderModuleType
            {
                Key = "card",
                Name = "Card",
                IconCssClass = "ti ti-id",
                Settings = new List<PageBuilderModuleSetting>
                {
                    new PageBuilderModuleSetting { Key = "Title", Name = "Title", DefaultValue = "Card" },
                    new PageBuilderModuleSetting { Key = "Text", Name = "Text", DefaultValue = "A short summary with a link to more." },
                    new PageBuilderModuleSetting { Key = "ButtonText", Name = "Button Text", DefaultValue = "View details" }
                },
                WebLavaTemplate = @"<div class=""panel panel-default""><div class=""panel-body""><h4>{{ Settings.Title | Escape }}</h4><p>{{ Settings.Text | Escape }}</p><a class=""btn btn-default"" href=""#"">{{ Settings.ButtonText | Escape }}</a></div></div>"
            },
            new PageBuilderModuleType
            {
                Key = "content",
                Name = "Content",
                IconCssClass = "ti ti-file-text",
                Settings = new List<PageBuilderModuleSetting>
                {
                    new PageBuilderModuleSetting { Key = "Title", Name = "Title", DefaultValue = "Content" },
                    new PageBuilderModuleSetting { Key = "Text", Name = "Text", DefaultValue = "A block of formatted text." }
                },
                WebLavaTemplate = @"<h2>{{ Settings.Title | Escape }}</h2><p>{{ Settings.Text | Escape }}</p>"
            },
            new PageBuilderModuleType
            {
                Key = "video",
                Name = "Video",
                IconCssClass = "ti ti-player-play",
                Settings = new List<PageBuilderModuleSetting>
                {
                    new PageBuilderModuleSetting { Key = "Title", Name = "Title", DefaultValue = "Video" }
                },
                WebLavaTemplate = @"<div class=""well text-center""><i class=""ti ti-player-play""></i> {{ Settings.Title | Escape }}</div>"
            }
        };

        /// <summary>
        /// Gets the module type with the given key.
        /// </summary>
        /// <param name="key">The module type's key.</param>
        /// <returns>The matching module type, or <c>null</c> if there is none.</returns>
        public static PageBuilderModuleType Get( string key )
        {
            return All.FirstOrDefault( moduleType => string.Equals( moduleType.Key, key, StringComparison.OrdinalIgnoreCase ) );
        }
    }

    /// <summary>
    /// A kind of module that can be placed on a page with the Page Builder.
    /// </summary>
    internal class PageBuilderModuleType
    {
        /// <summary>
        /// Gets the value that uniquely identifies the module type.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets the name shown on the Page Builder's sidebar tile and on a placed module.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the icon shown on the Page Builder's sidebar tile.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets the settings a placed module of this type can be configured with.
        /// </summary>
        public List<PageBuilderModuleSetting> Settings { get; set; }

        /// <summary>
        /// Gets the Lava template that renders a module of this type on the web, with its settings in the <c>Settings</c> merge field.
        /// </summary>
        public string WebLavaTemplate { get; set; }

        /// <summary>
        /// Gets the default value of each setting, keyed by setting key.
        /// </summary>
        /// <returns>The default settings for a newly placed module.</returns>
        public Dictionary<string, string> GetDefaultSettings()
        {
            return Settings.ToDictionary( setting => setting.Key, setting => setting.DefaultValue );
        }
    }

    /// <summary>
    /// A setting a placed module can be configured with.
    /// </summary>
    internal class PageBuilderModuleSetting
    {
        /// <summary>
        /// Gets the key the setting is stored and referenced in Lava by.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets the label shown for the setting.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the value a newly placed module starts with.
        /// </summary>
        public string DefaultValue { get; set; }
    }
}
