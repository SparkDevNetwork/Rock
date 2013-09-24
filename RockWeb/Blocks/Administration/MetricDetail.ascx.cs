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

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var metricService = new MetricService();
                Metric metric = null;
                int metricId = ( hfMetricId.Value ) != null ? Int32.Parse( hfMetricId.Value ) : 0;

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
                metric.MinValue = tbMinValue.Text != "" ? Int32.Parse( tbMinValue.Text, NumberStyles.AllowThousands ) : (int?)null;
                metric.MaxValue = tbMinValue.Text != "" ? Int32.Parse( tbMaxValue.Text, NumberStyles.AllowThousands ) : (int?)null;
                metric.Type = cbType.Checked;
                metric.CollectionFrequencyValueId = Int32.Parse( ddlCollectionFrequency.SelectedValue );
                metric.Source = tbSource.Text;
                metric.SourceSQL = tbSourceSQL.Text;

                metricService.Save( metric, CurrentPersonId );
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Binds the collection frequencies.
        /// </summary>
        private void BindCollectionFrequencies()
        {
            ddlCollectionFrequency.Items.Clear();

            var dTCollectionFrequency = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedType.METRIC_COLLECTION_FREQUENCY ) );

            if ( dTCollectionFrequency != null && dTCollectionFrequency.DefinedType.DefinedValues.Any() )
            {
                var definedValues = dTCollectionFrequency.DefinedType.DefinedValues.OrderBy( dv => dv.Order ).ToList();
                foreach ( var value in definedValues )
                {
                    ddlCollectionFrequency.Items.Add( new ListItem( value.Name, value.Id.ToString() ) );
                }
            }            
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
                lActionTitle.Text = ActionTitle.Edit( Metric.FriendlyTypeName );
                               
            }
            else
            {
                metric = new Metric { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( Metric.FriendlyTypeName );
                
                //tbTitle.Text = string.Empty;
                //tbSubtitle.Text = string.Empty;
                //tbDescription.Text = string.Empty;
                //tbMinValue.Text = string.Empty;
                //tbMaxValue.Text = string.Empty;
                //cbType.Checked = false;
                //tbSource.Text = string.Empty;
                //tbSourceSQL.Text = string.Empty;
            }

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

            BindCollectionFrequencies();

            bool readOnly = false;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Metric.FriendlyTypeName );
                lActionTitle.Text = ActionTitle.View( Metric.FriendlyTypeName );
                btnCancel.Text = "Close";
            }
                        
            btnSave.Visible = !readOnly;
            pnlDetails.Visible = true;
        }        

        #endregion
    }
}