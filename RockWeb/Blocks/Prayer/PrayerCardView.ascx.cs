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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
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

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Card View" )]
    [Category( "Prayer" )]
    [Description( "provides an additional experience to pray using a card based view." )]

    #region Block Attributes

    [CodeEditorField(
        "Display Lava Template",
        Key = AttributeKey.DisplayLavaTemplate,
        Description = "The Lava template that layouts out the view of the prayer requests.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = LavaTemplateDefaultValue,
        Order = 0 )]
    [TextField(
        "Prayed Button Text",
        Description = "The text to display inside the Prayed button.",
        DefaultValue = "I Prayed",
        IsRequired = true,
        Key = AttributeKey.PrayedButtonText,
        Order = 1 )]
    [CategoryField(
        "Category",
        Description = "A top level category. This controls which categories are shown when starting a prayer session.",
        IsRequired = false,
        EntityTypeName = "Rock.Model.PrayerRequest",
        AllowMultiple = false,
        Order = 2,
        Key = AttributeKey.Category )]
    [BooleanField(
        "Public Only",
        Description = "If selected, all non-public prayer request will be excluded.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Key = AttributeKey.PublicOnly,
        Order = 3 )]
    [BooleanField(
        "Enable Prayer Team Flagging",
        Description = "If enabled, members of the prayer team can flag a prayer request if they feel the request is inappropriate and needs review by an administrator.",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.EnablePrayerTeamFlagging )]
    [IntegerField(
        "Flag Limit",
        Description = "The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.",
        IsRequired = false,
        DefaultIntegerValue = 1,
        Order = 5,
        Key = AttributeKey.FlagLimit )]
    [CustomDropdownListField(
        "Order",
        Description = "The order that the requests should be displayed.",
        ListSource = "0^Least Prayed For,1^Newest,2^Oldest,3^Random",
        IsRequired = false,
        Order = 6,
        Key = AttributeKey.Order )]
    [BooleanField(
        "Show Campus Filter",
        Description = "Shows or hides the campus filter.",
        DefaultBooleanValue = false,
        Order = 7,
        Key = AttributeKey.ShowCampusFilter )]
    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "Allows selecting which campus types to filter campuses by.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 8 )]
    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = "This allows selecting which campus statuses to filter campuses by.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 9 )]
    [IntegerField(
        "Max Results",
        Description = "The maximum number of requests to display. Leave blank for all.",
        IsRequired = false,
        Order = 10,
        Key = AttributeKey.MaxResults )]
    [WorkflowTypeField(
        "Prayed Workflow",
        AllowMultiple = false,
        Key = AttributeKey.PrayedWorkflow,
        Description = "The workflow type to launch when someone presses the Pray button. Prayer Request will be passed to the workflow as a generic \"Entity\" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: PrayerOfferedByPersonId.",
        IsRequired = false,
        Order = 11 )]
    [WorkflowTypeField(
        "Flagged Workflow",
        AllowMultiple = false,
        Key = AttributeKey.FlaggedWorkflow,
        Description = "The workflow type to launch when someone presses the Flag button. Prayer Request will be passed to the workflow as a generic \"Entity\" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: FlaggedByPersonId.",
        IsRequired = false,
        Order = 12 )]
    [BooleanField(
        "Load Last Prayed Collection",
        Description = "Loads an optional collection of last prayed times for the requests. This is available as a separate merge field in Lava.",
        DefaultBooleanValue = false,
        Order = 13,
        Key = AttributeKey.LoadLastPrayedCollection )]
    
    #endregion Block Attributes
    public partial class PrayerCardView : RockBlock
    {
        #region Constants

        /// <summary>
        /// The Default Value for the LavaTemplate block attribute
        /// </summary>
        private const string LavaTemplateDefaultValue = @"
<div class=""row d-flex flex-wrap"">
  {% assign prayedButtonText = PrayedButtonText %}
  {% for item in PrayerRequestItems %}
  <div class=""col-md-4 col-sm-6 col-xs-12 mb-4"">
    <div class=""card h-100"">
      <div class=""card-body"">
        <h3 class=""card-title mt-0"">{{ item.FirstName }} {{ item.LastName }}</h3>
        {% if item.Category != null %}
        <p class=""card-subtitle mb-2""><span class=""label label-primary"">{{ item.Category.Name }}</span></p>
        {% endif %}
        <p class=""card-text"">
        {{ item.Text }}
        </p>
    </div>
       <div class=""card-footer"">
        {% if EnablePrayerTeamFlagging == true %}
        <a href = ""#"" class=""btn btn-link btn-sm pl-0 text-muted"" onclick=""{{ item.Id | Postback:'Flag' }}""><i class='fa fa-flag'></i> Flag</a>
        {% endif %}
   		<a class=""btn btn-primary btn-sm pull-right"" href=""#"" onclick=""iPrayed(this);{{ item.Id | Postback:'Pray' }}"">Pray</a>
        </div>
        </div>
    </div>
  {% endfor -%}
</div>
<script>function iPrayed(elmnt) { 
        var iPrayedText = '{{PrayedButtonText}}';
        elmnt.innerHTML = iPrayedText;
    }
</script>
<style>
.block-filter { margin-left: auto; }
</style>
";

        #endregion Constants

        #region Attribute Keys

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
            public const string CategoryId = "CategoryId";
        }

        private static class AttributeKey
        {
            public const string DisplayLavaTemplate = "DisplayLavaTemplate";
            public const string PrayedButtonText = "PrayedButtonText";
            public const string Category = "Category";
            public const string PublicOnly = "PublicOnly";
            public const string EnablePrayerTeamFlagging = "EnablePrayerTeamFlagging";
            public const string FlagLimit = "FlagLimit";
            public const string Order = "Order";
            public const string ShowCampusFilter = "ShowCampusFilter";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string MaxResults = "MaxResults";
            public const string PrayedWorkflow = "PrayedWorkflow";
            public const string FlaggedWorkflow = "FlaggedWorkflow";
            public const string LoadLastPrayedCollection = "LoadLastPrayedCollection";
        }

        #endregion Attribute Keys

        #region Fields

        private const string CAMPUS_SETTING = "PrayerCardView_SelectedCampus";

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPrayer );
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
                BindFilter();
                LoadContent();
            }
            else
            {
               RouteAction();
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
            BindFilter();
            LoadContent();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetUserPreference( CAMPUS_SETTING, cpCampus.SelectedCampusId.ToString() );
            LoadContent();
            upPrayer.Update();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Routes any actions that might have come from <seealso cref="AttributeKey.ScheduledTransactionsTemplate"/>
        /// </summary>
        private void RouteAction()
        {
            if ( this.Page.Request.Form["__EVENTARGUMENT"] != null )
            {
                string[] eventArgs = this.Page.Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];

                    int argument = 0;
                    int.TryParse( parameters, out argument );
                    switch ( action )
                    {
                        case "Pray":
                            PrayRequest( argument );
                            break;

                        case "Flag":
                            FlagPrayerRequest( argument );
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Flag Prayer Request.
        /// </summary>
        private void FlagPrayerRequest( int prayerRequestId )
        {
            var rockContext = new RockContext();
            var service = new PrayerRequestService( rockContext );
            var flagLimit = GetAttributeValue( AttributeKey.FlagLimit ).AsIntegerOrNull() ?? 1;
            PrayerRequest request = service.Get( prayerRequestId );

            if ( request != null )
            {
                request.FlagCount = ( request.FlagCount ?? 0 ) + 1;
                if ( request.FlagCount >= flagLimit )
                {
                    request.IsApproved = false;
                }

                rockContext.SaveChanges();

                StartWorkflow( request, rockContext, AttributeKey.FlaggedWorkflow );
            }
        }

        /// <summary>
        /// Pray Request.
        /// </summary>
        private void PrayRequest( int prayerRequestId )
        {
            var rockContext = new RockContext();
            var service = new PrayerRequestService( rockContext );
            var flagLimit = GetAttributeValue( AttributeKey.FlagLimit ).AsIntegerOrNull() ?? 1;
            PrayerRequest request = service.Get( prayerRequestId );

            if ( request != null )
            {
                request.PrayerCount = ( request.PrayerCount ?? 0 ) + 1;
                rockContext.SaveChanges();

                StartWorkflow( request, rockContext, AttributeKey.PrayedWorkflow );
                PrayerRequestService.EnqueuePrayerInteraction( request, CurrentPerson, PageCache.Layout.Site.Name, Request.UserAgent, RockPage.GetClientIpAddress(), RockPage.Session["RockSessionId"]?.ToString().AsGuidOrNull() );
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            // show campus filter
            var showCampusFilter = GetAttributeValue( AttributeKey.ShowCampusFilter ).AsBoolean();
            var showCampus = showCampusFilter;
            var qryCampusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            if ( qryCampusId.HasValue && showCampus )
            {
                var campus = CampusCache.Get( qryCampusId.Value );
                if ( campus != null )
                {
                    showCampus = false;
                }
            }

            if ( showCampus )
            {
                var selectedCampusTypeIds = GetAttributeValue( AttributeKey.CampusTypes )
                  .SplitDelimitedValues( true )
                  .AsGuidList()
                  .Select( a => DefinedValueCache.Get( a ) )
                  .Where( a => a != null )
                  .Select( a => a.Id )
                  .ToList();

                cpCampus.Campuses = CampusCache.All();

                if ( selectedCampusTypeIds.Any() )
                {
                    cpCampus.CampusTypesFilter = selectedCampusTypeIds;
                }

                var selectedCampusStatusIds = GetAttributeValue( AttributeKey.CampusStatuses )
                    .SplitDelimitedValues( true )
                    .AsGuidList()
                    .Select( a => DefinedValueCache.Get( a ) )
                    .Where( a => a != null )
                    .Select( a => a.Id )
                    .ToList();

                if ( selectedCampusStatusIds.Any() )
                {
                    cpCampus.CampusStatusFilter = selectedCampusStatusIds;
                }
            }

            var isCampusVisible = showCampus && cpCampus.Items.Count > 2;
            cpCampus.Visible = isCampusVisible;
            if ( isCampusVisible )
            {
                cpCampus.SelectedCampusId = GetUserPreference( CAMPUS_SETTING ).AsIntegerOrNull();
            }
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected void LoadContent()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

            RockContext rockContext = new RockContext();

            var prayerRequestService = new PrayerRequestService( rockContext );
            var qryPrayerRequests = prayerRequestService
                .Queryable()
                .Include( "Category" )
                .Where( r => ( r.IsActive ?? true ) && ( !r.ExpirationDate.HasValue || r.ExpirationDate >= RockDateTime.Now ) && r.IsApproved == true );

            if ( GetAttributeValue( AttributeKey.PublicOnly ).AsBoolean() )
            {
                qryPrayerRequests = qryPrayerRequests.Where( a => a.IsPublic == true );
            }

            // Filter by Category.  First see if there is a Block Setting, otherwise use the Grid Filter
            CategoryCache categoryFilter = null;
            var categoryGuid = GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();
            if ( categoryGuid.HasValue )
            {
                categoryFilter = CategoryCache.Get( categoryGuid.Value );
            }

            var categoryIdQryString = PageParameter( PageParameterKey.CategoryId ).AsIntegerOrNull();
            if ( categoryFilter == null && categoryIdQryString.HasValue )
            {
                categoryFilter = CategoryCache.Get( categoryIdQryString.Value );
            }

            if ( categoryFilter != null )
            {
                // If filtered by category, only show comments for prayer requests in that category or any of its decendent categories
                var categoryService = new CategoryService( rockContext );
                var categories = new CategoryService( rockContext ).GetAllDescendents( categoryFilter.Guid ).Select( a => a.Id ).ToList();

                qryPrayerRequests = qryPrayerRequests.Where( a => a.CategoryId.HasValue &&
                    ( a.Category.Guid == categoryFilter.Guid || categories.Contains( a.CategoryId.Value ) ) );
            }

            var qryCampusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();
            if ( qryCampusId.HasValue )
            {
                var campus = CampusCache.Get( qryCampusId.Value );

                if ( campus != null )
                {
                    qryPrayerRequests = qryPrayerRequests.Where( e => e.CampusId == campus.Id );
                }
            }
            else if ( cpCampus.Visible )
            {
                if ( cpCampus.SelectedCampusId.HasValue )
                {
                    qryPrayerRequests = qryPrayerRequests.Where( e => e.CampusId == cpCampus.SelectedCampusId.Value );
                }
                else
                {
                    var filteredCampusIds = cpCampus
                       .Items
                       .Cast<ListItem>()
                       .Select( i => i.Value )
                       .AsIntegerList();

                    qryPrayerRequests = qryPrayerRequests.Where( e => !e.CampusId.HasValue || filteredCampusIds.Contains( e.CampusId.Value ) );
                }
            }

            int sortBy = GetAttributeValue( AttributeKey.Order ).AsInteger();
            switch ( sortBy )
            {
                case 0:
                    qryPrayerRequests = qryPrayerRequests.OrderBy( a => a.PrayerCount );
                    break;

                case 1:
                    qryPrayerRequests = qryPrayerRequests.OrderByDescending( a => a.EnteredDateTime );
                    break;

                case 2:
                    qryPrayerRequests = qryPrayerRequests.OrderBy( a => a.EnteredDateTime );
                    break;

                case 4:
                    qryPrayerRequests = qryPrayerRequests.OrderBy( a => Guid.NewGuid() );
                    break;

                default:
                    qryPrayerRequests = qryPrayerRequests.OrderBy( a => a.PrayerCount );
                    break;
            }

            int? maxResults = GetAttributeValue( AttributeKey.MaxResults ).AsIntegerOrNull();
            if ( maxResults.HasValue && maxResults > 0 )
            {
                qryPrayerRequests = qryPrayerRequests.Take( maxResults.Value );
            }

            var prayerRequests = qryPrayerRequests.ToList();

            mergeFields.Add( "PrayerRequestItems", prayerRequests );
            mergeFields.Add( AttributeKey.PrayedButtonText, GetAttributeValue( AttributeKey.PrayedButtonText ) );
            mergeFields.Add( AttributeKey.EnablePrayerTeamFlagging, GetAttributeValue( AttributeKey.EnablePrayerTeamFlagging ).AsBoolean() );
            string template = GetAttributeValue( AttributeKey.DisplayLavaTemplate );

            // Add last prayed information if requested
            if ( GetAttributeValue( AttributeKey.LoadLastPrayedCollection ).AsBoolean() )
            {
                var prayerRequestIds = prayerRequests.Select( p => p.Id ).ToList();
                var prayerRequestInteractionChannel = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.PRAYER_EVENTS ).Id;

                var lastPrayedInteractions = new InteractionService( rockContext ).Queryable()
                                                .Where( i =>
                                                    i.InteractionComponent.EntityId.HasValue
                                                    && prayerRequestIds.Contains( i.InteractionComponent.EntityId.Value )
                                                    && i.InteractionComponent.InteractionChannelId == prayerRequestInteractionChannel )
                                                .GroupBy( i => i.InteractionComponentId )
                                                .Select( i => i.OrderByDescending( x => x.InteractionDateTime ).FirstOrDefault() )
                                                .Select( y => new
                                                {
                                                    RequestId = y.InteractionComponent.EntityId,
                                                    PrayerDateTime = y.InteractionDateTime,
                                                    FirstName = y.PersonAlias.Person.NickName,
                                                    LastName = y.PersonAlias.Person.LastName
                                                } )
                                                .ToList();

                mergeFields.Add( "LastPrayed", lastPrayedInteractions );
            }

            lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upPrayer.ClientID );
        }

        /// <summary>
        /// Starts the workflow if one was defined in the block setting.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="key">The key.</param>
        private void StartWorkflow( PrayerRequest prayerRequest, RockContext rockContext, string key )
        {
            Guid? workflowTypeGuid = GetAttributeValue( key ).AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    try
                    {
                        // Create parameters
                        var parameters = new Dictionary<string, string>();
                        parameters.Add( "EntityGuid", prayerRequest.Guid.ToString() );
                        if ( CurrentPerson != null )
                        {
                            if ( key == AttributeKey.PrayedWorkflow )
                            {
                                parameters.Add( "PrayerOfferedByPersonAliasGuid", CurrentPerson.PrimaryAlias.Guid.ToString() );
                            }
                            else
                            {
                                parameters.Add( "FlaggedByPersonAliasGuid", CurrentPerson.PrimaryAlias.Guid.ToString() );
                            }
                        }

                        prayerRequest.LaunchWorkflow( workflowTypeGuid.Value, prayerRequest.Name, parameters, CurrentPersonAliasId );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, this.Context );
                    }
                }
            }
        }

        #endregion
    }
}