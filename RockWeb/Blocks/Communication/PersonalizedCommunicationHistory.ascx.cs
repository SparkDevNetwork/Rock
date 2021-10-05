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

    public partial class PersonalizedCommunicationHistory : RockBlock, IPostBackEventHandler
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

        private const string _communicationItemLavaTemplate = @"
<table class='grid-table'>
    <tbody>
        <tr class='communication-item'>
            <td class='d-none d-sm-table-cell w-1 align-middle pr-0'>
                <div class='avatar avatar-lg avatar-icon'>
                    {% if Communication.CommunicationType == 'Email' %}
                        <i class='fa fa-envelope'></i>
                    {% elseif Communication.CommunicationType == 'SMS' %}
                        <i class='fa fa-comment-alt'></i>
                    {% elseif Communication.CommunicationType == 'PushNotification' %}
                        <i class='fa fa-mobile-alt'></i>
                    {% else %}
                        <i class='fa fa-question-circle'></i>
                    {% endif %}
                </div>
            </td>
            <td class='leading-snug'>
                <span class='d-block mb-1'>{{ Communication.Title }}</span>
                <span class='d-block text-sm text-muted mb-1'>{{ Communication.Sender.FullName }}</span>
                {% capture moreHtml %}<i class=&quot;fa fa-xs fa-chevron-right&quot;></i> More{% endcapture %}
                {% capture lessHtml %}<i class=&quot;fa fa-xs fa-chevron-up&quot;></i> Less{% endcapture %}
                {% if HasDetail == true %}
                    <a href='#' class='text-xs py-2 px-0' onclick=""toggleCommunicationDetail(this,'{{ Communication.RowId }}','{{ moreHtml }}','{{ lessHtml }}');return false;"">{{ lessHtml | HtmlDecode }}</a>
                {% else %}
                    <a href=""{{ ShowDetailPostBackEventReference }}"" class='text-xs py-2 px-0' onclick=""toggleCommunicationDetail(this,'{{ Communication.RowId }}','{{ moreHtml }}','{{ lessHtml }}');return true;"">{{ moreHtml | HtmlDecode }}</a>
                {% endif %}
            </td>
            <td class='w-1 align-middle text-right d-none d-sm-table-cell'>
                <span class='badge badge-info' data-toggle='tooltip' data-placement='top' title='{{ Communication.RecipientTotal }} recipients'>{{ Communication.RecipientTotal }}</span>
            </td>
            <td class='w-1 text-right'>
                <span class='d-block text-sm text-muted mb-1 text-nowrap'>{{ Communication.SendDateTimeDescription }}</span>
                {% if Communication.RecipientStatus == 'Delivered' %}
                    <span class='label label-info' data-toggle='tooltip' data-placement='top' title='Sent on {{ Communication.SendDateTime | Date:'dd-MM-yyyy' }} at {{ Communication.SendDateTime | Date:'hh:mmtt' }}'>Delivered</span>
                {% elseif Communication.RecipientStatus == 'Failed' %}
                    <span class='label label-danger' data-toggle='tooltip' data-placement='top' title='{{ Communication.RecipientStatusNote }}'>Failed</span>
                {% elseif Communication.RecipientStatus == 'Cancelled' %}
                    <span class='label label-warning'>Cancelled</span>
                {% elseif Communication.RecipientStatus == 'Opened' %}
                    <span class='label label-success'>Interacted</span>
                {% elseif Communication.RecipientStatus == 'Pending' %}
                    {% if Communication.CommunicationStatus == 'Approved' %}
                        <span class='label label-default' data-toggle='tooltip' data-placement='top' title='Scheduled for {{ Communication.SendDateTime | Date:'dd-MM-yyyy' }} at {{ Communication.SendDateTime | Date:'hh:mmtt' }}'>Pending</span>
                    {% elseif Communication.CommunicationStatus == 'PendingApproval' %}
                        <span class='label label-default' data-toggle='tooltip' data-placement='top' title='Pending Approval, Scheduled for {{ Communication.SendDateTime | Date:'dd-MM-yyyy' }} at {{ Communication.SendDateTime | Date:'hh:mmtt' }}'>Pending</span>
                    {% elseif Communication.CommunicationStatus == 'Denied' %}
                        <span class='label label-default' data-toggle='tooltip' data-placement='top' title='Approval Declined'>Pending</span>
                    {% endif %}
                {% else %}
                    <span class='label label-default'>{{ Communication.RecipientStatus }}</span>
                {% endif %}
            </td>
        </tr>
        {% if HasDetail == true %}
            <tr class='communication-details'>
                <td class='d-none d-sm-table-cell border-0 py-0'></td>
                <td class='border-0 py-0' colspan='3'>
                    <div id='details-{{ Communication.RowId }}' class='pb-5'>
                        <div class='row'>
                            <div class='col-md-12 mb-4'><div class='border-top border-panel'></div></div>
                            <div class='col-md-6'>
                                <div class='row'>
                                    <div class='col-xs-6 col-md-4 leading-snug mb-4'>
                                        <span class='control-label d-block text-muted'>Sent As</span>
                                        <span class='d-block text-lg font-weight-bold'>{{ Communication.CommunicationType | Humanize | Capitalize }}</span>
                                    </div>
                                    <div class='col-xs-6 col-md-4 leading-snug mb-4'>
                                        <span class='control-label d-block text-muted'>Recipients</span>
                                        <span class='d-block text-lg font-weight-bold'>{{ Communication.RecipientTotal }}</span>
                                    </div>
                                    <div class='col-xs-6 col-md-4 leading-snug mb-4'>
                                        {% if Communication.Detail.PersonalInteractionCount > 0 %}
                                            <span class='control-label d-block text-muted'>Activity Count</span>
                                            <span class='d-block text-lg font-weight-bold'>{{ Communication.Detail.PersonalInteractionCount }}</span>
                                        {% endif %}
                                    </div>
                                </div>
                                <dl>
                                    {% if Communication.Detail.CommunicationListName != empty %}
                                        <dt>Communication List</dt>
                                        {% if ListDetailUrl != empty %}
                                            <a href=""{{ ListDetailUrl }}"">{{ Communication.Detail.CommunicationListName }}</a></dd>
                                        {% else %}
                                            <dd>{{ Communication.Detail.CommunicationListName }}</dd>
                                        {% endif %}
                                        {% if Communication.Detail.CommunicationSegments != empty %}
                                            <dt>Segments ({{ Communication.Detail.CommunicationSegmentInclusionType }})</dt>
                                            <dd>
                                                {% for segment in Communication.Detail.CommunicationSegments %}
                                                    {% if ListSegmentDetailUrlTemplate != empty %}
                                                        <a href=""{{ ListSegmentDetailUrlTemplate | Replace:'{0}',segment.Id }}"">{{ segment.Name }}</a><br>
                                                    {% else %}
                                                        {{ segment.Name }}<br>
                                                    {% endif %}
                                                {% endfor %}
                                            </dd>
                                            {% if Communication.Detail.CommunicationTemplateName != empty %}
                                                <dt>Communication Template</dt>
                                                {% if TemplateDetailUrl != empty %}
                                                    <dd><a href=""{{ TemplateDetailUrl }}"">{{ Communication.Detail.CommunicationTemplateName }}</a></dd>
                                                {% else %}
                                                    <dd>{{ Communication.Detail.CommunicationTemplateName }}</dd>
                                                {% endif %}
                                            {% endif %}
                                        {% endif %}
                                    {% endif %}
                                </dl>
                            </div>
                            <div class='col-md-6'>
                                <span class='control-label d-block text-muted'>Message Preview</span>
                                {% if Communication.CommunicationType == 'SMS' %}
                                    <div class='card communication-preview'>
                                        <div class='card-heading text-center'><span class='d-block font-weight-semibold'>{{ Communication.Detail.SenderName }}</span> <span class='d-block text-xs text-muted'>{{ Communication.Detail.SenderAddress }}</span></div>
                                        <div class='card-body'>
                                            <div class='sms-bubble'>
                                            {{ Communication.Detail.Message }}
                                            </div>
                                            {% for attachmentUrl in Communication.Detail.Attachments %}
                                                <div class='sms-image'>
                                                    <img src='{{ attachmentUrl }}' alt='' class='img-responsive'>
                                                </div>
                                            {% endfor %}
                                        </div>
                                    </div>
                                {% elseif Communication.CommunicationType == 'PushNotification' %}
                                    <div class='card communication-preview'>
                                        <div class='card-heading'><span class='font-weight-semibold'>{{ Communication.Detail.RecipientName }}</span></div>
                                        <div class='card-body' style='background:#FCFCFC'>
                                            <div class='push-msg'>
                                                <div class='push-msg-header'>
                                                    <div class='push-msg-icon'></div>
                                                    <div class='push-msg-app-name'>{{ Communication.ApplicationName }}</div>
                                                </div>
                                                <div class='push-msg-body'>
                                                    <span class='push-msg-title'>{{ Communication.Title }}</span>
                                                    <span class='push-summary'>{{ Communication.Detail.Message }}</span>
                                                    {% for attachmentUrl in Communication.Detail.Attachments %}
                                                        <div class='sms-image'>
                                                            <img src='{{ attachmentUrl }}' alt='' class='img-responsive'>
                                                        </div>
                                                    {% endfor %}
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                {% else %}
                                    <div class='card communication-preview'>
                                        <div class='card-heading'><span class='font-weight-semibold'>{{ Communication.Detail.SenderName }}</span> <span class='text-muted'>{{ Communication.Detail.SenderEmail }}</span> </div>
                                        <div class='card-heading'>{{ Communication.Title }}</div>
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
                                {% endif %}

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
                                            <th>Details</th>
                                            <th class='w-1'>Date</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {% for item in Communication.Detail.Activities %}
                                            <tr>
                                                <td>
                                                    {{ item.Name }}
                                                    {% if item.Name == 'Click' %}
                                                        <a class='help' href='#' tabindex='-1' data-toggle='tooltip' data-placement='auto' data-container='body' data-html='true' title='' data-original-title='Clicked {{ item.Details }}'><i class='fa fa-info-circle'></i></a>
                                                    {% endif %}
                                                </td>
                                                <td>{{ item.DeviceDescription }}</td>
                                                <td class='w-1 text-nowrap'>{{ item.DateTime | Date:'dd/MM/yyyy hh:mm tt' }}</td>
                                            </tr>
                                        {% endfor %}
                                    </tbody>
                                </table>
                            </div>
                        {% endif %}
                    </div>
                </td>
            </tr>
        {% else %}
            <tr class='communication-details'>
                <td class='d-none d-sm-table-cell border-0 py-0'></td>
                <td class='border-0 py-0' colspan='3'>
                    <div id='details-{{ Communication.RowId }}' class='pb-5' style='display: none;'>
                        <div class='row'>
                            <div class='col-md-12 mb-4'><div class='border-top border-panel'></div></div>
                            <div class='col-md-6'>
                                <div class='row'>
                                    <div class='col-xs-6 col-md-4 leading-snug mb-4'>
                                        <span class='d-block text-sm text-muted mb-1'>Loading...</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </td>
            </tr>
        {% endif %}
    </tbody>
