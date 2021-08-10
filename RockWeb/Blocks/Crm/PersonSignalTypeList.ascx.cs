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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock.Attribute;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Security;
using Rock.Tasks;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Person Signal Type List" )]
    [Category( "CRM" )]
    [Description( "Shows a list of all signal types." )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    public partial class PersonSignalTypeList : RockBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }
        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gPersonSignalType.DataKeyNames = new string[] { "Id" };
            gPersonSignalType.Actions.AddClick += gPersonSignalType_Add;
            gPersonSignalType.GridReorder += gPersonSignalType_GridReorder;
            gPersonSignalType.GridRebind += gPersonSignalType_GridRebind;
            gPersonSignalType.RowItemText = "Signal Type";

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPersonSignalType.Actions.ShowAdd = canAddEditDelete;
            gPersonSignalType.IsDeleteEnabled = canAddEditDelete;
            if ( canAddEditDelete )
            {
                gPersonSignalType.RowSelected += gPersonSignalType_Edit;
            }

            SecurityField securityField = gPersonSignalType.Columns[4] as SecurityField;
            securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.SignalType ) ).Id;
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

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the Add event of the gPersonSignalType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPersonSignalType_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "SignalTypeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPersonSignalType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonSignalType_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "SignalTypeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gPersonSignalType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonSignalType_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var signalTypeService = new SignalTypeService( rockContext );
            var signalType = signalTypeService.Get( e.RowKeyId );

            if ( signalType != null )
            {
                string errorMessage;
                if ( !signalTypeService.CanDelete( signalType, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                var people = new PersonSignalService( rockContext ).Queryable()
                    .Where( s => s.SignalTypeId == signalType.Id )
                    .Select( s => s.PersonId )
                    .Distinct()
                    .ToList();

                signalTypeService.Delete( signalType );
                rockContext.SaveChanges();

                //
                // If less than 250 people with this signal type then just update them all now,
                // otherwise put something in the rock queue to take care of it.
                //
                if ( people.Count < 250 )
                {
                    new PersonService( rockContext ).Queryable()
                        .Where( p => people.Contains( p.Id ) )
                        .ToList()
                        .ForEach( p => p.CalculateSignals() );

                    rockContext.SaveChanges();
                }
                else
                {
                    var updatePersonSignalTypesMsg = new UpdatePersonSignalTypes.Message
                    {
                        PersonIds = people
                    };
                    updatePersonSignalTypesMsg.Send();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gPersonSignalType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs" /> instance containing the event data.</param>
        void gPersonSignalType_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new SignalTypeService( rockContext );
            var signalTypes = service.Queryable().OrderBy( b => b.Order );

            service.Reorder( signalTypes.ToList(), e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPersonSignalType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gPersonSignalType_GridRebind( object sender, EventArgs e )
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
            gPersonSignalType.DataSource = new SignalTypeService( new RockContext() )
                .Queryable().OrderBy( b => b.Order ).ToList();
            gPersonSignalType.DataBind();
        }

        #endregion
    }
}