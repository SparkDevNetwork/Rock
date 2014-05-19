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
using Rock.Attribute;
using System.ComponentModel;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    [DisplayName("Redirect")]
    [Category("CMS")]
    [Description("Redirects the page to the URL provided.")]
    [TextField( "Url", "The path to redirect to" )]
    public partial class Redirect : Rock.Web.UI.RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if (!Page.IsPostBack)
            {
                RefreshContent();
            }
        }

        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RefreshContent();
        }
        
        private void RefreshContent()
        {
            string url = GetAttributeValue( "Url" );

            if ( !string.IsNullOrEmpty( url ) )
            {
                if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
                {
                    nbAlert.Text = string.Format( "If you did not have Administrate permissions on this block, you would have been redirected to here: <a href='{0}'>{0}</a>.", Page.ResolveUrl( url ) );
                }
                else
                {
                    Response.Redirect( GetAttributeValue( "Url" ), false );
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
            else
            {
                nbAlert.Text = "Missing Url value for redirect!";
            }
        }

    }
}