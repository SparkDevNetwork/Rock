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
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "Personalized Communication History" )]
    [Category( "Communication" )]
    [Description( "Lists the communications sent to a specific individual" )]

    #region Block Attributes

    [ContextAware(
        typeof( Rock.Model.Person ),
        IsConfigurable = false )]
    [LinkedPage(
        "Communication Detail Page",
        required: false,
        Key = AttributeKey.CommunicationDetailPage )]
    [LinkedPage(
        "Communication List Detail Page",
        required: false,
        Key = AttributeKey.CommunicationListDetailPage )]
    [LinkedPage(
        "Communication Segment Detail Page",
        required: false,
        Key = AttributeKey.CommunicationSegmentDetailPage )]
    [LinkedPage(
        "Communication Template Detail Page",
        required: false,
        Key = AttributeKey.CommunicationTemplateDetailPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "3F294916-A02D-48D5-8FE4-E8D7B98F61F7" )]
    public partial class PersonalizedCommunicationHistory : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CommunicationDetailPage = "CommunicationDetailPage";
            public const string CommunicationListDetailPage = "CommunicationListDetailPage";
            public const string CommunicationSegmentDetailPage = "CommunicationSegmentDetailPage";
            public const string CommunicationTemplateDetailPage = "CommunicationTemplateDetailPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion

        #region Fields

        private Person _person = null;
        private PersonService _gridPersonService;
        private DataViewService _gridDataViewService;
        private BinaryFileService _gridBinaryFileService;
        private Dictionary<int, CommunicationType> _mediumEntityIdToCommunicationTypeMap;
        private List<int> _currentPersonAliasIdList;

        private const string _communicationItemLavaTemplate = @"
