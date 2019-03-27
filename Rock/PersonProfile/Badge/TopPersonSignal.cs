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
using System.Linq;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Shows the top person signal.
    /// </summary>
    [Description( "Shows the top person badge and the number of signals." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Top Person Signal" )]

    public class TopPersonSignal : BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            if ( !string.IsNullOrWhiteSpace( Person.TopSignalColor ) && Person.Signals.Count > 0 )
            {
                writer.Write( string.Format( @"
<div class='badge badge-signal badge-id-{0}' data-toggle='tooltip' title='{3} has the following {4}: {5}'>
    <div class='badge-content' style='color: {1};'>
        <i class='fa fa-flag badge-icon'></i>
        <span class='signal'>{2}</span>
    </div>
</div>",
                    badge.Id,
                    Person.TopSignalColor,
                    Person.Signals.Count,
                    Person.NickName,
                    "signal".PluralizeIf( Person.Signals.Count != 1 ),
                    string.Join( ", ", Person.Signals.Select( s => s.SignalType.Name.EncodeHtml() ) ) ) );
            }
        }
    }
}
