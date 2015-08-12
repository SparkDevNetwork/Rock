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

using System.Text;
using com.centralaz.Prayerbook.Utility;

namespace RockWeb.Plugins.com_centralaz.Prayerbook
{
    /// <summary>
    /// Manage UP Team Books.
    /// </summary>
    [DisplayName( "UP Team Prayerbook Management" )]
    [Category( "centralaz > Prayerbook" )]
    [Description( "Manage Books; Open, Close, Publish, etc." )]
    [LinkedPage( "UP Team Prayerbook Homepage", "The homepage of the Prayerbook App", false )]
    public partial class BookManagement : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private RockContext rockContext;
        private GroupService groupService;

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
            this.AddConfigurationUpdateTrigger( upnlOpenNewBook );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            //define context and service
            rockContext = new RockContext();
            groupService = new GroupService( rockContext );
            Boolean bookIsOpen;
            Boolean bookIsPublished;

            Group book = MostRecentBook.Get( rockContext ); ;

            book.LoadAttributes();
            bookIsOpen = Boolean.Parse( book.GetAttributeValue( "isOpen" ) );
            bookIsPublished = Boolean.Parse( book.GetAttributeValue( "isPublished" ) );

            if ( bookIsOpen && ( book.Id != 0 ) )
            {
                //set display to 'Close book'
                btnOpenNewBook.Enabled = false;
                btnCloseBook.Enabled = true;
                btnReopenBook.Enabled = false;
                btnPublishBook.Enabled = false;
                btnExportEntries.Enabled = false;

                lblBookStatus.Text = String.Format( "Book <b>{0}</b> can be closed.", book.Name );

                StringBuilder builder = new StringBuilder();
                builder.Append( "If you have received all of the entries for this book, you are ready to close it to further submissions. " );
                builder.Append( "Proof and edit entries as needed. " );
                builder.Append( "When you are ready, " );
                builder.Append( "close the book to further entries and prepare to publish." );
                litInstructions.Text = builder.ToString();
            }
            else if ( ( !bookIsOpen ) && ( !bookIsPublished ) && ( book.Id != 0 ) )
            {
                //set display to 'Re-open or publish closed but unpublished book'
                btnOpenNewBook.Enabled = false;
                btnCloseBook.Enabled = false;
                btnReopenBook.Enabled = true;
                btnPublishBook.Enabled = true;
                btnExportEntries.Enabled = true;

                lblBookStatus.Text = String.Format( "Book <b>{0}</b> can be re-opened or published.", book.Name );

                StringBuilder builder = new StringBuilder();
                builder.Append( "'Re-open Book' if you need to add/edit/delete entries before publishing. " );
                builder.Append( " You can export all the entries for import into InDesign (or your document layout application of choice). " );
                builder.Append( "'Publish Book' if this book has been printed and sent out." );
                litInstructions.Text = builder.ToString();
            }
            else
            {
                //set display to 'Open a new book'
                btnOpenNewBook.Enabled = true;
                btnCloseBook.Enabled = false;
                btnReopenBook.Enabled = false;
                btnPublishBook.Enabled = false;
                btnExportEntries.Enabled = true;

                lblBookStatus.Text = "All books have been published.";
                StringBuilder builder = new StringBuilder();
                builder.Append( "You are ready to open a new Book. " );
                builder.Append( "You can also export the entries from the last book, if available, to a text file if you need them." );
                litInstructions.Text = builder.ToString();
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
        /// Event handler for 'Open New Book' button click. 
        /// Opens a new book for entry submission.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnOpenNewBook_Click( object sender, EventArgs e )
        {
            //set the default open and close dates off of the previous book.
            //These can be changed by the user if needed.
            Group book = MostRecentBook.Get( rockContext ); ;
            DateTime openDate = DateTime.Now;
            DateTime closeDate = DateTime.Now.AddMonths( 1 );

            book.LoadAttributes();

            if ( !string.IsNullOrEmpty( book.Name ) )
            {
                if ( DateTime.TryParse( book.GetAttributeValue( "openDate" ), out openDate ) )
                {
                    openDate = openDate.AddMonths( 1 );
                }
                else
                {
                    openDate = DateTime.Now;
                }

                if ( DateTime.TryParse( book.GetAttributeValue( "closeDate" ), out closeDate ) )
                {
                    closeDate = closeDate.AddMonths( 1 );
                }
                else
                {
                    closeDate = openDate.AddMonths( 1 );
                }
            }

            openDate = openDate.AddDays( 25 - openDate.Day );
            closeDate = closeDate.AddDays( 15 - closeDate.Day );
            DateTime pubDate = closeDate.AddMonths( 1 );

            txtTitle.Text = pubDate.ToString( "MMMM yyyy" );
            dpOpenDate.SelectedDate = openDate;
            dpCloseDate.SelectedDate = closeDate;

            lblBookStatus.Visible = false;
            litInstructions.Visible = false;
            upnlOpenNewBook.Visible = true;
        }

        /// <summary>
        /// Event handler for 'Close Book' button click.
        /// Closes the currently open book.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCloseBook_Click( object sender, EventArgs e )
        {
            Group book = MostRecentBook.Get( rockContext ); ;
            book.LoadAttributes( rockContext );

            if ( Page.IsValid )
            {
                if ( book != null )
                {
                    book.SetAttributeValue( "isOpen", "false" );
                }

                book.SaveAttributeValues( rockContext );
                rockContext.SaveChanges();
            }

            Page_Load( btnCloseBook, e );
        }

        /// <summary>
        /// Event handler for 'Reopen Book' button click.
        /// Re-opens a closed, but not yet published book
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnReopenBook_Click( object sender, EventArgs e )
        {
            Group book = MostRecentBook.Get( rockContext ); ;
            book.LoadAttributes();

            if ( Page.IsValid )
            {
                if ( book != null )
                {
                    book.SetAttributeValue( "isOpen", "true" );
                    book.SaveAttributeValues( rockContext );
                    rockContext.SaveChanges();
                }
            }

            Page_Load( btnReopenBook, e );
        }

        /// <summary>
        /// Event handler for 'Export Entries' button click.
        /// Exports entries in current book to tab-delimited text file for physical publication.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnExportEntries_Click( object sender, EventArgs e )
        {
            TABExporter.WriteToTAB( MostRecentBook.Get( rockContext ) );
            return;
        }

        /// <summary>
        /// Event handler for 'Publish Book' button event.
        /// Set the book as published.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnPublishBook_Click( object sender, EventArgs e )
        {
            Group book = MostRecentBook.Get( rockContext );
            book.LoadAttributes( rockContext );

            if ( Page.IsValid )
            {
                if ( book != null )
                {
                    book.SetAttributeValue( "isPublished", "true" );
                    book.SaveAttributeValues( rockContext );
                    rockContext.SaveChanges();
                }
            }

            Page_Load( btnPublishBook, e );
        }

        /// <summary>
        /// Event handler for 'Cancel' button click.
        /// Return user to PrayerBook Home page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "UPTeamPrayerbookHomepage" );
        }

