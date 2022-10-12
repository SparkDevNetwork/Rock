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

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            lbAddBenevolenceType.Visible = UserCanAdministrate;

            gBenevolenceType.DataKeyNames = new string[] { "Id" };
            gBenevolenceType.Actions.ShowAdd = true;
            gBenevolenceType.Actions.AddClick += gBenevolenceType_Add;
            gBenevolenceType.GridReorder += gBenevolenceType_GridReorder;
            gBenevolenceType.GridRebind += gBenevolenceType_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );
            gBenevolenceType.Actions.ShowAdd = UserCanAdministrate;
            gBenevolenceType.IsDeleteEnabled = UserCanAdministrate;

            // Only display reordering column if user can edit the block
            gBenevolenceType.ColumnsOfType<ReorderField>().First().Visible = UserCanAdministrate;

            SecurityField securityField = gBenevolenceType.ColumnsOfType<SecurityField>().First();
            securityField.EntityTypeId = EntityTypeCache.GetId<Rock.Model.BenevolenceType>().Value;
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
                BindGrid();
            }
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
        /// Handles the ItemCommand event of the rptBenevolenceTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptBenevolenceTypes_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? benevolenceTypeId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( benevolenceTypeId.HasValue )
            {
                NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.BenevolenceTypeId, benevolenceTypeId.Value );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbAddBenevolenceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddBenevolenceType_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.BenevolenceTypeId, 0 );
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
                     var benevolenceWorkflowService = new BenevolenceWorkflowService( rockContext );
                     var benevolenceTypeService = new BenevolenceTypeService( rockContext );
                     var authService = new AuthService( rockContext );
                     BenevolenceType benevolenceType = benevolenceTypeService.Get( e.RowKeyId );

                     if ( benevolenceType != null )
                     {
                         // Do not allow deletions if the person is not authorized 
                         if ( !benevolenceType.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                         {
                             mdGridWarning.Show( "You are not authorized to delete this Benevolence type.", ModalAlertType.Information );
                             return;
                         }

                         // var benevolenceRequests = new Service<BenevolenceRequest>( rockContext ).Queryable().All( a => a.BenevolenceTypeId == BenevolenceType.Id );
                         var benevolenceRequests = benevolenceType.BenevolenceRequests.ToList();
                         var benevolenceRequestService = new BenevolenceRequestService( rockContext );

                         string errorMessageBenevolenceRequest = string.Empty;

                         foreach ( var benvolenceRequest in benevolenceRequests )
                         {
                             if ( !benevolenceRequestService.CanDelete( benvolenceRequest, out errorMessageBenevolenceRequest ) )
                             {
                                 mdGridWarning.Show( errorMessageBenevolenceRequest, ModalAlertType.Information );
                                 return;
                             }

                             benevolenceRequestService.Delete( benvolenceRequest );
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

                         // ToDo: benevolenceWorkflowService.RemoveCachedTriggers();
                     }
                 } );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gBenevolenceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gBenevolenceType_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();

            var benevolenceTypes = GetBenevolenceTypes( rockContext );
            if ( benevolenceTypes != null )
            {
                new BenevolenceTypeService( rockContext ).Reorder( benevolenceTypes, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
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

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var selectQry = GetBenevolenceTypes( new RockContext() )
              .Select( b => new
              {
                  b.Id,
                  b.Name,
                  b.Description,
                  b.ShowFinancialResults,
                  b.IsActive
              } );

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gBenevolenceType.Actions.ShowAdd = canAddEditDelete;
            gBenevolenceType.IsDeleteEnabled = canAddEditDelete;

            // The default benevolence type should not be edited or removed
            gBenevolenceType.EntityTypeId = EntityTypeCache.GetId<BenevolenceType>();
            gBenevolenceType.DataSource = selectQry.ToList();
            gBenevolenceType.DataBind();
        }

        /// <summary>
        /// Gets the Benevolence types.
        /// </summary>
        /// <returns></returns>
        private List<BenevolenceType> GetBenevolenceTypes( RockContext rockContext )
        {
            var allBenevolenceTypes = new BenevolenceTypeService( rockContext ).Queryable()
                .OrderBy( g => g.Name )
                .ToList();

            var authorizedBenevolenceTypes = new List<BenevolenceType>();
            foreach ( var benevolenceType in allBenevolenceTypes )
            {
                if ( UserCanEdit || benevolenceType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    authorizedBenevolenceTypes.Add( benevolenceType );
                }
            }

            return authorizedBenevolenceTypes;
        }

        #endregion
    }
}