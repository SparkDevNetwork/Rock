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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Block to display the benevolence types.
    /// </summary>
    [DisplayName( "Benevolence Type List" )]
    [Category( "Finance" )]
    [Description( "Block to display the benevolence types." )]

    #region Block Attributes
    [LinkedPage(
        "Detail Page",
        Description = "Page used to view details of a benevolence type.",
        Key = AttributeKey.DetailPage )]
    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A" )]
    public partial class BenevolenceTypeList : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string BenevolenceTypeId = "BenevolenceTypeId";
        }

        #endregion

        #region Fields

        private HashSet<int> _benevolenceTypesWithRequests = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBenevolenceType.DataKeyNames = new string[] { "Id" };
            gBenevolenceType.Actions.ShowAdd = true;
            gBenevolenceType.Actions.AddClick += gBenevolenceType_Add;
            gBenevolenceType.GridRebind += gBenevolenceType_GridRebind;
            gBenevolenceType.RowDataBound += gBenevolenceType_RowDataBound;

            // Block Security and special attributes (RockPage takes care of View)
            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );
            gBenevolenceType.Actions.ShowAdd = canEditBlock;
            gBenevolenceType.IsDeleteEnabled = canEditBlock;

            SecurityField securityField = gBenevolenceType.ColumnsOfType<SecurityField>().First();
            securityField.EntityTypeId = EntityTypeCache.GetId<Rock.Model.BenevolenceType>().Value;

            // We will implement a custom delete confirmation dialog
            gBenevolenceType.ShowConfirmDeleteDialog = false;

            string deleteScript = @"
    $('table.js-grid-benevolence-type-list a.grid-delete-button').on('click', function( e ){
        var $btn = $(this);
        e.preventDefault();

        var confirmMsg = 'Are you sure you want to delete this benevolence type?';
        if ($btn.closest('tr').hasClass('js-has-request')) {
            confirmMsg = 'This benevolence type has benevolence requests. Deleting it will result in the deletion of those requests. ' + confirmMsg;
        }

        Rock.dialogs.confirm(confirmMsg, function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gBenevolenceType, gBenevolenceType.GetType(), "deleteBenevolenceTypeScript", deleteScript, true );
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

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gBenevolenceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBenevolenceType_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.BenevolenceTypeId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gBenevolenceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBenevolenceType_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.BenevolenceTypeId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gBenevolenceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBenevolenceType_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.WrapTransaction( action: () =>
                 {
                     var benevolenceTypeService = new BenevolenceTypeService( rockContext );
                     BenevolenceType benevolenceType = benevolenceTypeService.Get( e.RowKeyId );

                     if ( benevolenceType != null )
                     {
                         // Do not allow deletions if the person is not authorized 
                         if ( !benevolenceType.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                         {
                             mdGridWarning.Show( "You are not authorized to delete this Benevolence type.", ModalAlertType.Information );
                             return;
                         }

                         var benevolenceRequests = benevolenceType.BenevolenceRequests.ToList();
                         var benevolenceRequestService = new BenevolenceRequestService( rockContext );

                         string errorMessageBenevolenceRequest = string.Empty;

                         foreach ( var benevolenceRequest in benevolenceRequests )
                         {
                             if ( !benevolenceRequestService.CanDelete( benevolenceRequest, out errorMessageBenevolenceRequest ) )
                             {
                                 mdGridWarning.Show( errorMessageBenevolenceRequest, ModalAlertType.Information );
                                 return;
                             }

                             benevolenceRequestService.Delete( benevolenceRequest );
                         }

                         // Save deleting the benevolence requests for the benevolence type id
                         rockContext.SaveChanges();

                         string errorMessageBenevolenceType;
                         if ( !benevolenceTypeService.CanDelete( benevolenceType, out errorMessageBenevolenceType ) )
                         {
                             mdGridWarning.Show( errorMessageBenevolenceType, ModalAlertType.Information );
                             return;
                         }

                         benevolenceTypeService.Delete( benevolenceType );
                         rockContext.SaveChanges();

                         BenevolenceWorkflowService.RemoveCachedTriggers();
                     }
                 } );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBenevolenceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gBenevolenceType_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gBenevolenceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void gBenevolenceType_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow
                && e.Row.DataItem is BenevolenceType benevolenceType
                && _benevolenceTypesWithRequests.Contains( benevolenceType.Id ) )
            {
                e.Row.AddCssClass( "js-has-request" );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var selectQry = GetBenevolenceTypes( new RockContext() );

            // The default benevolence type should not be edited or removed
            gBenevolenceType.EntityTypeId = EntityTypeCache.GetId<BenevolenceType>();
            gBenevolenceType.DataSource = selectQry.ToList();
            gBenevolenceType.DataBind();
        }

        /// <summary>
        /// Gets the Benevolence types.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<BenevolenceType> GetBenevolenceTypes( RockContext rockContext )
        {
            var allBenevolenceTypes = new BenevolenceTypeService( rockContext ).Queryable()
                .OrderBy( g => g.Name );

            // populate _benevolenceTypesWithRequests so that a warning can be displayed if a benevolence type with requests is deleted.
            var benevolenceTypeIds = allBenevolenceTypes.Select( a => a.Id ).ToList();
            var benevolenceTypesWithRequestsQry = new BenevolenceRequestService( rockContext )
                .Queryable()
                .Where( a => benevolenceTypeIds.Contains( a.BenevolenceTypeId ) )
                .Select( a => a.BenevolenceTypeId );

            _benevolenceTypesWithRequests = new HashSet<int>( benevolenceTypesWithRequestsQry.Distinct().ToList() );

            return allBenevolenceTypes;
        }

        #endregion
    }
}