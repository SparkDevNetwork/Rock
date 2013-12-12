using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.UI;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [LinkedPage( "Detail Page" )]
    public partial class RestControllerList : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gItems.DataKeyNames = new string[] { "ControllerType" };
            gItems.GridRebind += gItems_GridRebind;
            gItems.Actions.ShowAdd = false;
            gItems.IsDeleteEnabled = false;
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
        /// Handles the GridRebind event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gItems_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var config = GlobalConfiguration.Configuration;
            var explorer = config.Services.GetApiExplorer();

            var apiList = explorer.ApiDescriptions.OrderBy( a => a.RelativePath ).ThenBy( a => a.HttpMethod.Method ).ToList();
            var controllerList = apiList
                .GroupBy( a => a.ActionDescriptor.ControllerDescriptor.ControllerName )
                .Select( s => new
                {
                    ControllerName = s.Key,
                    ControllerType = s.Max( a => a.ActionDescriptor.ControllerDescriptor.ControllerType.FullName ),
                    Actions = s.Count()
                } ).ToList();

            gItems.DataSource = controllerList;
            gItems.DataBind();
        }

        /// <summary>
        /// Handles the RowSelected event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gItems_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "controllerType", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", queryParams );
        }
    }
}