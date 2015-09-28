// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.ComponentModel;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    [DisplayName( "Media Block" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "A lava block for searching the external website using OpenSearchServer." )]

    [IntegerField("Maximum Number Of Tweets", "The maximum allowed number of tweets.", true, 10, order: 1)]
    [TextField( "Twitter Username", "The Twitter Username", true, "CentralAZ", order: 2 )]
    [TextField( "Twitter Widget Id", "The twitter widget Id.", true, "626813522828132352", order: 3 )]

    [TextField( "Instagram Client Id", "The Instagram Client Id", true, "dd090c6166f14026b4443b6db4070ebb", order: 4 )]
    [TextField( "Instagram User Id", "The Instagram User Id", true, "1469755635", order: 5 )]

    [IntegerField( "Number Of Images", "The number of images. Keep to a number whose square root is an integer", true, 9, order: 6 )]
    [TextField( "Pixel Size", "The pixel size to use as length and width for the images.", true, "380px", order: 7 )]
    public partial class MediaBlock : RockBlock
    {

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddCSSLink( ResolveRockUrl( "~/Plugins/com_centralaz/Widgets/Styles/style.css" ) );
        }

        #endregion

    }
}