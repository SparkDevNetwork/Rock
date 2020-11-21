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

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Steps Badge
    /// </summary>
    [Description( "Shows a badge for each step that the person has taken in a program." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Steps" )]

    [StepProgramField(
        name: "Step Program",
        description: "The program in which the steps are contained",
        required: true,
        order: 1,
        key: AttributeKey.StepProgram )]

    [BooleanField(
        name: "Display Mode",
        trueText: "Condensed",
        falseText: "Normal",
        description: "Choose a mode which determines how large the badges will be.",
        defaultValue: false,
        order: 2,
        key: AttributeKey.IsCondensed )]

    public class Steps : BadgeComponent
    {
        #region Keys

        /// <summary>
        /// Keys to use for the attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The step program
            /// </summary>
            public const string StepProgram = "StepProgram";

            /// <summary>
            /// The is condensed
            /// </summary>
            public const string IsCondensed = "IsCondensed";
        }

        #endregion Keys

        /// <summary>
        /// Determines of this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool DoesApplyToEntityType( string type )
        {
            return type.IsNullOrWhiteSpace() || typeof( Person ).FullName == type;
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( BadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            if ( Person == null )
            {
                return;
            }

            var stepProgramGuid = GetStepProgramGuid( badge );

            if ( !stepProgramGuid.HasValue )
            {
                return;
            }

            var isCondensed = IsCondensed( badge );
            var domElementKey = GenerateBadgeKey( badge );
            var html = GetHtmlTemplate( isCondensed, domElementKey );
            writer.Write( html );
        }

        /// <summary>
        /// Gets the java script.
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        protected override string GetJavaScript( BadgeCache badge )
        {
            if ( Person == null )
            {
                return null;
            }

            var stepProgramGuid = GetStepProgramGuid( badge );

            if ( !stepProgramGuid.HasValue )
            {
                return null;
            }

            var domElementKey = GenerateBadgeKey( badge );
            var isCondensed = IsCondensed( badge );
            return GetScript( stepProgramGuid.Value, Person.Id, domElementKey, isCondensed );
        }

        /// <summary>
        /// Get the step program guid described by the attribute value
        /// </summary>
        /// <param name="badgeCache"></param>
        /// <returns></returns>
        private Guid? GetStepProgramGuid( BadgeCache badgeCache )
        {
            return GetAttributeValue( badgeCache, AttributeKey.StepProgram ).AsGuidOrNull();
        }

        /// <summary>
        /// Determines whether the display mode is condensed.
        /// </summary>
        /// <param name="badgeCache">The badge cache.</param>
        /// <returns>
        ///   <c>true</c> if the display mode is condensed; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCondensed( BadgeCache badgeCache )
        {
            return GetAttributeValue( badgeCache, AttributeKey.IsCondensed ).AsBoolean();
        }

        /// <summary>
        /// Gets the HTML template depending on the display mode (normal or condensed).
        /// </summary>
        /// <param name="isCondensed"></param>
        /// <param name="domElementKey"></param>
        /// <returns></returns>
        private string GetHtmlTemplate( bool isCondensed, string domElementKey )
        {
            if ( isCondensed )
            {
                return
$@"<div class=""badge"">
    <div class=""badge-grid"" data-html=""true"" data-original-title=""<p>Loading...</p>"" data-tooltip-key=""{domElementKey}"">
        <div class=""badge-row"" data-placeholder-key=""{domElementKey}""></div>
        <div class=""badge-row"" data-placeholder-key=""{domElementKey}""></div>
    </div>
</div>";
            }
            else
            {
                return
$@"<div class=""badge"" data-placeholder-key=""{domElementKey}""></div>";
            }
        }

        /// <summary>
        /// Gets the JavaScript that will load data and render the badge.
        /// </summary>
        /// <param name="stepProgramGuid">The step program unique identifier.</param>
        /// <param name="personId">The identifier.</param>
        /// <param name="domElementKey"></param>
        /// <param name="isCondensed"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string GetScript( Guid stepProgramGuid, int personId, string domElementKey, bool isCondensed )
        {
            if ( isCondensed )
            {
                return
$@"$.ajax({{
    type: 'GET',
    url: Rock.settings.get('baseUrl') + 'api/StepPrograms/BadgeData/{stepProgramGuid}/{personId}',
    statusCode: {{
        200: function (data) {{
            var htmlRow1 = [];
            var htmlRow2 = [];
            var tooltip = [];

            if (data) {{
                for(var i = 0; i < data.length; i++) {{
                    var html = i % 2 == 0 ? htmlRow1 : htmlRow2;

                    var stepTypeData = data[i];
                    var isComplete = stepTypeData.CompletionCount > 0;
                    var color = isComplete ? (stepTypeData.HighlightColor || '#16c98d') : '#dbdbdb';
                    var iconClass = stepTypeData.IconCssClass || (isComplete ? 'fa fa-check' : 'fa fa-times');

                    html.push('<div class=""badge"">');
                    html.push('    <span class=""fa-stack"">');
                    html.push('        <i style=""color: ' + color + ';"" class=""fa fa-circle fa-stack-2x""></i>');
                    html.push('        <i class=""fa ' + iconClass + ' fa-stack-1x""></i>');
                    html.push('    </span>');
                    html.push('</div>\n');

                    tooltip.push('<p class=""margin-b-sm"">');
                    tooltip.push('    <span class=""fa-stack"">');
                    tooltip.push('        <i style=""color: ' + color + ';"" class=""fa fa-circle fa-stack-2x""></i>');
                    tooltip.push('        <i class=""fa ' + iconClass + ' fa-stack-1x""></i>');
                    tooltip.push('    </span>');
                    tooltip.push('    <strong>' + stepTypeData.StepTypeName + ':</strong> ' + (stepTypeData.Statuses[0] || 'Incomplete'));
                    tooltip.push('</p>\n');
                }}
            }}

            var rows = $('[data-placeholder-key=""{domElementKey}""]');
            rows.first().html(htmlRow1.join(''));
            rows.last().html(htmlRow2.join(''));
            $('[data-tooltip-key=""{domElementKey}""]').attr('data-original-title', tooltip.join('')).tooltip({{ sanitize: false }});
        }}
    }},
}});";
            }
            else
            {
                return
$@"$.ajax({{
    type: 'GET',
    url: Rock.settings.get('baseUrl') + 'api/StepPrograms/BadgeData/{stepProgramGuid}/{personId}',
    statusCode: {{
        200: function (data) {{
            var html = [];
            var tooltip = [];

            if (data) {{
                for(var i = 0; i < data.length; i++) {{
                    var stepTypeData = data[i];
                    var isComplete = stepTypeData.CompletionCount > 0;
                    var color = isComplete ? (stepTypeData.HighlightColor || '#16c98d') : '#dbdbdb';
                    var iconClass = stepTypeData.IconCssClass || (isComplete ? 'fa fa-check' : 'fa fa-times');

                    html.push('<div class=""badge badge-step"" data-tooltip-key=""{domElementKey}"" style=""color:' + color + '"" data-toggle=""tooltip"" data-original-title=""' + stepTypeData.StepTypeName + '"">');
                    html.push('    <i class=""badge-icon ' + iconClass + '""></i>');

                    if (stepTypeData.CompletionCount > 1 && stepTypeData.ShowCountOnBadge) {{
                        html.push('    <span class=""badge-count"">' + stepTypeData.CompletionCount + '</span>');
                    }}

                    html.push('</div>\n');
                }}
            }}

            $('[data-placeholder-key=""{domElementKey}""]').replaceWith(html.join(''));
            $('[data-tooltip-key=""{domElementKey}""]').tooltip();
        }}
    }},
}});";
            }
        }
    }
}