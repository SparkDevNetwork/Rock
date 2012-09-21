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
		private const string definedTypeName = "Frequency";

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
				_canConfigure = CurrentPage.IsAuthorized( "Configure", CurrentPerson );

				BindCategoryFilter();				

				if ( _canConfigure )
				{
					rGridMetric.DataKeyNames = new string[] { "id" };
					rGridMetric.Actions.IsAddEnabled = true;

					rGridMetric.Actions.AddClick += rGridMetric_Add;
					rGridMetric.GridRebind += rGridMetric_GridRebind;

					rGridValue.DataKeyNames = new string[] { "id" };
					rGridValue.Actions.IsAddEnabled = true;

					rGridValue.Actions.AddClick += rGridValue_Add;
					rGridValue.GridRebind += rGridValue_GridRebind;

					modalValue.SaveClick += btnSaveValue_Click;
					modalValue.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );

					this.Page.ClientScript.RegisterStartupScript( this.GetType(), 
						string.Format( "grid-confirm-delete-{0}", CurrentBlock.Id ), @"
						Sys.Application.add_load(function () {{
							$('td.grid-icon-cell.delete a').click(function(){{
								return confirm('Are you sure you want to delete this value?');
								}});
						}});", true 
					);								
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
				BindGridMetric();

			base.OnLoad( e );
		}

		#endregion

		#region Events

		protected void ddlCategoryFilter_SelectedIndexChanged( object sender, EventArgs e )
		{
			BindCategoryFilter();
			BindGridMetric();
		}

		protected void rGridMetric_Add( object sender, EventArgs e )
		{
			BindCollectionFrequencies();
			ShowEditMetric( 0 );
		}

		protected void rGridMetric_Edit( object sender, RowEventArgs e )
		{
			BindCollectionFrequencies();
			ShowEditMetric( (int)rGridMetric.DataKeys[e.RowIndex]["id"] );
		}

		protected void rGridMetric_EditValue( object sender, RowEventArgs e )
		{
			hfIdMetric.Value = rGridMetric.DataKeys[e.RowIndex]["id"].ToString();
			BindGridValue();

			pnlMetricList.Visible = false;
			pnlValueList.Visible = true;
		}

		protected void rGridMetric_Delete( object sender, RowEventArgs e )
		{
			var metricService = new Rock.Core.MetricService();

			Rock.Core.Metric metric = metricService.Get( (int)rGridMetric.DataKeys[e.RowIndex]["id"] );
			if ( metric != null )
			{
				Rock.Web.Cache.Metric.Flush( metric.Id );

				metricService.Delete( metric, CurrentPersonId );
				metricService.Save( metric, CurrentPersonId );
			}

			BindGridMetric();
		}

		protected void btnSaveMetric_Click( object sender, EventArgs e )
		{
			using ( new Rock.Data.UnitOfWorkScope() )
			{
				var metricService = new Rock.Core.MetricService();
				Rock.Core.Metric metric;				
				int metricId = ( hfIdMetric.Value ) != null ? Int32.Parse( hfIdMetric.Value ) : 0;

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
				metric.MinValue = tbMinValue.Text != "" ? Int32.Parse( tbMinValue.Text, NumberStyles.AllowThousands ) : (int?)null;
				metric.MaxValue = tbMinValue.Text != "" ? Int32.Parse( tbMaxValue.Text, NumberStyles.AllowThousands ) : (int?)null;
				metric.Type = cbType.Checked;
				metric.CollectionFrequencyId = Int32.Parse( ddlCollectionFrequency.SelectedValue );
				metric.Source = tbSource.Text;
				metric.SourceSQL = tbSourceSQL.Text;

				metricService.Save( metric, CurrentPersonId );
			}

			BindCategoryFilter();
			BindGridMetric();

			pnlMetricDetails.Visible = false;
			pnlMetricList.Visible = true;
		}

		protected void btnCancelMetric_Click( object sender, EventArgs e )
		{
			pnlMetricDetails.Visible = false;
			pnlMetricList.Visible = true;
		}

		void rGridMetric_GridRebind( object sender, EventArgs e )
		{
			BindCategoryFilter();
			BindGridMetric();
		}
		
		protected void rGridValue_Add( object sender, EventArgs e )
		{
			BindMetricFilter();
			ShowEditValue( 0 );
		}

		protected void rGridValue_Edit( object sender, RowEventArgs e )
		{
			BindMetricFilter();
			ShowEditValue( (int)rGridValue.DataKeys[e.RowIndex]["id"] );
		}

		protected void rGridValue_Delete( object sender, RowEventArgs e )
		{
			var metricValueService = new Rock.Core.MetricValueService();

			Rock.Core.MetricValue metricValue = metricValueService.Get( (int)rGridValue.DataKeys[e.RowIndex]["id"] );
			if ( metricValue != null )
			{
				Rock.Web.Cache.Metric.Flush( metricValue.Id );

				metricValueService.Delete( metricValue, CurrentPersonId );
				metricValueService.Save( metricValue, CurrentPersonId );
			}

			BindGridValue();
		}

		protected void btnValueDone_Click( object sender, EventArgs e )
		{
			BindCategoryFilter();
			BindGridMetric();
			pnlValueList.Visible = false;
			pnlMetricList.Visible = true;		
		}

		protected void btnSaveValue_Click( object sender, EventArgs e )
		{
			using ( new Rock.Data.UnitOfWorkScope() )
			{
				int metricValueId = ( hfIdValue.Value ) != null ? Int32.Parse( hfIdValue.Value ) : 0;
				var metricValueService = new Rock.Core.MetricValueService();
				Rock.Core.MetricValue metricValue;								

				if ( metricValueId == 0 )
				{
					metricValue = new Rock.Core.MetricValue();
					metricValue.IsSystem = false;
					metricValue.MetricId = Int32.Parse( hfIdMetric.Value );
					metricValueService.Add( metricValue, CurrentPersonId );
				}
				else
				{
					Rock.Web.Cache.Metric.Flush( metricValueId );
					metricValue = metricValueService.Get( metricValueId );
				}

				metricValue.Value = tbValue.Text;
				metricValue.Description = tbValueDescription.Text;
				metricValue.xValue = tbXValue.Text;
				metricValue.Label = tbLabel.Text;
				metricValue.isDateBased = cbIsDateBased.Checked;
				metricValue.MetricId = Int32.Parse(ddlMetricFilter.SelectedValue);
				metricValueService.Save( metricValue, CurrentPersonId );
			}

			BindGridValue();

			modalValue.Hide();
			pnlValueList.Visible = true;
		}

		protected void btnCancelValue_Click( object sender, EventArgs e )
		{
			modalValue.Hide();
			pnlValueList.Visible = true;
		}

		void rGridValue_GridRebind( object sender, EventArgs e )
		{
			BindGridValue();
		}
		
		#endregion

		#region Methods
				
		private void BindCategoryFilter()
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

		private void BindGridMetric()
		{
			var queryable = new Rock.Core.MetricService().Queryable();

			if ( ddlCategoryFilter.SelectedValue != "[All]" )
				queryable = queryable.
					Where( a => a.Category == ddlCategoryFilter.SelectedValue );

			SortProperty sortProperty = rGridMetric.SortProperty;
			if ( sortProperty != null )
				queryable = queryable.
					Sort( sortProperty );
			else
				queryable = queryable.
					OrderBy( a => a.Category ).
					ThenBy( a => a.Title );

			rGridMetric.DataSource = queryable.ToList();
			rGridMetric.DataBind();

		}

		private void BindCollectionFrequencies()
		{
			ddlCollectionFrequency.Items.Clear();

			List<DefinedValue> definedValues = new List<DefinedValue>();
			using ( new Rock.Data.UnitOfWorkScope() )
			{
				definedValues = definedValueService.Queryable().
					Where( definedValue => definedValue.DefinedType.Name == definedTypeName ).
					ToList();
			}

			foreach ( DefinedValue value in definedValues )
				ddlCollectionFrequency.Items.Add( new ListItem( value.Name, value.Id.ToString() ) );

		}

		protected void ShowEditMetric( int metricId )
		{
			hfIdMetric.Value = metricId.ToString();

			var metricModel = new Rock.Core.MetricService().Get( metricId );
			
			if ( metricModel != null )
			{
				var metric = Rock.Web.Cache.Metric.Read( metricModel );

				lAction.Text = "Edit";
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

			pnlMetricList.Visible = false;
			pnlMetricDetails.Visible = true;
		}

		private void BindMetricFilter()
		{
			ddlMetricFilter.Items.Clear();

			List<Metric> metrics = new List<Metric>();
			using ( new Rock.Data.UnitOfWorkScope() )
			{
				metrics = metricService.Queryable().
					Where( a => a.Title != "" && a.Title != null ).
					OrderBy( a => a.Title ).
					ToList();
			}

			foreach ( Metric metric in metrics )
				ddlMetricFilter.Items.Add( new ListItem( metric.Title, metric.Id.ToString() ) );

			ddlMetricFilter.SelectedValue = hfIdMetric.Value;
		}
		
		private void BindGridValue( )
		{
			int metricId = ( hfIdMetric.Value != null ) ? Int32.Parse( hfIdMetric.Value ) : 0;
			var queryable = new Rock.Core.MetricValueService().Queryable();
			
			queryable = queryable.
				Where( a => a.MetricId == metricId );

			SortProperty sortProperty = rGridValue.SortProperty;
			if ( sortProperty != null )
				queryable = queryable.
					Sort( sortProperty );
			else
				queryable = queryable.
					OrderBy( a => a.Id ).
					ThenBy( a => a.Value );

			rGridValue.DataSource = queryable.ToList();
			rGridValue.DataBind();
		}

		protected void ShowEditValue( int metricValueId )
		{
			hfIdValue.Value = metricValueId.ToString();
			
			var metricValueModel = new Rock.Core.MetricValueService().Get( metricValueId );

			if ( metricValueModel != null )
			{
				var metricValue = Rock.Web.Cache.MetricValue.Read( metricValueModel );
				lValue.Text = "Edit";
				tbValue.Text = metricValue.Value;
				tbValueDescription.Text = metricValue.Description;
				tbXValue.Text = metricValue.xValue;
				tbLabel.Text = metricValue.Label;
				cbIsDateBased.Checked = metricValue.isDateBased;
			}
			else 
			{
				lValue.Text = "Add";
				tbValue.Text = string.Empty;
				tbValueDescription.Text = string.Empty;
				tbXValue.Text = string.Empty;
				tbLabel.Text = string.Empty;
				cbIsDateBased.Checked = false;
			}

			ddlMetricFilter.SelectedValue = hfIdMetric.Value;				
			modalValue.Show();
			
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