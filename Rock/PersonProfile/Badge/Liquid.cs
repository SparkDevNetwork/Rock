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
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Lava Badge
    /// </summary>
    [Description( "Lava Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Lava Badge" )]

    [CodeEditorField( "Display Text", "The text (or html) to display as a badge", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200 )]
    [BooleanField( "Enable Debug", "Outputs the object graph to help create your Lava syntax.", false )]
    public class Liquid : BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            string displayText = GetAttributeValue( badge, "DisplayText" );
            if ( Person != null )
            {
                Dictionary<string, object> mergeValues = new Dictionary<string, object>();
                mergeValues.Add( "Person", Person );
                displayText = displayText.ResolveMergeFields( mergeValues );

                if ( GetAttributeValue( badge, "EnableDebug" ).AsBoolean() )
                {
                    string debugInfo = string.Format( @"
                            <small><a data-toggle='collapse' data-parent='#accordion' href='#badge-debug'><i class='fa fa-eye'></i></a></small>
                            <div id='badge-debug' class='collapse well badge-debug'>
                                {0}
                            </div>
                        ", mergeValues.lavaDebugInfo() );

                    displayText += debugInfo;
                }

            }
            writer.Write( displayText );
        }
    }
}