        /// <summary>
        /// Event handler for 'Save Book' button click.
        /// Saves the new book to the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSaveBook_Click( object sender, EventArgs e )
        {
            Group book = new Group();

            book.GroupTypeId = GroupTypeIdByGuid.Get( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE );
            book.ParentGroupId = groupService.Get( Guid.Parse( com.centralaz.Prayerbook.SystemGuid.Group.BOOKS_GROUP ) ).Id;
            book.LoadAttributes( rockContext );

            book.Name = txtTitle.Text;
            book.SetAttributeValue( "openDate", dpOpenDate.SelectedDate.Value.ToShortDateString() );
            book.SetAttributeValue( "closeDate", dpCloseDate.SelectedDate.Value.ToShortDateString() );
            book.SetAttributeValue( "isPublished", "false" ); ;
            book.SetAttributeValue( "isOpen", "true" );

            groupService.Add( book );

            // Do this BEFORE .SaveAttributeValues(), especially for new objects.
            rockContext.SaveChanges();

            book.SaveAttributeValues( rockContext );

            NavigateToLinkedPage( "UPTeamPrayerbookHomepage" );
        }

        /// <summary>
        /// Change the book title when the close date changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void dpCloseDate_TextChanged( object sender, EventArgs e )
        {
            txtTitle.Text = dpCloseDate.SelectedDate.Value.AddMonths( 1 ).ToString( "MMMM yyyy" );
        }

        #endregion
    }
}