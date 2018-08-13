// <copyright>
// Copyright 2015 by NewSpring Church
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Globalization;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.Headings
{
    [DisplayName( "Headings" )]
    [Category( "NewSpring" )]
    [Description( "Insert Headings into your layouts, the name of this block is used for the heading text" )]
    [CustomDropdownListField( "Heading Type", "", "H1,H2,H3,H4,H5", DefaultValue = "H2" )]
    [CustomDropdownListField( "Number of Columns", "", "1,2,3,4,5,6,7,8,9,10,11,12", DefaultValue = "12" )]
    public partial class Headings : Rock.Web.UI.RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Set the heading value
            headingType.Text = GetAttributeValue( "HeadingType" );
            headingWidth.Text = GetAttributeValue( "NumberofColumns" );
            headingText.Text = BlockName;
        }
    }
}