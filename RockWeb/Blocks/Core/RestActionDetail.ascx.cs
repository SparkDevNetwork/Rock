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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using System.Web.Http;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "RestActionDetail" )]
    [Category( "Core" )]
    [Description( "Detail block for a Rest Action." )]
    public partial class RestActionDetail : RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // added for your convenience

                var service = new RestActionService( new RockContext() );
                var restAction = service.Get( PageParameter( "RestActionId" ).AsInteger() );

                if ( restAction != null )
                {
                    var config = GlobalConfiguration.Configuration;
                    var explorer = config.Services.GetApiExplorer();
                    var controllerActions = explorer.ApiDescriptions.Where( a => ( a.ActionDescriptor.ControllerDescriptor.ControllerName == restAction.Controller.Name ) );
                    var apiDescription = controllerActions.FirstOrDefault( a => a.ID == restAction.ApiId );
                    if ( apiDescription != null )
                    {
                        hfUrl.Value = this.ResolveUrl( "~/" + apiDescription.RelativePath );
                        btnDELETE.Visible = apiDescription.HttpMethod == System.Net.Http.HttpMethod.Delete;
                        btnGET.Visible = apiDescription.HttpMethod == System.Net.Http.HttpMethod.Get;
                        btnPUT.Visible = apiDescription.HttpMethod == System.Net.Http.HttpMethod.Put;
                        btnPOST.Visible = apiDescription.HttpMethod == System.Net.Http.HttpMethod.Post;
                        //lstParameterValues.Value = "key^value|key^value|key^value|key^value|key^value";
                        foreach ( var param in apiDescription.ParameterDescriptions )
                        {
                            lstParameterValues.Value += param.Name + "|";
                        }
                    }
                    else
                    {
                        // todo warning
                    }
                }
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
    }
}