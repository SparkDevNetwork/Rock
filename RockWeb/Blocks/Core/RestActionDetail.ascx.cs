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
using System.Linq;
using System.Web.Http;
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "REST Action Detail" )]
    [Category( "Core" )]
    [Description( "Detail block for a REST Action that can be used to test the REST action." )]
    public partial class RestActionDetail : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
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
                        lUrlPreview.Text = apiDescription.HttpMethod.ToString().ToUpper() + " " + hfUrl.Value;
                        btnDELETE.Visible = apiDescription.HttpMethod == System.Net.Http.HttpMethod.Delete;
                        
                        btnGET.Visible = apiDescription.HttpMethod == System.Net.Http.HttpMethod.Get;
                        rblLoadAttributes.Visible = apiDescription.HttpMethod == System.Net.Http.HttpMethod.Get;
                        
                        btnPUT.Visible = apiDescription.HttpMethod == System.Net.Http.HttpMethod.Put;
                        btnPOST.Visible = apiDescription.HttpMethod == System.Net.Http.HttpMethod.Post;
                        tbPayload.Visible = apiDescription.HttpMethod == System.Net.Http.HttpMethod.Post || apiDescription.HttpMethod == System.Net.Http.HttpMethod.Put;
                        
                        foreach ( var param in apiDescription.ParameterDescriptions )
                        {
                            if ( param.Source == System.Web.Http.Description.ApiParameterSource.FromUri )
                            {
                                lstParameterValues.Value += param.Name + "|";
                            }
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

        #endregion
    }
}