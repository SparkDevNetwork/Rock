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
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User contols for managing metric values
    /// </summary>
    public partial class MetricValueList : Rock.Web.UI.RockBlock
    {       
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMetricValues.DataKeyNames = new string[] { "id" };
            gMetricValues.Actions.ShowAdd = true;
            gMetricValues.Actions.AddClick += gMetricValues_Add;
            gMetricValues.GridRebind += gMetricValues_GridRebind;            

            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gMetricValues.Actions.ShowAdd = canAddEditDelete;
            gMetricValues.IsDeleteEnabled = canAddEditDelete;

            modalValue.SaveClick += btnSaveValue_Click;
            modalValue.OnCancelScript = string.Format( "$('#{0}').val('');", hfMetricValueId.ClientID );
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
                string itemId = PageParameter( "metricId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    hfMetricId.Value = itemId.AsInteger( false ).ToString();
                    BindGrid();
                }
                else
                {
                    pnlList.Visible = false;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Add event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gMetricValues_Add( object sender, EventArgs e )
        {
            BindMetricFilter();
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMetricValues_Edit( object sender, RowEventArgs e )
        {
            BindMetricFilter();
            ShowEdit( (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMetricValues_Delete( object sender, RowEventArgs e )
        {
            var metricValueService = new MetricValueService();

            MetricValue metricValue = metricValueService.Get( (int)e.RowKeyValue );
            if ( metricValue != null )
            {
                metricValueService.Delete( metricValue, CurrentPersonId );
                metricValueService.Save( metricValue, CurrentPersonId );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveValue_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                int metricValueId = hfMetricValueId.ValueAsInt();
                var metricValueService = new MetricValueService();
                MetricValue metricValue;

                if ( metricValueId == 0 )
                {
                    metricValue = new MetricValue();
                    metricValue.IsSystem = false;
                    metricValue.MetricId = hfMetricId.ValueAsInt();
                    metricValueService.Add( metricValue, CurrentPersonId );
                }
                else
                {
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

            BindGrid();
            modalValue.Hide();            
        }

        /// <summary>
        /// Handles the GridRebind event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gMetricValues_GridRebind( object sender, EventArgs e )
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
            int metricId = hfMetricId.ValueAsInt();
            var queryable = new MetricValueService().Queryable()
                .Where( a => a.MetricId == metricId );

            SortProperty sortProperty = gMetricValues.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( a => a.Id ).ThenBy( a => a.Value );
            }

            gMetricValues.DataSource = queryable.ToList();
            gMetricValues.DataBind();
        }

        /// <summary>
        /// Binds the metric filter.
        /// </summary>
        private void BindMetricFilter()
        {
            ddlMetricFilter.Items.Clear();

            var metricList = new MetricService().Queryable().OrderBy( m => m.Title ).ToList();

            foreach ( Metric metric in metricList )
            {
                ddlMetricFilter.Items.Add( new ListItem( metric.Title, metric.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Shows the edit modal.
        /// </summary>
        /// <param name="valueId">The value unique identifier.</param>
        protected void ShowEdit( int valueId )
        {
            var metricId = hfMetricId.ValueAsInt();
            var metric = new MetricService().Get( metricId );

            MetricValue metricValue = null;
            if ( !valueId.Equals( 0 ) )
            {
                metricValue = new MetricValueService().Get( valueId );
                if ( metric != null )
                {
                    lActionTitle.Text = ActionTitle.Edit( "metric value for " + metric.Title );
                }
            }
            else
            {
                metricValue = new MetricValue { Id = 0 };
                metricValue.MetricId = metricId;
                if ( metric != null )
                {
                    lActionTitle.Text = ActionTitle.Add( "metric value for " + metric.Title );
                }
            }

            hfMetricValueId.SetValue( metricValue.Id );
            ddlMetricFilter.SelectedValue = hfMetricId.Value;
            tbValue.Text = metricValue.Value;
            tbValueDescription.Text = metricValue.Description;
            tbXValue.Text = metricValue.xValue;
            tbLabel.Text = metricValue.Label;
            cbIsDateBased.Checked = metricValue.isDateBased; 

            modalValue.Show();            
        }

        #endregion
    }
}