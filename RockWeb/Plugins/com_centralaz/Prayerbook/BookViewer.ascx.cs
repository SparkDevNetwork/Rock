// <copyright>
// Copyright by Central Christian Church
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

using com.centralaz.Prayerbook.Utility;
using System.Data;
using System.Web.UI.HtmlControls;

namespace RockWeb.Plugins.com_centralaz.Prayerbook
{
    /// <summary>
    /// For viewing a prayer book online.
    /// </summary>
    [DisplayName( "Book Viewer" )]
    [Category( "com_centralaz > Prayerbook" )]
    [Description( "For viewing a prayer book online." )]
    public partial class BookViewer : Rock.Web.UI.RockBlock
    {
        #region Fields
        private string _ministrySectionHeader = "";
        private string _sessionKey = "com_central.Prayerbook.BookViewer";
        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upBookViewer );
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

                //get list of all published books in db
                RockContext rockContext = new RockContext();
                var publishedBooks = new GroupService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g => g.GroupTypeId == groupTypeId )
                    .WhereAttributeValue( rockContext, "IsPublished", "True" )
                    .OrderByDescending( b => b.CreatedDateTime )
                    .ToList();

                if ( publishedBooks.Count > 0 )
                {
                    ddlBooks.DataSource = publishedBooks;
                    ddlBooks.DataBind();
                }
            }

            if ( ddlBooks.SelectedIndex != -1 )
            {
                BindGrid( int.Parse( ddlBooks.SelectedValue ) );
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
        /// Handles the SelectedIndexChanged event of the ddlBooks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlBooks_SelectedIndexChanged( object sender, EventArgs e )
        {
            hfEntryIndex.Value = "0";
            BindGrid( int.Parse( ddlBooks.SelectedValue ) );
            divViewEntry.Visible = false;
        }

        /// <summary>
        /// Handles the OnRowDataBound event of the grdBookEntries control.  Adds a row that
        /// shows the ministry name so that it acts like a section header for each group of unique
        /// ministries.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gBookEntries_OnRowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                EntryWithMinistry drv = (EntryWithMinistry)e.Row.DataItem;
                string currentRowMinistry = drv.Ministry;
                if ( _ministrySectionHeader != currentRowMinistry )
                {
                    _ministrySectionHeader = currentRowMinistry;

                    Table tbl = e.Row.Parent as Table;
                    if ( tbl != null )
                    {
                        GridViewRow row = new GridViewRow( -1, -1, DataControlRowType.DataRow, DataControlRowState.Normal );
                        TableCell cell = new TableCell();

                        cell.ColumnSpan = this.gBookEntries.Columns.Count;

                        HtmlGenericControl span = new HtmlGenericControl( "Span" );
                        span.InnerHtml = "<b>" + _ministrySectionHeader + "</b>";

                        cell.Controls.Add( span );
                        row.Cells.Add( cell );

                        tbl.Rows.AddAt( tbl.Rows.Count - 1, row );
                    }
                }
            }
        }

        /// <summary>
        /// This event handler is called when one of the entries is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gBookEntries_Command( object sender, CommandEventArgs e )
        {
            int entryId = -1;
            int.TryParse( e.CommandArgument as string, out entryId );
            if ( entryId == -1 )
            {
                return;
            }

            List<int> entryIds = (List<int>)Session[_sessionKey];
            hfEntryIndex.Value = entryIds.IndexOf( entryId ).ToStringSafe();

            divViewEntry.Visible = true;
            ViewEntry( entryId );
        }

        /// <summary>
        /// Handler that gets the next entry and displays it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbNext_Click( object sender, EventArgs e )
        {
            int index = hfEntryIndex.ValueAsInt();
            lbNext.Enabled = true;

            index++;

            List<int> entryIds = (List<int>)Session[_sessionKey];
            int currentNumber = index + 1;
            if ( currentNumber <= entryIds.Count )
            {
                hfEntryIndex.Value = index.ToString();
                var rockContext = new RockContext();
                PrayerRequestService service = new PrayerRequestService( rockContext );
                int entryId = entryIds[index];
                ViewEntry( entryId );
            }
            
            if (currentNumber == entryIds.Count )
            {
                lbNext.Enabled = false;
            }
            if ( currentNumber == 1 )
            {
                lbNext.Enabled = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Views the entry.
        /// </summary>
        /// <param name="entry_id">The entryId.</param>
        protected void ViewEntry( int entryId )
        {
            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            DefinedValueService definedValueService = new DefinedValueService( rockContext );
            GroupMember entry = groupMemberService.Get( entryId );

            entry.LoadAttributes( rockContext );

            //Get the book the entry is in
            Group book = entry.Group;

            //Display the contributor name and, if exists, spouse name
            lContributor.Text = entry.Person.FullName;
            if ( entry.Person.GetSpouse() != null )
            {
                lSpouseName.Text = entry.Person.GetSpouse().FirstName;
                dtSpouse.Visible = true;
            }
            else
            {
                dtSpouse.Visible = false;
            }

            // select the ministry and subminitry from the entry in the ddls
            string entryMinistryAttributeValueGuid = entry.GetAttributeValue( "Ministry" );
            string entrySubministryAttributeValueGuid = entry.GetAttributeValue( "Subministry" );

            if ( entryMinistryAttributeValueGuid != String.Empty )
            {
                var entryMinistryAttributeValue = definedValueService.GetByGuid( Guid.Parse( entryMinistryAttributeValueGuid ) );
                lMinistry.Text = entryMinistryAttributeValue.Value;
            }
            else
            {
                lMinistry.Text = string.Empty;
            }

            if ( entrySubministryAttributeValueGuid != String.Empty )
            {
                var entrySubministryAttributeValue = definedValueService.GetByGuid( Guid.Parse( entrySubministryAttributeValueGuid ) );
                lSubministry.Text = entrySubministryAttributeValue.Value;
            }
            else
            {
                lSubministry.Text = string.Empty;
            }

            // insert the text of the submissions
            txtPraise1.Text = entry.AttributeValues["Praise1"].Value;
            txtMinistryNeed1.Text = entry.AttributeValues["MinistryNeed1"].Value;
            txtMinistryNeed2.Text = entry.AttributeValues["MinistryNeed2"].Value;
            txtMinistryNeed3.Text = entry.AttributeValues["MinistryNeed3"].Value;
            txtPersonalRequest1.Text = entry.AttributeValues["PersonalRequest1"].Value;
            txtPersonalRequest2.Text = entry.AttributeValues["PersonalRequest2"].Value;

            // Hide fields that are empty...
            lMinistry.Visible = !string.IsNullOrEmpty( lMinistry.Text );
            lSubministry.Visible = !string.IsNullOrEmpty( lSubministry.Text );

            txtPraise1.Visible = !string.IsNullOrEmpty( txtPraise1.Text );

            lMinistryNeed1.Visible = !string.IsNullOrEmpty( txtMinistryNeed1.Text );
            lMinistryNeed2.Visible = !string.IsNullOrEmpty( txtMinistryNeed2.Text );
            lMinistryNeed3.Visible = !string.IsNullOrEmpty( txtMinistryNeed3.Text );

            lPersonalRequest1.Visible = !string.IsNullOrEmpty( txtPersonalRequest1.Text );
            lPersonalRequest2.Visible = !string.IsNullOrEmpty( txtPersonalRequest2.Text );

            // Decide if we should disable the Next button.
            List<int> entryIds = (List<int>)Session[_sessionKey];
            if ( entryIds.Count <= 1 || entryIds.IndexOf( entryId ) == entryIds.Count-1)
            {
                lbNext.Enabled = false;
            }
            else
            {
                lbNext.Enabled = true;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        /// <param name="book_id">The bookId.</param>
        protected void BindGrid( int bookId )
        {
            //collection of all entries for current book
            var book = new GroupService( new RockContext() ).Get( bookId );
            List<GroupMember> entries = book.Members.ToList();

            // Add Ministry to each row, so we can sort/group/subhead on it
            List<EntryWithMinistry> entriesWithMinistry = new List<EntryWithMinistry>();
            foreach ( GroupMember e in entries )
            {
                e.LoadAttributes();
                EntryWithMinistry em = new EntryWithMinistry();
                em.Person = e.Person;
                em.Id = e.Id;
                em.Ministry = e.GetAttributeValue( "Ministry" );
                em.SubMinistry = e.GetAttributeValue( "Subministry" );

                // Convert to human readable name
                if ( !string.IsNullOrEmpty( em.Ministry ) )
                {
                    var ministry = DefinedValueCache.Read( em.Ministry );
                    em.Ministry = ministry.Value;
                    em.Order = ministry.Order;
                }
                else
                {
                    em.Ministry = "no ministry";
                    em.Order = 9999;
                }

                if ( !string.IsNullOrEmpty( em.SubMinistry ) )
                {
                    var sm = DefinedValueCache.Read( em.SubMinistry );
                    if ( sm != null && !string.IsNullOrEmpty( sm.Value ) )
                    {
                        em.SubMinistry = sm.Value;
                    }
                }

                entriesWithMinistry.Add( em );
            }

            entriesWithMinistry = entriesWithMinistry.OrderBy( x => x.Order ).ToList();
            List<int> entryIds = entriesWithMinistry.Select( e => e.Id ).ToList<int>();
            Session[_sessionKey] = entryIds;

            gBookEntries.DataSource = entriesWithMinistry;
            gBookEntries.DataBind();
        }
        #endregion
    }

    #region Helper Classes

    public class EntryWithMinistry : GroupMember
    {
        public int Order { get; set; }
        public string Ministry { get; set; }
        public string SubMinistry { get; set; }
    }

    #endregion
}