<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionRequestBoard.ascx.cs" Inherits="RockWeb.Blocks.Connection.ConnectionRequestBoard" %>

<script type="text/javascript">
    // This is the source for the "dragscroll" library. It has a modification in the
    // mousedown handler. The modification is to prevent dragscroll from acting
    // on events that originate within cards. Cards can be dragged using Dragula.
    // We do not want these events conflicting. All of the source is the original
    // from GitHub except for the called out modifications.

    /**
     * @fileoverview dragscroll - scroll area by dragging
     * @version 0.0.8
     *
     * @license MIT, see http://github.com/asvd/dragscroll
     * @copyright 2015 asvd <heliosframework@gmail.com>
     */

    (function (root, factory) {
        if (typeof define === 'function' && define.amd) {
            define(['exports'], factory);
        } else if (typeof exports !== 'undefined') {
            factory(exports);
        } else {
            factory((root.dragscroll = {}));
        }
    }(this, function (exports) {
        var _window = window;
        var _document = document;
        var mousemove = 'mousemove';
        var mouseup = 'mouseup';
        var mousedown = 'mousedown';
        var EventListener = 'EventListener';
        var addEventListener = 'add' + EventListener;
        var removeEventListener = 'remove' + EventListener;
        var newScrollX, newScrollY;

        var dragged = [];
        var reset = function (i, el) {
            for (i = 0; i < dragged.length;) {
                el = dragged[i++];
                el = el.container || el;
                el[removeEventListener](mousedown, el.md, 0);
                _window[removeEventListener](mouseup, el.mu, 0);
                _window[removeEventListener](mousemove, el.mm, 0);
            }

            // cloning into array since HTMLCollection is updated dynamically
            dragged = [].slice.call(_document.getElementsByClassName('dragscroll'));
            for (i = 0; i < dragged.length;) {
                (function (el, lastClientX, lastClientY, pushed, scroller, cont) {
                    (cont = el.container || el)[addEventListener](
                        mousedown,
                        cont.md = function (e) {
                            //
                            // *** BEGIN: Modifications for this block ***
                            //
                            if ($(e.target).closest('.board-card-content').length) {
                                // This event originated within a card. Do no drag scrolling because
                                // that will interfere with card dragging
                                return;
                            }
                            //
                            // *** END: Modifications for this block ***
                            //

                            if (!el.hasAttribute('nochilddrag') ||
                                _document.elementFromPoint(
                                    e.pageX, e.pageY
                                ) == cont
                            ) {
                                pushed = 1;
                                lastClientX = e.clientX;
                                lastClientY = e.clientY;

                                e.preventDefault();
                            }
                        }, 0
                    );

                    _window[addEventListener](
                        mouseup, cont.mu = function () { pushed = 0; }, 0
                    );

                    _window[addEventListener](
                        mousemove,
                        cont.mm = function (e) {
                            if (pushed) {
                                (scroller = el.scroller || el).scrollLeft -=
                                    newScrollX = (- lastClientX + (lastClientX = e.clientX));
                                scroller.scrollTop -=
                                    newScrollY = (- lastClientY + (lastClientY = e.clientY));
                                if (el == _document.body) {
                                    (scroller = _document.documentElement).scrollLeft -= newScrollX;
                                    scroller.scrollTop -= newScrollY;
                                }
                            }
                        }, 0
                    );
                })(dragged[i++]);
            }
        }


        if (_document.readyState == 'complete') {
            reset();
        } else {
            _window[addEventListener]('load', reset, 0);
        }

        exports.reset = reset;
    }));
</script>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        // Transfer mode: when user selects "Select Connector" show the connector picker
        var syncTransferConnectorControls = function () {
            var selectedOptionIsSelectConnector = $(this).is('#<%= rbRequestModalViewModeTransferModeSelectConnector.ClientID %>');
                $("#<%=ddlRequestModalViewModeTransferModeOpportunityConnector.ClientID%>").toggle(selectedOptionIsSelectConnector);
            };

        $('#<%= upnlRoot.ClientID %> .js-transfer-connector').on('click', syncTransferConnectorControls);
        $("#<%=ddlRequestModalViewModeTransferModeOpportunityConnector.ClientID%>").toggle($('#<%=rbRequestModalViewModeTransferModeSelectConnector.ClientID%>').is(":checked"));
    });

    const toggleFilterDrawer = function () {
        $('#<%= divFilterDrawer.ClientID %>').slideToggle();
    };
</script>

