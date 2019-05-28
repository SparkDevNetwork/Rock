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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rockweb.Blocks.Crm
{
    /// <summary>
    /// Lists all avalable assesments for the individual.
    /// </summary>
    [DisplayName( "Assessment List" )]
    [Category( "CRM" )]
    [Description( "Allows you to view and take any available assessments." )]

    #region Block Attributes

    [BooleanField(
        "Only Show Requested",
        Key = AttributeKey.OnlyShowRequested,
        Description = "If enabled, limits the list to show only assessments that have been requested or completed.",
        DefaultValue = "True",
        Order = 0 )]

    [BooleanField(
        "Hide If No Active Requests",
        Key = AttributeKey.HideIfNoActiveRequests,
        Description = "If enabled, nothing will be shown if there are not pending (waiting to be taken) assessment requests.",
        DefaultValue = "False",
        Order = 1 )]

    [BooleanField(
        "Hide If No Requests",
        Key = AttributeKey.HideIfNoRequests,
        Description = "If enabled, nothing will be shown where there are no requests (pending or completed).",
        DefaultValue = "False",
        Order = 2 )]

    [CodeEditorField(
        "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The lava template to use to format the entire block.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = lavaTemplateDefaultValue,
        Order = 3)]

    #endregion Block Attributes

    public partial class AssessmentList : Rock.Web.UI.RockBlock
    {
        #region Atrribute Keys
        protected static class AttributeKey
        {

            /// <summary>
            /// Attribute value if only requested assessments should be shown.
            /// </summary>
            public const string OnlyShowRequested = "OnlyShowRequested";

            /// <summary>
            /// Attribute value if the block should be hidden if there are no active requests.
            /// </summary>
            public const string HideIfNoActiveRequests = "HideIfNoActiveRequests";

            /// <summary>
            /// Attribute value if the block should be hidden if there are no requests (active or not)
            /// </summary>
            public const string HideIfNoRequests = "HideIfNoRequests";

            /// <summary>
            /// The block lava template to display the assessments
            /// </summary>
            public const string LavaTemplate = "LavaTemplate";
        }
        #endregion Atrribute Keys

        #region constants

        protected const string lavaTemplateDefaultValue = @"<div class='panel panel-default'>
    <div class='panel-heading'>Assessments</div>
    <div class='panel-body'>
            {% for assessmenttype in AssessmentTypes %}
                {% if assessmenttype.LastRequestObject %}
                    {% if assessmenttype.LastRequestObject.Status == 'Complete' %}
                        <div class='panel panel-success'>
                            <div class='panel-heading'>{{ assessmenttype.Title }}<br />
                                Completed: {{ assessmenttype.LastRequestObject.CompletedDate | Date:'M/d/yyyy'}} <br />
                                <a href='{{ assessmenttype.AssessmentResultsPath}}'>View Results</a>
                                &nbsp;&nbsp;{{ assessmenttype.AssessmentRetakeLinkButton }}
                            </div>
                        </div>
                    {% elseif assessmenttype.LastRequestObject.Status == 'Pending' %}
                        <div class='panel panel-warning'>
                            <div class='panel-heading'> {{ assessmenttype.Title }}<br />
                                Requested: {{assessmenttype.LastRequestObject.Requester}} ({{ assessmenttype.LastRequestObject.RequestedDate | Date:'M/d/yyyy'}})<br />
                                <a href='{{ assessmenttype.AssessmentPath}}'>Start Assessment</a>
                            </div>
                        </div>
                    {% endif %}
                    {% else %}
                        <div class='panel panel-default'>
                            <div class='panel-heading'> {{ assessmenttype.Title }}<br />
                                Available<br />
                                <a href='{{ assessmenttype.AssessmentPath}}'>Start Assessment</a>
                            </div>
                        </div>
                {% endif %}
            {% endfor %}
    </div>
</div>";

        #endregion constants

        #region Base Control Methods

        /// <summary>
        /// On initialize
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( CurrentPerson == null )
            {
                return;
            }

            if ( !Page.IsPostBack )
            {
                BindData();
            }
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindData();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Bind the data and merges the Lava fields using the template.
        /// </summary>
        private void BindData()
        {
            lAssessments.Visible = true;

            // Gets Assessment types and assessments for each
            RockContext rockContext = new RockContext();
            AssessmentTypeService assessmentTypeService = new AssessmentTypeService( rockContext );
            var allAssessmentsOfEachType = assessmentTypeService.Queryable().AsNoTracking()
                .Where( x => x.IsActive == true )
                .Select( t => new AssessmentTypeListItem 
                {
                    Title = t.Title,
                    AssessmentPath = t.AssessmentPath,
                    AssessmentResultsPath = t.AssessmentResultsPath,
                    //AssessmentRetakePath = t.AssessmentPath + "?AssessmentId=0",
                    AssessmentRetakeLinkButton = "",
                    RequiresRequest = t.RequiresRequest,
                    MinDaysToRetake = t.MinimumDaysToRetake,
                    LastRequestObject = t.Assessments
                            .Where( a => a.PersonAlias.Person.Id == CurrentPersonId )
                            .OrderBy( a => a.Status ) // pending first
                            .Select( a => new LastAssessmentTaken
                            {
                                RequestedDate = a.RequestedDateTime,
                                CompletedDate = a.CompletedDateTime,
                                Status = a.Status,
                                Requester = a.RequesterPersonAlias.Person.NickName + " " + a.RequesterPersonAlias.Person.LastName
                            } )
                            .OrderBy( x => x.Status )
                            .ThenByDescending( x => x.CompletedDate )
                            .FirstOrDefault(),
                    
                    }
                )
                // order by requested then by pending, completed, then by available to take
                .OrderByDescending( x => x.LastRequestObject.Status )
                .ThenBy( x => x.LastRequestObject )
                .ToList();
            
            // Checks current request types to use against the settings
            bool areThereAnyPendingRequests = false;
            bool areThereAnyRequests = false;

            foreach ( var item in allAssessmentsOfEachType.Where( a => a.LastRequestObject != null ) )
            {
                areThereAnyRequests = true;

                if ( item.LastRequestObject.Status == AssessmentRequestStatus.Pending )
                {
                    areThereAnyPendingRequests = true;
                }
                else if ( item.LastRequestObject.Status == AssessmentRequestStatus.Complete )
                {
                    if ( item.LastRequestObject.CompletedDate.HasValue && item.LastRequestObject.CompletedDate.Value.AddDays( item.MinDaysToRetake ) <= RockDateTime.Now )
                    {
                        if ( IsBlockConfiguredToAllowRetakes( item ) )
                        {
                            item.AssessmentRetakeLinkButton = "<a href='" + item.AssessmentPath + "?AssessmentId=0'>Retake Assessment</a>";
                        }
                    }
                }
            }

            // Decide if anything is going to display
            bool hideIfNoActiveRequests = GetAttributeValue( AttributeKey.HideIfNoActiveRequests ).AsBoolean();
            bool hidIfNoRequests = GetAttributeValue( AttributeKey.HideIfNoRequests ).AsBoolean();
            if ( ( hideIfNoActiveRequests && !areThereAnyPendingRequests ) || ( hidIfNoRequests && !areThereAnyRequests ) )
            {
                lAssessments.Visible = false;
            }
            else
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson );

                // Show only the tests requested or completed?...
                if ( GetAttributeValue( AttributeKey.OnlyShowRequested ).AsBoolean() )
                {
                    // the completed data is only populated if the assessment was actually completed, where as a complete status can be assinged if it was not taken. So use date instead of status for completed.
                    var onlyRequestedOrCompleted = allAssessmentsOfEachType
                        .Where( x => x.LastRequestObject != null )
                        .Where( x => x.LastRequestObject.Requester != null )
                        .Where( x => x.LastRequestObject.Status == AssessmentRequestStatus.Pending || x.LastRequestObject.CompletedDate != null );

                    mergeFields.Add( "AssessmentTypes", onlyRequestedOrCompleted );
                }
                else
                {
                    // ...Otherwise show any allowed, requested or completed requests.
                    // the completed data is only populated if the assessment was actually completed, where as a complete status can be assinged if it was not taken. So use date instead of status for completed.
                    var onlyAllowedRequestedOrCompleted = allAssessmentsOfEachType
                        .Where( x => x.RequiresRequest != true
                            || ( x.LastRequestObject != null && x.LastRequestObject.Status == AssessmentRequestStatus.Pending )
                            || ( x.LastRequestObject != null && x.LastRequestObject.CompletedDate != null ) 
                        );

                    mergeFields.Add( "AssessmentTypes", onlyAllowedRequestedOrCompleted );
                }

                lAssessments.Text = GetAttributeValue( AttributeKey.LavaTemplate ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
            }
        }

        /// <summary>
        /// Determines whether [is block configured to allow retakes] [the specified assessment type list item].
        /// </summary>
        /// <param name="assessmentTypeListItem">The assessment type list item.</param>
        /// <returns>
        ///   <c>true</c> if [is block configured to allow retakes] [the specified assessment type list item]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsBlockConfiguredToAllowRetakes( AssessmentTypeListItem assessmentTypeListItem )
        {
            string domain = System.Web.HttpContext.Current.Request.Url.GetLeftPart( UriPartial.Authority ).Replace( "https://", string.Empty ).Replace( "http://", string.Empty );
            string route = assessmentTypeListItem.AssessmentPath.Replace( "/", string.Empty );

            var rockContext = new RockContext();
            var pageRouteService = new PageRouteService( rockContext );
            var pageId = pageRouteService
                .Queryable()
                .Where( r => r.Route == route )
                .Where( r => r.Page.Layout.Site.SiteDomains.Select( d => d.Domain == domain ).FirstOrDefault() )
                .Select( r => r.PageId )
                .FirstOrDefault();

            Guid blockTypeGuid = Guid.Empty;
            switch ( route )
            {
                case "ConflictProfile":
                    blockTypeGuid = Rock.SystemGuid.BlockType.CONFLICT_PROFILE.AsGuid();
                    break;
                case "EQ":
                    blockTypeGuid = Rock.SystemGuid.BlockType.EQ_INVENTORY.AsGuid();
                    break;
                case "Motivators":
                    blockTypeGuid = Rock.SystemGuid.BlockType.MOTIVATORS.AsGuid();
                    break;
                case "SpiritualGifts":
                    blockTypeGuid = Rock.SystemGuid.BlockType.GIFTS_ASSESSMENT.AsGuid();
                    break;
                case "DISC":
                    blockTypeGuid = Rock.SystemGuid.BlockType.DISC.AsGuid();
                    break;
            }

            int? blockTypeId = BlockTypeCache.GetId( blockTypeGuid );
            var blockService = new BlockService( rockContext );
            var block = blockTypeGuid != Guid.Empty ? blockService.GetByPageAndBlockType( pageId, blockTypeId.Value ).FirstOrDefault() : null;

            if ( block != null )
            {
                block.LoadAttributes();
                return block.GetAttributeValue( "AllowRetakes" ).AsBooleanOrNull() ?? true;
            }

            return true;
        }

        public class LastAssessmentTaken : DotLiquid.Drop
        {
            public DateTime? RequestedDate { get; set; }
            public DateTime? CompletedDate { get; set; }
            public AssessmentRequestStatus Status { get; set; }
            public string Requester { get; set; }
        }

        public class AssessmentTypeListItem : DotLiquid.Drop
        {
            public string Title { get; set; }
            public string AssessmentPath { get; set; }
            public string AssessmentResultsPath { get; set; }
            public string AssessmentRetakeLinkButton { get; set; }
            public bool RequiresRequest { get; set; }
            public int MinDaysToRetake { get; set; }
            public LastAssessmentTaken LastRequestObject { get; set; }

            //public object ToLiquid()
            //{
            //    return this;
            //}
        }

        #endregion Methods
    }
}