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
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{

    /// <summary>
    /// User input controls for metric details
    /// </summary>
    public partial class MetricDetail : Rock.Web.UI.RockBlock
    {
        #region Control Methods

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
                    ShowDetail( "metricId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var metricService = new MetricService();
            var metric = metricService.Get( hfMetricId.ValueAsInt() );
            ShowEdit( metric );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                int metricId = hfMetricId.ValueAsInt();
                var metricService = new MetricService();
                Metric metric = null;                

                if ( metricId == 0 )
                {
                    metric = new Metric();
                    metric.IsSystem = false;
                    metricService.Add( metric, CurrentPersonId );
                }
                else
                {
                    metric = metricService.Get( metricId );
                }

                metric.Category = tbCategory.Text;
                metric.Title = tbTitle.Text;
                metric.Subtitle = tbSubtitle.Text;
                metric.Description = tbDescription.Text;
                metric.MinValue = tbMinValue.Text.AsType<int?>();
                metric.MaxValue = tbMaxValue.Text.AsType<int?>();
                metric.Type = cbType.Checked;
                metric.CollectionFrequencyValueId = Int32.Parse( ddlCollectionFrequency.SelectedValue );
                metric.Source = tbSource.Text;
                metric.SourceSQL = tbSourceSQL.Text;

                if ( !metric.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                metricService.Save( metric, CurrentPersonId );
                hfMetricId.SetValue( metric.Id );
            }

            var savedMetric = new MetricService().Get( hfMetricId.ValueAsInt() );
            ShowReadOnly( savedMetric );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var metric = new MetricService().Get( hfMetricId.ValueAsInt() );
            ShowReadOnly( metric );
        }
        
        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the collection frequencies.
        /// </summary>
        private void BindCollectionFrequencies()
        {
            ddlCollectionFrequency.Items.Clear();

            var dTCollectionFrequency = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.METRIC_COLLECTION_FREQUENCY ) );

            if ( dTCollectionFrequency != null && dTCollectionFrequency.DefinedValues.Any() )
            {
                var definedValues = dTCollectionFrequency.DefinedValues.OrderBy( dv => dv.Order ).ToList();
                foreach ( var value in definedValues )
                {
                    ddlCollectionFrequency.Items.Add( new ListItem( value.Name, value.Id.ToString() ) );
                }
            }            
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="metric">The metric.</param>
        private void ShowReadOnly( Metric metric )
        {
            SetEditMode( false );

            hfMetricId.SetValue( metric.Id );            
            lblDetails.Text = new DescriptionList()
                .Add( "Title", metric.Title )                
                .Add( "Description", metric.Description )
                .Add( "Category", metric.Category )
                .Add( "Collection Frequency", metric.CollectionFrequencyValue.Name )
                .Add( "MinValue", metric.MinValue )
                .Add( "MaxValue", metric.MaxValue )
                .Add( "Source", metric.Source )
                .Html;
        }        

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="metric">The metric.</param>
        protected void ShowEdit( Metric metric )
        {
            if ( metric.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( DefinedType.FriendlyTypeName );
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( DefinedType.FriendlyTypeName );
            }

            SetEditMode( true );

            BindCollectionFrequencies();
            hfMetricId.Value = metric.Id.ToString();
            tbCategory.Text = metric.Category;
            tbTitle.Text = metric.Title;
            tbSubtitle.Text = metric.Subtitle;
            tbDescription.Text = metric.Description;
            tbMinValue.Text = metric.MinValue.ToString();
            tbMaxValue.Text = metric.MaxValue.ToString();
            cbType.Checked = metric.Type;
            ddlCollectionFrequency.SelectedValue = metric.CollectionFrequencyValueId.ToString();
            tbSource.Text = metric.Source;
            tbSourceSQL.Text = metric.SourceSQL;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;            
            fieldsetViewDetails.Visible = !editable;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        protected void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "metricId" ) )
            {
                return;
            }
            
            Metric metric = null;
            if ( !itemKeyValue.Equals( 0 ) )
            {
                metric = new MetricService().Get( itemKeyValue );                
            }
            else
            {
                metric = new Metric { Id = 0 };                
            }

            bool readOnly = false;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Metric.FriendlyTypeName );
            }

            if ( !readOnly )
            {
                btnEdit.Visible = true;
                if ( metric.Id > 0 )
                {
                    ShowReadOnly( metric );
                }
                else
                {
                    ShowEdit( metric );
                }                
            }
            else
            {
                btnEdit.Visible = false;
                ShowReadOnly( metric );                
            }
                                    
            btnSave.Visible = !readOnly;
        }        

        #endregion
    }
}