<asp:UpdatePanel ID="upnlRoot" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <asp:UpdatePanel ID="upnlBlockLevelControls" runat="server">
            <ContentTemplate>
                <asp:LinkButton ID="lbJavaScriptCommand" runat="server" CssClass="hidden" />
                <Rock:NotificationBox runat="server" ID="nbNotificationBox" Visible="false" />
              </ContentTemplate>
        </asp:UpdatePanel>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block styled-scroll">

            <asp:UpdatePanel ID="upnlHeader" runat="server" UpdateMode="Conditional" class="panel-heading panel-follow d-flex flex-wrap flex-sm-nowrap justify-content-between">
                <ContentTemplate>
                    <h2 class="panel-title">
                        <asp:Literal ID="lTitle" runat="server" />
                    </h2>
                    <div class="panel-labels">
                        <asp:LinkButton ID="btnAddCampaignRequests" runat="server" OnClick="btnAddCampaignRequests_Click" CssClass="btn btn-default btn-xs">
                                <i class="fa fa-plus"></i>
                                Campaign Requests
                        </asp:LinkButton>
                    </div>
                    <asp:Panel runat="server" ID="pnlFollowing" CssClass="panel-follow-status js-follow-status" data-toggle="tooltip" data-placement="top" title="Click to Follow"></asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>

            <div class="panel-collapsable">
                <div class="panel-toolbar d-flex flex-wrap flex-sm-nowrap justify-content-between">
                    <asp:UpdatePanel ID="upnlOpportunitiesList" runat="server">
                        <ContentTemplate>
                            <div class="d-inline-block btn-group-mega js-btn-group-mega">
                                <button type="button" class="btn btn-xs btn-tool dropdown-toggle js-dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    <i class="fa fa-plug"></i>
                                    Opportunities
                                </button>
                                <ul class="dropdown-menu dropdown-menu-mega js-dropdown-menu-mega">
                                    <li runat="server" id="liFavoritesHeader" class="dropdown-header">
                                        <i class="far fa-star"></i>
                                        Favorites
                                    </li>
                                    <asp:Repeater ID="rptFavoriteConnectionOpportunities" runat="server" OnItemCommand="rptFavoriteConnectionOpportunities_ItemCommand">
                                        <ItemTemplate>
                                            <li>
                                                <asp:LinkButton runat="server" CommandArgument='<%# Eval("Id") %>'>
                                                            <i class="<%# Eval("IconCssClass") %>"></i>
                                                            <%# Eval("PublicName") %>
                                                            <span class="pull-right text-muted small"><%# Eval("ConnectionTypeName") %></span>
                                                </asp:LinkButton>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>

                                    <asp:Repeater ID="rptConnnectionTypes" runat="server" OnItemDataBound="rptConnnectionTypes_ItemDataBound">
                                        <ItemTemplate>
                                            <li class="dropdown-header">
                                                <i class="<%# Eval("IconCssClass") %>"></i>
                                                <%# Eval("Name") %>
                                            </li>
                                            <asp:Repeater ID="rptConnectionOpportunities" runat="server" OnItemCommand="rptConnectionOpportunities_ItemCommand">
                                                <ItemTemplate>
                                                    <li>
                                                        <asp:LinkButton runat="server" CommandArgument='<%# Eval("Id") %>'>
                                                                    <i class="<%# Eval("IconCssClass") %>"></i>
                                                                    <%# Eval("PublicName") %>
                                                        </asp:LinkButton>
                                                    </li>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </div>
                            <asp:LinkButton ID="lbAddRequest" runat="server" CssClass="btn btn-xs btn-tool" OnClick="lbAddRequest_Click" CausesValidation="false">
                                        <i class="fa fa-plus"></i>
                                        Add Request
                            </asp:LinkButton>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <div class="d-block">
                        <div class="btn-group">
                            <button id="btnConnectors" runat="server" type="button" class="btn btn-xs btn-tool dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                <i class="fa fa-user"></i>
                                <asp:Literal runat="server" ID="lConnectorText" />
                            </button>
                            <ul class="dropdown-menu">
                                <li>
                                    <asp:LinkButton runat="server" ID="lbAllConnectors" OnClick="lbAllConnectors_Click">
                                                All Connectors
                                    </asp:LinkButton>
                                </li>
                                <li>
                                    <asp:LinkButton runat="server" ID="lbMyConnections" OnClick="lbMyConnections_Click">
                                                My Connections
                                    </asp:LinkButton>
                                </li>
                                <li role="separator" class="divider"></li>
                                <asp:Repeater ID="rConnectors" runat="server" OnItemCommand="rConnectors_ItemCommand">
                                    <ItemTemplate>
                                        <li>
                                            <asp:LinkButton runat="server" CommandArgument='<%# Eval("PersonAliasId") %>'>
                                                        <%# Eval("FullName") %>
                                            </asp:LinkButton>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </div>
                    </div>
                    <div class="d-block">
                        <div class="btn-group">
                            <button type="button" class="btn btn-xs btn-tool dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                <i class="fa fa-sort"></i>
                                <asp:Literal runat="server" ID="lSortText" />
                            </button>
                            <ul class="dropdown-menu">
                                <asp:Repeater ID="rptSort" runat="server" OnItemCommand="rptSort_ItemCommand">
                                    <ItemTemplate>
                                        <li>
                                            <asp:LinkButton runat="server" CommandArgument='<%# Eval("SortBy") %>'>
                                                        <%# Eval("Title") %>
                                                        &nbsp;
                                                        <small class="text-muted"><%# Eval("SubTitle") %></small>
                                            </asp:LinkButton>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </div>
                        <a id="aFilterDrawerToggle" runat="server" href="javascript:toggleFilterDrawer()" class="btn btn-xs btn-tool">
                            <i class="fa fa-filter"></i>
                            Filters
                        </a>
                        <div runat="server" id="divCampusBtnGroup" class="btn-group">
                            <button type="button" class="btn btn-xs btn-tool dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                <i class="fa fa-building"></i>
                                <asp:Literal runat="server" ID="lCurrentCampusName" />
                            </button>
                            <ul class="dropdown-menu">
                                <li>
                                    <asp:LinkButton runat="server" ID="lbAllCampuses" OnClick="lbAllCampuses_Click">&nbsp</asp:LinkButton>
                                </li>
                                <asp:Repeater ID="rptCampuses" runat="server" OnItemCommand="rptCampuses_ItemCommand">
                                    <ItemTemplate>
                                        <li>
                                            <asp:LinkButton runat="server" CommandArgument='<%# Eval("Id") %>'>
                                                        <%# Eval("Name") %>
                                            </asp:LinkButton>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </div>
                        <asp:LinkButton ID="lbToggleViewMode" runat="server" CssClass="btn btn-xs btn-tool" OnClick="lbToggleViewMode_Click" />
                    </div>
                </div>

                <div runat="server" id="divFilterDrawer" class="panel-drawer" style="display: none;">
                    <div class="container-fluid padding-t-md padding-b-md">
                        <div class="row">
                            <div class="col-md-4">
                                <Rock:SlidingDateRangePicker ID="sdrpLastActivityDateRangeFilter" runat="server" Label="Last Activity Date Range" EnabledSlidingDateRangeUnits="Day, Week, Month, Year" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                            </div>
                            <div class="col-md-4">
                                <Rock:PersonPicker ID="ppRequesterFilter" runat="server" Label="Requester" />
                            </div>
                            <div class="col-md-4">
                                <Rock:RockCheckBoxList ID="cblStatusFilter" runat="server" Label="Status" DataValueField="Value" DataTextField="Text" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-4">
                                <Rock:RockCheckBoxList ID="cblStateFilter" runat="server" Label="State" DataValueField="Value" DataTextField="Text" />
                            </div>
                            <div class="col-md-4">
                                <Rock:RockCheckBox ID="rcbPastDueOnly" runat="server" Label="Only Past Due" Help="Only show future follow-up requests that are past due." />
                            </div>
                            <div class="col-md-4">
                                <Rock:RockCheckBoxList ID="cblLastActivityFilter" runat="server" Label="Last Activity" DataValueField="Value" DataTextField="Text" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-sm-12">
                                <asp:LinkButton runat="server" ID="lbApplyFilter" CssClass="btn btn-primary btn-xs" OnClick="lbApplyFilter_Click">
                                            Apply
                                </asp:LinkButton>
                                <asp:LinkButton runat="server" ID="lbClearFilter" CssClass="btn btn-link btn-xs" OnClick="lbClearFilter_Click">
                                            Clear
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <asp:UpdatePanel ID="upnlGridView" runat="server">
                <ContentTemplate>
                    <Rock:Grid ID="gRequests" CssClass="border-top-0" runat="server" OnRowDataBound="gRequests_RowDataBound" OnRowSelected="gRequests_RowSelected" OnGridRebind="gRequests_GridRebind">
                        <Columns>
                            <Rock:SelectField></Rock:SelectField>
                            <Rock:RockLiteralField ID="lStatusIcons" HeaderText="" HeaderStyle-CssClass="w-1" ItemStyle-CssClass="w-1 align-middle" />
                            <Rock:RockBoundField DataField="PersonFullname" HeaderText="Name" />
                            <Rock:RockBoundField DataField="CampusName" HeaderText="Campus" />
                            <Rock:RockBoundField DataField="GroupName" HeaderText="Group" />
                            <Rock:RockBoundField DataField="ConnectorPersonFullname" HeaderText="Connector" />
                            <Rock:RockBoundField DataField="LastActivityText" HeaderText="Last Activity" HtmlEncode="false" />
                            <Rock:RockLiteralField ID="lState" HeaderText="State" HeaderStyle-CssClass="w-1" ItemStyle-CssClass="w-1" />
                            <Rock:RockLiteralField ID="lStatus" HeaderText="Status" HeaderStyle-CssClass="w-1" ItemStyle-CssClass="w-1" />
                            <Rock:SecurityField />
                            <Rock:PersonProfileLinkField LinkedPageAttributeKey="PersonProfilePage" />
                            <Rock:DeleteField OnClick="gRequests_Delete" />
                        </Columns>
                    </Rock:Grid>
                </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="upnlBoardView" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="drag-scroll-zone-container position-relative">
                        <div class="panel-body p-0 overflow-scroll board-column-container dragscroll js-dragscroll">
                            <div class="d-flex flex-row w-100 h-100 js-column-container"></div>
                            <div class="js-drag-scroll-zone js-drag-scroll-zone-left drag-scroll-zone drag-scroll-zone-left"></div>
                            <div class="js-drag-scroll-zone js-drag-scroll-zone-right drag-scroll-zone drag-scroll-zone-right"></div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>

        </asp:Panel>

        <asp:UpdatePanel ID="upnlModals" runat="server">
            <ContentTemplate>

                <Rock:ModalDialog ID="mdAddCampaignRequests" runat="server" Title="Get Additional Requests" SaveButtonText="Assign" CancelLinkVisible="true" ValidationGroup="vgAddCampaignRequests" OnSaveClick="mdAddCampaignRequests_SaveClick">
                    <Content>
                        <Rock:RockLiteral ID="lCampaignConnectionItemSingle" runat="server" Label="Campaign" />
                        <Rock:RockDropDownList ID="ddlCampaignConnectionItemsMultiple" runat="server" Label="Campaign" AutoPostBack="true" OnSelectedIndexChanged="ddlCampaignConnectionItem_SelectedIndexChanged" />
                        <Rock:NotificationBox ID="nbAddConnectionRequestsMessage" runat="server" Visible="False" />

                        <Rock:NumberBox ID="nbNumberOfRequests" runat="server" Label="Number of Requests" NumberType="Integer" Required="true" MinimumValue="0" ValidationGroup="vgAddCampaignRequests" />
                    </Content>
                </Rock:ModalDialog>

                <Rock:ModalDialog ID="mdRequest" runat="server" ValidationGroup="vgRequestModal" Title="Connection Request" Visible="false" ClickBackdropToClose="true">
                    <Content>
                        <Rock:NotificationBox runat="server" ID="nbRequestModalNotificationBox" Visible="false" />
                        <asp:CustomValidator ID="cvRequestModalCustomValidator" runat="server" Display="None" />

                        <div id="divRequestModalViewMode" runat="server">
                            <asp:Literal ID="lRequestModalViewModeHeading" runat="server" />
                            <div class="row">
                                <div class="col-sm-2">
                                    <div id="divRequestModalViewModePhoto" runat="server" class="request-modal-photo mx-auto mb-3"></div>
                                </div>
                                <div class="col-sm-10">
                                    <asp:Literal runat="server" ID="lRequestModalViewModeStatusIcons" />
                                    <a runat="server" class="small pull-right" id="aRequestModalViewModeProfileLink">
                                        <i class="fa fa-user-alt"></i>
                                        Person Profile
                                    </a>
                                    <h3 class="mt-0 mb-3">
                                        <asp:Literal ID="lRequestModalViewModePersonFullName" runat="server" />
                                    </h3>
                                    <div class="row">
                                        <div class="col-sm-6 col-md-5 mb-3">
                                            <h6 class="mt-0 mb-1">Contact Information</h6>
                                            <div class="personcontact">
                                                <ul class="list-unstyled phonenumbers mb-1">
                                                    <asp:Repeater ID="rRequestModalViewModePhones" runat="server">
                                                        <ItemTemplate>
                                                            <li>
                                                                <%# Eval("FormattedPhoneNumber") %>
                                                                <small>
                                                                    <%# Eval("PhoneType") %>
                                                                    <%# Eval("SmsLinkHtml") %>
                                                                </small>
                                                            </li>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </ul>
                                                <div class="email">
                                                    <asp:Literal ID="lRequestModalViewModeEmail" runat="server" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-sm-6 col-md-4 mb-3">
                                            <h6 class="mt-0 mb-1">Connector</h6>
                                            <div class="btn-group">
                                                <div class="dropdown-toggle cursor-pointer" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    <asp:Literal runat="server" ID="lRequestModalViewModeConnectorProfilePhoto" />
                                                    <asp:Literal ID="lRequestModalViewModeConnectorFullName" runat="server" />
                                                </div>
                                                <ul class="dropdown-menu">
                                                    <asp:Repeater ID="rRequestModalViewModeConnectorSelect" runat="server" OnItemCommand="rRequestModalViewModeConnectorSelect_ItemCommand">
                                                        <ItemTemplate>
                                                            <li>
                                                                <asp:LinkButton runat="server" CommandArgument='<%# Eval("PersonAliasId") %>'>
                                                                    <%# Eval("FullName") %>
                                                                </asp:LinkButton>
                                                            </li>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </ul>
                                            </div>
                                        </div>

                                        <div class="col-sm-12 col-md-3 text-left text-md-right mb-3 mb-md-0">
                                            <asp:Literal ID="lRequestModalViewModeSideDescription" runat="server" />
                                            <Rock:DynamicPlaceHolder ID="phGroupMemberAttributesView" runat="server" />
                                        </div>

                                    </div>

                                    <asp:Panel runat="server" CssClass="badge-bar margin-b-sm" ID="pnlRequestModalViewModeBadges">
                                        <Rock:BadgeListControl ID="blRequestModalViewModeBadges" runat="server" />
                                    </asp:Panel>
                                </div>

                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <asp:Literal ID="lRequestModalViewModeComments" runat="server" />
                                </div>
                            </div>
                            <asp:Literal ID="lRequestModalViewModeBadgeBar" runat="server" />
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:AttributeValuesContainer ID="avcRequestModalViewModeAttributesReadOnly" runat="server" DisplayAsTabs="true" NumberOfColumns="2" />
                                </div>
                                <div id="divRequestModalViewModeWorkflows" runat="server" class="col-md-6">
                                    <Rock:ModalAlert ID="mdWorkflowLaunched" runat="server" />
                                    <asp:Label ID="lblWorkflows" Text="Available Workflows" Font-Bold="true" runat="server" />
                                    <div class="margin-b-lg">
                                        <asp:Repeater ID="rptRequestWorkflows" runat="server" OnItemCommand="rptRequestWorkflows_ItemCommand">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="lbRequestWorkflow" runat="server" CssClass="btn btn-default btn-xs" CommandArgument='<%# Eval("Id") %>' CommandName="LaunchWorkflow">
                                                    <%# Eval("WorkflowType.Name") %>
                                                </asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <asp:Panel ID="pnlRequestModalViewModeRequirements" runat="server">
                                        <Rock:RockControlWrapper ID="rcwRequestModalViewModeRequirements" runat="server" Label="Group Requirements">
                                            <Rock:RockCheckBoxList ID="cblRequestModalViewModeManualRequirements" RepeatDirection="Vertical" runat="server" Label="" />
                                            <div class="labels">
                                                <asp:Literal ID="lRequestModalViewModeRequirementsLabels" runat="server" />
                                            </div>
                                        </Rock:RockControlWrapper>
                                    </asp:Panel>
                                </div>
                            </div>
                            <div class="actions d-flex mb-4 mt-md-4 mb-md-5">
                                <asp:LinkButton ID="btnRequestModalViewModeEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnRequestModalViewModeEdit_Click" CausesValidation="false" />
                                <asp:LinkButton ID="btnRequestModalViewModeTransfer" runat="server" Text="Transfer" CssClass="btn btn-link" CausesValidation="false" OnClick="btnRequestModalViewModeTransfer_Click" />
                                <asp:LinkButton ID="btnRequestViewModeViewHistory" runat="server" Text="View History" CssClass="btn btn-link" CausesValidation="false" OnClick="btnRequestViewModeViewHistory_Click" />
                                <asp:LinkButton ID="btnRequestModalViewModeConnect" runat="server" Text="Connect" CssClass="btn btn-primary ml-auto" CausesValidation="false" OnClick="btnRequestModalViewModeConnect_Click" />
                            </div>

                            <div runat="server" id="divRequestModalViewModeActivityGridMode" class="mb-4">
                                <div class="row">
                                    <div class="col-md-6">
                                        <h5 class="mt-0">Activities</h5>
                                    </div>
                                    <div class="col-md-6 text-right">
                                        <asp:LinkButton runat="server" ID="lbRequestModalViewModeAddActivity" OnClick="lbRequestModalViewModeAddActivity_Click" CssClass="btn btn-default btn-xs">
                                            <i class="fa fa-plus"></i>
                                            Add Activity
                                        </asp:LinkButton>
                                    </div>
                                </div>
                                <Rock:Grid ID="gRequestModalViewModeActivities" runat="server" AllowPaging="false" DisplayType="Light" AllowSorting="false" OnRowSelected="gRequestModalViewModeActivities_RowSelected" OnRowDataBound="gRequestModalViewModeActivities_RowDataBound">
                                    <Columns>
                                        <Rock:DateTimeField HeaderText="Date" DataField="Date" DataFormatString="{0:d}" />
                                        <Rock:RockBoundField DataField="ActivityMarkup" HeaderText="Activity" HtmlEncode="false" />
                                        <Rock:RockBoundField DataField="ConnectorPersonFullname" HeaderText="Connector" />
                                        <Rock:RockBoundField DataField="OpportunityMarkup" HeaderText="Opportunity" HtmlEncode="false" />
                                        <Rock:DeleteField OnClick="gRequestModalViewModeActivities_Delete" />
                                    </Columns>
                                </Rock:Grid>
                                <div class="row">
                                    <div class="col-12 text-right">
                                        <asp:LinkButton runat="server" ID="lbRequestModalViewModeShowAllActivities" OnClick="lbRequestModalViewModeShowAllActivities_Click" CssClass="small">
                                            View More
                                        </asp:LinkButton>
                                    </div>
                                </div>
                                <Rock:PanelWidget ID="wpRequestModalViewModeWorkflow" runat="server" Title="Workflows" CssClass="clickable mt-4 mb-0">
                                    <div class="grid">
                                        <Rock:Grid ID="gRequestModalViewModeWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Location" OnRowSelected="gRequestModalViewModeWorkflows_RowSelected">
                                            <Columns>
                                                <Rock:RockBoundField DataField="WorkflowType" HeaderText="Workflow Type" />
                                                <Rock:RockBoundField DataField="Trigger" HeaderText="Trigger" />
                                                <Rock:RockBoundField DataField="CurrentActivity" HeaderText="Current Activity" />
                                                <Rock:RockBoundField DataField="Date" HeaderText="Start Date" />
                                                <Rock:RockBoundField DataField="Status" HeaderText="Status" HtmlEncode="false" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                </Rock:PanelWidget>
                            </div>

                            <div runat="server" id="divRequestModalViewModeAddActivityMode">
                                <h5>Activities</h5>
                                <div class="row">
                                    <Rock:RockDropDownList runat="server" ID="ddlRequestModalViewModeAddActivityModeType" Label="Activity Type" FormGroupCssClass="col-md-6" />
                                    <Rock:RockDropDownList runat="server" ID="ddlRequestModalViewModeAddActivityModeConnector" Label="Connector" FormGroupCssClass="col-md-6" />
                                    <Rock:RockTextBox runat="server" ID="tbRequestModalViewModeAddActivityModeNote" Label="Note" TextMode="MultiLine" Rows="4" FormGroupCssClass="col-md-12" />
                                </div>
                                <div class="actions text-right">
                                    <asp:LinkButton ID="btnRequestModalViewModeAddActivityModeSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnRequestModalViewModeAddActivityModeSave_Click" />
                                    <asp:LinkButton ID="btnRequestModalViewModeAddActivityModeCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnRequestModalViewModeAddActivityModeCancel_Click" CausesValidation="false" />
                                </div>
                            </div>

                            <div runat="server" id="divRequestModalViewModeTransferMode">
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:NotificationBox
                                            runat="server"
                                            ID="nbTranferFailed"
                                            Text="You must select a new opportunity to transfer this request."
                                            NotificationBoxType="Warning"
                                            Visible="false"></Rock:NotificationBox>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockControlWrapper ID="rcwRequestModalViewModeTransferModeTransferOpportunity" runat="server" Label="Opportunity">
                                            <div class="row">
                                                <div class="col-md-8">
                                                    <Rock:RockDropDownList ID="ddlRequestModalViewModeTransferModeOpportunity" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlRequestModalViewModeTransferModeOpportunity_SelectedIndexChanged" EnhanceForLongLists="true" />
                                                </div>
                                                <div class="col-md-4">
                                                    <Rock:BootstrapButton ID="btnRequestModalViewModeTransferModeSearch" runat="server" CssClass="btn btn-primary" Text="Search" OnClick="btnRequestModalViewModeTransferModeSearch_Click" />
                                                </div>
                                            </div>
                                        </Rock:RockControlWrapper>

                                        <Rock:RockControlWrapper ID="rcwRequestModalViewModeTransferModeConnector" runat="server" Label="Connector">
                                            <div>
                                                <Rock:RockRadioButton ID="rbRequestModalViewModeTransferModeDefaultConnector" runat="server" CssClass="js-transfer-connector" Text="Default Connector" GroupName="TransferOpportunityConnector" />
                                            </div>
                                            <div>
                                                <Rock:RockRadioButton ID="rbTRequestModalViewModeTransferModeCurrentConnector" runat="server" CssClass="js-transfer-connector" Text="Current Connector" GroupName="TransferOpportunityConnector" />
                                            </div>
                                            <div>
                                                <Rock:RockRadioButton ID="rbRequestModalViewModeTransferModeSelectConnector" runat="server" CssClass="js-transfer-connector" Text="Select Connector" GroupName="TransferOpportunityConnector" />
                                                <Rock:RockDropDownList ID="ddlRequestModalViewModeTransferModeOpportunityConnector" CssClass="margin-l-lg" runat="server" Style="display: none" />
                                            </div>
                                            <div>
                                                <Rock:RockRadioButton ID="rbRequestModalViewModeTransferModeNoConnector" runat="server" CssClass="js-transfer-connector" Text="No Connector" GroupName="TransferOpportunityConnector" />
                                            </div>
                                        </Rock:RockControlWrapper>
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlRequestModalViewModeTransferModeStatus" runat="server" Label="Status" />
                                        <Rock:CampusPicker ID="cpTransferCampus" runat="server" Label="Campus" AutoPostBack="true" OnSelectedIndexChanged="cpTransferCampus_SelectedIndexChanged" />
                                    </div>
                                </div>
                                <Rock:RockTextBox ID="tbRequestModalViewModeTransferModeNote" runat="server" Label="Note" TextMode="MultiLine" Rows="4" />
                                <div class="actions text-right">
                                    <asp:LinkButton ID="btnRequestModalViewModeTransferModeSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Transfer" CssClass="btn btn-primary" OnClick="btnRequestModalViewModeTransferModeSave_Click"></asp:LinkButton>
                                    <asp:LinkButton ID="btnRequestModalViewModeTransferModeCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnRequestModalViewModeTransferModeCancel_Click" CausesValidation="false"></asp:LinkButton>
                                </div>

                            </div>

                        </div>

                        <div id="divRequestModalAddEditMode" runat="server">
                            <div class="row">
                                <div class="col-md-3">
                                    <Rock:PersonPicker runat="server" ID="ppRequestModalAddEditModePerson" Label="Requester" Required="true" OnSelectPerson="ppRequestModalAddEditModePerson_SelectPerson" />
                                </div>
                                <div class="col-md-3">
                                    <Rock:RockDropDownList ID="ddlRequestModalAddEditModeConnector" runat="server" Label="Connector" EnhanceForLongLists="true" />
                                </div>
                                <div class="col-md-4 col-md-offset-2">
                                    <Rock:RockRadioButtonList ID="rblRequestModalAddEditModeState" runat="server" Label="State" RepeatDirection="Horizontal" OnSelectedIndexChanged="rblRequestModalAddEditModeState_SelectedIndexChanged" AutoPostBack="true" />
                                    <Rock:DatePicker ID="dpRequestModalAddEditModeFollowUp" runat="server" Label="Follow-up Date" AllowPastDateSelection="false" Visible="false" />
                                </div>
                            </div>

                            <Rock:RockTextBox ID="tbRequestModalAddEditModeComments" Label="Comments" runat="server" TextMode="MultiLine" Rows="4" ValidateRequestMode="Disabled" />

                            <Rock:RockRadioButtonList ID="rblRequestModalAddEditModeStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlRequestModalAddEditModePlacementGroup" runat="server" Label="Placement Group" AutoPostBack="true" OnSelectedIndexChanged="ddlRequestModalAddEditModePlacementGroup_SelectedIndexChanged" EnhanceForLongLists="true" />
                                    <Rock:RockDropDownList ID="ddlRequestModalAddEditModePlacementRole" runat="server" Label="Group Member Role" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="ddlRequestModalAddEditModePlacementRole_SelectedIndexChanged" />
                                    <Rock:RockDropDownList ID="ddlRequestModalAddEditModePlacementStatus" runat="server" Label="Group Member Status" Visible="false" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:CampusPicker ID="cpRequestModalAddEditModeCampus" runat="server" Label="Campus" AutoPostBack="true" OnSelectedIndexChanged="cpRequestModalAddEditModeCampus_SelectedIndexChanged" IncludeInactive="false" />
                                </div>
                            </div>

                            <Rock:AttributeValuesContainer ID="avcRequestModalAddEditModeRequest" runat="server" NumberOfColumns="2" />
                            <Rock:AttributeValuesContainer ID="avcRequestModalAddEditModeGroupMember" runat="server" NumberOfColumns="2" />

                            <div class="actions text-right">
                                <asp:LinkButton ID="lbRequestModalAddEditModeSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbRequestModalAddEditModeSave_Click" />
                                <asp:LinkButton ID="lbRequestModalAddEditModeCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="lbRequestModalAddEditModeCancel_Click" CausesValidation="false" />
                            </div>
                        </div>

                    </Content>
                </Rock:ModalDialog>

                <Rock:ModalDialog ID="mdSearchModal" runat="server" ValidationGroup="Search" Title="Search Opportunities">
                    <Content>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbSearchModalName" runat="server" Label="Name" />
                                <Rock:RockCheckBoxList ID="cblSearchModalCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                                <Rock:DynamicPlaceholder ID="phSearchModalAttributeFilters" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <asp:Repeater ID="rptSearchModalResult" runat="server" OnItemCommand="rptSearchModalResult_ItemCommand">
                                    <ItemTemplate>
                                        <Rock:PanelWidget ID="pwSearchModalPanel" runat="server" CssClass="panel panel-block" TitleIconCssClass='<%# Eval("IconCssClass") %>' Title='<%# Eval("Name") %>'>
                                            <div class="row">
                                                <div class="col-md-4">
                                                    <div class="photo">
                                                        <img src='<%# Eval("PhotoUrl") %>'></img>
                                                    </div>
                                                </div>
                                                <div class="col-md-8">
                                                    <%# Eval("Description") %>
                                                    <br />
                                                    <Rock:BootstrapButton ID="btnSearchModalSelect" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display" Text="Select" CssClass="btn btn-default btn-sm" />
                                                </div>
                                            </div>
                                        </Rock:PanelWidget>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="lbSearchModalSearch" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Search" CssClass="btn btn-primary" OnClick="lbSearchModalSearch_Click" />
                            <asp:LinkButton ID="lbSearchModalCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="lbSearchModalCancel_Click" CausesValidation="false" />
                        </div>
                    </Content>
                </Rock:ModalDialog>

            </ContentTemplate>
        </asp:UpdatePanel>

    </ContentTemplate>
