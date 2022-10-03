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
using System.IO;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Shows the top person signal.
    /// </summary>
    [Description( "Shows the top person badge and the number of signals." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Top Person Signal" )]

    [Rock.SystemGuid.EntityTypeGuid( "1BC1335A-A37E-4C02-83C1-AD2883FD954E")]
    public class TopPersonSignal : BadgeComponent
    {
        /// <summary>
        /// Determines of this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool DoesApplyToEntityType( string type )
        {
            return type.IsNullOrWhiteSpace() || typeof( Person ).FullName == type;
        }

        /// <inheritdoc/>
        public override void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
            if ( !( entity is Person person ) )
            {
                return;
            }

            var signalCount = person.Signals.Where( s => !s.ExpirationDate.HasValue || s.ExpirationDate >= RockDateTime.Now ).Count();
            if ( !string.IsNullOrWhiteSpace( person.TopSignalColor ) && signalCount > 0 )
            {
                writer.Write( string.Format( @"
<div class='rockbadge rockbadge-overlay rockbadge-overlay-invert rockbadge-signal rockbadge-id-{0}' data-toggle='tooltip' title='{3} has the following {4}: {5}' style='color: {1};'>
        <i class='badge-icon fa fa-flag'></i>
        <span class='metric-value'>{2}</span>
</div>",
                    badge.Id,
                    person.TopSignalColor,
                    signalCount,
                    person.NickName,
                    "signal".PluralizeIf( person.Signals.Count != 1 ),
                    string.Join( ", ", person.Signals.Select( s => s.SignalType.Name.EncodeHtml() ) ) ) );
            }
        }
    }
}
