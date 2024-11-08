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
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
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

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.BlockTypeGuid( "20AD75DD-0DF3-49E9-9DB1-8537C12B1664" )]
    public partial class RestActionList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }


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
            if ( GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull() != null )
            {
                gActions.RowSelected += gActions_RowSelected;
            }

            var securityField = gActions.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.RestAction ) ).Id;
            }

            modalActionSettings.SaveClick += btnSaveActionSettings_Click;
            modalActionSettings.OnCancelScript = string.Format( "$('#{0}').val('');", hfControllerActionId.ClientID );
        }



        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
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
            if ( int.TryParse( PageParameter( "Controller" ), out controllerId ) )
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
                        if ( obsoleteAttribute != null )
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
        /// Handles the RowSelected event of the gActions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gActions_RowSelected( object sender, RowEventArgs e )
        {
            ShowActionSettingsEdit( ( int ) e.RowKeyValue );
        }


        /// <summary>
        /// Handles the GridRebind event of the gActions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gActions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
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

        private void btnSaveActionSettings_Click( object sender, EventArgs e )
        {
            SaveRestAction();
        }

        protected void gActions_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var restAction = e.Row.DataItem as RestAction;
                var litCacheHeader = e.Row.FindControl( "litCacheHeader" ) as Literal;
                if ( litCacheHeader != null
                        && restAction != null
                        && restAction.CacheControlHeader != null
                        && restAction.CacheControlHeader.RockCacheablityType == Rock.Utility.RockCacheablityType.Public )
                {
                    litCacheHeader.Text = string.Format( "<span class='text-success fa fa-tachometer-alt' title='{0}' data-toggle='tooltip'></span>",
                        restAction.CacheControlHeader.ToString() );
                }
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Bind the grid
        /// </summary>
        public void BindGrid()
        {
            int controllerId = int.MinValue;
            if ( int.TryParse( PageParameter( "Controller" ), out controllerId ) )
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

        private void ShowActionSettingsEdit( int controllerActionId )
        {
            var restAction = new RestActionService( new RockContext() ).Get( controllerActionId );

            if ( restAction.Method == "GET" )
            {
                modalActionSettings.SubTitle = restAction.Path;

                hfControllerActionId.Value = controllerActionId.ToString();

                var cacheHeader = restAction.CacheControlHeader;
                if ( cacheHeader == null )
                {
                    cacheHeader = new Rock.Utility.RockCacheability
                    {
                        RockCacheablityType = Rock.Utility.RockCacheablityType.NoCache
                    };
                }
                cpActionCacheSettings.CurrentCacheability = cacheHeader;

                modalActionSettings.Show();
            }
        }

        private void SaveRestAction()
        {
            var controllerActionId = hfControllerActionId.Value.AsInteger();
            var rockContext = new RockContext();
            var restAction = new RestActionService( rockContext ).Get( controllerActionId );
            if ( restAction != null )
            {
                restAction.CacheControlHeaderSettings = cpActionCacheSettings.CurrentCacheability.ToJson();
                rockContext.SaveChanges();
            }

            modalActionSettings.Hide();
            BindGrid();
        }

        #endregion


    }
}