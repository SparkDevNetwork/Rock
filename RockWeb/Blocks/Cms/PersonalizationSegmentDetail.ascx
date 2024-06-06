<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalizationSegmentDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersonalizationSegmentDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfPersonalizationSegmentId" runat="server" />
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-user-tag"></i>
                    <asp:Literal ID="lPanelTitle" runat="server" Text="" />

                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <%-- Segment Name --%>
                <div class="row mb-5">
                    <div class="col-md-6">
                        <Rock:HiddenFieldWithClass ID="hfExistingSegmentKeyNames" runat="server" CssClass="js-existing-key-names" />
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.PersonalizationSegment, Rock" PropertyName="Name" Required="true" onblur="populateSegmentKey()" />
                        <Rock:DataTextBox ID="tbSegmentKey" runat="server" SourceTypeName="Rock.Model.PersonalizationSegment, Rock" PropertyName="SegmentKey" Label="Key" Required="true" />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                    </div>

                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.PersonalizationSegment, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                    </div>
                </div>

                <%-- Person Filters --%>
                <asp:Panel ID="pnlPersonFilters" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <h1 class="panel-title">Person Filters</h1>
                    </div>
                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataViewItemPicker ID="dvpFilterDataView" runat="server" Label="Filter Data View" />
                            </div>

                            <div class="col-md-6">
                                Adding a data view to the segment will exclude anonymous visitors.
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <span class="segment-and"><span class="text">And</span></span>

                <%-- Session Filters --%>
                <asp:Panel ID="pnlSessionCountFilters" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <h1 class="panel-title">Session Filters</h1>
                        <div class="panel-labels">
                            <Rock:Toggle ID="tglSessionCountFiltersAllAny" runat="server" OnText="All" OffText="Any" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" />
                        </div>
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gSessionCountFilters" runat="server" DisplayType="Light" RowItemText="Session Filter">
                            <Columns>
                                <Rock:RockLiteralField ID="lSessionCountFilterDescription" HeaderText="Description" OnDataBound="lSessionCountFilterDescription_DataBound" />
                                <Rock:EditField OnClick="gSessionCountFilters_EditClick" />
                                <Rock:DeleteField OnClick="gSessionCountFilters_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <span class="segment-and"><span class="text">And</span></span>

                <%-- Page View Filters --%>
                <asp:Panel ID="pnlPageViewFilters" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <h1 class="panel-title">Page View Filters</h1>
                        <Rock:Toggle ID="tglPageViewFiltersAllAny" runat="server" OnText="All" OffText="Any" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" />
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gPageViewFilters" runat="server" DisplayType="Light" RowItemText="Page View Filter">
                            <Columns>
                                <Rock:RockLiteralField ID="lPageViewFilterDescription" HeaderText="Description" OnDataBound="lPageViewFilterDescription_DataBound" />
                                <Rock:EditField OnClick="gPageViewFilters_EditClick" />
                                <Rock:DeleteField OnClick="gPageViewFilters_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </asp:Panel>

                <span class="segment-and"><span class="text">And</span></span>

                <%-- Interaction Filters --%>
                <asp:Panel ID="pnlInteractionFilters" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <h1 class="panel-title">Interaction Filters</h1>
                        <Rock:Toggle ID="tglInteractionFiltersAllAny" runat="server" OnText="All" OffText="Any" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" />
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gInteractionFilters" runat="server" DisplayType="Light" RowItemText="Interaction Filter">
                            <Columns>
                                <Rock:RockBoundField DataField="InteractionChannelName" HeaderText="Channel" />
                                <Rock:RockBoundField DataField="InteractionComponentName" HeaderText="Component" />
                                <Rock:RockBoundField DataField="Operation" HeaderText="Operation" />
                                <Rock:RockBoundField DataField="ComparisonText" HeaderText="Quantity" />
                                <Rock:RockBoundField DataField="DateRangeText" HeaderText="Date Range" />
                                <Rock:EditField OnClick="gInteractionFilters_EditClick" />
                                <Rock:DeleteField OnClick="gInteractionFilters_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <Rock:NotificationBox ID="nbSegmentDataUpdateError" runat="server" NotificationBoxType="Warning" />

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>

            <%-- Modal for Session Count Filter --%>
            <Rock:ModalDialog ID="mdSessionCountFilterConfiguration" runat="server" OnSaveClick="mdSessionCountFilterConfiguration_SaveClick" ValidationGroup="vgSessionCountFilterConfiguration">
                <Content>
                    <asp:HiddenField ID="hfSessionCountFilterGuid" runat="server" />

                    <asp:ValidationSummary ID="vsSessionCountFilterConfiguration" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgSessionCountFilterConfiguration" />

                    <div class="row form-row d-flex flex-wrap align-items-center form-group">
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:RockDropDownList ID="ddlSessionCountFilterComparisonType" CssClass="input-width-xl js-filter-compare" runat="server" ValidationGroup="vgSessionCountFilterConfiguration" />
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:NumberBox ID="nbSessionCountFilterCompareValue" runat="server" Required="true" CssClass="input-width-sm js-filter-control" ValidationGroup="vgSessionCountFilterConfiguration" />
                            </div>
                        </div>

                        <div class="col flex-sm-grow-0">
                            <div class="form-group"><span class="text-nowrap">sessions on the</span></div>
                        </div>
                        <div class="w-100 d-sm-none"></div>
                        <div class="col">
                            <div class="form-group">
                                <Rock:RockListBox ID="lstSessionCountFilterWebSites" runat="server" Required="true" ValidationGroup="vgSessionCountFilterConfiguration" RequiredErrorMessage="Website is required." />
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <span>site(s)</span>
                        </div>
                    </div>


                    <div class="row form-row d-flex flex-wrap align-items-center">
                        <div class="col flex-grow-0"><span class="text-nowrap">In the following date range</span></div>
                        <div class="col">
                            <Rock:SlidingDateRangePicker ID="drpSessionCountFilterSlidingDateRange" runat="server" Label="" PreviewLocation="Right" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" ValidationGroup="vgSessionCountFilterConfiguration" />
                        </div>
                    </div>
                </Content>
            </Rock:ModalDialog>

            <%-- Modal for Page Views Filter --%>
            <Rock:ModalDialog ID="mdPageViewFilterConfiguration" runat="server" OnSaveClick="mdPageViewFilterConfiguration_SaveClick" ValidationGroup="vgPageViewFilterConfiguration">
                <Content>

                    <asp:HiddenField ID="hfPageViewFilterGuid" runat="server" />

                    <asp:ValidationSummary ID="vsPageViewFilterConfiguration" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgPageViewFilterConfiguration" />
                    <div class="row form-row d-flex flex-wrap align-items-center form-group">
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:RockDropDownList ID="ddlPageViewFilterComparisonType" CssClass="input-width-xl js-filter-compare" runat="server" ValidationGroup="vgPageViewFilterConfiguration" />
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:NumberBox ID="nbPageViewFilterCompareValue" runat="server" Required="true" CssClass="input-width-sm js-filter-control" ValidationGroup="vgPageViewFilterConfiguration" />
                            </div>
                        </div>

                        <div class="col flex-sm-grow-0">
                            <div class="form-group"><span class="text-nowrap">page views on the</span></div>
                        </div>

                        <div class="col">
                            <Rock:RockListBox ID="lstPageViewFilterWebSites" runat="server" Required="true" ValidationGroup="vgPageViewFilterConfiguration" RequiredErrorMessage="Web Site is required." />
                        </div>

                        <div class="col flex-grow-0">
                            <span>site(s)</span>
                        </div>
                    </div>

                    <div class="row form-row d-flex flex-wrap align-items-center">
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <span class="text-nowrap">In the following date range</span>
                            </div>
                        </div>
                        <div class="col">
                            <div class="form-group">
                                <Rock:SlidingDateRangePicker ID="drpPageViewFilterSlidingDateRange" runat="server" Label="" PreviewLocation="Right" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" ValidationGroup="vgPageViewFilterConfiguration" ToolTip="<div class='js-slidingdaterange-info'></>" />
                            </div>
                        </div>
                    </div>
                    <div class="row form-row d-flex flex-wrap align-items-center">
                        <div class="col flex-grow-0"><span class="text-nowrap">optionally limited to the following pages</span></div>

                        <div class="col">
                            <Rock:PagePicker ID="ppPageViewFilterPages" runat="server" AllowMultiSelect="true" ValidationGroup="vgPageViewFilterConfiguration" Label="" ShowSelectCurrentPage="false" />
                        </div>
                        <div class="col">
                            <Rock:RockCheckBox ID="cbIncludeChildPages" runat="server" Label="Include descendant pages" />
                        </div>
                    </div>

                    <div class="row form-row mt-3">
                        <asp:LinkButton ID="lbPageViewAdvancedOptions" runat="server" Text="Advanced Options" CausesValidation="false" OnClick="lbPageViewAdvancedOptions_Click" />
                    </div>

                    <asp:Panel ID="pnlAdvancedOptions" runat="server" Visible="false">

                        <div class="row form-row d-flex flex-wrap align-items-center form-group mt-3">

                            <div class="col flex-sm-grow-0">
                                <div class="form-group"><span class="text-nowrap">where the URL</span></div>
                            </div>

                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockDropDownList ID="ddlPageUrlFilterComparisonType" CssClass="input-width-xl js-filter-compare" runat="server" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>

                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockTextBox ID="rtbPageUrlCompareValue" runat="server" Required="false" CssClass="input-width-xxl js-filter-control" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>

                        </div>

                        <div class="row form-row d-flex flex-wrap align-items-center form-group">

                            <div class="col flex-sm-grow-0">
                                <div class="form-group"><span class="text-nowrap">and the Referrer</span></div>
                            </div>

                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockDropDownList ID="ddlPageReferrerFilterComparisonType" CssClass="input-width-xl js-filter-compare" runat="server" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>

                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockTextBox ID="rtbPageReferrerCompareValue" runat="server" Required="false" CssClass="input-width-xxl js-filter-control" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>

                        </div>

                        <div class="row form-row d-flex flex-wrap align-items-center form-group">
                            <div class="col flex-sm-grow-0">
                                <div class="form-group"><span class="text-nowrap">and the Source</span></div>
                            </div>
                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockDropDownList ID="ddlPageSourceFilterComparisonType" CssClass="input-width-xl js-filter-compare" runat="server" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>
                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockTextBox ID="rtbPageSourceCompareValue" runat="server" Required="false" CssClass="input-width-xxl js-filter-control" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>
                        </div>

                        <div class="row form-row d-flex flex-wrap align-items-center form-group">
                            <div class="col flex-sm-grow-0">
                                <div class="form-group"><span class="text-nowrap">and the Medium</span></div>
                            </div>
                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockDropDownList ID="ddlMediumComparisonType" CssClass="input-width-xl js-filter-compare" runat="server" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>
                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockTextBox ID="rtbMediumCompareValue" runat="server" Required="false" CssClass="input-width-xxl js-filter-control" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>
                        </div>

                        <div class="row form-row d-flex flex-wrap align-items-center form-group">
                            <div class="col flex-sm-grow-0">
                                <div class="form-group"><span class="text-nowrap">and the Campaign</span></div>
                            </div>
                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockDropDownList ID="ddlCampaignComparisonType" CssClass="input-width-xl js-filter-compare" runat="server" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>
                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockTextBox ID="rtbCampaignCompareValue" runat="server" Required="false" CssClass="input-width-xxl js-filter-control" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>
                        </div>

                        <div class="row form-row d-flex flex-wrap align-items-center form-group">
                            <div class="col flex-sm-grow-0">
                                <div class="form-group"><span class="text-nowrap">and the Content</span></div>
                            </div>
                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockDropDownList ID="ddlContentComparisonType" CssClass="input-width-xl js-filter-compare" runat="server" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>
                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockTextBox ID="rtbContentCompareValue" runat="server" Required="false" CssClass="input-width-xxl js-filter-control" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>
                        </div>

                        <div class="row form-row d-flex flex-wrap align-items-center form-group">
                            <div class="col flex-sm-grow-0">
                                <div class="form-group"><span class="text-nowrap">and the Term</span></div>
                            </div>
                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockDropDownList ID="ddlTermComparisonType" CssClass="input-width-xl js-filter-compare" runat="server" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>
                            <div class="col flex-grow-0">
                                <div class="form-group">
                                    <Rock:RockTextBox ID="rtbTermCompareValue" runat="server" Required="false" CssClass="input-width-xxl js-filter-control" ValidationGroup="vgPageViewFilterConfiguration" />
                                </div>
                            </div>
                        </div>

                    </asp:Panel>

                </Content>
            </Rock:ModalDialog>

            <%-- Modal for Interactions Filter --%>
            <Rock:ModalDialog ID="mdInteractionFilterConfiguration" runat="server" OnSaveClick="mdInteractionFilterConfiguration_SaveClick" ValidationGroup="vgInteractionFilterConfiguration">
                <Content>
                    <asp:HiddenField ID="hfInteractionFilterGuid" runat="server" />

                    <asp:ValidationSummary ID="vsInteractionFilterConfiguration" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgInteractionFilterConfiguration" />

                    <div class="row form-row d-flex flex-wrap align-items-center form-group">
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:RockDropDownList ID="ddlInteractionFilterComparisonType" CssClass="input-width-xl js-filter-compare" runat="server" ValidationGroup="vgInteractionFilterConfiguration" />
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:NumberBox ID="nbInteractionFilterCompareValue" runat="server" Required="true" CssClass="input-width-sm js-filter-control" ValidationGroup="vgInteractionFilterConfiguration" />
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <span class="text-nowrap">interactions in the channel/component</span>
                            </div>
                        </div>
                    </div>

                    <div class="row form-row">
                        <div class="col">
                            <Rock:InteractionChannelPicker ID="pInteractionFilterInteractionChannel" runat="server" Label="Channel" CssClass="input-width-xxl" ValidationGroup="vgInteractionFilterConfiguration" Required="true" AutoPostBack="true" OnSelectedIndexChanged="pInteractionFilterInteractionChannel_SelectedIndexChanged" />
                        </div>
                    </div>
                    <div class="row form-row">
                        <div class="col">
                            <Rock:InteractionComponentPicker ID="pInteractionFilterInteractionComponent" runat="server" Label="Component" CssClass="input-width-xxl" ValidationGroup="vgInteractionFilterConfiguration" Required="false" />
                        </div>
                    </div>

                    <div class="row form-row">
                        <div class="col">
                            <Rock:RockTextBox ID="tbInteractionFilterOperation" runat="server" Label="Operation" CssClass="input-width-xxl" Help="Examples: 'Viewed', 'Opened', 'Click', 'Prayed', 'Form Viewed', 'Form Completed'" />
                        </div>
                    </div>


                    <div class="row form-row d-flex flex-wrap align-items-center">
                        <div class="col flex-grow-0"><span class="text-nowrap">In the following date range</span></div>

                        <div class="col">
                            <Rock:SlidingDateRangePicker ID="drpInteractionFilterSlidingDateRange" runat="server" Label="" PreviewLocation="Right" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" ValidationGroup="vgInteractionFilterConfiguration" ToolTip="<div class='js-slidingdaterange-info'></>" />
                        </div>
                    </div>

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

        <script>
            function populateSegmentKey() {
                // if the segment key hasn't been filled in yet, populate it with the segment name minus whitespace and special chars
                var $keyControl = $('#<%=tbSegmentKey.ClientID%>');
                var keyValue = $keyControl.val();

                var reservedKeyJson = $('#<%=hfExistingSegmentKeyNames.ClientID%>').val();
                var reservedKeyNames = eval('(' + reservedKeyJson + ')');

                if ($keyControl.length && (keyValue == '')) {

                    keyValue = $('#<%=tbName.ClientID%>').val().replace(/[^a-zA-Z0-9_.\-]/g, '');
                    var newKeyValue = keyValue;

                    var i = 1;
                    while ($.inArray(newKeyValue, reservedKeyNames) >= 0) {
                        newKeyValue = keyValue + i++;
                    }

                    $keyControl.val(newKeyValue);
                }
            }

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
