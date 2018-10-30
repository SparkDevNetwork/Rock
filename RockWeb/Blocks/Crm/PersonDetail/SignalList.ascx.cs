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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CRM.PersonDetail
{
    [DisplayName( "Signal List" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Lists all the signals on a person." )]

    public partial class SignalList : PersonBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            AddDynamicColumns();

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gSignal.DataKeyNames = new[] { "Id" };
            gSignal.Actions.AddClick += gSignal_Add;
            gSignal.Actions.ShowAdd = canAddEditDelete;
            gSignal.IsDeleteEnabled = canAddEditDelete;
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
        /// Handles the Add event of the gSignal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSignal_Add( object sender, EventArgs e )
        {
            ddlSignalType.Items.Clear();
            ddlSignalType.Items.Add( new ListItem() );

            new SignalTypeService( new RockContext() ).Queryable()
                .ToList()
                .Where( t => t.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Id )
                .ToList()
                .ForEach( t => ddlSignalType.Items.Add( new ListItem( t.Name, t.Id.ToString() ) ) );

            ddlSignalType.SelectedValue = string.Empty;
            dpExpirationDate.SelectedDate = null;
            tbNote.Text = string.Empty;
            ppSignalOwner.SetValue( CurrentPerson );
            hfEditSignalId.Value = "0";

            mdEditSignal.Title = "Add Signal";
            mdEditSignal.Show();
        }

        /// <summary>
        /// Handles the Edit event of the gSignal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSignal_Edit( object sender, RowEventArgs e )
        {
            var signal = new PersonSignalService( new RockContext() ).Get( e.RowKeyId );

            if ( !signal.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                mdGridWarning.Show( "Not authorized to make changes to this signal.", ModalAlertType.Information );
                return;
            }

            ddlSignalType.Items.Clear();
            ddlSignalType.Items.Add( new ListItem() );

            new SignalTypeService( new RockContext() ).Queryable()
                .ToList()
                .Where( t => t.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Id )
                .ToList()
                .ForEach( t => ddlSignalType.Items.Add( new ListItem( t.Name, t.Id.ToString() ) ) );

            ddlSignalType.SelectedValue = signal.SignalTypeId.ToString();
            dpExpirationDate.SelectedDate = signal.ExpirationDate;
            tbNote.Text = signal.Note;
            ppSignalOwner.SetValue( signal.OwnerPersonAlias.Person );
            hfEditSignalId.Value = signal.Id.ToString();

            mdEditSignal.Title = "Edit Signal";
            mdEditSignal.Show();
        }

        /// <summary>
        /// Handles the Delete event of the gSignal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSignal_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var personSignalService = new PersonSignalService( rockContext );
            var person = new PersonService( rockContext ).Get( Person.Id );
            var signal = person.Signals.Where( s => s.Id == e.RowKeyId ).FirstOrDefault();

            if ( signal != null )
            {
                if ( !signal.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    mdGridWarning.Show( "Not authorized to make changes to this signal.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !personSignalService.CanDelete( signal, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                personSignalService.Delete( signal );

                person.CalculateSignals();
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSignal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gSignal_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SaveClick event of the mdEditSignal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdEditSignal_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personSignalService = new PersonSignalService( rockContext );
            PersonSignal signal = null;

            if ( hfEditSignalId.Value.AsInteger() != 0 )
            {
                signal = personSignalService.Get( hfEditSignalId.Value.AsInteger() );
            }
            else
            {
                signal = new PersonSignal
                {
                    PersonId = Person.Id,
                    OwnerPersonAliasId = CurrentPersonAliasId.Value
                };
                personSignalService.Add( signal );
            }

            signal.SignalTypeId = ddlSignalType.SelectedValue.AsInteger();
            signal.ExpirationDate = dpExpirationDate.SelectedDate;
            signal.Note = tbNote.Text;

            rockContext.SaveChanges();

            var person = new PersonService( rockContext ).Get( Person.Id );
            person.CalculateSignals();
            rockContext.SaveChanges();
            
            mdEditSignal.Hide();
            BindGrid();
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Add all the dynamic columns needed to properly display devices.
        /// </summary>
        protected void AddDynamicColumns()
        {
            var deleteField = new DeleteField();
            gSignal.Columns.Add( deleteField );
            deleteField.Click += gSignal_Delete;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var queryable = new PersonSignalService( new RockContext() )
                .Queryable()
                .Where( s => s.PersonId == Person.Id );

            gSignal.ObjectList = new Dictionary<string, object>();
            queryable.ToList().ForEach( s => gSignal.ObjectList.Add( s.Id.ToString(), s ) );

            var gridList = queryable.ToList().Select( a =>
                new
                {
                    a.Id,
                    a.SignalType.Order,
                    a.SignalType.Name,
                    a.OwnerPersonAlias,
                    Note = a.IsAuthorized( Authorization.VIEW, CurrentPerson ) ? a.Note : string.Empty,
                    ExpirationDate = a.IsAuthorized( Authorization.VIEW, CurrentPerson ) ? a.ExpirationDate : null
                } ).AsQueryable();

            if ( gSignal.SortProperty != null )
            {
                gSignal.DataSource = gridList.Sort( gSignal.SortProperty ).ToList();
            }
            else
            {
                gSignal.DataSource = gridList.OrderBy( s => s.Order ).ThenBy( s => s.Id ).ToList();
            }

            gSignal.EntityTypeId = EntityTypeCache.Get<Rock.Model.Device>().Id;
            gSignal.DataBind();
        }

        #endregion
    }
}