<div class='communication-item pt-3 d-flex flex-row cursor-default'>
    <div class='d-none d-sm-block pt-1 pl-2 pr-3'>
        {% case Communication.CommunicationType %}
            {% when 'Email' %}
                <div class='avatar avatar-lg avatar-icon avatar-email'>
                    <i class='fa fa-envelope'></i>
                </div>
            {% when 'SMS' %}
                <div class='avatar avatar-lg avatar-icon avatar-sms'>
                    <i class='fa fa-comment-alt'></i>
                </div>
            {% when 'PushNotification' %}
                <div class='avatar avatar-lg avatar-icon avatar-push'>
                    <i class='fa fa-mobile-alt'></i>
                </div>
            {% else %}
                <div class='avatar avatar-lg avatar-icon avatar-othercomm'>
                    <i class='fa fa-question-circle'></i>
                </div>
        {% endcase %}
    </div>
    <div class='flex-grow-1'>
        <div class='d-flex flex-row align-items-top align-items-sm-center pb-3'>
            <div class='flex-fill pr-sm-3 leading-snug'>
                <span class='d-block text-wrap text-break mb-1'>{{ Communication.Title }}</span>
                <span class='d-block text-sm text-muted mb-1'>{{ Communication.Sender.FullName }}</span>
                {% capture moreHtml %}<i class=&quot;fa fa-xs mr-1 fa-chevron-right&quot;></i> <span>More</span>{% endcapture %}
                {% capture lessHtml %}<i class=&quot;fa fa-xs mr-1 fa-chevron-down&quot;></i> <span>Less</span>{% endcapture %}
                {% if HasDetail %}
                    <a href='#' class='text-xs py-1 d-inline-flex align-items-center' onclick=""toggleCommunicationDetail(this,'{{ Communication.RowId }}','{{ moreHtml }}','{{ lessHtml }}');return false;"">{{ lessHtml | HtmlDecode }}</a>
                {% else %}
                    <a href=""{{ ShowDetailPostBackEventReference }}"" class='text-xs py-1 d-inline-flex align-items-center' onclick=""toggleCommunicationDetail(this,'{{ Communication.RowId }}','{{ moreHtml }}','{{ lessHtml }}');return true;"">{{ moreHtml | HtmlDecode }}</a>
                {% endif %}
            </div>
            <div class='text-right d-none d-sm-block'>
                <span class='badge badge-info' data-toggle='tooltip' data-placement='top' title='{{ Communication.RecipientTotal }} {{ 'Recipient' | PluralizeForQuantity:Communication.RecipientTotal }}'>{{ Communication.RecipientTotal }}</span>
            </div>
            <div class='text-right pl-3 pr-2'>
                <span class='d-block text-sm text-muted mb-1 text-nowrap' title='{{ Communication.SendDateTime }}'>{{ Communication.SendDateTime | HumanizeDateTime | SentenceCase }}</span>
                {% case Communication.RecipientStatus %}
                {% when 'Delivered' %}
                    <span class='label label-info' data-toggle='tooltip' data-placement='top' title='Sent on {{ Communication.SendDateTime | Date:'sd' }} at {{ Communication.SendDateTime | Date:'st' }}'>Delivered</span>
                {% when 'Failed' %}
                    <span class='label label-danger' data-toggle='tooltip' data-placement='top' title='{{ Communication.RecipientStatusNote }}'>Failed</span>
                {% when 'Cancelled' %}
                    <span class='label label-warning'>Cancelled</span>
                {% when 'Opened' %}
                    <span class='label label-success'>Interacted</span>
                {% when 'Pending' %}
                    {% case Communication.CommunicationStatus %}
                    {% when 'Approved' %}
                        {% if Communication.SendDateTime != '' %}
                            <span class='label label-default' data-toggle='tooltip' data-placement='top' title='Now sending'>Sending</span>
                        {% else %}
                            <span class='label label-default' data-toggle='tooltip' data-placement='top' title='Scheduled for {{ Communication.SendDateTime | Date:'sd' }} at {{ Communication.SendDateTime | Date:'st' }}'>Pending</span>
                        {% endif %}
                    {% when 'PendingApproval' %}
                        <span class='label label-default' data-toggle='tooltip' data-placement='top' title='Pending Approval, Scheduled for {{ Communication.SendDateTime | Date:'sd' }} at {{ Communication.SendDateTime | Date:'st' }}'>Pending</span>
                    {% when 'Denied' %}
                        <span class='label label-default' data-toggle='tooltip' data-placement='top' title='Approval Declined'>Pending</span>
                    {% when 'Draft' %}
                        <span class='label label-default'>Pending</span>
                    {% endcase %}
                {% else %}
                    <span class='label label-default'>{{ Communication.RecipientStatus }}</span>
                {% endcase %}
            </div>
        </div>

        {% if HasDetail %}
            <div class='communication-details'>
                <div class='border-0 py-0'>
                    <div id='details-{{ Communication.RowId }}' class='pb-5'>
                        <div class='row'>
                            <div class='col-md-12 mb-4'><div class='border-top border-panel'></div></div>
                            <div class='col-md-6'>
                                <div class='row'>
                                    <div class='col-xs-6 col-md-4 leading-snug mb-4'>
                                        <span class='control-label d-block text-muted'>Sent As</span>
                                        <span class='d-block text-lg font-weight-bold'>
                                        {% if Communication.AllowRecipientPreference %}Recipient Preference{% else %}{{ Communication.CommunicationType | AsString | Humanize | Capitalize }}{% endif %}
                                        </span>
                                    </div>
                                    <div class='col-xs-6 col-md-4 leading-snug mb-4'>
                                        <span class='control-label d-block text-muted'>{{ 'Recipient' | PluralizeForQuantity:Communication.RecipientTotal }}</span>
                                        <span class='d-block text-lg font-weight-bold'>{{ Communication.RecipientTotal }}</span>
                                    </div>
                                    {% if Communication.Detail.PersonalInteractionCount > 0 %}
                                    <div class='col-xs-6 col-md-4 leading-snug mb-4'>
                                        <span class='control-label d-block text-muted'>Activity Count</span>
                                        <span class='d-block text-lg font-weight-bold'>{{ Communication.Detail.PersonalInteractionCount }}</span>
                                    </div>
                                    {% endif %}
                                </div>
                                <dl>
                                    {% if Communication.Detail.CommunicationListName != null %}
                                        <dt>Communication List</dt>
                                        {% if ListDetailUrl != empty %}
                                            <dd><a href=""{{ ListDetailUrl }}"">{{ Communication.Detail.CommunicationListName }}</a></dd>
                                        {% else %}
                                            <dd>{{ Communication.Detail.CommunicationListName }}</dd>
                                        {% endif %}
                                    {% endif %}
                                    {% if Communication.Detail.CommunicationSegments != null %}
                                        <dt>Segments ({{ Communication.Detail.CommunicationSegmentInclusionType }})</dt>
                                        <dd>
                                            {% for segment in Communication.Detail.CommunicationSegments %}
                                                {% if ListSegmentDetailUrlTemplate != empty %}
                                                    <a href=""{{ ListSegmentDetailUrlTemplate | Replace:'@segmentId',segment.Id }}"">{{ segment.Name }}</a><br>
                                                {% else %}
                                                    {{ segment.Name }}<br>
                                                {% endif %}
                                            {% endfor %}
                                        </dd>
                                    {% endif %}
                                    {% if Communication.Detail.CommunicationTemplateName != empty %}
                                        <dt>Communication Template</dt>
                                        {% if TemplateDetailUrl != empty %}
                                            <dd><a href=""{{ TemplateDetailUrl }}"">{{ Communication.Detail.CommunicationTemplateName }}</a></dd>
                                        {% else %}
                                            <dd>{{ Communication.Detail.CommunicationTemplateName }}</dd>
                                        {% endif %}
                                    {% endif %}
                                </dl>
                            </div>
                            <div class='col-md-6'>
                                <span class='control-label d-block text-muted'>Message Preview</span>
                                {% case Communication.CommunicationType %}
                                {% when 'SMS' %}
                                    <div class='card communication-preview'>
                                        <div class='card-heading text-center'><span class='d-block font-weight-semibold'>{{ Communication.Detail.SenderName }}</span> <span class='d-block text-xs text-muted'>{{ Communication.Detail.SenderAddress }}</span></div>
                                        <div class='card-body'>
                                            {% if Communication.Detail.Message != null %}
                                                <div class='sms-bubble'>
                                                    {{ Communication.Detail.Message }}
                                                </div>
                                            {% endif %}
                                            {% for attachmentUrl in Communication.Detail.Attachments %}
                                                <div class='sms-image'>
                                                    <img src='{{ attachmentUrl }}' alt='' class='w-100'>
                                                </div>
                                            {% endfor %}
                                        </div>
                                    </div>
                                {% when 'PushNotification' %}
                                    <div class='card communication-preview'>
                                        <div class='card-heading'><span class='font-weight-semibold'>{{ Communication.Detail.RecipientName }}</span></div>
                                        <div class='card-body' style='background:#FCFCFC'>
                                            <div class='push-msg'>
                                                <div class='push-msg-header'>
                                                    <div class='push-msg-icon'></div>
                                                    <div class='push-msg-app-name'>{{ Communication.Detail.ApplicationName }}</div>
                                                </div>
                                                <div class='push-msg-body'>
                                                    <span class='push-msg-title'>{{ Communication.Title }}</span>
                                                    <span class='push-summary'>{{ Communication.Detail.Message }}</span>
                                                </div>
                                            </div>
                                            {% for attachmentUrl in Communication.Detail.Attachments %}
                                                <div class='sms-image'>
                                                    <img src='{{ attachmentUrl }}' alt='' class='w-100'>
                                                </div>
                                            {% endfor %}
                                        </div>
                                    </div>
                                {% else %}
                                    {%- assign orgName = 'Global' | Attribute:'OrganizationName' -%}
                                    <div class='card communication-preview'>
                                        <div class='card-heading text-wrap' title='Message From'><span class='font-weight-semibold'>{{ Communication.Detail.SenderName | Default:orgName }}</span> {% if Communication.Detail.SenderAddress != '' %}<span class='text-muted'>{{ Communication.Detail.SenderAddress }}</span>{% endif %} </div>
                                        <div class='card-heading text-wrap' title='Message Subject'>{{ Communication.Title }}</div>
                                        <div class='card-body p-0 position-relative' style='background:#FCFCFC'>
                                            {% if Communication.ViewDetailIsAllowed and DetailUrl != empty %}
                                                <div class='d-flex justify-content-center align-items-center position-absolute inset-0 z-10'>
                                                    <a href='{{ DetailUrl }}' class='btn btn-default'>View Message</a>
                                                </div>
                                            {% endif %}

                                            {% comment %} TEMPORARY EXAMPLE FOR PREVIEW SVG will be included as image {% endcomment %}

                                            <svg class='d-block' style='filter: blur(4px);' fill='none' xmlns='http://www.w3.org/2000/svg' viewBox='0 0 364 150'><path fill='#FCFCFC' d='M0 0h364v226H0z'/><path d='M240.1 8H123.9c-3 0-5.4 2.4-5.4 5.4v199.2c0 3 2.4 5.4 5.4 5.4h116.2c3 0 5.4-2.4 5.4-5.4V13.4c0-3-2.4-5.4-5.4-5.4Z' fill='#fff' stroke='#DBDBDB' stroke-miterlimit='10'/><path d='M229.3 131.8h-94.6a4 4 0 0 0-4 4v2.8a4 4 0 0 0 4 4h94.6a4 4 0 0 0 4-4v-2.7a4 4 0 0 0-4-4ZM214.4 28.2H155a2.7 2.7 0 1 0 0 5.4h59.4a2.7 2.7 0 1 0 0-5.4ZM138.8 36.3a5.4 5.4 0 1 0 0-10.8 5.4 5.4 0 0 0 0 10.8Z' fill='#737475'/><path d='M230.6 48.4h-97.2a2.7 2.7 0 0 0-2.7 2.7V113c0 1.5 1.2 2.7 2.7 2.7h97.2c1.5 0 2.7-1.2 2.7-2.7V51c0-1.4-1.2-2.6-2.7-2.6Z' fill='#E6E6E6'/><path d='M215.4 72a8.4 8.4 0 1 0 0-17 8.4 8.4 0 0 0 0 17Z' fill='#fff'/><path d='M210.4 115.7h-40.6l20.3-40.4 20.3 40.4Z' fill='#737475'/><path d='M196.9 115.7h-57.5l28.8-53.9 28.7 53.9Z' fill='#737475'/></svg>
                                        </div>
                                    </div>
                                {% endcase %}

                                {% if Communication.ViewDetailIsAllowed and DetailUrl != empty %}
                                    <div class='text-right'><a href=""{{ DetailUrl }}"" class='text-xs'>View Communication</a></div>
                                {% endif %}
                            </div>
                        </div>
                        {% if Communication.Detail.Activities != empty %}
                            <div class='mt-5'>
                                <span class='control-label d-block text-muted'>Activity</span>
                                <table class='table table-striped table-bordered table-condensed text-sm bg-white'>
                                    <thead>
                                        <tr>
                                            <th>Activity</th>
                                            {% if Communication.CommunicationType != 'PushNotification' %}
                                            <th>Details</th>
                                            {% endif %}
                                            <th class='w-1'>Date</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {% for item in Communication.Detail.Activities %}
                                            <tr>
                                                <td>
                                                    {{ item.Name }}
                                                    {% if item.Name == 'Click' %}
                                                        <a class='help' href='#' tabindex='-1' data-toggle='tooltip' data-placement='auto' data-container='body' data-html='true' title='' data-original-title='{{ item.Details }}'><i class='fa fa-info-circle'></i></a>
                                                    {% endif %}
                                                </td>
                                                {% if Communication.CommunicationType != 'PushNotification' %}
                                                <td class='wrap-contents'>{{ item.Details }}</td>
                                                {% endif %}
                                                <td class='w-1 text-nowrap'>{{ item.DateTime | Date:'' }}</td>
                                            </tr>
                                        {% endfor %}
                                    </tbody>
                                </table>
                            </div>
                        {% endif %}
                    </div>
                </div>
            </div>

        {% else %}
            <div id='details-{{ Communication.RowId }}' class='communication-details' style='display: none;'>
            </div>
        {% endif %}
    </div>
