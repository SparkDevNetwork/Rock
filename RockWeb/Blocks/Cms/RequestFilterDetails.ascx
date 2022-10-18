<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RequestFilterDetails.ascx.cs" Inherits="RockWeb.Blocks.Cms.RequestFilterDetails" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfRequestFilterId" runat="server" />
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-user-tag"></i>
                    <asp:Literal ID="lPanelTitle" runat="server" Text="" />

                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <%-- Request Filter Name --%>
                <div class="row mb-5">
                    <div class="col-md-6">
                        <Rock:HiddenFieldWithClass ID="hfExistingRequestFilterKeyNames" runat="server" CssClass="js-existing-key-names" />
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.RequestFilter, Rock" PropertyName="Name" onblur="populateRequestFilterKey()" />
                        <Rock:DataTextBox ID="tbKey" runat="server" SourceTypeName="Rock.Model.RequestFilter, Rock" Required="true" PropertyName="RequestFilterKey" Label="Key" />
                        <Rock:RockDropDownList ID="ddlSiteKey" runat="server" SourceTypeName="Rock.Model.RequestFilter, Rock" PropertyName="SiteId"
                            Label="Site" Help="Site - Optional site to limit the request filter to." />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                    </div>
                </div>

                <%-- Previous Activity --%>
                <asp:Panel ID="pnlPreviousActivity" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">Previous Activity</div>
                    </div>
                    <div class="panel-body">
                        <Rock:RockCheckBoxList ID="cblPreviousActivity" runat="server" Label="Visitor Type" RepeatDirection="Horizontal" />
                    </div>
                </asp:Panel>

                <span class="segment-and"><span class="text">And</span></span>

                <%-- Device Types --%>
                <asp:Panel ID="pnlDeviceType" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">Device Types</div>
                    </div>
                    <div class="panel-body">
                        <Rock:RockCheckBoxList ID="cblDeviceTypes" runat="server" Label="Device Type" RepeatDirection="Horizontal" />
                    </div>
                </asp:Panel>

                <span class="segment-and"><span class="text">And</span></span>

                <%-- Query String Filter --%>
                <asp:Panel ID="pnlQueryStringFilter" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title pull-left">Query String Filter</div>
                        <Rock:Toggle ID="tglQueryStringFiltersAllAny" runat="server" OnText="All" OffText="Any" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" CssClass="panel-title pull-right" />
                        <div class="clearfix"></div>
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gQueryStringFilter" runat="server" DisplayType="Light" RowItemText="Query String Filter">
                            <Columns>
                                <Rock:RockBoundField DataField="Key" HeaderText="Key" />
                                <Rock:EnumField DataField="ComparisonType" HeaderText="Match Type" />
                                <Rock:RockBoundField DataField="ComparisonValue" HeaderText="Value" />
                                <Rock:EditField OnClick="gQueryStringFilter_EditClick" />
                                <Rock:DeleteField OnClick="gQueryStringFilter_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <span class="segment-and"><span class="text">And</span></span>

                <%-- Cookie --%>
                <asp:Panel ID="pnlCookie" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title pull-left">Cookie</div>
                        <Rock:Toggle ID="tglCookiesAllAny" runat="server" OnText="All" OffText="Any" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" CssClass="panel-title pull-right" />
                        <div class="clearfix"></div>
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gCookie" runat="server" DisplayType="Light" RowItemText="Cookie">
                            <Columns>
                                <Rock:RockBoundField DataField="Key" HeaderText="Key" />
                                <Rock:EnumField DataField="ComparisonType" HeaderText="Match Type" />
                                <Rock:RockBoundField DataField="ComparisonValue" HeaderText="Value" />
                                <Rock:EditField OnClick="gCookie_EditClick" />
                                <Rock:DeleteField OnClick="gCookie_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <span class="segment-and"><span class="text">And</span></span>

                <%-- Browser --%>
                <asp:Panel ID="pnlBrowser" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">Browser</div>
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gBrowser" runat="server" DisplayType="Light" RowItemText="Browser">
                            <Columns>
                                <Rock:RockBoundField DataField="BrowserFamily" HeaderText="Browser Family" />
                                <Rock:EnumField DataField="VersionComparisonType" HeaderText="Match Type" />
                                <Rock:RockBoundField DataField="MajorVersion" HeaderText="Major Version" />
                                <Rock:EditField OnClick="gBrowser_EditClick" />
                                <Rock:DeleteField OnClick="gBrowser_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <span class="segment-and"><span class="text">And</span></span>

                <%-- IP Addresses --%>
                <asp:Panel ID="pnlIpAddress" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">IP Addresses</div>
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gIPAddress" runat="server" DisplayType="Light" RowItemText="IP Address">
                            <Columns>
                                <Rock:RockBoundField DataField="BeginningIPAddress" HeaderText="Beginning Address" />
                                <Rock:RockBoundField DataField="EndingIPAddress" HeaderText="Ending Address" />
                                <Rock:EnumField DataField="MatchType" HeaderText="Match Type" />
                                <Rock:EditField OnClick="gIpAddress_EditClick" />
                                <Rock:DeleteField OnClick="gIpAddress_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>

            <%-- Modal for Query String Filter --%>
            <Rock:ModalDialog ID="mdQueryStringFilter" runat="server" OnSaveClick="mdQueryStringFilter_SaveClick" ValidationGroup="vgQueryStringFilter">
                <Content>
                    <asp:HiddenField ID="hfQueryStringFilterGuid" runat="server" />

                    <asp:ValidationSummary ID="vsQueryFilterString" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgQueryFilterString" />

                    <div class="row form-row d-flex flex-wrap align-items-center">
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <span class="text-nowrap">Where the parameter</span>
                            </div>
                        </div>
                        <div class="col">
                            <div class="form-group">
                                <Rock:RockTextBox ID="tbQueryStringFilterParameter" runat="server" CssClass="" ValidationGroup="vgQueryStringFilter" />
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:RockDropDownList ID="ddlQueryStringFilterComparisonType" runat="server" CssClass="input-width-lg js-filter-compare" ValidationGroup="vgQueryStringFilter" />
                            </div>
                        </div>
                    </div>
                    <div class="row form-row d-flex flex-wrap align-items-center">
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <span class="text-nowrap">the value</span>
                            </div>
                        </div>
                        <div class="col">
                            <div class="form-group">
                                <Rock:RockTextBox ID="tbQueryStringFilterComparisonValue" runat="server" CssClass="js-filter-control" ValidationGroup="vgQueryStringFilter" />
                            </div>
                        </div>
                    </div>
                </Content>
            </Rock:ModalDialog>

            <%-- Modal for Cookie --%>
            <Rock:ModalDialog ID="mdCookie" runat="server" OnSaveClick="mdCookie_SaveClick" ValidationGroup="vgCookie">
                <Content>
                    <asp:HiddenField ID="hfCookieFilterGuid" runat="server" />

                    <asp:ValidationSummary ID="vsCookie" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgCookie" />

                    <div class="row form-row d-flex flex-wrap align-items-center">
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <span class="text-nowrap">Where the key</span>
                            </div>
                        </div>
                        <div class="col">
                            <div class="form-group">
                                <Rock:RockTextBox ID="tbCookieParameter" runat="server" CssClass="" ValidationGroup="vgCookie" />
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:RockDropDownList ID="ddlCookieFilterComparisonType" runat="server" CssClass="input-width-lg js-filter-compare" ValidationGroup="vgCookie" />
                            </div>
                        </div>
                    </div>

                    <div class="row form-row d-flex flex-wrap align-items-center">
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <span class="text-nowrap">the value</span>
                            </div>
                        </div>
                        <div class="col">
                            <div class="form-group">
                                <Rock:RockTextBox ID="tbCookieFilterComparisonValue" runat="server" ValidationGroup="vgCookie" CssClass="js-filter-control" />
                            </div>
                        </div>
                    </div>
                </Content>
            </Rock:ModalDialog>

            <%-- Modal for Browser Filter --%>
            <Rock:ModalDialog ID="mdBrowser" runat="server" OnSaveClick="mdBrowser_SaveClick" ValidationGroup="vgBrowser">
                <Content>
                    <asp:HiddenField ID="hfBrowserFilterGuid" runat="server" />

                    <asp:ValidationSummary ID="vsBrowser" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgBrowser" />

                    <div class="row form-row d-flex flex-wrap align-items-center">
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <span class="text-nowrap">Where</span>
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:RockDropDownList ID="ddlBrowserFamily" runat="server" CssClass="input-width-lg" ValidationGroup="vgBrowser" />
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <span class="text-nowrap">version</span>
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:RockDropDownList ID="ddlBrowserVersionComparisonType" runat="server" CssClass="input-width-lg js-filter-compare" ValidationGroup="vgBrowser" />
                            </div>
                        </div>
                        <div class="col">
                            <div class="form-group">
                                <Rock:NumberBox ID="nbBrowserVersionCompareValue" runat="server" CssClass="js-filter-control" ValidationGroup="vgBrowser" Required="true" NumberType="Integer" />
                            </div>
                        </div>
                    </div>
                </Content>
            </Rock:ModalDialog>

            <%-- Modal for IP Address Filter --%>
            <Rock:ModalDialog ID="mdIPAddress" runat="server" OnSaveClick="mdIpAddress_SaveClick" ValidationGroup="vgIPAddress">
                <Content>
                    <asp:HiddenField ID="hfIPAddressFilterGuid" runat="server" />

                    <asp:ValidationSummary ID="vsIPAddress" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgIPAddress" />

                    <div class="row form-row d-flex flex-wrap align-items-center">
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <span class="text-nowrap">Where the client IP is</span>
                            </div>
                        </div>
                        <div class="col flex-grow-0">
                            <div class="form-group">
                                <Rock:Toggle ID="tglIPAddressRange" runat="server" ButtonGroupCssClass="d-flex" OnText="Not in Range" OffText="In Range"
                                    ActiveButtonCssClass="btn-primary" ValidationGroup="vgIPAddress" />
                            </div>
                        </div>
                        <div class="col-xs-12 col-sm">
                            <div class="form-group">
                                <Rock:RockTextBox ID="tbIPAddressStartRange" runat="server" CssClass="" ValidationGroup="vgIPAddress" />
                            </div>
                        </div>
                        <div class="col-xs-12 col-sm">
                            <div class="form-group">
                                <Rock:RockTextBox ID="tbIPAddressEndRange" runat="server" CssClass="" ValidationGroup="vgIPAddress" />
                            </div>
                        </div>
                    </div>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

         <script>
            function populateRequestFilterKey() {
                // if the request filter key hasn't been filled in yet, populate it with the segment name minus whitespace and special chars
                var $keyControl = $('#<%=tbKey.ClientID%>');
                var keyValue = $keyControl.val();

                var reservedKeyJson = $('#<%=hfExistingRequestFilterKeyNames.ClientID%>')
                    .val();
                var reservedKeyNames = eval(`( ${reservedKeyJson} )`);

                if ($keyControl.length && (keyValue == '')) {

                    keyValue = $('#<%=tbName.ClientID%>')
                        .val()
                        .replace(/[^a-zA-Z0-9_.\-]/g, '');
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