</asp:UpdatePanel>

<div class="js-connection-board-loading" role="status" aria-hidden="true" style="display: none;">
    <div class="updateprogress-status">
        <div class="spinner">
            <div class="rect1"></div>
            <div class="rect2"></div>
            <div class="rect3"></div>
            <div class="rect4"></div>
            <div class="rect5"></div>
        </div>
    </div>
    <div class="updateprogress-bg modal-backdrop"></div>
</div>

<asp:UpdatePanel ID="upnlJavaScript" runat="server" UpdateMode="Conditional">
</asp:UpdatePanel>

<script id="js-template-column" type="text/template">
    <div class="board-column">
        <div class="board-heading">
            <div class="board-heading-details">
                <span class="board-column-title">{{Name}}</span>
                <span class="board-count">{{RequestCount}}</span>
            </div>
            <div class="board-heading-pill" style="background: {{HighlightColor}}"></div>
        </div>
        <div class="board-cards js-card-container" data-status-id="{{Id}}">
        </div>
        <div class="js-drag-scroll-zone js-drag-scroll-zone-top drag-scroll-zone drag-scroll-zone-top"></div>
        <div class="js-drag-scroll-zone js-drag-scroll-zone-bottom drag-scroll-zone drag-scroll-zone-bottom"></div>
    </div>
