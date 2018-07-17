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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    [DisplayName( "Badges" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Handles displaying badges for a person." )]

    [PersonBadgesField( "Badges" )]
    public partial class Badges : Rock.Web.UI.PersonBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !Page.IsPostBack && Person != null && Person.Id != 0 )
            {
                string badgeList = GetAttributeValue( "Badges" );
                if ( !string.IsNullOrWhiteSpace( badgeList ) )
                {
                    foreach ( string badgeGuid in badgeList.SplitDelimitedValues() )
                    {
                        Guid guid = badgeGuid.AsGuid();
                        if ( guid != Guid.Empty )
                        {
                            var personBadge = PersonBadgeCache.Get( guid );
                            if ( personBadge != null )
                            {
                                blBadges.PersonBadges.Add( personBadge );
                            }
                        }
                    }
                }
            }
        }
    }
}