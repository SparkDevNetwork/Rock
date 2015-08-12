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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    [DisplayName( "Twitter Block" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Displays a twitter feed" )]
    [TextField("Twitter Username")]
    [IntegerField( "Number Of Tweets", required: true )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the package details.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/TwitterLava.lava' %}", "", 2 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 3 )]
    public partial class TwitterBlock : RockBlock
    {

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( ResolveRockUrl( "~/Plugins/com_centralaz/Widgets/Styles/twitterstyle.css" ) );
            RockPage.AddScriptLink( "~/Plugins/com_centralaz/Widgets/Scripts/twitterFetcher_min.js" );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContent();
        }

        #endregion

        #region Internal Methods


        public void LoadContent()
        {
            var mergeFields = new Dictionary<string, object>();

            // add linked pages
            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            mergeFields.Add( "TwitterWidgetId", GetAttributeValue( "TwitterWidgetId" ) );

            mergeFields.Add( "CurrentPerson", CurrentPerson );
            mergeFields.Add( "CurrentUser", CurrentUser );

            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

            //lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            //// show debug info
            //if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            //{
            //    lDebug.Visible = true;
            //    lDebug.Text = mergeFields.lavaDebugInfo();
            //}
        }

        #endregion

    }
}