//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Core;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{

	public partial class Metrics : Rock.Web.UI.Block
	{
		#region Fields

		private MetricService metricService = new MetricService();
		private MetricValueService metricValueService = new MetricValueService();
		private DefinedValueService definedValueService = new DefinedValueService();
		private bool _canConfigure = false;

		#endregion

		#region Control Methods

		protected override void OnInit( EventArgs e )
		{
			base.OnInit( e );

			try
			{
				_canConfigure = PageInstance.Authorized( "Configure", CurrentUser );

				BindFilter();

				BindCollectionFrequencies();

				if ( _canConfigure )
				{
					rGrid.DataKeyNames = new string[] { "id" };
					rGrid.Actions.EnableAdd = true;

					rGrid.Actions.AddClick += rGrid_Add;
					rGrid.GridRebind += rGrid_GridRebind;

					string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('#{0} td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this metric?');
                }});
        }});
    ", rGrid.ClientID );
					this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", BlockInstance.Id ), script, true );

					//// Create the dropdown list for listing the available field types
					//var fieldTypeService = new Rock.Core.FieldTypeService();
					//var items = fieldTypeService.
					//    Queryable().
					//    Select( f => new { f.Id, f.Name } ).
					//    OrderBy( f => f.Name );

					//ddlFieldType.AutoPostBack = true;
					//ddlFieldType.SelectedIndexChanged += new EventHandler( ddlFieldType_SelectedIndexChanged );
					//ddlFieldType.Items.Clear();
					//foreach ( var item in items )
					//    ddlFieldType.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
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
			ShowEdit( (int)rGrid.DataKeys[e.RowIndex]["id"] );
		}

		protected void rGrid_Delete( object sender, RowEventArgs e )
		{
			var metricService = new Rock.Core.MetricService();

			Rock.Core.Metric metric = metricService.Get( (int)rGrid.DataKeys[e.RowIndex]["id"] );
			if ( metric != null )
			{
				Rock.Web.Cache.Metric.Flush( metric.Id );

				metricService.Delete( metric, CurrentPersonId );
				metricService.Save( metric, CurrentPersonId );
			}

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

		protected void btnSave_Click( object sender, EventArgs e )
		{
			using ( new Rock.Data.UnitOfWorkScope() )
			{
				var metricService = new Rock.Core.MetricService();

				Rock.Core.Metric metric;

				int metricId = 0;
				if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out metricId ) )
					metricId = 0;

				if ( metricId == 0 )
				{
					metric = new Rock.Core.Metric();
					metric.IsSystem = false;
					metricService.Add( metric, CurrentPersonId );
				}
				else
				{
					Rock.Web.Cache.Metric.Flush( metricId );
					metric = metricService.Get( metricId );
				}

				metric.Category = tbCategory.Text;
				metric.Title = tbTitle.Text;
				metric.Subtitle = tbSubtitle.Text;
				metric.Description = tbDescription.Text;
				metric.MinValue = Int32.Parse( tbMinValue.Text, NumberStyles.AllowThousands );
				metric.MaxValue = Int32.Parse( tbMaxValue.Text, NumberStyles.AllowThousands );
				metric.Type = cbType.Checked;
				metric.CollectionFrequencyId = Int32.Parse(ddlCollectionFrequency.SelectedValue);
				metric.CollectionFrequency = definedValueService.Get(metric.CollectionFrequencyId);
				metric.Source = tbSource.Text;
				metric.SourceSQL = tbSourceSQL.Text;

				metricService.Save( metric, CurrentPersonId );
			}

			BindGrid();

			pnlDetails.Visible = false;
			pnlList.Visible = true;
		}

		protected void btnCancel_Click( object sender, EventArgs e )
		{
			pnlDetails.Visible = false;
			pnlList.Visible = true;
		}

		#endregion

		#region Methods

		private void BindCollectionFrequencies()
		{
			ddlCollectionFrequency.Items.Clear();
			
			List<DefinedValue> definedValues = new List<DefinedValue>();
			using ( new Rock.Data.UnitOfWorkScope() )
			{
				definedValues = definedValueService
					.Queryable()
					.Where( 	definedValue => definedValue.DefinedType.Name == "Frequency" )
					.ToList();
			}

			foreach ( DefinedValue value in definedValues )
				ddlCollectionFrequency.Items.Add( new ListItem( value.Name, value.Id.ToString() ) );
						
		}

		private void BindFilter()
		{
			ddlCategoryFilter.Items.Clear();
			ddlCategoryFilter.Items.Add( "[All]" );

			MetricService metricService = new MetricService();
			var items = metricService.Queryable().
				Where( a => a.Category != "" && a.Category != null ).
				OrderBy( a => a.Category ).
				Select( a => a.Category ).
				Distinct().ToList();

			foreach ( var item in items )
				ddlCategoryFilter.Items.Add( item );
		}

		private void BindGrid()
		{
			var queryable = new Rock.Core.MetricService().Queryable();

			if ( ddlCategoryFilter.SelectedValue != "[All]" )
				queryable = queryable.
					Where( a => a.Category == ddlCategoryFilter.SelectedValue );

			SortProperty sortProperty = rGrid.SortProperty;
			if ( sortProperty != null )
				queryable = queryable.
					Sort( sortProperty );
			else
				queryable = queryable.
					OrderBy( a => a.Category ).
					ThenBy( a => a.Title );

			rGrid.DataSource = queryable.ToList();
			rGrid.DataBind();
		}

		
		protected void ShowEdit( int metricId )
		{
			var metricModel = new Rock.Core.MetricService().Get( metricId );

			if ( metricModel != null )
			{
				var metric = Rock.Web.Cache.Metric.Read( metricModel );

				lAction.Text = "Edit";
				hfId.Value = metric.Id.ToString();

				tbCategory.Text = metric.Category;
				tbTitle.Text = metric.Title;
				tbSubtitle.Text = metric.Subtitle;
				tbDescription.Text = metric.Description;
				tbMinValue.Text = metric.MinValue.ToString();
				tbMaxValue.Text = metric.MaxValue.ToString();
				cbType.Checked = metric.Type;
				ddlCollectionFrequency.SelectedValue = metric.CollectionFrequencyId.ToString();
				tbSource.Text = metric.Source;
				tbSourceSQL.Text = metric.SourceSQL;
			}
			else
			{
				lAction.Text = "Add";
				hfId.Value = string.Empty;
				tbCategory.Text = ddlCategoryFilter.SelectedValue != "[All]" ? ddlCategoryFilter.SelectedValue : string.Empty;
				tbTitle.Text = string.Empty;
				tbSubtitle.Text = string.Empty;
				tbDescription.Text = string.Empty;
				tbMinValue.Text = string.Empty;
				tbMaxValue.Text = string.Empty;
				cbType.Checked = false;
				tbSource.Text = string.Empty;
				tbSourceSQL.Text = string.Empty;
			}

			pnlList.Visible = false;
			pnlDetails.Visible = true;
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