<div>
";

        #endregion

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
            rFilter.ClearFilterClick += rFilter_ClearFilterClick;

            // Configure the Communication List for a minimal grid with support for custom paging.
            gCommunication.DataKeyNames = new string[] { "Id" };

            gCommunication.AllowCustomPaging = true;
            gCommunication.ShowHeader = false;
            gCommunication.Actions.ShowAdd = false;
            gCommunication.Actions.ShowExcelExport = false;
            gCommunication.Actions.ShowMergeTemplate = false;

            gCommunication.RowDataBound += gCommunication_RowDataBound;
            gCommunication.GridRebind += gCommunication_GridRebind;

            // Reconfigure the full-page progress meter to prevent it from appearing immediately when loading the detail panel.
            var updateProgress = ( UpdateProgress ) this.Page.Master.FindControl( "updateProgress" );

            if ( updateProgress != null )
            {
                updateProgress.DisplayAfter = 5000;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            InitializeContextPerson();
            InitializeCommunicationMediumMap();

            if ( Page.IsPostBack )
            {
                // Handle postback request to load communication details.
                var postbackCtl = Request.Params.Get( "__EVENTTARGET" ) ?? string.Empty;

                if ( postbackCtl.EndsWith( nameof( upPanel ) ) )
                {
                    HandlePostBack( postbackCtl, Request["__EVENTARGUMENT"] );
                }
            }
            else
            {
                // Full page load.
                if ( _person != null )
                {
                    lBlockTitle.Text = $"{_person.FullName}'s Communication History";
                }

                SetFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Filter Events

        /// <summary>
        /// Keys to use for Filter Settings
        /// </summary>
        private static class FilterSettingName
        {
            public const string Subject = "Subject";
            public const string Medium = "Medium";
            public const string Status = "Status";
            public const string CreatedBy = "Created By";
            public const string SendDateRange = "Send Date";
            public const string CommunicationTemplate = "Communication Template";
            public const string SystemCommunicationType = "System Communication Type";
            public const string BulkStatus = "Bulk Status";
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            int personId = ppCreatedBy.PersonId ?? 0;

            rFilter.SetFilterPreference( FilterSettingName.Subject, tbSubject.Text );
            rFilter.SetFilterPreference( FilterSettingName.Medium, ddlMedium.SelectedValue );
            rFilter.SetFilterPreference( FilterSettingName.SendDateRange, drpDates.DelimitedValues );
            rFilter.SetFilterPreference( FilterSettingName.CreatedBy, personId.ToString() );
            rFilter.SetFilterPreference( FilterSettingName.SystemCommunicationType, ddlSystemCommunicationType.SelectedValue );
            rFilter.SetFilterPreference( FilterSettingName.CommunicationTemplate, ddlTemplate.SelectedValue );
            rFilter.SetFilterPreference( FilterSettingName.Status, ddlStatus.SelectedValue );
            rFilter.SetFilterPreference( FilterSettingName.BulkStatus, ddlBulk.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteFilterPreferences();

            SetFilter();
            BindGrid();
        }

        /// <summary>
        /// Handles the filter display for each saved user value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case FilterSettingName.Medium:
                    {
                        var item = ddlMedium.Items.FindByValue( e.Value );
                        e.Value = item != null ? item.Text : string.Empty;
                        break;
                    }
                case FilterSettingName.Status:
                    {
                        var item = ddlStatus.Items.FindByValue( e.Value );
                        e.Value = item != null ? item.Text : string.Empty;
                        break;
                    }
                case FilterSettingName.CreatedBy:
                    {
                        InitializeDataBindingServices();

                        var personName = string.Empty;

                        int? personId = e.Value.AsIntegerOrNull();
                        if ( personId.HasValue )
                        {
                            var person = _gridPersonService.Get( personId.Value );

                            if ( person != null )
                            {
                                personName = person.FullName;
                            }
                        }

                        e.Value = personName;

                        break;
                    }
                case FilterSettingName.SendDateRange:
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                case FilterSettingName.BulkStatus:
                    {
                        var item = ddlBulk.Items.FindByValue( e.Value );
                        e.Value = item != null ? item.Text : string.Empty;
                        break;
                    }
                case FilterSettingName.CommunicationTemplate:
                    {
                        var item = ddlTemplate.Items.FindByValue( e.Value );
                        e.Value = item != null ? item.Text : string.Empty;
                        break;
                    }
                case FilterSettingName.SystemCommunicationType:
                    {
                        var item = ddlSystemCommunicationType.Items.FindByValue( e.Value );
                        e.Value = item != null ? item.Text : string.Empty;
                        break;
                    }
            }
        }

        #endregion

        #region Communication List Events

        /// <summary>
        /// Handles the RowDataBound event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gCommunication_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var communication = e.Row.DataItem as CommunicationListItem;
            if ( communication == null )
            {
                return;
            }

            var up = e.Row.FindControl( "upCommunicationItem" );
            var lLava = e.Row.FindControl( "lCommunicationDetailRow" ) as Literal;
            if ( lLava == null )
            {
                return;
            }

            lLava.Text = GetCommunicationListItemHtml( up, communication, false );
        }

        /// <summary>
        /// Handles the GridRebind event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gCommunication_GridRebind( object sender, EventArgs e )
        {
            BindGrid();

            upPanel.Update();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Initialize the Person whose Communication history will be displayed.
        /// </summary>
        private void InitializeContextPerson()
        {
            // Prefer the page parameter value if it is specified, unless a context person is required by the block configuration.
            int personEntityTypeId = EntityTypeCache.Get<Person>().Id;

            var contextIsRequired = ContextTypesRequired.Any( x => x.Id == personEntityTypeId );

            var personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

            if ( personId != null && contextIsRequired )
            {
                var rockContext = new RockContext();

                var personService = new PersonService( rockContext );

                _person = personService.Get( personId.Value );
            }
            else
            {
                _person = this.ContextEntity() as Person;
            }
        }

        /// <summary>
        /// Create a map of Communication Mediums to Communication Types.
        /// </summary>
        private void InitializeCommunicationMediumMap()
        {
            _mediumEntityIdToCommunicationTypeMap = new Dictionary<int, CommunicationType>();

            _mediumEntityIdToCommunicationTypeMap.Add( EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ) ?? 0, CommunicationType.Email );
            _mediumEntityIdToCommunicationTypeMap.Add( EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ) ?? 0, CommunicationType.SMS );
            _mediumEntityIdToCommunicationTypeMap.Add( EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() ) ?? 0, CommunicationType.PushNotification );
        }

        /// <summary>
        /// Binds the Communication List filter.
        /// </summary>
        private void SetFilter()
        {
            var rockContext = new RockContext();

            InitializeDataBindingServices();

            // Subject
            tbSubject.Text = rFilter.GetFilterPreference( FilterSettingName.Subject );

            // Communication Medium
            ddlMedium.Items.Clear();
            ddlMedium.Items.Add( new ListItem() );

            var activeMediums = MediumContainer.Instance.Components
                .Select( x => x.Value.Value )
                .Where( x => x.IsActive )
                .Select( x => x.TypeGuid )
                .ToList();

            if ( activeMediums.Contains( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ) )
            {
                ddlMedium.Items.Add( new ListItem( "Email", CommunicationType.Email.ConvertToInt().ToString() ) );
            }
            if ( activeMediums.Contains( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ) )
            {
                ddlMedium.Items.Add( new ListItem( "SMS", CommunicationType.SMS.ConvertToInt().ToString() ) );
            }
            if ( activeMediums.Contains( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() ) )
            {
                ddlMedium.Items.Add( new ListItem( "Push Notification", CommunicationType.PushNotification.ConvertToInt().ToString() ) );
            }

            ddlMedium.SetValue( rFilter.GetFilterPreference( FilterSettingName.Medium ) );

            // Status.
            // "Opened" status is displayed as "Interacted".
            ddlStatus.Items.Clear();
            ddlStatus.Items.Add( new ListItem() );
            ddlStatus.Items.Add( new ListItem( CommunicationRecipientStatus.Cancelled.ToString(), CommunicationRecipientStatus.Cancelled.ConvertToInt().ToString() ) );
            ddlStatus.Items.Add( new ListItem( CommunicationRecipientStatus.Delivered.ToString(), CommunicationRecipientStatus.Delivered.ConvertToInt().ToString() ) );
            ddlStatus.Items.Add( new ListItem( CommunicationRecipientStatus.Failed.ToString(), CommunicationRecipientStatus.Failed.ConvertToInt().ToString() ) );
            ddlStatus.Items.Add( new ListItem( "Interacted", CommunicationRecipientStatus.Opened.ConvertToInt().ToString() ) );
            ddlStatus.Items.Add( new ListItem( CommunicationRecipientStatus.Pending.ToString(), CommunicationRecipientStatus.Pending.ConvertToInt().ToString() ) );
            ddlStatus.Items.Add( new ListItem( CommunicationRecipientStatus.Sending.ToString(), CommunicationRecipientStatus.Sending.ConvertToInt().ToString() ) );

            ddlStatus.SelectedValue = rFilter.GetFilterPreference( FilterSettingName.Status );

            // Created By
            ppCreatedBy.PersonId = rFilter.GetFilterPreference( FilterSettingName.CreatedBy ).AsIntegerOrNull();

            if ( !ppCreatedBy.PersonId.HasValue )
            {
                ppCreatedBy.SetValue( null );
            }

            // Send Date
            drpDates.DelimitedValues = rFilter.GetFilterPreference( FilterSettingName.SendDateRange );

            // System Communication Template
            LoadSystemCommunicationTemplatesSelectionList( rockContext );

            ddlSystemCommunicationType.SetValue( rFilter.GetFilterPreference( FilterSettingName.SystemCommunicationType ) );

            // Is Bulk?
            ddlBulk.Items.Clear();
            ddlBulk.Items.Add( new ListItem() );
            ddlBulk.Items.Add( new ListItem( "Bulk Messages Only", "Bulk" ) );
            ddlBulk.Items.Add( new ListItem( "Non-bulk Messages Only", "NotBulk" ) );

            ddlBulk.SetValue( rFilter.GetFilterPreference( FilterSettingName.BulkStatus ) );

            // Communication Template
            LoadCommunicationTemplatesSelectionList( rockContext );

            ddlTemplate.SetValue( rFilter.GetFilterPreference( FilterSettingName.CommunicationTemplate ) );
        }

        /// <summary>
        /// Binds the Communication List grid.
        /// </summary>
        private void BindGrid()
        {
            // Configure the grid.
            int personId;

            if ( _person == null )
            {
                gCommunication.EmptyDataText = "There is no Person in the current context for this block.";
                personId = 0;
            }
            else
            {
                personId = _person.Id;
                gCommunication.EntityTypeId = EntityTypeCache.Get<Rock.Model.Communication>().Id;
            }

            // Get the grid data source for the current filter settings.
            bool? isBulk = null;

            if ( ddlBulk.SelectedValue == "Bulk" )
            {
                isBulk = true;
            }
            else if ( ddlBulk.SelectedValue == "NotBulk" )
            {
                isBulk = false;
            }

            /*
             * Retrieving list items can be an expensive process for large data sets, particularly as we need to check the View permissions for the current user.
             * To ensure this process is scalable, we identify the candidate Communication records for the current page and then retrieve only
             * the extended data set for those records.
             */
            var rockContext = new RockContext();

            var qryCommunications = GetCommunicationQuery( rockContext,
                personId,
                tbSubject.Text,
                ddlMedium.SelectedValueAsEnumOrNull<CommunicationType>(),
                ppCreatedBy.PersonId,
                ddlStatus.SelectedValue.ConvertToEnumOrNull<CommunicationRecipientStatus>(),
                drpDates.LowerValue,
                drpDates.UpperValue,
                ddlSystemCommunicationType.SelectedValue.AsIntegerOrNull(),
                ddlTemplate.SelectedValue.AsIntegerOrNull(),
                isBulk );

            // Get the total count of available Recipient records.
            // Note that some Communications may have multiple recipient records for the same person.
            var qryCommunicationRecipients = GetCommunicationListItemsQuery( rockContext, qryCommunications, personId );

            var totalCount = qryCommunicationRecipients.Count();

            // Paginate the query.
            qryCommunications = qryCommunications
                .Skip( gCommunication.PageIndex * gCommunication.PageSize )
                .Take( gCommunication.PageSize );

            qryCommunicationRecipients = GetCommunicationListItemsQuery( rockContext, qryCommunications, personId );

            var items = GetCommunicationListItems( rockContext, qryCommunicationRecipients );

            // Bind the grid data.
            InitializeDataBindingServices();

            gCommunication.VirtualItemCount = totalCount;
            gCommunication.DataSource = items;

            gCommunication.DataBind();

            upPanel.Update();
        }

        private void InitializeDataBindingServices()
        {
            // Initialize the services and data used during the Grid data binding process.
            var rockContext = new RockContext();

            _gridPersonService = _gridPersonService ?? new PersonService( rockContext );
            _gridDataViewService = _gridDataViewService ?? new DataViewService( rockContext );
            _gridBinaryFileService = _gridBinaryFileService ?? new BinaryFileService( rockContext );

            _currentPersonAliasIdList = this.CurrentPerson?.Aliases.Select( p => p.Id ).ToList() ?? new List<int>();
        }

        private IQueryable<CommunicationRecipientListQueryItem> GetCommunicationListItemsQuery( RockContext rockContext, IQueryable<Rock.Model.Communication> qryCommunications, int personId )
        {
            // Get the set of Recipient records for the context person.
            var qryRecipients = new CommunicationRecipientService( rockContext ).Queryable()
                .Where( x => x.PersonAlias.PersonId == personId );

            // Get the set of list entries for the specified page.
            var queryListItems = qryCommunications
                .Join( qryRecipients, c => c.Id, r => r.CommunicationId, ( c, r ) => new CommunicationRecipientListQueryItem { Communication = c, Recipient = r } );


            return queryListItems;
        }

        /// <summary>
        /// Gets a page of list items for the Communications List.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="qryCommunicationRecipients"></param>
        /// <returns></returns>
        private List<CommunicationListItem> GetCommunicationListItems( RockContext rockContext, IQueryable<CommunicationRecipientListQueryItem> qryCommunicationRecipients )
        {
            var qryListItems = qryCommunicationRecipients
                .OrderByDescending( c => c.Communication.CreatedDateTime )
                .AsNoTracking()
                .Select( ciGroup =>
                    new CommunicationListItem
                    {
                        RowId = "C" + ciGroup.Communication.Id + "R" + ciGroup.Recipient.Id,
                        Id = ciGroup.Communication.Id,
                        CommunicationType = ciGroup.Communication.CommunicationType,
                        CommunicationStatus = ciGroup.Communication.Status,
                        Title = ciGroup.Communication.CommunicationType == CommunicationType.Email ? ciGroup.Communication.Subject
                            : ciGroup.Communication.CommunicationType == CommunicationType.SMS ? ciGroup.Communication.Name
                            : ciGroup.Communication.CommunicationType == CommunicationType.PushNotification ? ciGroup.Communication.PushTitle
                            : ciGroup.Communication.Name,
                        CreatedDateTime = ciGroup.Communication.CreatedDateTime,
                        SendDateTime = ciGroup.Communication.SendDateTime ?? ciGroup.Communication.FutureSendDateTime ?? ciGroup.Communication.CreatedDateTime,
                        Sender = ciGroup.Communication.SenderPersonAlias != null ? ciGroup.Communication.SenderPersonAlias.Person : null,
                        RecipientStatus = ciGroup.Recipient.Status,
                        RecipientStatusNote = ciGroup.Recipient.StatusNote,
                        CreatedByPersonAliasId = ciGroup.Communication.CreatedByPersonAliasId,
                        RecipientTotal = ciGroup.Communication.Recipients.Count,
                        InternalCommunicationMediumId = ciGroup.Recipient.MediumEntityTypeId
                    } );

            var items = qryListItems.ToList();

            GetAdditionalCommunicationListItemInfo( items );

            return items;
        }

        /// <summary>
        /// Retrieve the details associated with a Communication List Item.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="qryCommunications"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        private CommunicationListItemDetail GetCommunicationListItemDetail( RockContext rockContext, int communicationId, int personId )
        {
            var communicationService = new CommunicationService( rockContext );
            var qryCommunications = communicationService.Queryable().Where( x => x.Id == communicationId );

            // Get the set of Recipient records for the context person.
            var qryRecipients = new CommunicationRecipientService( rockContext ).Queryable()
                .Where( x => x.PersonAlias.PersonId == personId );

            // Get the details of the specific message sent to this Communication Recipient.
            var qryItems = qryCommunications
                .Join( qryRecipients, c => c.Id, r => r.CommunicationId, ( c, r ) => new { Communication = c, Recipient = r } )
                .Select( ciGroup =>
                        new CommunicationListItemDetail
                        {
                            CommunicationId = ciGroup.Communication.Id,
                            RecipientId = ciGroup.Recipient.Id,
                            CommunicationListId = ciGroup.Communication.ListGroupId,
                            CommunicationListName = ciGroup.Communication.ListGroupId == null ? null : ciGroup.Communication.ListGroup.Name,
                            CommunicationTemplateId = ciGroup.Communication.CommunicationTemplateId,
                            CommunicationTemplateName = ciGroup.Communication.CommunicationTemplateId == null ? null : ciGroup.Communication.CommunicationTemplate.Name,
                            SenderName = ciGroup.Communication.FromName,
                            InternalSenderEmail = ciGroup.Communication.FromEmail,
                            InternalSenderSmsName = ciGroup.Communication.SmsFromSystemPhoneNumber.Name,
                            InternalSenderSmsNumber = ciGroup.Communication.SmsFromSystemPhoneNumber.Number,
                            InternalPushImageFileId = ciGroup.Communication.PushImageBinaryFileId,
                            InternalPushData = ciGroup.Communication.PushData,
                            InternalAttachments = ciGroup.Communication.Attachments.Select( x => new CommunicationAttachmentInfo { BinaryFileId = x.BinaryFileId, CommunicationType = x.CommunicationType } ).ToList(),
                            RecipientName = ciGroup.Recipient.PersonAlias.Person.NickName + " " + ciGroup.Recipient.PersonAlias.Person.LastName,
                            RecipientAddress = ciGroup.Recipient.PersonAlias.Person.Email,
                            /* 
                             * [2021-10-08] DJL - The SentMessage field may not be populated if the message failed to send,
                             * depending on which specific transport processed the message.
                             */
                            Message = ciGroup.Recipient.SentMessage ??
                              ( ciGroup.Communication.CommunicationType == CommunicationType.SMS ? ciGroup.Communication.SMSMessage
                                : ciGroup.Communication.CommunicationType == CommunicationType.PushNotification ? ciGroup.Communication.PushMessage
                                : ciGroup.Communication.Message ),
                            CommunicationSegmentInclusionType = ciGroup.Communication.SegmentCriteria.ToString(),
                            InternalCommunicationSegmentData = ciGroup.Communication.Segments,
                            ApplicationName = ciGroup.Recipient.PersonalDevice.Site.Name
                        }
                    );

            var item = qryItems.FirstOrDefault();

            // Retrieve the interactions linked to this Communication Recipient record.
            // We need to be careful to take advantage of available indexes here, because this has the potential to be an expensive operation
            // if there are a very large number of interactions.
            var interactionsService = new InteractionService( rockContext );

            var communicationChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );

            var interactions = interactionsService.Queryable()
                .Where( i => i.InteractionComponent.EntityId == communicationId
                             && i.InteractionComponent.InteractionChannelId == communicationChannelId
                             && i.EntityId == item.RecipientId )
                .Select( x => new CommunicationItemActivity
                {
                    Id = x.Id,
                    DateTime = x.InteractionDateTime,
                    Name = x.Operation,
                    DeviceDescription = x.InteractionSession.DeviceType.DeviceTypeData,
                    InternalInteraction = x
                } )
                .ToList();

            foreach ( var interaction in interactions )
            {
                interaction.Details = CommunicationRecipient.GetInteractionDetails( interaction.InternalInteraction );
            }

            item.Activities = interactions;
            item.PersonalInteractionCount = interactions.Count;

            return item;
        }

        /// <summary>
        /// Get the query that represents that candidate Communication records according to the current filter settings and user permissions.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="subject"></param>
        /// <param name="communicationType"></param>
        /// <param name="createdByPersonId"></param>
        /// <param name="communicationStatus"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="systemCommunicationTypeId"></param>
        /// <param name="communicationTemplateId"></param>
        /// <param name="isBulkCommunication"></param>
        /// <returns></returns>
        private IQueryable<Rock.Model.Communication> GetCommunicationQuery( RockContext rockContext, int personId, string subject, CommunicationType? communicationType, int? createdByPersonId, CommunicationRecipientStatus? communicationStatus, DateTime? startDate, DateTime? endDate, int? systemCommunicationTypeId, int? communicationTemplateId, bool? isBulkCommunication )
        {
            // Get the base query, excluding items that are current being processed.
            var qryCommunications = new CommunicationService( rockContext ).Queryable().Where( c => c.Status != CommunicationStatus.Transient );

            // Apply Filter: Subject
            if ( !string.IsNullOrWhiteSpace( subject ) )
            {
                qryCommunications = qryCommunications.Where( c => ( c.Name != null && c.Name.Contains( subject ) )
                    || ( c.Subject != null && c.Subject.Contains( subject ) )
                    || ( c.PushTitle != null && c.PushTitle.Contains( subject ) ) );
            }

            // Apply Filter: Medium
            if ( communicationType != null )
            {
                qryCommunications = qryCommunications.Where( c => c.CommunicationType == communicationType );
            }

            // Apply Filter: CreatedBy
            if ( createdByPersonId.HasValue )
            {
                qryCommunications = qryCommunications.Where( c => c.CreatedByPersonAlias.PersonId == createdByPersonId.Value );
            }

            // Apply Filter: Sent Date Range
            if ( startDate.HasValue )
            {
                qryCommunications = qryCommunications.Where( a => a.SendDateTime >= startDate.Value );
            }

            if ( endDate.HasValue )
            {
                var upperDate = endDate.Value.Date.AddDays( 1 );

                qryCommunications = qryCommunications.Where( a => a.SendDateTime < upperDate );
            }

            // Apply Filter: System Communication Type
            if ( systemCommunicationTypeId.HasValue )
            {
                qryCommunications = qryCommunications.Where( c => c.SystemCommunicationId == systemCommunicationTypeId );
            }

            // Apply Filter: Communication Template
            if ( communicationTemplateId.HasValue )
            {
                qryCommunications = qryCommunications.Where( c => c.CommunicationTemplateId == communicationTemplateId );
            }

            // Apply Filter: Bulk Status
            if ( isBulkCommunication.HasValue )
            {
                qryCommunications = qryCommunications.Where( c => c.IsBulkCommunication == isBulkCommunication.Value );
            }

            // Apply Filters: Person/Communication Recipient Status
            qryCommunications = qryCommunications
                .Where( c =>
                    c.Recipients.Any( a =>
                        a.PersonAlias.PersonId == personId && ( communicationStatus == null || a.Status == communicationStatus ) ) );

            // Apply security settings for the current user.
            qryCommunications = qryCommunications
                .WherePersonAuthorizedToView( rockContext, this.CurrentPerson );

            // Apply the sort order.
            qryCommunications = qryCommunications.OrderByDescending( c => c.CreatedDateTime );

            return qryCommunications;
        }

        /// <summary>
        /// Load the Communication Templates selection list with templates that the current user is authorized to view.
        /// </summary>
        private void LoadCommunicationTemplatesSelectionList( RockContext rockContext )
        {
            var selectedValue = ddlTemplate.SelectedValue;

            ddlTemplate.Items.Clear();
            ddlTemplate.Items.Add( new ListItem( string.Empty, string.Empty ) );

            var templateService = new CommunicationTemplateService( rockContext );

            var templates = templateService.Queryable()
                .AsNoTracking()
                .Where( a => a.IsActive )
                .OrderBy( t => t.Name )
                .ToList();

            foreach ( var template in templates )
            {
                if ( template.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                {
                    var li = new ListItem( template.Name, template.Id.ToString() );

                    if ( template.Id.ToString() == selectedValue )
                    {
                        li.Selected = true;
                    }

                    ddlTemplate.Items.Add( li );
                }
            }
        }

        /// <summary>
        /// Load the System Communication Templates selection list with templates that the current user is authorized to view.
        /// </summary>
        private void LoadSystemCommunicationTemplatesSelectionList( RockContext rockContext )
        {
            var systemCommunicationService = new SystemCommunicationService( rockContext );

            var systemCommunications = systemCommunicationService.Queryable()
                .ToList()
                .Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                .OrderBy( e => e.Title );

            ddlSystemCommunicationType.Items.Clear();
            ddlSystemCommunicationType.Items.Add( new ListItem() );

            if ( systemCommunications.Any() )
            {
                ddlSystemCommunicationType.Items.AddRange( systemCommunications.Select( x => new ListItem { Text = x.Title, Value = x.Id.ToString() } ).ToArray() );
            }
        }

        /// <summary>
        /// Get the markup for a Communication list item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string GetCommunicationListItemHtml( Control rowContainerControl, CommunicationListItem item, bool includeDetailInfo )
        {
            if ( includeDetailInfo )
            {
                GetAdditionalCommunicationEntryData( item );
            }

            var mergeValues = new Dictionary<string, object>();

            mergeValues.Add( "Communication", item );

            // Add Page Links.
            AddMergeFieldForPageLink( mergeValues, "DetailUrl", LinkedPageUrl( AttributeKey.CommunicationDetailPage ), $"CommunicationId={item.Id}" );
            AddMergeFieldForPageLink( mergeValues, "ListSegmentDetailUrlTemplate", LinkedPageUrl( AttributeKey.CommunicationSegmentDetailPage ), "DataViewId=@segmentId" );

            if ( includeDetailInfo )
            {
                AddMergeFieldForPageLink( mergeValues, "TemplateDetailUrl", LinkedPageUrl( AttributeKey.CommunicationTemplateDetailPage ), $"TemplateId={item.Detail.CommunicationTemplateId}" );
                AddMergeFieldForPageLink( mergeValues, "ListDetailUrl", LinkedPageUrl( AttributeKey.CommunicationListDetailPage ), $"GroupId={item.Detail.CommunicationListId}" );
            }

            // Create the "More" PostBack link and arguments.
            var args = new CommunicationDetailPostbackArgs
            {
                Action = "ShowDetail",
                CommunicationId = item.Id,
                DetailContainerControlId = rowContainerControl.ClientID
            };

            var argsString = WebUtility.UrlEncode( args.ToJson() );

            var postbackLink = Page.ClientScript.GetPostBackClientHyperlink( this.upPanel, argsString, false );

            mergeValues.Add( "ShowDetailPostBackEventReference", postbackLink );
            mergeValues.Add( "HasDetail", includeDetailInfo );

            var output = LavaService.RenderTemplate( _communicationItemLavaTemplate, mergeValues );

            return output.Text;
        }

        /// <summary>
        /// Add an entry for a linked page to the Lava merge objects dictionary.
        /// </summary>
        /// <param name="mergeValues"></param>
        /// <param name="pageLinkKey"></param>
        /// <param name="pageLinkUrl"></param>
        /// <param name="pageLinkQueryParameters"></param>
        private void AddMergeFieldForPageLink( Dictionary<string, object> mergeValues, string pageLinkKey, string pageLinkUrl, string pageLinkQueryParameters )
        {
            if ( !string.IsNullOrWhiteSpace( pageLinkUrl ) )
            {
                var url = pageLinkUrl;

                if ( !string.IsNullOrWhiteSpace( pageLinkQueryParameters ) )
                {
                    url += $"?{pageLinkQueryParameters}";
                }

                mergeValues.Add( pageLinkKey, url );
            }
        }

        /// <summary>
        /// Retrieve additional information about a Communication list item that is too complex/expensive to include in the initial query.
        /// </summary>
        /// <param name="info"></param>
        private void GetAdditionalCommunicationListItemInfo( List<CommunicationListItem> items )
        {
            // Resolve the specific CommunicationType if the Communication specifies user preference.
            var itemsWithRecipientPreference = items.Where( x => x.CommunicationType == CommunicationType.RecipientPreference );

            foreach ( var item in itemsWithRecipientPreference )
            {
                item.CommunicationType = _mediumEntityIdToCommunicationTypeMap.GetValueOrDefault( item.InternalCommunicationMediumId.GetValueOrDefault(), CommunicationType.Email );

                item.AllowRecipientPreference = true;
            }
        }

        /// <summary>
        /// Retrieve additional information about a Communication list item that is too complex/expensive to include in the initial query.
        /// </summary>
        /// <param name="info"></param>
        private void GetAdditionalCommunicationEntryData( CommunicationListItem info )
        {
            // Set the View permission for the communication detail.
            // The current person should have view permission for the communication if:
            // 1. They are the creator or sender; or
            // 2. They have Edit permission for this block.
            var senderPrimaryAliasId = info.Sender?.PrimaryAliasId ?? 0;

            info.ViewDetailIsAllowed = UserCanEdit
                || _currentPersonAliasIdList.Contains( info.CreatedByPersonAliasId.GetValueOrDefault() )
                || _currentPersonAliasIdList.Contains( info.Sender?.PrimaryAliasId ?? 0 );

            // Get Communication List Segments.
            if ( !string.IsNullOrWhiteSpace( info?.Detail?.InternalCommunicationSegmentData ) )
            {
                var segmentGuidList = info.Detail.InternalCommunicationSegmentData.Split( ',' ).Select( x => x.AsGuidOrNull() );

                var segments = _gridDataViewService.Queryable()
                    .Where( x => segmentGuidList.Contains( x.Guid ) )
                    .Select( x => new CommunicationSegment { Id = x.Id, Name = x.Name } )
                    .ToList();

                info.Detail.CommunicationSegments = segments;
            }

            // Set Sender details.
            if ( info.CommunicationType == CommunicationType.SMS )
            {
                info.Detail.SenderName = info.Detail.InternalSenderSmsName;
                info.Detail.SenderAddress = PhoneNumber.FormattedNumber( string.Empty, info.Detail.InternalSenderSmsNumber );
            }
            else if ( info.CommunicationType == CommunicationType.Email )
            {
                info.Detail.SenderAddress = info.Detail.InternalSenderEmail;
            }

            // Set the Application Name for a Push Notification.
            // This is the name of the Rock site that generated the notification.
            if ( info.CommunicationType == CommunicationType.PushNotification )
            {
                if ( !string.IsNullOrWhiteSpace( info.Detail.InternalPushData ) )
                {
                    var pushData = info.Detail.InternalPushData.FromJsonOrNull<Rock.Communication.PushData>();
                    if ( pushData != null )
                    {
                        var site = SiteCache.Get( pushData.MobileApplicationId.GetValueOrDefault() );
                        info.Detail.ApplicationName = site?.Name;
                    }
                }
            }

            // Create URLs for attachments to be accessed via a web service call, to ensure that the content is accessible regardless of where it is stored.
            if ( info.CommunicationType == CommunicationType.PushNotification )
            {
                // Resolve the URL for a Push notification image file.
                if ( info.Detail.InternalPushImageFileId != null )
                {
                    var file = _gridBinaryFileService.Get( info.Detail.InternalPushImageFileId.ToIntSafe( 0 ) );
                    if ( file != null )
                    {
                        info.Detail.Attachments = new List<string>() { FileUrlHelper.GetImageUrl( file.Id ) };
                    }
                }
            }
            else
            {
                // Resolve the URL for file attachments.
                if ( info.Detail.InternalAttachments != null )
                {
                    info.Detail.Attachments = new List<string>();

                    foreach ( var attachment in info.Detail.InternalAttachments.Where( x => x.CommunicationType == info.CommunicationType ) )
                    {
                        var file = _gridBinaryFileService.Get( attachment.BinaryFileId );
                        if ( file != null )
                        {
                            info.Detail.Attachments.Add( FileUrlHelper.GetFileUrl( file.Id ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles postback events for this block.
        /// </summary>
        /// <param name="controlId"></param>
        /// <param name="eventArgument"></param>
        /// <remarks>
        /// Note that we deliberately avoid using IPostBackHandler to process requests for this block, because registering the block
        /// as the postback target has the unwanted side-effect of causing the entire grid to refresh when loading the detail for a single row.
        /// </remarks>
        public void HandlePostBack( string controlId, string eventArgument )
        {
            var argsString = WebUtility.UrlDecode( eventArgument );

            var args = argsString.FromJsonOrNull<CommunicationDetailPostbackArgs>();
            if ( args == null )
            {
                return;
            }

            if ( args.Action == "ShowDetail" )
            {
                InitializeDataBindingServices();

                var communicationId = args.CommunicationId;
                if ( communicationId == null )
                {
                    return;
                }

                var rockContext = new RockContext();

                var communicationService = new CommunicationService( rockContext );
                var qryCommunication = communicationService.Queryable()
                    .Where( x => x.Id == communicationId );

                var qryCommunicationRecipients = GetCommunicationListItemsQuery( rockContext, qryCommunication, _person.Id );

                // Get the Communication List Item, then load the additional detail for the panel.
                var communicationItem = GetCommunicationListItems( rockContext, qryCommunicationRecipients )
                    .FirstOrDefault();

                communicationItem.Detail = GetCommunicationListItemDetail( rockContext, communicationId.Value, _person.Id );

                var ctlContainer = FindControlRecursive( gCommunication, args.DetailContainerControlId ) as UpdatePanel;

                var lr = FindControlRecursive( ctlContainer, "lCommunicationDetailRow" ) as Literal;
                if ( lr != null )
                {
                    var lavaHeader = GetCommunicationListItemHtml( ctlContainer, communicationItem, true );

                    lr.Text = lavaHeader;

                    ctlContainer.Update();
                }
            }
        }

        /// <summary>
        /// Finds the first control with the specified identifier, searching from the specified top-level control recursively.
        /// </summary>
        /// <param name="control">The top-level control to begin the search.</param>
        /// <param name="id">The control identifier.</param>
        /// <returns></returns>
        private System.Web.UI.Control FindControlRecursive( System.Web.UI.Control control, string id )
        {
            if ( control != null )
            {
                if ( control.ClientID == id || control.ID.EndsWith( id ) )
                {
                    return control;
                }

                if ( control.HasControls() )
                {
                    foreach ( var childControl in control.Controls )
                    {
                        var foundCtl = FindControlRecursive( ( System.Web.UI.Control ) childControl, id );

                        if ( foundCtl != null )
                        {
                            return foundCtl;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Interim storage for a query result.
        /// </summary>
        private class CommunicationRecipientListQueryItem
        {
            public Rock.Model.Communication Communication;
            public Rock.Model.CommunicationRecipient Recipient;
        }

        /// <summary>
        /// Information about a Communication for a specific recipient that is intended for display in a list.
        /// </summary>
        private class CommunicationListItem : RockDynamic
        {
            /// <summary>
            /// Can the current user view the detail of this communication?
            /// </summary>
            public bool ViewDetailIsAllowed { get; set; }

            /// <summary>
            /// A unique identifier for this list item.
            /// </summary>
            public string RowId { get; set; }

            /// <summary>
            /// The identifier of the communication.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The type of communication.
            /// </summary>
            public CommunicationType CommunicationType { get; set; }

            /// <summary>
            /// Indicates if this communication was configured to send to the preferred medium for each recipient.
            /// </summary>
            public bool AllowRecipientPreference { get; set; }

            /// <summary>
            /// The date and time on which this communication was or will be sent.
            /// </summary>
            public DateTime? SendDateTime { get; set; }

            /// <summary>
            /// A descriptive title for the communication.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// The person who originated this communication.
            /// </summary>
            public Person Sender { get; set; }

            /// <summary>
            /// The status of this specific recipient of the communication.
            /// </summary>
            public CommunicationRecipientStatus RecipientStatus { get; set; }

            /// <summary>
            /// Additional detail about the recipient status for this communication.
            /// </summary>
            public string RecipientStatusNote { get; set; }

            /// <summary>
            /// The status of the Communication.
            /// </summary>
            public CommunicationStatus CommunicationStatus { get; set; }

            /// <summary>
            /// The person who created this communication.
            /// </summary>
            public int? CreatedByPersonAliasId { get; set; }

            /// <summary>
            /// The date and time on which this communication was created.
            /// </summary>
            public DateTime? CreatedDateTime { get; set; }

            /// <summary>
            /// The total number of intended recipients for this communication.
            /// </summary>
            public int RecipientTotal { get; set; }

            /// <summary>
            /// Additional details about the communication that do not form part of the main list entry.
            /// </summary>
            public CommunicationListItemDetail Detail { get; set; }

            #region Internal Fields used to store data for further processing.

            public int? InternalCommunicationMediumId { get; set; }

            #endregion
        }

        /// <summary>
        /// Additional details associated with a specific Communication list item.
        /// </summary>
        protected class CommunicationListItemDetail : RockDynamic
        {
            /// <summary>
            /// The unique identifier of the Communication.
            /// </summary>
            public int CommunicationId { get; set; }

            /// <summary>
            /// The unique identifier of the Communication Recipient record.
            /// </summary>
            public int RecipientId { get; set; }

            /// <summary>
            /// The number of interactions with the communication recorded for the context person.
            /// </summary>
            public int PersonalInteractionCount { get; set; }

            /// <summary>
            /// The unique identifier of the Communication List used to generate this communication instance, if any.
            /// </summary>
            public int? CommunicationListId { get; set; }

            /// <summary>
            /// The name of the Communication List used to generate this communication instance, if any.
            /// </summary>
            public string CommunicationListName { get; set; }

            /// <summary>
            /// The Communication List Segments used to determine the set of communication recipients.
            /// </summary>
            public List<CommunicationSegment> CommunicationSegments { get; set; }

            /// <summary>
            /// Indicates the way in which Communication Segments have been combined to determine the set of communication recipients.
            /// </summary>
            public string CommunicationSegmentInclusionType { get; set; }

            /// <summary>
            /// The unique identifier of the Communication Template used to generate this communication, if any.
            /// </summary>
            public int? CommunicationTemplateId { get; set; }

            /// <summary>
            /// The name of the Communication Template used to generate this communication, if any.
            /// </summary>
            public string CommunicationTemplateName { get; set; }

            /// <summary>
            /// The main content of the communication.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// The name of the communication sender.
            /// </summary>
            public string SenderName { get; set; }

            /// <summary>
            /// The medium-specific address used to identify the communication sender.
            /// The form of the address is determined by the communication type: phone number, email address, etc.
            /// </summary>
            public string SenderAddress { get; set; }

            /// <summary>
            /// The name of the communication recipient.
            /// </summary>
            public string RecipientName { get; set; }

            /// <summary>
            /// The medium-specific address used to identify the communication recipient.
            /// The form of the address is determined by the communication type: phone number, email address, etc.
            /// </summary>
            public string RecipientAddress { get; set; }

            /// <summary>
            /// The collection of activities recorded for this communication.
            /// </summary>
            public List<CommunicationItemActivity> Activities { get; set; }

            /// <summary>
            /// A collection of URLs identifying the file attachments associated with this communication
            /// </summary>
            public List<string> Attachments { get; set; }

            /// <summary>
            /// The name of the application used to generate this communication.
            /// </summary>
            public string ApplicationName { get; set; }

            #region Internal Fields used to store data for further processing.

            internal string InternalCommunicationSegmentData { get; set; }

            internal string InternalSenderEmail { get; set; }

            internal string InternalSenderSmsName { get; set; }

            internal string InternalSenderSmsNumber { get; set; }

            internal int? InternalCommunicationMediumId { get; set; }

            internal int? InternalPushImageFileId { get; set; }

            internal string InternalPushData { get; set; }

            internal List<CommunicationAttachmentInfo> InternalAttachments { get; set; }

            #endregion
        }

        /// <summary>
        /// Information about an attachment for a communication.
        /// </summary>
        /// <remarks>This type does not inherit from RockDynamic because it is not Lava-accessible.</remarks>
        protected class CommunicationAttachmentInfo
        {
            /// <summary>
            /// The type of communication.
            /// </summary>
            public CommunicationType CommunicationType { get; set; }

            /// <summary>
            /// The unique identifier of the binary file object that stores the content of the attachment.
            /// </summary>
            public int BinaryFileId { get; set; }
        }

        /// <summary>
        /// Detail about an Activity associated with a Communication Item.
        /// </summary>
        protected class CommunicationItemActivity : RockDynamic
        {
            /// <summary>
            /// The unique identifier of the interaction which this activity represents.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The name of the activity performed.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Specific information about the activity.
            /// </summary>
            public string Details { get; set; }

            /// <summary>
            /// A description of the device used to perform the interaction.
            /// </summary>
            public string DeviceDescription { get; set; }

            /// <summary>
            /// The date and time when the activity was performed.
            /// </summary>
            public DateTime? DateTime { get; set; }

            #region Internal Fields used to store data for further processing.

            internal Interaction InternalInteraction { get; set; }

            #endregion
        }

        /// <summary>
        /// A Communication List Segment associated with a Communication Item.
        /// </summary>
        protected class CommunicationSegment : RockDynamic
        {
            /// <summary>
            /// The unique identifier of the Communication Segment.
            /// This is a reference to a Data View that defines the candidates referenced by this segment.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The name of the Communication List segment.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// The postback arguments of a request for Comunication List Item details.
        /// </summary>
        private class CommunicationDetailPostbackArgs
        {
            public string Action { get; set; }

            public int? CommunicationId { get; set; }

            public string DetailContainerControlId { get; set; }
        }

        #endregion
    }
}