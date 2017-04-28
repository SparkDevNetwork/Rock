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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Session" )]
    [Category( "Prayer" )]
    [Description( "Allows a user to start a session to pray for active, approved prayer requests." )]

    [CodeEditorField( "Welcome Introduction Text", "Some text (or HTML) to display on the first step.", CodeEditorMode.Html, height: 100, required: false, defaultValue: "<h2>Let's get ready to pray...</h2>", order: 1 )]
    [CategoryField( "Category", "A top level category. This controls which categories are shown when starting a prayer session.", false, "Rock.Model.PrayerRequest", "", "", false, "", "Filtering", 2, "CategoryGuid" )]
    [BooleanField( "Enable Prayer Team Flagging", "If enabled, members of the prayer team can flag a prayer request if they feel the request is inappropriate and needs review by an administrator.", false, "Flagging", 3, "EnableCommunityFlagging" )]
    [IntegerField( "Flag Limit", "The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.", false, 1, "Flagging", 4 )]

    [CodeEditorField( "Prayer Person Lava", "The Lava Template for how the person details are shown in the header", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, order: 5, defaultValue:
        @"
{% if PrayerRequest.RequestedByPersonAlias %}
<img src='{{ PrayerRequest.RequestedByPersonAlias.Person.PhotoUrl }}' class='pull-left margin-r-md img-thumbnail' width=50 />
{% endif %}
<span class='first-word'>{{ PrayerRequest.FirstName }}</span> {{ PrayerRequest.LastName }}
" )]
    [CodeEditorField( "Prayer Display Lava", "The Lava Template which will show the details of the Prayer Request", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, order: 5,
        defaultValue: @"
<div class='row'>
    <div class='col-md-6'>
        {% if PrayerRequest.Campus %}
        <strong>Campus</strong>
        <p>{{ PrayerRequest.Campus.Name }}</p>
        {% endif %} 
        <strong>Prayer Request</strong>
    </div>
    <div class='col-md-6 text-right'>
      {% if PrayerRequest.EnteredDateTime  %}
          Date Entered: {{  PrayerRequest.EnteredDateTime | Date:'M/d/yyyy'  }}          
      {% endif %}
    </div>
</div>
                                                
{{ PrayerRequest.Text | NewlineToBr }}

<div class='attributes margin-t-md'>
{% for prayerRequestAttribute in PrayerRequest.AttributeValues %}
    {% if prayerRequestAttribute.Value != '' %}
    <strong>{{ prayerRequestAttribute.AttributeName }}</strong> 
    <p>{{ prayerRequestAttribute.ValueFormatted }}</p>
    {% endif %}
{% endfor %}
</div>

{% if PrayerRequest.Answer %}
<div class='margin-t-lg'>
    <strong>Update</strong> 
    <br />
    {{ PrayerRequest.Answer | Escape | NewlineToBr }}
</div>
{% endif %}

" )]
    public partial class PrayerSession : RockBlock
    {
        #region Fields
        private bool _enableCommunityFlagging = false;
        private string _categoryGuidString = string.Empty;
        private int? _flagLimit = 1;
        private string[] _savedCategoryIdsSetting;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the note type identifier.
        /// </summary>
        /// <value>
        /// The note type identifier.
        /// </value>
        public int? NoteTypeId
        {
            get { return ViewState["NoteTypeId"] as int?; }
            set { ViewState["NoteTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the current prayer request identifier.
        /// </summary>
        /// <value>
        /// The current prayer request identifier.
        /// </value>
        public int? CurrentPrayerRequestId
        {
            get { return ViewState["CurrentPrayerRequestId"] as int?; }
            set { ViewState["CurrentPrayerRequestId"] = value; }
        }

        /// <summary>
        /// Gets or sets the prayer request ids.
        /// </summary>
        /// <value>
        /// The prayer request ids.
        /// </value>
        public List<int> PrayerRequestIds
        {
            get { return ViewState["PrayerRequestIds"] as List<int>; }
            set { ViewState["PrayerRequestIds"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            mdFlag.SaveClick += mdFlag_SaveClick;

            _flagLimit = GetAttributeValue( "FlagLimit" ).AsIntegerOrNull();
            _categoryGuidString = GetAttributeValue( "CategoryGuid" );
            _enableCommunityFlagging = GetAttributeValue( "EnableCommunityFlagging" ).AsBoolean();
            lWelcomeInstructions.Text = GetAttributeValue( "WelcomeIntroductionText" );
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                DisplayCategories();
                SetNoteType();
                lbStart.Focus();
                lbFlag.Visible = _enableCommunityFlagging;
            }

            if ( NoteTypeId.HasValue )
            {
                var noteType = NoteTypeCache.Read( NoteTypeId.Value );
                if ( noteType != null )
                {
                    notesComments.NoteTypes = new List<NoteTypeCache> { noteType };
                }
            }

            notesComments.EntityId = CurrentPrayerRequestId;

            if ( lbNext.Visible )
            {
                lbNext.Focus();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handler that saves the user's category preferences and starts a prayer session
        /// for their selected categories.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbStart_Click( object sender, EventArgs e )
        {
            // Make sure they selected at least one category
            if ( cblCategories.SelectedValues.Count == 0 )
            {
                nbSelectCategories.Visible = true;
                return;
            }
            else
            {
                nbSelectCategories.Visible = false;
            }

            lbNext.Focus();
            lbBack.Visible = false;

            pnlChooseCategories.Visible = false;

            string settingPrefix = string.Format( "prayer-categories-{0}-", this.BlockId );
            SavePreferences( settingPrefix );

            SetAndDisplayPrayerRequests( cblCategories );
        }

        /// <summary>
        /// Handler that gets the next prayer request and updates its prayer count.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbNext_Click( object sender, EventArgs e )
        {
            int index = hfPrayerIndex.ValueAsInt();

            index++;

            List<int> prayerRequestIds = this.PrayerRequestIds;
            int currentNumber = index + 1;
            if ( ( prayerRequestIds != null ) && ( currentNumber <= prayerRequestIds.Count ) )
            {
                UpdateSessionCountLabel( currentNumber, prayerRequestIds.Count );

                hfPrayerIndex.Value = index.ToString();
                var rockContext = new RockContext();
                PrayerRequestService service = new PrayerRequestService( rockContext );
                int prayerRequestId = prayerRequestIds[index];
                PrayerRequest request = service.Queryable( "RequestedByPersonAlias.Person" ).FirstOrDefault( p => p.Id == prayerRequestId );
                ShowPrayerRequest( request, rockContext );
            }
            else
            {
                pnlFinished.Visible = true;
                pnlPrayer.Visible = false;
                lbStartAgain.Focus();
            }

            lbBack.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            int index = hfPrayerIndex.ValueAsInt();

            index--;

            List<int> prayerRequestIds = this.PrayerRequestIds;
            int currentNumber = index + 1;
            if ( ( prayerRequestIds != null ) && ( currentNumber > 0 ) )
            {
                UpdateSessionCountLabel( currentNumber, prayerRequestIds.Count );

                hfPrayerIndex.Value = index.ToString();
                var rockContext = new RockContext();
                PrayerRequestService service = new PrayerRequestService( rockContext );
                int prayerRequestId = prayerRequestIds[index];
                PrayerRequest request = service.Queryable( "RequestedByPersonAlias.Person" ).FirstOrDefault( p => p.Id == prayerRequestId );
                ShowPrayerRequest( request, rockContext );
            }
            else
            {
                lbBack.Visible = false;
            }
        }

        /// <summary>
        /// Handler for when the user has decided to call it quits for their current prayer session.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbStop_Click( object sender, EventArgs e )
        {
            pnlFinished.Visible = true;
            pnlPrayer.Visible = false;
        }

        /// <summary>
        /// Handler for when/if the user wants to start a new prayer session.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbStartAgain_Click( object sender, EventArgs e )
        {
            lbBack.Visible = false;
            pnlChooseCategories.Visible = true;
            pnlFinished.Visible = false;
            pnlNoPrayerRequestsMessage.Visible = false;
            pnlPrayer.Visible = false;
            lbStart.Focus();

            DisplayCategories();
        }

        /// <summary>
        /// Called when the user clicks on the "Flag" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbFlag_Click( object sender, EventArgs e )
        {
            mdFlag.SaveButtonText = "Yes, Flag This Request";
            mdFlag.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdFlag control and flags the prayer request and moves to the next.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdFlag_SaveClick( object sender, EventArgs e )
        {
            int prayerRequestId = hfIdValue.ValueAsInt();

            var rockContext = new RockContext();
            var service = new PrayerRequestService( rockContext );

            PrayerRequest request = service.Get( prayerRequestId );

            if ( request != null )
            {
                request.FlagCount = ( request.FlagCount ?? 0 ) + 1;
                if ( request.FlagCount >= _flagLimit )
                {
                    request.IsApproved = false;
                }

                rockContext.SaveChanges();
            }

            mdFlag.Hide();
            lbNext_Click( sender, e );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the type of the note.
        /// </summary>
        private void SetNoteType()
        {
            var rockContext = new RockContext();
            var service = new NoteTypeService( rockContext );
            var noteType = service.Get( Rock.SystemGuid.NoteType.PRAYER_COMMENT.AsGuid() );
            NoteTypeId = noteType.Id;
        }

        /// <summary>
        /// Updates the Hightlight label that shows how many prayers have been made out of the total for this session.
        /// </summary>
        /// <param name="currentNumber"></param>
        /// <param name="total"></param>
        private void UpdateSessionCountLabel( int currentNumber, int total )
        {
            hlblNumber.Text = string.Format( "{0} of {1}", currentNumber, total );
        }

        /// <summary>
        /// Displays any 'active' prayer categories or shows a message if there are none.
        /// </summary>
        private void DisplayCategories()
        {
            // If there are no categories, then it means there are no prayer requests (in those categories)
            if ( !BindCategories( _categoryGuidString ) )
            {
                cblCategories.Visible = false;
                pnlChooseCategories.Visible = false;
                pnlNoPrayerRequestsMessage.Visible = true;
            }
        }

        /// <summary>
        /// Binds the 'active' categories for the given top-level category GUID to the list for 
        /// the user to choose.
        /// </summary>
        /// <param name="categoryGuid">the guid string of a top-level prayer category</param>
        /// <returns>true if there were active categories or false if there were none</returns>
        private bool BindCategories( string categoryGuid )
        {
            string settingPrefix = string.Format( "prayer-categories-{0}-", this.BlockId );

            IQueryable<PrayerRequest> prayerRequestQuery = new PrayerRequestService( new RockContext() ).GetActiveApprovedUnexpired();

            // Filter categories if one has been selected in the configuration
            if ( !string.IsNullOrEmpty( categoryGuid ) )
            {
                Guid guid = new Guid( categoryGuid );
                var filterCategory = CategoryCache.Read( guid );
                if ( filterCategory != null )
                {
                    prayerRequestQuery = prayerRequestQuery.Where( p => p.Category.ParentCategoryId == filterCategory.Id );
                }
            }

            var categoryList = prayerRequestQuery
                .Where( p => p.Category != null )
                .Select( p => new { p.Category.Id, p.Category.Name } )
                .GroupBy( g => new { g.Id, g.Name } )
                .OrderBy( g => g.Key.Name )
                .Select( a => new
                {
                    Id = a.Key.Id,
                    Name = a.Key.Name + " (" + System.Data.Entity.SqlServer.SqlFunctions.StringConvert( (double)a.Count() ).Trim() + ")",
                    Count = a.Count()
                } ).ToList();

            cblCategories.DataTextField = "Name";
            cblCategories.DataValueField = "Id";
            cblCategories.DataSource = categoryList;
            cblCategories.DataBind();

            // use the users preferences to set which items are checked.
            _savedCategoryIdsSetting = this.GetUserPreference( settingPrefix ).SplitDelimitedValues();
            for ( int i = 0; i < cblCategories.Items.Count; i++ )
            {
                ListItem item = (ListItem)cblCategories.Items[i];
                item.Selected = _savedCategoryIdsSetting.Contains( item.Value );
            }

            return categoryList.Count() > 0;
        }

        /// <summary>
        /// Saves the users selected prayer categories for use during the next prayer session.
        /// </summary>
        /// <param name="settingPrefix"></param>
        private void SavePreferences( string settingPrefix )
        {
            var previouslyCheckedIds = this.GetUserPreference( settingPrefix ).SplitDelimitedValues();

            IEnumerable<string> allIds = cblCategories.Items.Cast<ListItem>()
                              .Select( i => i.Value );

            // find the items that were previously saved but are no longer in the checkboxlist...
            // because we want to retain those as they may be active categories again in the future.
            var itemsToKeep = previouslyCheckedIds.Except( allIds );

            string categoryValues = cblCategories.SelectedValuesAsInt
                .Where( v => v != 0 )
                .Select( c => c.ToString() )
                .Concat( itemsToKeep )
                .ToList()
                .AsDelimited( "," );

            this.SetUserPreference( settingPrefix, categoryValues );
        }

        /// <summary>
        /// Finds all approved prayer requests for the given selected categories and orders them by least prayed-for.
        /// Also updates the prayer count for the first item in the list.
        /// </summary>
        /// <param name="categoriesList"></param>
        private void SetAndDisplayPrayerRequests( RockCheckBoxList categoriesList )
        {
            RockContext rockContext = new RockContext();
            PrayerRequestService service = new PrayerRequestService( rockContext );
            var prayerRequests = service.GetByCategoryIds( categoriesList.SelectedValuesAsInt ).OrderByDescending( p => p.IsUrgent ).ThenBy( p => p.PrayerCount ).ToList();
            List<int> list = prayerRequests.Select( p => p.Id ).ToList<int>();

            PrayerRequestIds = list;
            if ( list.Count > 0 )
            {
                UpdateSessionCountLabel( 1, list.Count );
                hfPrayerIndex.Value = "0";
                PrayerRequest request = prayerRequests.First();
                ShowPrayerRequest( request, rockContext );
            }
        }

        /// <summary>
        /// Displays the details for a single, given prayer request.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ShowPrayerRequest( PrayerRequest prayerRequest, RockContext rockContext )
        {
            pnlPrayer.Visible = true;

            prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;
            hlblPrayerCountTotal.Text = prayerRequest.PrayerCount.ToString() + " team prayers";
            hlblUrgent.Visible = prayerRequest.IsUrgent ?? false;

            hlblCategory.Text = prayerRequest.Category.Name;
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );

            // need to load attributes so that lava can loop thru PrayerRequest.Attributes
            prayerRequest.LoadAttributes();

            mergeFields.Add( "PrayerRequest", prayerRequest );
            string prayerPersonLava = this.GetAttributeValue( "PrayerPersonLava" );
            string prayerDisplayLava = this.GetAttributeValue( "PrayerDisplayLava" );
            lPersonLavaOutput.Text = prayerPersonLava.ResolveMergeFields( mergeFields );
            lPrayerLavaOutput.Text = prayerDisplayLava.ResolveMergeFields( mergeFields );

            pnlPrayerComments.Visible = prayerRequest.AllowComments ?? false;
            if ( notesComments.Visible )
            {
                notesComments.EntityId = prayerRequest.Id;
                notesComments.RebuildNotes( true );
            }

            CurrentPrayerRequestId = prayerRequest.Id;

            try
            {
                // save because the prayer count was just modified.
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
            }
        }

        #endregion
    }
}