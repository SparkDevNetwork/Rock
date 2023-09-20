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
using com.pushpay.RockRMS.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

namespace RockWeb.Plugins.com_pushPay.RockRMS
{
    /// <summary>
    /// Block for viewing list of tags
    /// </summary>
    [DisplayName( "Payer Key List" )]
    [Category( "Pushpay" )]
    [Description( "Block for viewing a list of Pushpay Payer Keys." )]

    public partial class PayerList : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            rGrid.DataKeyNames = new string[] { "Id" };
            rGrid.Actions.ShowAdd = false;
            rGrid.GridRebind += rGrid_GridRebind;

            modalValue.SaveClick += btnSaveValue_Click;
            modalValue.OnCancelScript = string.Format( "$('#{0}').val('');", hfPayerId.ClientID );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var payer = new PayerService( rockContext ).Get( e.RowKeyId );
                if ( payer != null )
                {
                    hfPayerId.Value = payer.Id.ToString();
                    lPayerKey.Text = payer.PayerKey;

                    if ( payer.PersonAlias != null )
                    {
                        ppPersonEdit.SetValue( payer.PersonAlias.Person );
                    }
                    else
                    {
                        ppPersonEdit.SetValue( null );
                    }

                    modalValue.Show();
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var payerService = new PayerService( rockContext );
            var payer = payerService.Get( e.RowKeyId );

            if ( payer != null )
            {
                string errorMessage;
                if ( !payerService.CanDelete( payer, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                payerService.Delete( payer );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Person":
                    int? personId = e.Value.AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( new RockContext() ).Get( personId.Value );
                        if ( person != null )
                        {
                            e.Value = person.FullNameReversed;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Payer Key", tbPayerKey.Text );
            rFilter.SaveUserPreference( "Person", ppPerson.PersonId.HasValue ? ppPerson.PersonId.Value.ToString() : string.Empty );
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveValue_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var payerService = new PayerService( rockContext );
            var payer = payerService.Get( hfPayerId.ValueAsInt() );
            int? personAliasId = ppPersonEdit.PersonAliasId;

            if ( payer != null && personAliasId.HasValue )
            {
                payer.PersonAliasId = personAliasId.Value;
                rockContext.SaveChanges();
            }

            BindGrid();

            hfPayerId.Value = string.Empty;
            modalValue.Hide();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbPayerKey.Text = rFilter.GetUserPreference( "Payer Key" );

            Person person = null;
            int? savedPersonId = rFilter.GetUserPreference( "Person" ).AsIntegerOrNull();
            if ( savedPersonId.HasValue )
            {
                person = new PersonService( new RockContext() ).Queryable()
                    .Where( p => p.Id == savedPersonId.Value )
                    .FirstOrDefault();
            }
            ppPerson.SetValue( person );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var qry = new PayerService( rockContext ).Queryable();

                string payerKey = rFilter.GetUserPreference( "Payer Key" );
                if ( payerKey.IsNotNullOrWhiteSpace() )
                {
                    qry = qry.Where( p => p.PayerKey.StartsWith( payerKey ) );
                }

                int? personId = rFilter.GetUserPreference( "Person" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    qry = qry.Where( p => p.PersonAlias != null && p.PersonAlias.PersonId == personId.Value );
                }

                var sortProperty = rGrid.SortProperty;
                if ( sortProperty != null )
                {
                    qry =  qry.Sort( sortProperty );//.ToList();
                }
                else
                {
                    qry = qry.OrderBy( d => d.PayerKey );//.ToList();
                }

                rGrid.DataSource = qry.Select( p => new
                {
                    p.Id,
                    p.PayerKey,
                    PersonId = p.PersonAlias.PersonId,
                    LastName = p.PersonAlias.Person.LastName + ( string.IsNullOrEmpty( p.PersonAlias.Person.NickName ) ? "" : ", " ),
                    NickName = p.PersonAlias.Person.NickName,
                } ).ToList();
                rGrid.DataBind();
            }
        }

        #endregion

    }
}