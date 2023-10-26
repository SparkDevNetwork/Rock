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

using Rock.Attribute;
using Rock.Badge.Component;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Crm.AssessmentList;
using Rock.Web.UI.Controls;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays the details of a particular assessment.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Assessment List" )]
    [Category( "CRM" )]
    [Description( "Displays the details of a particular assessment." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

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
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "3509289a-2e12-443f-87bf-2179bc36fecd" )]
    [Rock.SystemGuid.BlockTypeGuid( "5ecca4fb-f8fb-49db-96b7-082bb4e4c170" )]
    public class AssessmentList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
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

        #endregion Keys

        #region Constants

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

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                return GetAssessmentListBag( rockContext );
            }
        }

        private AssessmentListBag GetAssessmentListBag( RockContext rockContext )
        {
            var assessmentListBag = new AssessmentListBag();
            var currentPersonId = RequestContext?.CurrentPerson?.Id;

            // Gets Assessment types and assessments for each
            AssessmentTypeService assessmentTypeService = new AssessmentTypeService( rockContext );
            var allAssessmentsOfEachType = assessmentTypeService.Queryable().AsNoTracking()
                .Where( x => x.IsActive )
                .Select( t => new AssessmentTypeListItem
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    AssessmentPath = t.AssessmentPath,
                    AssessmentResultsPath = t.AssessmentResultsPath,
                    AssessmentRetakeLinkButton = "",
                    RequiresRequest = t.RequiresRequest,
                    MinDaysToRetake = t.MinimumDaysToRetake,
                    LastRequestObject = t.Assessments
                            .Where( a => a.PersonAlias.Person.Id == currentPersonId )
                            .OrderBy( a => a.Status ) // pending first
                            .Select( a => new LastAssessmentTaken
                            {
                                RequestedDate = a.RequestedDateTime,
                                CompletedDate = a.CompletedDateTime,
                                Status = a.Status,
                                Requester = a.RequesterPersonAlias.Person.NickName + " " + a.RequesterPersonAlias.Person.LastName
                            } )
                            .OrderByDescending( x => x.RequestedDate )
                            .ThenByDescending( x => x.CompletedDate )
                            .FirstOrDefault(),
                })
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
                    if ( item.LastRequestObject.CompletedDate.HasValue &&
                        item.LastRequestObject.CompletedDate.Value.AddDays( item.MinDaysToRetake ) <= RockDateTime.Now &&
                        !item.RequiresRequest )
                    {
                        item.AssessmentRetakeLinkButton = string.Format( "<a href='{0}?AssessmentId=0'>Retake Assessment</a>", item.AssessmentPath );
                    }
                }
            }

            // Decide if anything is going to display
            bool hideIfNoActiveRequests = GetAttributeValue( AttributeKey.HideIfNoActiveRequests ).AsBoolean();
            bool hidIfNoRequests = GetAttributeValue( AttributeKey.HideIfNoRequests ).AsBoolean();
            if ( ( hideIfNoActiveRequests && !areThereAnyPendingRequests ) || ( hidIfNoRequests && !areThereAnyRequests ) )
            {
                assessmentListBag.HasContent = false;
            }
            else
            {
                assessmentListBag.HasContent = true;
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, RequestContext.CurrentPerson );

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
                    // the completed data is only populated if the assessment was actually completed, where as a complete status can be assigned if it was not taken. So use date instead of status for completed.
                    var onlyAllowedRequestedOrCompleted = allAssessmentsOfEachType
                        .Where( x => !x.RequiresRequest
                            || ( x.LastRequestObject != null && x.LastRequestObject.Status == AssessmentRequestStatus.Pending )
                            || ( x.LastRequestObject != null && x.LastRequestObject.CompletedDate != null )
                        );

                    mergeFields.Add( "AssessmentTypes", onlyAllowedRequestedOrCompleted );
                }

                assessmentListBag.AssessmentList = GetAttributeValue( AttributeKey.LavaTemplate ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
            }

            return assessmentListBag;
        }

        #endregion Methods

        #region Helper Classes

        public class LastAssessmentTaken : RockDynamic
        {
            public DateTime? RequestedDate { get; set; }
            public DateTime? CompletedDate { get; set; }
            public AssessmentRequestStatus Status { get; set; }
            public string Requester { get; set; }
        }

        public class AssessmentTypeListItem : RockDynamic
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string AssessmentPath { get; set; }
            public string AssessmentResultsPath { get; set; }
            public string AssessmentRetakeLinkButton { get; set; }
            public bool RequiresRequest { get; set; }
            public int MinDaysToRetake { get; set; }
            public LastAssessmentTaken LastRequestObject { get; set; }
        }

        #endregion Helper Classes
    }
}