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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Nameless Person List" )]
    [Category( "CRM" )]
    [Description( "List unmatched phone numbers with an option to link to a person that has the same phone number." )]
    [Rock.SystemGuid.BlockTypeGuid( "41AE0574-BE1E-4656-B45D-2CB734D1BE30" )]
    public partial class NamelessPersonList : RockBlock, ICustomGridColumns
    {
        #region PageParameterKeys

        public static class PageParameterKey
        {
            public const string NamelessPersonId = "NamelessPersonId";
        }

        #endregion PageParameterKeys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            newPersonEditor.ShowEmail = false;
            gNamelessPersonList.GridRebind += gList_GridRebind;
            gNamelessPersonList.Actions.ShowMergeTemplate = false;

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
            PersonService personService = new PersonService( rockContext );

            var namelessPersonRecordTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() );
            var currentMergeRequestQry = PersonService.GetMergeRequestQuery( rockContext );

            var qry = personService
                .Queryable( new PersonService.PersonQueryOptions() { IncludeNameless = true } )
                .Where( p => p.RecordTypeValueId == namelessPersonRecordTypeId )
                .Where( p => !currentMergeRequestQry.Any( mr => mr.Items.Any( i => i.EntityId == p.Id ) ) )
                .AsNoTracking();

            int? namelessPersonId = PageParameter( PageParameterKey.NamelessPersonId ).AsIntegerOrNull();
            if ( namelessPersonId.HasValue )
            {
                qry = qry.Where( a => a.Id == namelessPersonId.Value );
            }

            // sort the query based on the column that was selected to be sorted
            var sortProperty = gNamelessPersonList.SortProperty;
            if ( gNamelessPersonList.AllowSorting && sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( a => a.CreatedDateTime );
            }

            // set the datasource as a query. This allows the grid to only fetch the records that need to be shown based on the grid page and page size
            gNamelessPersonList.SetLinqDataSource( qry );

            gNamelessPersonList.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnLinkToPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void btnLinkToPerson_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var selectedPersonId = e.RowKeyId;
            hfNamelessPersonId.Value = selectedPersonId.ToString();

            var person = new PersonService( new RockContext() ).Get( selectedPersonId );
            var title = "Link Nameless Person";
            var phoneNumbers = person.PhoneNumbers.Select( a => a.NumberFormatted ).ToList();
            if ( phoneNumbers.Any() )
            {
                title = string.Format( "Link Phone Numbers {0} To Person ", phoneNumbers.AsDelimited( "," ) );

            }
            mdLinkToPerson.Title = title;

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

                int namelessPersonId = hfNamelessPersonId.Value.AsInteger();
                Person namelessPerson = personService.Get( namelessPersonId );

                if ( namelessPerson == null )
                {
                    // shouldn't happen, but just in case
                    return;
                }

                EntitySet mergeRequest = null;
                if ( pnlLinkToExistingPerson.Visible )
                {
                    var existingPersonId = ppPerson.PersonId;
                    if ( !existingPersonId.HasValue )
                    {
                        return;
                    }

                    var existingPerson = personService.Get( existingPersonId.Value );

                    mergeRequest = namelessPerson.CreateMergeRequest( existingPerson );
                    var entitySetService = new EntitySetService( rockContext );
                    entitySetService.Add( mergeRequest );

                    rockContext.SaveChanges();
                }
                else
                {
                    // new Person and new family
                    var newPerson = new Person();

                    newPersonEditor.UpdatePerson( newPerson, rockContext );

                    personService.Add( newPerson );
                    rockContext.SaveChanges();

                    mergeRequest = namelessPerson.CreateMergeRequest( newPerson );
                    var entitySetService = new EntitySetService( rockContext );
                    entitySetService.Add( mergeRequest );
                    rockContext.SaveChanges();
                }

                RedirectToMergeRequest( mergeRequest );
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
        /// Handles the DataBound event of the lUnmatchedPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void lUnmatchedPerson_DataBound( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            Literal lPhoneNumberDisplay = sender as Literal;
            var person = e.Row.DataItem as Person;
            if ( person != null )
            {
                var phoneNumbers = person.PhoneNumbers.Select( a => a.NumberFormatted ).ToList();
                if ( phoneNumbers.Any() )
                {
                    lPhoneNumberDisplay.Text = string.Format( "{0} (Unknown Person)", phoneNumbers.AsDelimited( "," ) );
                }
                else
                {
                    lPhoneNumberDisplay.Text = "Unknown Person";
                }
            }
        }

        /// <summary>
        /// Redirects to merge request.
        /// </summary>
        /// <param name="mergeRequest">The merge request.</param>
        private void RedirectToMergeRequest( EntitySet mergeRequest )
        {
            if ( mergeRequest != null )
            {
                Page.Response.Redirect( string.Format( "~/PersonMerge/{0}", mergeRequest.Id ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}