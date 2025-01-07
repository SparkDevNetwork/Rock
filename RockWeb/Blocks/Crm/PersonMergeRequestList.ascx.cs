// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
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

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Person Merge Request List" )]
    [Category( "CRM" )]
    [Description( "Lists Person Merge Requests" )]

    [Rock.SystemGuid.BlockTypeGuid( "4CBFB5FC-0174-489A-8B95-90BB8FAA2144" )]
    public partial class PersonMergeRequestList : RockBlock, ICustomGridColumns
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.DataKeyNames = new string[] { "Id" };
            gList.GridRebind += gList_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Rock.Security.Authorization.EDIT );
            gList.IsDeleteEnabled = canAddEditDelete;
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

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            var entitySetService = new EntitySetService( rockContext );

            var entitySetPurposeGuid = Rock.SystemGuid.DefinedValue.ENTITY_SET_PURPOSE_PERSON_MERGE_REQUEST.AsGuid();
            
            var currentDateTime = RockDateTime.Now;
            var entitySetQry = entitySetService.Queryable()
                .Where( a => a.EntitySetPurposeValue.Guid == entitySetPurposeGuid )
                .Where( s => !s.ExpireDateTime.HasValue || s.ExpireDateTime.Value > currentDateTime );

            SortProperty sortProperty = gList.SortProperty;

            var qryPersonEntities = entitySetService.GetEntityItems<Person>();

            var joinQry = entitySetQry.GroupJoin( qryPersonEntities, n => n.Id, o => o.EntitySetId, ( a, b ) => new
            {
                a.Id,
                a.ModifiedDateTime,
                a.Note,
                a.CreatedByPersonAlias,
                MergeRecords = b.Select( x => x.Item )
            } );

            if ( sortProperty != null )
            {
                joinQry = joinQry.Sort( sortProperty );
            }
            else
            {
                joinQry = joinQry.OrderBy( s => s.ModifiedDateTime );
            }

            gList.SetLinqDataSource( joinQry );
            gList.DataBind();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.DataItem != null )
            {
                dynamic rowData = e.Row.DataItem;
                IEnumerable<Person> mergeRecords = rowData.MergeRecords;
                if ( mergeRecords != null )
                {
                    Literal lMergeRecords = e.Row.FindControl( "lMergeRecords" ) as Literal;
                    if ( lMergeRecords != null )
                    {
                        lMergeRecords.Text = mergeRecords.ToList().AsDelimited( "<br />" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, RowEventArgs e )
        {
            Page.Response.Redirect( string.Format( gList.PersonMergePageRoute, e.RowKeyId ), false );
            Context.ApplicationInstance.CompleteRequest();
        }


        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var entitySetService = new EntitySetService( rockContext );
            EntitySet entitySet = entitySetService.Get( e.RowKeyId );
            if ( entitySet != null )
            {
                string errorMessage;
                if ( !entitySetService.CanDelete( entitySet, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                // mark it as expired and RockCleanup will delete it later
                entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( -1 );
                entitySet.EntitySetPurposeValueId = null;
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion
    }
}