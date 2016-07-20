using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Core
{
    [DisplayName( "Bulk Person Inactivator" )]
    [Category( "CCV > Core" )]
    [Description( "Block for mass updating person records to inactive" )]

    [CodeEditorField( "Query", "The Query that determines the list of people that need to be inactivated. Leave this blank to use the DataView instead.", CodeEditorMode.Sql, order: 10 )]
    [DataViewField( "DataView", "The DataView that determines the list of people that need to be inactivated.", order: 11 )]
    public partial class BulkPersonInactivator : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                ShowDetail();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        protected void ShowDetail()
        {
            var query = this.GetAttributeValue( "Query" );
            nbConfigurationWarning.Visible = false;

            if ( !string.IsNullOrEmpty( query ) )
            {
                lDataViewName.Text = string.Format( "SQL: <pre>{0}</pre>", query.Truncate( 255 ) );
                var dataTable = DbService.GetDataTable( query, System.Data.CommandType.Text, null );
                lRecordCount.Text = dataTable.Rows.Count.ToString();
            }
            else
            {
                var dataViewGuid = this.GetAttributeValue( "DataView" ).AsGuidOrNull();
                if ( dataViewGuid.HasValue )
                {
                    var rockContext = new RockContext();
                    var dataView = new DataViewService( rockContext ).Get( dataViewGuid.Value );
                    if ( dataView != null )
                    {
                        lDataViewName.Text = dataView.Name;
                        List<string> errorMessages;
                        var qry = dataView.GetQuery( null, null, out errorMessages );
                        lRecordCount.Text = qry.Count().ToString();
                    }
                    else
                    {
                        nbConfigurationWarning.Visible = true;
                    }
                }
                else
                {
                    nbConfigurationWarning.Visible = true;
                }
            }
        }


        protected void btnInactivateRecords_Click( object sender, EventArgs e )
        {
            List<int> personIds = null;
            var query = this.GetAttributeValue( "Query" );

            if ( !string.IsNullOrEmpty( query ) )
            {
                var dataTable = DbService.GetDataTable( query, System.Data.CommandType.Text, null );
                personIds = dataTable.Select().Select( a => (int)a["Id"] ).OrderBy( a => a ).ToList();
            }
            else
            {
                var dataViewGuid = this.GetAttributeValue( "DataView" ).AsGuidOrNull();
                if ( dataViewGuid.HasValue )
                {
                    var rockContext = new RockContext();
                    var dataView = new DataViewService( rockContext ).Get( dataViewGuid.Value );
                    if ( dataView != null )
                    {

                        List<string> errorMessages;
                        var qry = dataView.GetQuery( null, null, out errorMessages );
                        personIds = qry.Select( a => a.Id ).OrderBy( a => a ).ToList();
                    }
                }
            }

            if ( personIds != null )
            {
                // TODO
            }
        }


        #endregion
    }
}