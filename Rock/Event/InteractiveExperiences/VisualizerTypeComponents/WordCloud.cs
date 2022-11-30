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
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Event.InteractiveExperiences;

namespace Rock.Event.InteractiveExperiences.ActionTypeComponents
{
    /// <summary>
    /// Displays results as a word cloud, best used for multiple choice
    /// type questions.
    /// </summary>
    [Description( "Displays results as a word cloud." )]
    [Export( typeof( VisualizerTypeComponent ) )]
    [ExportMetadata( "ComponentName", "Word Cloud" )]

    #region Component Attributes

    [TextField( "Colors",
        Description = "A list of colors to use when rendering the bards. The border will be this color exactly, the filled center of the bar will have the Fill Opacity applied first. If not specified a default color set will be used.",
        IsRequired = false,
        Key = AttributeKey.Colors,
        Order = 0 )]

    [IntegerField( "Angle Count",
        Description = "The number of angles between -90 and +90 degrees. If two or more angles are specified then two of them will always be -90 and +90 with the remainder divided between.",
        IsRequired = false,
        DefaultIntegerValue = 5,
        Key = AttributeKey.AngleCount,
        Order = 1)]

    [TextField( "Font Name",
        Description = "The name of the font to use when rendering the words. If not set a default font will be used.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKey.FontName,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "dc35f0f7-83e5-47d8-aa27-b448962b60dd" )]
    internal class WordCloud : VisualizerTypeComponent
    {
        #region Keys

        private static class AttributeKey
        {
            public const string FontName = "visualizerFontName";
            public const string AngleCount = "visualizerAngleCount";
            public const string Colors = "visualizerColors";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override VisualizerRenderConfigurationBag GetRenderConfiguration( InteractiveExperienceAction action )
        {
            var bag = base.GetRenderConfiguration( action );

            bag.ConfigurationValues.AddOrReplace( "angleCount", GetAttributeValue( action, AttributeKey.AngleCount ) );
            bag.ConfigurationValues.AddOrReplace( "colors", GetAttributeValue( action, AttributeKey.Colors ) );
            bag.ConfigurationValues.AddOrReplace( "fontName", GetAttributeValue( action, AttributeKey.FontName ) );

            return bag;
        }

        #endregion
    }
}
