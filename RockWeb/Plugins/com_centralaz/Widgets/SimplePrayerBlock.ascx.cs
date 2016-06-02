// <copyright>
// Copyright by Central Christian Church
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
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web;


namespace RockWeb.Plugins.com_centralaz.Widgets
{
    [DisplayName("Simple Prayer Block")]
    [Category("com_centralaz > Widgets")]
    [Description("Adds simple prayer input box to page.")]
    [LinkedPage("Target Page", "Target page containing Prayer Request Entry Block.")]

    public partial class SimplePrayerBlock : Rock.Web.UI.RockBlock
    {

        protected void btnComplete_Click(object sender, EventArgs e)
        {        
            var parms = new Dictionary<string, string>();
            parms.Add("Request", dtbRequest.Value);
            Response.Redirect(new PageReference(GetAttributeValue("TargetPage"), parms).BuildUrl(), false);
        }
    }
}
