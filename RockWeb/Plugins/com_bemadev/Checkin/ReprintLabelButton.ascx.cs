// <copyright>
// Copyright by LCBC Church
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using RockWeb;


namespace RockWeb.Plugins.com_bemaservices.Checkin
{
    [DisplayName( "Reprint Label Button" )]
    [Category( "BEMA Services > Check-in" )]
    [Description( "Displays a button to print rosters for location" )]
    [LinkedPage( "Reprint Label Page" )]
    public partial class ReprintLabelButton : CheckInBlockMultiPerson
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        protected void btnReprint_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "ReprintLabelPage" );
        }
    }
}