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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    [DisplayName( "HtmlEditor AssetManager" )]
    [Category( "Utility" )]
    [Description( "Block to be used as part of the RockAssetManager HtmlEditor Plugin" )]
    public partial class HtmlEditorAssetManager : RockBlock
    {
        #region Properties


        #endregion Properties

        #region Base Control Methods
        protected void Page_Load( object sender, EventArgs e )
        {

        }

        #endregion Base Control Methods
    }
}