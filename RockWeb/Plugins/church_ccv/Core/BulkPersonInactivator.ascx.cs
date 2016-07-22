using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.SignalR;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Core
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [DisplayName( "Bulk Person Inactivator" )]
    [Category( "CCV > Core" )]
    [Description( "Block for mass updating person records to inactive" )]

    public partial class BulkPersonInactivator : RockBlock
    {
        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();


        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", fingerprint: false );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            dvpRecordStatusReason.DefinedTypeId = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() ).Id;
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
            ShowDetail();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        protected void ShowDetail()
        {
            var recordStatusInactive = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
            var inactiveReasonNoActivity = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_NO_ACTIVITY.AsGuid() );
            lUpdatedRecordStatus.Text = recordStatusInactive.Value;
            dvpRecordStatusReason.SelectedValue = inactiveReasonNoActivity.Id.ToString();
            lRecordCount.Text = "-";

            var dataViewId = dvpDataview.SelectedValue.AsIntegerOrNull();
            if ( dataViewId.HasValue )
            {
                var rockContext = new RockContext();
                var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
                if ( dataView != null )
                {
                    List<string> errorMessages;
                    var qry = dataView.GetQuery( null, null, out errorMessages );
                    lRecordCount.Text = qry.Count().ToString();
                }
            }
        }

        private static bool _cancel = false;

        /// <summary>
        /// Handles the Click event of the btnInactivateRecords control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnInactivateRecords_Click( object sender, EventArgs e )
        {
            List<int> personIds = null;
            string newInactiveReasonNote = tbInactiveReasonNote.Text;

            var dataViewId = dvpDataview.SelectedValue.AsIntegerOrNull();
            if ( dataViewId.HasValue )
            {
                var rockContext = new RockContext();
                var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
                if ( dataView != null )
                {

                    List<string> errorMessages;
                    var qry = dataView.GetQuery( null, null, out errorMessages );
                    personIds = qry.Select( a => a.Id ).OrderBy( a => a ).ToList();
                }
            }


            if ( personIds != null )
            {
                var recordStatusInactive = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
                var inactiveReasonValue = DefinedValueCache.Read( dvpRecordStatusReason.SelectedValue.AsInteger() );
                btnCancel.Visible = true;
                _cancel = false;

                var threadContext = HttpContext.Current;

                System.Threading.Tasks.Task.Run( () =>
                {
                    HttpContext.Current = threadContext;
                    int _personId = 0;
                    try
                    {
                        int progress = 0;
                        int countInactivated = 0;
                        int countAlreadyInactive = 0;
                        int count = personIds.Count();
                        int lastPercent = -1;
                        foreach ( var personId in personIds )
                        {
                            _personId = personId;
                            if ( _cancel )
                            {
                                // if the webpage time to load back up on the client
                                System.Threading.Thread.Sleep( 2000 );
                                break;
                            }

                            List<string> changes = new List<string>();
                            using ( var rockContext = new RockContext() )
                            {
                                var personService = new PersonService( rockContext );

                                var person = personService.Get( personId );
                                
                                if ( person.RecordStatusValueId != recordStatusInactive.Id )
                                {
                                    History.EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( person.RecordStatusValueId ), recordStatusInactive.Value );
                                    person.RecordStatusValueId = recordStatusInactive.Id;

                                    History.EvaluateChange( changes, "Inactive Reason", DefinedValueCache.GetName( person.RecordStatusReasonValueId ), inactiveReasonValue.Value );
                                    person.RecordStatusReasonValueId = inactiveReasonValue.Id;

                                    if ( !string.IsNullOrWhiteSpace( newInactiveReasonNote ) )
                                    {
                                        History.EvaluateChange( changes, "Inactive Reason Note", person.InactiveReasonNote, newInactiveReasonNote );
                                        person.InactiveReasonNote = newInactiveReasonNote;
                                    }

                                    HistoryService.AddChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, changes );
                                    countInactivated++;

                                    rockContext.SaveChanges();
                                }
                                else
                                {
                                    countAlreadyInactive++;
                                }

                                progress++;

                                int percent = (int)( (double)( progress * 100 ) / count );
                                if ( lastPercent != percent )
                                {
                                    _hubContext.Clients.All.receiveNotification( percent, string.Format( "{0}/{1}", progress, count ) );
                                }

                                lastPercent = percent;
                            }
                        }

                        _hubContext.Clients.All.receiveNotification( "completed", string.Format(
                            @"
<pre>
<h2>{3}</h2>
{0} records selected: 
    {1} records inactivated successfully
    {2} already set to inactive
</pre>", count, countInactivated, countAlreadyInactive, _cancel ? "Cancelled" : "Completed" ) );
                    }
                    catch ( Exception ex )
                    {
                        _hubContext.Clients.All.receiveNotification( "exception", string.Format( "PersonId:{0}, Ex:{1}", _personId, ex.Message ) );
                    }
                } );
            }
        }


        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the dvpDataview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dvpDataview_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowDetail();
        }


        protected void btnCancel_Click( object sender, EventArgs e )
        {
            _cancel = true;
            btnCancel.Visible = false;
        }
    }
}