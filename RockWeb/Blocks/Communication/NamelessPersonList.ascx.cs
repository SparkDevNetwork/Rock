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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Nameless Person List" )]
    [Category( "Communication" )]
    [Description( "List unmatched phone numbers with an option to link to a person that has the same phone number." )]
    public partial class NamelessPersonList : RockBlock, ICustomGridColumns
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            newPersonEditor.ShowEmail = false;
            gNamelessPersonPhoneNumberList.GridRebind += gList_GridRebind;
            gNamelessPersonPhoneNumberList.Actions.ShowMergeTemplate = false;

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

        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
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
            PhoneNumberService phoneNumberService = new PhoneNumberService( rockContext );

            var namelessPersonRecordTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() );

            var qry = phoneNumberService.Queryable()
                    .Where( p => p.Person.RecordTypeValueId == namelessPersonRecordTypeId )
                    .AsNoTracking();

            // sort the query based on the column that was selected to be sorted
            var sortProperty = gNamelessPersonPhoneNumberList.SortProperty;
            if ( gNamelessPersonPhoneNumberList.AllowSorting && sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( a => a.Number );
            }

            // set the datasource as a query. This allows the grid to only fetch the records that need to be shown based on the grid page and page size
            gNamelessPersonPhoneNumberList.SetLinqDataSource( qry );

            gNamelessPersonPhoneNumberList.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnLinkToPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void btnLinkToPerson_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var selectedPhoneNumberId = e.RowKeyId;
            var selectedPhoneNumber = new PhoneNumberService( new RockContext() ).GetNoTracking( selectedPhoneNumberId );
            if (selectedPhoneNumber == null)
            {
                return;
            }

            hfNamelessPersonId.Value = selectedPhoneNumber.PersonId.ToString();

            mdLinkToPerson.Title = string.Format( "Link Phone Number {0} to Person ", PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), selectedPhoneNumber.NumberFormatted, false ) );

            ppPerson.SetValue( null );
            newPersonEditor.SetFromPerson( null );

            mdLinkToPerson.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdLinkToPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdLinkToPerson_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var personService = new PersonService( rockContext );

                // Get the person record from the person associated with the selected phonenumber
                int namelessPersonId = hfNamelessPersonId.Value.AsInteger();
                var phoneNumberService = new PhoneNumberService( rockContext );
                Person namelessPerson = personService.Get( namelessPersonId );

                if ( namelessPerson == null )
                {
                    // shouldn't happen, but just in case
                    return;
                }

                if ( pnlLinkToExistingPerson.Visible )
                {
                    var existingPersonId = ppPerson.PersonId;
                    if ( !existingPersonId.HasValue )
                    {
                        return;
                    }

                    var existingPerson = personService.Get( existingPersonId.Value );

                    personService.MergeNamelessPersonToExistingPerson( namelessPerson, existingPerson );

                    rockContext.SaveChanges();
                }
                else
                {
                    // new Person and new family
                    var newPerson = new Person();

                    newPersonEditor.UpdatePerson( newPerson );
                    personService.MergeNamelessPersonToNewPerson( namelessPerson, newPerson, newPersonEditor.PersonGroupRoleId );
                    rockContext.SaveChanges();
                }
            }

            mdLinkToPerson.Hide();
            BindGrid();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglLinkPersonMode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglLinkPersonMode_CheckedChanged( object sender, EventArgs e )
        {
            pnlLinkToExistingPerson.Visible = tglLinkPersonMode.Checked;
            pnlLinkToNewPerson.Visible = !tglLinkPersonMode.Checked;
        }

        /// <summary>
        /// Handles the DataBound event of the lUnmatchedPhoneNumber control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void lUnmatchedPhoneNumber_DataBound( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            Literal lPhoneNumberDisplay = sender as Literal;
            var phoneNumber = e.Row.DataItem as PhoneNumber;
            if ( phoneNumber != null )
            {
                lPhoneNumberDisplay.Text = string.Format( "{0} (Unknown Person)", phoneNumber.NumberFormatted );
            }
        }
    }
}