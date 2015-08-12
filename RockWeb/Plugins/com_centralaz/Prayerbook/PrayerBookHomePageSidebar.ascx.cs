// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Security;

using com.centralaz.Prayerbook.Utility;

namespace RockWeb.Plugins.com_centralaz.Prayerbook
{
    /// <summary>
    /// Sidebar for Prayerbook Home Page.
    /// </summary>
    [DisplayName( "UP Team Prayerbook Home Page Sidebar" )]
    [Category( "centralaz > Prayerbook" )]
    [Description( "Sidebar to UP Team Prayerbook Home Page" )]
    [LinkedPage( "Edit Entry Page" )]
    public partial class PrayerbookHomePageSidebar : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
                int groupTypeId = GroupTypeIdByGuid.Get( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE );

                //get list of all books in db
                var allBooks = new GroupService( new RockContext() )
                    .Queryable()
                    .Where( g => g.GroupTypeId == groupTypeId )
                    .OrderByDescending( b => b.CreatedDateTime )
                    .ToList();

                //if there are books
                if ( allBooks.Count > 0 )
                {
                    //bind book list to book dropdown
                    ddlBooks.DataSource = allBooks;
                    ddlBooks.DataBind();

                    //if the selected book is open to new entries
                    var selectedBook = allBooks[ddlBooks.SelectedIndex];
                    selectedBook.LoadAttributes();

                    if ( selectedBook.GetAttributeValue( "isOpen" ).AsBoolean() )
                    {
                        //enable 'new entry'
                        bbtnAddNewRequest.Enabled = true;
                        bbtnAddNewRequest.Text = "Add New Request";

                        //Present that book is open
                        lActiveBook.Text = allBooks.FirstOrDefault().Name;
                    }
                    ///else the selected book is closed
                    else
                    {
                        //disable 'new entry'
                        bbtnAddNewRequest.Enabled = false;
                        bbtnAddNewRequest.Text = "This Book is closed.";

                        //Present that book is not open
                        lActiveBook.Text = "NONE";
                    }

                    //update the list of entries based on the selected book in the dropdownlist
                    UpdateEntryGrid( int.Parse( ddlBooks.SelectedValue ) );
                }
                //there are no books in the app
                else
                {    //disable 'new entry'
                    bbtnAddNewRequest.Visible = false;
                }
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

        /// <summary>
        /// Add New Request button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void bbtnAddNewRequest_Click( object sender, EventArgs e )
        {
            //navigate to EditEntry page
            NavigateToLinkedPage( "EditEntryPage" );
        }

        /// <summary>
        /// Books dropdownlist selected changed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlBooks_SelectedIndexChanged( object sender, EventArgs e )
        {
            //Update the list of entries to match the selected book
            UpdateEntryGrid( int.Parse( ddlBooks.SelectedValue ) );
        }

        /// <summary>
        /// Book Entries grid row selected event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gBookEntries_RowSelected( object sender, RowEventArgs e )
        {
            //navigate to editentry page, passing in the entry Id
            NavigateToLinkedPage( "EditEntryPage", "Id", (int)e.RowKeyValues["Id"] );
        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        /// <summary>
        /// Update the list of entries to match the currently selected book in the book dropdownlist
        /// </summary>
        /// <param name="bookId">the Id of the selected book</param>
        protected void UpdateEntryGrid( int bookId )
        {
            //collection of all entries for current book
            var book = new GroupService( new RockContext() ).Get( bookId );
            List<GroupMember> entries = book.Members.ToList();

            //if the current person can Administrate
            if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                //set datasource for grid
                gBookEntries.DataSource = entries;// entries;
            }
            //if the current person and only EDIT
            else if ( IsUserAuthorized( Authorization.EDIT ) )
            {
                //filter entry list for current person only
                entries = ( entries.Where( e => e.PersonId == CurrentPerson.Id ) ).ToList();

                //set datasource for grid
                gBookEntries.DataSource = entries;

                if ( entries.Count > 0 )
                {
                    bbtnAddNewRequest.Enabled = false;
                }
            }

            //bind the data for the grid
            gBookEntries.DataBind();
        }

        #endregion
    }
}