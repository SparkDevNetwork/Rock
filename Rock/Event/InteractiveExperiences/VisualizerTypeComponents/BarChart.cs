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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Event.InteractiveExperiences;
using Rock.Web.Cache;

namespace Rock.Event.InteractiveExperiences.ActionTypeComponents
{
    /// <summary>
    /// The Short Answer action type. This displays a question and allows the
    /// individual to provide a short text answer.
    /// </summary>
    [Description( "Displays the answers as a bar chart." )]
    [Export( typeof( VisualizerTypeComponent ) )]
    [ExportMetadata( "ComponentName", "Bar Chart" )]

    #region Component Attributes

    [CustomDropdownListField( "Orientation",
        DefaultValue = "horizontal",
        ListSource = "horizontal^Horizontal,vertical^Vertical",
        IsRequired = true,
        Key = AttributeKey.Orientation,
        Order = 0 )]

    [TextField( "Colors",
        Description = "A semicolon separated list of colors to use when rendering the bars. The border will be this color exactly, the filled center of the bar will have the Fill Opacity applied first. If not specified a default color set will be used.",
        IsRequired = false,
        Key = AttributeKey.Colors,
        Order = 1 )]

    [IntegerField( "Border Width",
        Description = "The width of the border drawn on each bar.",
        IsRequired = false,
        DefaultIntegerValue = 4,
        Key = AttributeKey.BorderWidth,
        Order = 2 )]

    [DecimalField( "Fill Opacity",
        Description = "The opacity to apply to the bar color when filling the bar area.",
        IsRequired = true,
        DefaultDecimalValue = 0.5,
        Key = AttributeKey.FillOpacity,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b1dfd377-9ef7-407f-9097-6206b98aec0d" )]
    internal class BarChart : VisualizerTypeComponent
    {
        #region Keys

        private static class AttributeKey
        {
            public const string BorderWidth = "visualizerBorderWidth";
            public const string Colors = "visualizerColors";
            public const string Orientation = "visualizerOrientation";
            public const string FillOpacity = "visualizerFillOpacity";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override VisualizerRenderConfigurationBag GetRenderConfiguration( InteractiveExperienceAction action )
        {
            var bag = base.GetRenderConfiguration( action );

            if ( action.Attributes == null )
            {
                LoadAttributes( action );
            }

            // This is a bit of a hack, but don't currently have a better way
            // to do this. The Bar Chart needs to pre-populate ansers with
            // zeros and be in the correct order when using a Poll question.
            // So sneek into the raw data to get the original question list
            // if it is a Poll question.
            if ( action.ActionEntityTypeId == EntityTypeCache.GetId<Poll>() )
            {
                var rawAnswers = GetAttributeValue( action, Poll.AttributeKey.Answers );
                var values = rawAnswers?.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) ?? new string[0];
                values = values.Select( s => System.Net.WebUtility.UrlDecode( s ) ).ToArray();

                bag.ConfigurationValues.Add( "answerOrder", values.ToJson() );
            }

            bag.ConfigurationValues.AddOrReplace( "orientation", GetAttributeValue( action, AttributeKey.Orientation ) );
            bag.ConfigurationValues.AddOrReplace( "colors", GetAttributeValue( action, AttributeKey.Colors ) );
            bag.ConfigurationValues.AddOrReplace( "borderWidth", GetAttributeValue( action, AttributeKey.BorderWidth ) );
            bag.ConfigurationValues.AddOrReplace( "fillOpacity", GetAttributeValue( action, AttributeKey.FillOpacity ) );

            return bag;
        }

        #endregion
    }
}
