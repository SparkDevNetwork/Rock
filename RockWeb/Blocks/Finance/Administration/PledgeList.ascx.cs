//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance.Administration
{
    [DetailPage]
    public partial class PledgeList : RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gPledges.DataKeyNames = new[] { "id" };
            gPledges.Actions.IsAddEnabled = true;
            gPledges.Actions.AddClick += gPledges_Add;
            gPledges.GridRebind += gPledges_GridRebind;
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gPledges.Actions.IsAddEnabled = canAddEditDelete;
            gPledges.IsDeleteEnabled = canAddEditDelete;
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        private void gPledges_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        private void gPledges_Add( object sender, EventArgs e )
        {
            NavigateToDetailPage( "pledgeId", 0 );
        }

        protected void gPledges_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( "pledgeId", (int) e.RowKeyValue );
        }

        protected void gPledges_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
                {
                    var pledgeService = new PledgeService();
                    var pledge = pledgeService.Get( (int) e.RowKeyValue );
                    string errorMessage;
                    
                    if ( pledge == null )
                    {
                        return;
                    }

                    if ( !pledgeService.CanDelete( pledge, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    pledgeService.Delete( pledge, CurrentPersonId );
                    pledgeService.Save( pledge, CurrentPersonId );
                });

            BindGrid();
        }

        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            BindGrid();
        }

        private void BindGrid()
        {
            var pledgeService = new PledgeService();
            var sortProperty = gPledges.SortProperty;
            var pledges = pledgeService.Queryable();
            int personId;
            //int fundId;

            if ( ppFilterPerson.SelectedValue != "0" && int.TryParse( ppFilterPerson.PersonId, out personId ) )
            {
                pledges = pledges.Where( p => p.PersonId == personId );
            }

            //if ( fundId.HasValue )
            //{
            //    pledges = pledges.Where( p => p.FundId == fundId.Value );
            //}

            gPledges.DataSource = sortProperty != null ? pledges.Sort( sortProperty ).ToList() : pledges.OrderBy( p => p.FundId ).ToList();
            gPledges.DataBind();
        }
    }
}