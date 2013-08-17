//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
    [LinkedPage( "Schedule Builder Page" )]
    [LinkedPage( "Configure Groups Page", "Page for configuration of Check-in Groups/Locations" )]
    public partial class CheckinGroupTypeList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gGroupType.DataKeyNames = new string[] { "id" };
            gGroupType.Actions.ShowAdd = false;
            gGroupType.IsDeleteEnabled = false;
            gGroupType.GridRebind += gGroupType_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );

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

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the GridRebind event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gGroupType_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            GroupTypeService groupTypeService = new GroupTypeService();
            SortProperty sortProperty = gGroupType.SortProperty;

            var qry = groupTypeService.Queryable();

            // limit to show only GroupTypes that have a group type purpose of Checkin Template
            int groupTypePurposeCheckInTemplateId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ) ).Id;
            qry = qry.Where( a => a.GroupTypePurposeValueId == groupTypePurposeCheckInTemplateId );

            if ( sortProperty != null )
            {
                gGroupType.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gGroupType.DataSource = qry.OrderBy( p => p.Name ).ToList();
            }

            gGroupType.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the RowCommand event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewCommandEventArgs"/> instance containing the event data.</param>
        protected void gGroupType_RowCommand( object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e )
        {
            int rowIndex = int.Parse( ( e.CommandArgument as string ) );
            GridViewRow row = ( sender as Grid ).Rows[rowIndex];
            RowEventArgs rowEventArgs = new RowEventArgs( row );

            if ( e.CommandName.Equals( "schedule" ) )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "groupTypeId", rowEventArgs.RowKeyId.ToString() );
                this.NavigateToLinkedPage( "ScheduleBuilderPage", qryParams );
            }
            else if ( e.CommandName.Equals( "configure" ) )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "groupTypeId", rowEventArgs.RowKeyId.ToString() );
                this.NavigateToLinkedPage( "ConfigureGroupsPage", qryParams );
            }
        }

    }
}