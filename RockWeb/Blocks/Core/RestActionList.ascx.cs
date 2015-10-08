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
using System.Linq;
using System.Reflection;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Displays the actions for a given REST controller.
    /// </summary>
    [DisplayName( "REST Action List" )]
    [Category( "Core" )]
    [Description( "Displays the actions for a given REST controller." )]

    [LinkedPage( "Detail Page" )]
    public partial class RestActionList : RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gActions.DataKeyNames = new string[] { "Id" };
            gActions.GridRebind += gActions_GridRebind;
            gActions.Actions.ShowAdd = false;
            gActions.IsDeleteEnabled = false;
            if ( GetAttributeValue( "DetailPage" ).AsGuidOrNull() != null )
            {
                gActions.RowSelected += gActions_RowSelected;
            }

            SecurityField securityField = gActions.Columns[2] as SecurityField;
            securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.RestAction ) ).Id;
        }

        /// <summary>
        /// Handles the RowSelected event of the gActions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gActions_RowSelected( object sender, RowEventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "RestActionId", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", queryParams );
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
                BindGrid();
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int controllerId = int.MinValue;
            if ( int.TryParse( PageParameter( "controller" ), out controllerId ) )
            {
                var controller = new RestControllerService( new RockContext() ).Get( controllerId );
                if ( controller != null )
                {
                    string name = controller.Name.SplitCase();
                    var controllerType = Reflection.FindTypes( typeof( Rock.Rest.ApiControllerBase ) )
                        .Where( a => a.Key.Equals( controller.ClassName ) ).Select( a => a.Value ).FirstOrDefault();
                    if ( controllerType != null )
                    {
                        var obsoleteAttribute = controllerType.GetCustomAttribute<System.ObsoleteAttribute>();
                        if (obsoleteAttribute != null)
                        {
                            hlblWarning.Text = string.Format( "Obsolete: {1}", controller.Name.SplitCase(), obsoleteAttribute.Message );
                        }
                    }

                    lControllerName.Text = name + " Controller";
                    breadCrumbs.Add( new BreadCrumb( name, pageReference ) );
                }
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the GridRebind event of the gActions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gActions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Bind the grid
        /// </summary>
        public void BindGrid()
        {
            int controllerId = int.MinValue;
            if ( int.TryParse( PageParameter( "controller" ), out controllerId ) )
            {
                var service = new RestActionService( new RockContext() );
                var sortProperty = gActions.SortProperty;

                IQueryable<RestAction> qry = service.Queryable()
                    .Where( a => a.ControllerId == controllerId );

                if ( sortProperty != null )
                {
                    qry = qry.Sort( sortProperty );
                }
                else
                {
                    qry = qry.OrderBy( c => c.Method );
                }

                gActions.DataSource = qry.ToList();
                gActions.DataBind();
            }
        }

        /// <summary>
        /// Handles the OnFormatDataValue event of the gActionsPath control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CallbackField.CallbackEventArgs"/> instance containing the event data.</param>
        protected void gActionsPath_OnFormatDataValue( object sender, CallbackField.CallbackEventArgs e )
        {
            e.FormattedValue = e.DataValue.ToString();
            if ( e.FormattedValue.EndsWith( "?key={key}" ) )
            {
                e.FormattedValue = e.FormattedValue.Replace( "?key={key}", "(id)" );
            }
        }

        #endregion
    }
}