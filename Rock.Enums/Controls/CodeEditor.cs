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

using System.ComponentModel;

namespace Rock.Enums.Controls
{
    /// <summary>
    /// The CodeEditor Mode, also defined in Rock.Web.UI.Controls.CodeEditor, so if changed, please update there as well
    /// </summary>
    public enum CodeEditorMode
    {
        /// <summary>
        /// text
        /// </summary>
        [Description( "text" )]
        Text = 0,

        /// <summary>
        /// CSS
        /// </summary>
        [Description( "css" )]
        Css = 1,

        /// <summary>
        /// HTML
        /// </summary>
        [Description( "html" )]
        Html = 2,

        /// <summary>
        /// The lava
        /// </summary>
        [Description( "lava" )]
        Lava = 3,

        /// <summary>
        /// java script
        /// </summary>
        [Description( "javascript" )]
        JavaScript = 4,

        /// <summary>
        /// less
        /// </summary>
        [Description( "less" )]
        Less = 5,

        /// <summary>
        /// powershell
        /// </summary>
        [Description( "powershell" )]
        Powershell = 6,

        /// <summary>
        /// SQL
        /// </summary>
        [Description( "sql" )]
        Sql = 7,

        /// <summary>
        /// type script
        /// </summary>
        [Description( "typescript" )]
        TypeScript = 8,

        /// <summary>
        /// c sharp
        /// </summary>
        [Description( "csharp" )]
        CSharp = 9,

        /// <summary>
        /// markdown
        /// </summary>
        [Description( "markdown" )]
        Markdown = 10,

        /// <summary>
        /// The XML
        /// </summary>
        [Description( "xml" )]
        Xml = 11
    }

    /// <summary>
    /// The CodeEditor Theme, also defined in Rock.Web.UI.Controls.CodeEditor, so if changed, please update there as well
    /// </summary>
    public enum CodeEditorTheme
    {
        /// <summary>
        /// rock
        /// </summary>
        [Description( "rock" )]
        Rock = 0,

        /// <summary>
        /// chrome
        /// </summary>
        [Description( "chrome" )]
        Chrome = 1,

        /// <summary>
        /// crimson editor
        /// </summary>
        [Description( "crimson_editor" )]
        CrimsonEditor = 2,

        /// <summary>
        /// dawn
        /// </summary>
        [Description( "dawn" )]
        Dawn = 3,

        /// <summary>
        /// dreamweaver
        /// </summary>
        [Description( "dreamweaver" )]
        Dreamweaver = 4,

        /// <summary>
        /// eclipse
        /// </summary>
        [Description( "eclipse" )]
        Eclipse = 5,

        /// <summary>
        /// solarized light
        /// </summary>
        [Description( "solarized_light" )]
        SolarizedLight = 6,

        /// <summary>
        /// textmate
        /// </summary>
        [Description( "textmate" )]
        Textmate = 7,

        /// <summary>
        /// tomorrow
        /// </summary>
        [Description( "tomorrow" )]
        Tomorrow = 8,

        /// <summary>
        /// xcode
        /// </summary>
        [Description( "xcode" )]
        Xcode = 9,

        /// <summary>
        /// github
        /// </summary>
        [Description( "github" )]
        Github = 10,

        /// <summary>
        /// ambiance dark
        /// </summary>
        [Description( "ambiance" )]
        AmbianceDark = 11,

        /// <summary>
        /// chaos dark
        /// </summary>
        [Description( "chaos" )]
        ChaosDark = 12,

        /// <summary>
        /// clouds midnight dark
        /// </summary>
        [Description( "clouds_midnight" )]
        CloudsMidnightDark = 13,

        /// <summary>
        /// cobalt dark
        /// </summary>
        [Description( "cobalt" )]
        CobaltDark = 14,

        /// <summary>
        /// idle fingers dark
        /// </summary>
        [Description( "idle_fingers" )]
        IdleFingersDark = 15,

        /// <summary>
        /// kr theme dark
        /// </summary>
        [Description( "kr_theme" )]
        krThemeDark = 16,

        /// <summary>
        /// merbivore dark
        /// </summary>
        [Description( "merbivore" )]
        MerbivoreDark = 17,

        /// <summary>
        /// merbivore soft dark
        /// </summary>
        [Description( "merbivore_soft" )]
        MerbivoreSoftDark = 18,

        /// <summary>
        /// mono industrial dark
        /// </summary>
        [Description( "mono_industrial" )]
        MonoIndustrialDark = 19,

        /// <summary>
        /// monokai dark
        /// </summary>
        [Description( "monokai" )]
        MonokaiDark = 20,

        /// <summary>
        /// pastel on dark
        /// </summary>
        [Description( "pastel_on_dark" )]
        PastelOnDark = 21,

        /// <summary>
        /// solarized dark
        /// </summary>
        [Description( "solarized_dark" )]
        SolarizedDark = 22,

        /// <summary>
        /// terminal dark
        /// </summary>
        [Description( "terminal" )]
        TerminalDark = 23,

        /// <summary>
        /// tomorrow night dark
        /// </summary>
        [Description( "tomorrow_night" )]
        TomorrowNightDark = 24,

        /// <summary>
        /// tomorrow night blue dark
        /// </summary>
        [Description( "tomorrow_night_blue" )]
        TomorrowNightBlueDark = 25,

        /// <summary>
        /// tomorrow night bright dark
        /// </summary>
        [Description( "tomorrow_night_bright" )]
        TomorrowNightBrightDark = 26,

        /// <summary>
        /// tomorrow night eighties dark
        /// </summary>
        [Description( "tomorrow_night_eighties" )]
        TomorrowNightEightiesDark = 27,

        /// <summary>
        /// twilight dark
        /// </summary>
        [Description( "twilight" )]
        TwilightDark = 28,

        /// <summary>
        /// vibrant ink dark
        /// </summary>
        [Description( "vibrant_ink" )]
        VibrantInkDark = 29,
    }
}