</script>

<script id="js-template-column-sentry" type="text/template">
    <div class="board-card-base board-column-sentry small">
        <p class="mb-2"><strong>More requests exist</strong></p>
        <p>Please adjust sorting, use filters, or even use the grid mode to interact with them.</p>
    </div>
</script>

<script id="js-template-card" type="text/template">
    <div class="board-card js-board-card" data-request-id="{{Id}}" data-opportunity-id="{{ConnectionOpportunityId}}">
        <div class="board-card-content js-board-card-content">
            <div class="board-card-header">
                {{StatusIconsHtml}}
                {{CampusHtml}}
            </div>
            <div class="board-card-main">
                <div class="flex-grow-1 mb-2">
                    <div class="board-card-photo" style="background-image: url( '{{PersonPhotoUrl}}' );" title="{{PersonFullname}} Profile Photo"></div>
                    <div class="board-card-name">
                        {{PersonFullname}}
                    </div>
                    <span class="board-card-assigned">{{ConnectorPersonFullname}}
                    </span>
                </div>
                <div>
                    <div class="btn-group dropdown-right">
                        <button type="button" class="btn btn-sm text-muted bg-white dropdown-toggle pr-0" data-toggle="dropdown">
                            <i class="fa fa-ellipsis-h"></i>
                        </button>
                        <ul class="dropdown-menu">
                            <li>
                                <a href="javascript:void(0);" class="js-view">View Details
                                </a>
                            </li>
                            <li class="can-connect-{{CanConnect}}">
                                <a href="javascript:void(0);" class="js-connect">Connect
                                </a>
                            </li>
                            <% if ( ShowSecurityButton ) { %>
                            <li>
                                <a href="javascript:Rock.controls.modal.show($(this), '/Secure/<%= ConnectionRequestEntityTypeId %>/{{Id}}?t=Security&pb=&sb=Done')">Security
                                </a>
                            </li>
                            <% } %>
                            <li role="separator" class="divider can-connect-{{CanCurrentUserEdit}}"></li>
                            <li class="can-connect-{{CanCurrentUserEdit}}">
                                <a href="javascript:void(0);" class="dropdown-item-danger js-delete">Delete
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
            <div class="board-card-meta">
                <span title="{{ActivityCountText}} - {{DaysSinceLastActivityLongText}}">
                    <i class="fa fa-list"></i>
                    {{ActivityCount}} - {{DaysSinceLastActivityShortText}}
                </span>
                <span title="{{DaysSinceOpeningLongText}}">
                    <i class="fa fa-calendar"></i>
                    {{DaysSinceOpeningShortText}}
                </span>
            </div>
        </div>
    </div>
</script>
