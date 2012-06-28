//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;

using Rock.Core;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing the metrics that are available 
    /// </summary>
    public partial class Metrics : Rock.Web.UI.Block
    {
        #region Fields

		protected int metricId = 0;     

        private bool _canConfigure = false;
        private Rock.Core.MetricService _metricService = new Rock.Core.MetricService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {

                _canConfigure = PageInstance.Authorized( "Configure", CurrentUser );

                BindFilter();

                if ( _canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.Actions.EnableAdd = true;

                    rGrid.Actions.AddClick += rGrid_Add;
                    rGrid.GridRebind += rGrid_GridRebind;
                    modalDetails.SaveClick += modalDetails_SaveClick;

                    string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('#{0} td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this setting?');
                }});
        }});
    ", rGrid.ClientID );
                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", BlockInstance.Id ), script, true );

                }
                else
                {
                    DisplayError( "You are not authorized to configure this page" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _canConfigure )
                BindGrid();

            base.OnLoad( e );
        }

        #endregion

        #region Events

        protected void ddlCategoryFilter_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
           

            BindGrid();
        }

        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void modalDetails_SaveClick( object sender, EventArgs e )
        {
            Rock.Core.Metric metric;

			if ( hfMetricId.Value != string.Empty && !Int32.TryParse( hfMetricId.Value, out metricId ) )
				metricId = 0;

			if ( metricId == 0 )
			{
				metric = new Rock.Core.Metric();
				metric.System = false;
				metric.Order = 0;
			} 
			else {
				Rock.Web.Cache.Attribute.Flush( metricId );
				metric = _metricService.Get( metricId );
			}

			metric.Type = Convert.ToBoolean( ddlType.SelectedIndex );
			metric.Category = tbCategory.Text;
			metric.Title = tbTitle.Text;
			metric.Subtitle = tbSubtitle.Text;
			metric.Description = tbDescription.Text;
			metric.MinValue = Int32.Parse( tbMinValue.Text );
			metric.MaxValue = Int32.Parse( tbMaxValue.Text );
			metric.CollectionFrequency = ddlCollectionFrequency.SelectedIndex;
			metric.Source = tbSource.Text;
			metric.Source = tbSourceSQL.Text;

			_metricService.Save( metric, CurrentPersonId );

            BindGrid();
        }

        #endregion

        #region Methods

        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( "[All]" );

			MetricService metricService = new MetricService();
            
        }

        private void BindGrid()
        {
            

           

            rGrid.DataBind();
        }

        protected void ShowEdit( int metricId )
        {
			var metric = _metricService.Get( metricId );

			if ( metric != null )
			{
				modalDetails.Title = "Edit Metric";
				hfMetricId.Value = metric.Id.ToString();

				tbTitle.Text = metric.Title;
				tbSubtitle.Text = metric.Subtitle;
				tbDescription.Text = metric.Description;
				tbSource.Text = metric.Source;
				tbSourceSQL.Text = metric.SourceSQL;
				tbCategory.Text = metric.Category;
			}
			else
			{
				modalDetails.Title = "Add Metric";
				
				tbTitle.Text = string.Empty;
				tbSubtitle.Text = string.Empty;
				tbDescription.Text = string.Empty;
				tbSource.Text = string.Empty;
				tbSourceSQL.Text = string.Empty;
				tbCategory.Text = ddlCategoryFilter.SelectedValue != "[All]" ? ddlCategoryFilter.SelectedValue : string.Empty;
				try
				{
					ddlType.SelectedValue = ddlType.Items.FindByText( "Text" ).Value;
				}
				catch
				{
					ddlType.SelectedIndex = 0;
				}

			}

            modalDetails.Show();
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }

        #endregion
    }
}