</table>
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
            gCommunication.Actions.ShowAdd = false;
            gCommunication.Actions.ShowExcelExport = false;
            gCommunication.Actions.ShowMergeTemplate = false;

            gCommunication.RowDataBound += gCommunication_RowDataBound;
            gCommunication.GridRebind += gCommunication_GridRebind;
            gCommunication.RowCreated += gCommunication_RowCreated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            InitializeContextPerson();
            InitializeCommunicationMediumMap();

            if ( !Page.IsPostBack )
            {
                SetFilter();
            }

            if ( !Page.IsPostBack )
            {
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

            rFilter.SaveUserPreference( FilterSettingName.Subject, tbSubject.Text );
            rFilter.SaveUserPreference( FilterSettingName.Medium, ddlMedium.SelectedValue );
            rFilter.SaveUserPreference( FilterSettingName.SendDateRange, drpDates.DelimitedValues );
            rFilter.SaveUserPreference( FilterSettingName.CreatedBy, personId.ToString() );
            rFilter.SaveUserPreference( FilterSettingName.SystemCommunicationType, ddlSystemCommunicationType.SelectedValue );
            rFilter.SaveUserPreference( FilterSettingName.CommunicationTemplate, ddlTemplate.SelectedValue );
            rFilter.SaveUserPreference( FilterSettingName.Status, ddlStatus.SelectedValue );
            rFilter.SaveUserPreference( FilterSettingName.BulkStatus, ddlBulk.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();

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
                        if ( !string.IsNullOrWhiteSpace( e.Value ) )
                        {
                            e.Value = ( ( CommunicationType ) System.Enum.Parse( typeof( CommunicationType ), e.Value ) ).ConvertToString();
                        }
                        break;
                    }
                case FilterSettingName.Status:
                    {
                        if ( !string.IsNullOrWhiteSpace( e.Value ) )
                        {
                            var status = e.Value.ConvertToEnumOrNull<CommunicationRecipientStatus>();
                            e.Value = status.ConvertToString();
                        }
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
                        if ( !string.IsNullOrWhiteSpace( e.Value ) )
                        {
                            e.Value = ddlBulk.Items.FindByValue( e.Value ).Text;
                        }
                        break;
                    }
                case FilterSettingName.CommunicationTemplate:
                    {
                        if ( !string.IsNullOrWhiteSpace( e.Value ) )
                        {
                            e.Value = ddlTemplate.Items.FindByValue( e.Value ).Text;
                        }
                        break;
                    }
                case FilterSettingName.SystemCommunicationType:
                    {
                        if ( !string.IsNullOrWhiteSpace( e.Value ) )
                        {
                            e.Value = ddlSystemCommunicationType.Items.FindByValue( e.Value ).Text;
                        }
                        break;
                    }
            }
        }

        #endregion

        #region Communication List Events

        /// <summary>
        /// Handles the RowCreated event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gCommunication_RowCreated( object sender, GridViewRowEventArgs e )
        {
            var communication = e.Row.DataItem as CommunicationListItem;

            if ( communication == null )
            {
                return;
            }

            var lDetail = e.Row.FindControl( "lCommunicationDetailRow" ) as RockLiteral;

            if ( lDetail == null )
            {
                return;
            }

            lDetail.ClientIDMode = ClientIDMode.Static;
            lDetail.ID = GetCommunicationDetailControlId( communication.Id );
        }

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

            var ctlName = GetCommunicationDetailControlId( communication.Id );

            var up = e.Row.FindControl( "upCommunicationItem" );

            var lLava = e.Row.FindControl( ctlName ) as RockLiteral;

            if ( lLava == null )
            {
                return;
            }

            lLava.Text = GetCommunicationItemHeaderHtml( up, communication, false );
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
        /// Get a unique identifier for a communication list item detail panel control.
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        private string GetCommunicationDetailControlId( int communicationId )
        {
            return $"{ gCommunication.ID }-Detail-{ communicationId }";
        }

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
            tbSubject.Text = rFilter.GetUserPreference( FilterSettingName.Subject );

            // Communication Medium
            ddlMedium.BindToEnum( insertBlankOption: true, ignoreTypes: new CommunicationType[] { CommunicationType.RecipientPreference } );
            ddlMedium.SetValue( rFilter.GetUserPreference( FilterSettingName.Medium ) );

            // Status
            ddlStatus.BindToEnum<CommunicationRecipientStatus>( insertBlankOption: true );
            ddlStatus.SelectedValue = rFilter.GetUserPreference( FilterSettingName.Status );

            // Created By
            ppCreatedBy.PersonId = rFilter.GetUserPreference( FilterSettingName.CreatedBy ).AsIntegerOrNull();

            if ( !ppCreatedBy.PersonId.HasValue )
            {
                ppCreatedBy.SetValue( null );
            }

            // Send Date
            drpDates.DelimitedValues = rFilter.GetUserPreference( FilterSettingName.SendDateRange );

            // System Communication Template
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

            ddlSystemCommunicationType.SetValue( rFilter.GetUserPreference( FilterSettingName.Medium ) );

            // Is Bulk?
            ddlBulk.Items.Clear();
            ddlBulk.Items.Add( new ListItem() );
            ddlBulk.Items.Add( new ListItem( "Bulk Messages Only", "Bulk" ) );
            ddlBulk.Items.Add( new ListItem( "Non-bulk Messages Only", "NotBulk" ) );

            ddlBulk.SetValue( rFilter.GetUserPreference( FilterSettingName.BulkStatus ) );

            // Communication Template
            LoadCommunicationTemplatesSelectionList();

            ddlTemplate.SetValue( rFilter.GetUserPreference( FilterSettingName.CommunicationTemplate ) );
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

            var items = GetCommunicationListItems( rockContext,
                qryCommunications,
                personId,
                gCommunication.PageIndex,
                gCommunication.PageSize );

            // Bind the grid data.
            InitializeDataBindingServices();

            gCommunication.VirtualItemCount = qryCommunications.Count();
            gCommunication.DataSource = items;

            gCommunication.DataBind();
        }

        private void InitializeDataBindingServices()
        {
            // Initialize the services used during the Grid data binding process.
            var rockContext = new RockContext();

            _gridPersonService = _gridPersonService ?? new PersonService( rockContext );
            _gridDataViewService = _gridDataViewService ?? new DataViewService( rockContext );
            _gridBinaryFileService = _gridBinaryFileService ?? new BinaryFileService( rockContext );
        }

        /// <summary>
        /// Gets a page of list items for the Communications List.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="qryCommunications"></param>
        /// <param name="personId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private List<CommunicationListItem> GetCommunicationListItems( RockContext rockContext, IQueryable<Rock.Model.Communication> qryCommunications, int personId, int pageIndex, int pageSize )
        {
            qryCommunications = qryCommunications.Skip( pageIndex * pageSize ).Take( pageSize );

            // Get the set of Recipient records for the context person.
            var recipientsQuery = new CommunicationRecipientService( rockContext ).Queryable()
                .Where( x => x.PersonAlias.PersonId == personId );

            // Get the set of list entries for the specified page.
            var queryListItems = qryCommunications
                .Join( recipientsQuery, c => c.Id, r => r.CommunicationId, ( c, r ) => new { Communication = c, Recipient = r } )
                .OrderByDescending( c => c.Communication.CreatedDateTime )
                .AsNoTracking()
                .Select( ciGroup =>
                    new CommunicationListItem
                    {
                        RowId = "C" + ciGroup.Communication.Id + "R" + ciGroup.Recipient.Id,
                        Id = ciGroup.Communication.Id,
                        CommunicationType = ciGroup.Communication.CommunicationType,
                        CommunicationStatus = ciGroup.Communication.Status,
                        Title = string.IsNullOrEmpty( ciGroup.Communication.Subject ) ? ( string.IsNullOrEmpty( ciGroup.Communication.PushTitle ) ? ciGroup.Communication.Name : ciGroup.Communication.PushTitle ) : ciGroup.Communication.Subject,
                        CreatedDateTime = ciGroup.Communication.CreatedDateTime,
                        SendDateTime = ciGroup.Communication.SendDateTime,
                        Sender = ciGroup.Communication.SenderPersonAlias != null ? ciGroup.Communication.SenderPersonAlias.Person : null,
                        RecipientStatus = ciGroup.Recipient.Status,
                        RecipientStatusNote = ciGroup.Recipient.StatusNote,
                        CreatedByPersonAliasId = ciGroup.Communication.CreatedByPersonAliasId,
                        RecipientTotal = ciGroup.Communication.Recipients.Count,
                        InternalCommunicationMediumId = ciGroup.Recipient.MediumEntityTypeId
                    } );

            var items = queryListItems.ToList();

            GetAdditionalCommunicationListItemInfo( items );

            return items;
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
            // Get the base query, excludings items that are current being processed.
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
        /// Retrieve the details associated with a set of Communication List Items.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="qryCommunications"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        private List<CommunicationListItem> GetCommunicationListItemDetail( RockContext rockContext, IQueryable<Rock.Model.Communication> qryCommunications, int personId )
        {
            // Get interactions that are linked to the current Person as a Communication Recipient.
            var interactionsService = new InteractionService( rockContext );

            var communicationChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );

            var interactionsQuery = interactionsService.Queryable()
                .Where( i => i.InteractionComponent.InteractionChannelId == communicationChannelId );

            // Get the set of Recipient records for the context person.
            var recipientsQuery = new CommunicationRecipientService( rockContext ).Queryable()
                .Where( x => x.PersonAlias.PersonId == personId );

            // Get a query that links Communications having the context person as a Recipient with the collection of Interactions
            // for each of those Recipients. The results are selected into a top-level object representing the list entry, and
            // a child object that contains optional additional detail.
            var query = qryCommunications
                .Join( recipientsQuery, c => c.Id, r => r.CommunicationId, ( c, r ) => new { Communication = c, Recipient = r } )
                .GroupJoin( interactionsQuery,
                            cr => cr.Recipient.Id,
                            i => i.EntityId,
                            ( cr, i ) => new { Communication = cr.Communication, Recipient = cr.Recipient, Interactions = i.OrderBy( x => x.InteractionDateTime ) } )
                .AsNoTracking()
                .Select( ciGroup =>
                    new CommunicationListItem
                    {
                        RowId = "C" + ciGroup.Communication.Id + "R" + ciGroup.Recipient.Id,
                        Id = ciGroup.Communication.Id,
                        CommunicationType = ciGroup.Communication.CommunicationType,
                        CommunicationStatus = ciGroup.Communication.Status,
                        Title = string.IsNullOrEmpty( ciGroup.Communication.Subject ) ? ( string.IsNullOrEmpty( ciGroup.Communication.PushTitle ) ? ciGroup.Communication.Name : ciGroup.Communication.PushTitle ) : ciGroup.Communication.Subject,
                        CreatedDateTime = ciGroup.Communication.CreatedDateTime,
                        SendDateTime = ciGroup.Communication.SendDateTime,
                        Sender = ciGroup.Communication.SenderPersonAlias != null ? ciGroup.Communication.SenderPersonAlias.Person : null,
                        RecipientStatus = ciGroup.Recipient.Status,
                        RecipientStatusNote = ciGroup.Recipient.StatusNote,
                        CreatedByPersonAliasId = ciGroup.Communication.CreatedByPersonAliasId,
                        RecipientTotal = ciGroup.Communication.Recipients.Count,
                        InternalCommunicationMediumId = ciGroup.Recipient.MediumEntityTypeId,
                        Detail = new CommunicationListItemDetail
                        {
                            CommunicationId = ciGroup.Communication.Id,
                            Activities = ciGroup.Interactions.Select( x => new CommunicationItemActivity
                            {
                                Id = x.Id,
                                DateTime = x.InteractionDateTime,
                                Name = x.Operation,
                                DeviceDescription = x.InteractionSession.DeviceType.DeviceTypeData,
                                Details = x.InteractionData
                            } ).ToList(),
                            CommunicationListId = ciGroup.Communication.ListGroupId,
                            CommunicationListName = ciGroup.Communication.ListGroupId == null ? null : ciGroup.Communication.ListGroup.Name,
                            CommunicationTemplateId = ciGroup.Communication.CommunicationTemplateId,
                            CommunicationTemplateName = ciGroup.Communication.CommunicationTemplateId == null ? null : ciGroup.Communication.CommunicationTemplate.Name,
                            SenderName = ciGroup.Communication.FromName,
                            InternalSenderEmail = ciGroup.Communication.FromEmail,
                            InternalSenderSmsName = ciGroup.Communication.SMSFromDefinedValue.Description,
                            InternalSenderSmsNumber = ciGroup.Communication.SMSFromDefinedValue.Value,
                            InternalPushImageFileId = ciGroup.Communication.PushImageBinaryFileId,
                            InternalAttachments = ciGroup.Communication.Attachments.Select( x => new CommunicationAttachmentInfo { BinaryFileId = x.BinaryFileId, CommunicationType = x.CommunicationType } ).ToList(),
                            RecipientName = ciGroup.Recipient.PersonAlias.Person.NickName + " " + ciGroup.Recipient.PersonAlias.Person.LastName,
                            RecipientAddress = ciGroup.Recipient.PersonAlias.Person.Email,
                            Message = ciGroup.Recipient.SentMessage,
                            CommunicationSegmentInclusionType = ciGroup.Communication.SegmentCriteria.ToString(),
                            InternalCommunicationSegmentData = ciGroup.Communication.Segments,
                            PersonalInteractionCount = ciGroup.Interactions.Count(),
                            ApplicationName = ciGroup.Recipient.PersonalDevice.Site.Name
                        }
                    } );

            var items = query.ToList();

            GetAdditionalCommunicationListItemInfo( items );

            return items;
        }

        /// <summary>
        /// Load the Communication Templates selection list with templates that the current user is authorized to view.
        /// </summary>
        private void LoadCommunicationTemplatesSelectionList()
        {
            var selectedValue = ddlTemplate.SelectedValue;

            ddlTemplate.Items.Clear();
            ddlTemplate.Items.Add( new ListItem( string.Empty, string.Empty ) );

            var templateService = new CommunicationTemplateService( new RockContext() );

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
        /// Get the markup for a Communication list item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string GetCommunicationItemHeaderHtml( Control rowContainerControl, CommunicationListItem item, bool includeDetailInfo )
        {
            if ( includeDetailInfo )
            {
                GetAdditionalCommunicationEntryData( item );
            }

            var mergeValues = new Dictionary<string, object>();

            mergeValues.Add( "Communication", item );

            // Add Page Links.
            AddMergeFieldForPageLink( mergeValues, "DetailUrl", LinkedPageUrl( AttributeKey.CommunicationDetailPage ), $"CommunicationId={ item.Id }" );
            AddMergeFieldForPageLink( mergeValues, "ListSegmentDetailUrlTemplate", LinkedPageUrl( AttributeKey.CommunicationSegmentDetailPage ), "DataViewId={0}" );

            if ( includeDetailInfo )
            {
                AddMergeFieldForPageLink( mergeValues, "TemplateDetailUrl", LinkedPageUrl( AttributeKey.CommunicationTemplateDetailPage ), $"TemplateId={ item.Detail.CommunicationTemplateId }" );
                AddMergeFieldForPageLink( mergeValues, "ListDetailUrl", LinkedPageUrl( AttributeKey.CommunicationListDetailPage ), $"GroupId={ item.Detail.CommunicationListId }" );
            }

            // Create the "More" PostBack link and arguments.
            // The PostBack link must be generated for the same control that implements IPostbackHandler (in this case, the block usercontrol),
            // so we need to also include an argument to identify the specific detail panel control that should be updated for this request.
            var args = new CommunicationDetailPostbackArgs
            {
                Action = "ShowDetail",
                CommunicationId = item.Id,
                DetailContainerControlId = rowContainerControl.ClientID
            };

            var argsString = args.ToJson();

            argsString = WebUtility.UrlEncode( argsString );

            var postbackLink = Page.ClientScript.GetPostBackClientHyperlink( this, argsString );

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
                    url += $"?{ pageLinkQueryParameters }";
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
            }
        }

        /// <summary>
        /// Retrieve additional information about a Communication list item that is too complex/expensive to include in the initial query.
        /// </summary>
        /// <param name="info"></param>
        private void GetAdditionalCommunicationEntryData( CommunicationListItem info )
        {
            // Set the View permission for the communication detail.
            info.ViewDetailIsAllowed = UserCanEdit
                || ( CurrentPersonAliasId != null && info.CreatedByPersonAliasId != null && info.CreatedByPersonAliasId.Value == CurrentPersonAliasId.Value );

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

            // Set additional details for each type of Communication.
            if ( info.CommunicationType == CommunicationType.SMS )
            {
                info.Detail.SenderName = info.Detail.InternalSenderSmsName;
                info.Detail.SenderAddress = info.Detail.InternalSenderSmsNumber;
            }
            else if ( info.CommunicationType == CommunicationType.Email )
            {
                info.Detail.SenderAddress = info.Detail.InternalSenderEmail;
            }

            // Resolve the URLs for attachments.
            if ( info.CommunicationType == CommunicationType.PushNotification )
            {
                // Resolve the URL for a Push notification image file.
                if ( info.Detail.InternalPushImageFileId != null )
                {
                    var file = _gridBinaryFileService.Get( info.Detail.InternalPushImageFileId.ToIntSafe( 0 ) );

                    info.Detail.Attachments = new List<string>();

                    info.Detail.Attachments.Add( file.Url );
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

                        info.Detail.Attachments.Add( file.Url );
                    }
                }
            }
        }

        /// <summary>
        /// Handles postback events for this block.
        /// </summary>
        /// <param name="eventArgument"></param>
        public void RaisePostBackEvent( string eventArgument )
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

                var rockContext = new RockContext();

                var communicationService = new CommunicationService( rockContext );
                var communicationQuery = communicationService.Queryable().Where( x => x.Id == communicationId );

                var communicationItem = GetCommunicationListItemDetail( rockContext, communicationQuery, _person.Id ).FirstOrDefault();

                var ctlContainer = FindControlRecursive( gCommunication, args.DetailContainerControlId ) as UpdatePanel;

                var ctlName = GetCommunicationDetailControlId( communicationId.Value );

                var lr = FindControlRecursive( ctlContainer, "lCommunicationDetailRow" ) as RockLiteral;

                if ( lr != null )
                {
                    var lavaHeader = GetCommunicationItemHeaderHtml( ctlContainer, communicationItem, true );

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
            /// The identifier of the Communication.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The type of communication.
            /// </summary>
            public CommunicationType CommunicationType { get; set; }

            /// <summary>
            /// The date and time on which this communication was or will be sent.
            /// </summary>
            public DateTime? SendDateTime { get; set; }

            /// <summary>
            /// A friendly description of the date and time this communication was or will be sent.
            /// </summary>
            public string SendDateTimeDescription
            {
                get
                {
                    return Humanizer.DateHumanizeExtensions.Humanize( SendDateTime );
                }
            }

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
        private class CommunicationListItemDetail : RockDynamic
        {
            /// <summary>
            /// The unique identifier of the Communication.
            /// </summary>
            public int CommunicationId { get; set; }

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

            internal List<CommunicationAttachmentInfo> InternalAttachments { get; set; }

            #endregion
        }

        /// <summary>
        /// Information about an attachment for a communication.
        /// </summary>
        /// <remarks>This type does not inherit from RockDynamic because it is not Lava-accessible.</remarks>
        private class CommunicationAttachmentInfo
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
        private class CommunicationItemActivity : RockDynamic
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
        }

        /// <summary>
        /// A Communication List Segment associated with a Communication Item.
        /// </summary>
        private class CommunicationSegment : RockDynamic
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
        private class CommunicationDetailPostbackArgs : RockDynamic
        {
            public string Action { get; set; }
            public int? CommunicationId { get; set; }

            public string DetailContainerControlId { get; set; }
        }

        #endregion
    }
}
