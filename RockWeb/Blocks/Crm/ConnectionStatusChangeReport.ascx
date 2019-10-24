<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionStatusChangeReport.ascx.cs" Inherits="RockWeb.Blocks.Crm.ConnectionStatusChangeReport" %>
<%@ Register TagPrefix="Rock" Namespace="Rock.Web.UI.Controls" Assembly="Rock" %>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading clearfix">
                <h1 class="panel-title pull-left"><i class="fa fa-exchange-alt"></i>Connection Status Changes</h1>
            </div>

            <div class="panel-body" runat="server" id="pnlBody">

                <Rock:NotificationBox ID="nbNotice" runat="server" />
                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following:" CssClass="alert alert-danger" />

                <%-- Settings Panel --%>
                <asp:Panel ID="pnlSettings" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:SlidingDateRangePicker ID="drpDateRange" runat="server" Label="Date Range"
                                Help="If selected, only changes within this time period will be shown." />

                        </div>

                        <div class="col-md-6">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" AllowMultiSelect="false" OnSelectItem="gpGroup_OnSelectItem"
                                Help="If selected, only people from this campus will be shown." />

                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlFromConnectionStatus" runat="server" Label="Original Status"
                                Help="If selected, only changes from this connection status will be shown." />

                        </div>

                        <div class="col-md-6">

                            <Rock:RockDropDownList ID="ddlToConnectionStatus" runat="server" Label="Updated Status"
                                Help="If selected, only changes to this connection status will be shown." />
                        </div>
                    </div>

                </asp:Panel>
                <div>
                    <div class="actions">
                        <asp:LinkButton ID="btnApplyFilter" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="lbApplyFilter_Click" />
                    </div>
                </div>
            </div>
        </div>

        <%-- Results Panel --%>
        <asp:Panel ID="pnlResults" runat="server">

            <div class="panel panel-block">

                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left"><i class="fa fa-list"></i>Results</h1>
                </div>

                <div class="panel-body">
                    <script>
                        Sys.Application.add_load(function () {
                            $("div.photo-round").lazyload({
                                effect: "fadeIn"
                            });
                        });
                    </script>

                    <div class="grid grid-panel">
                        <Rock:Grid ID="gChanges" runat="server" EmptyDataText="No Connection Status Changes Found" AllowSorting="true" OnRowSelected="gEvents_OnRowSelected">
                            <Columns>
                                <Rock:SelectField />
                                <Rock:RockTemplateField HeaderText="Name" SortExpression="LastName,FirstName">
                                    <ItemTemplate>
                                        <asp:Literal ID="lPerson" runat="server" />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:DateField
                                    DataField="DateChanged"
                                    HeaderText="Change Date"
                                    SortExpression="DateChanged desc"
                                    ColumnPriority="Desktop" />
                                <Rock:RockBoundField
                                    DataField="ChangedBy"
                                    SortExpression="ChangedBy"
                                    HeaderText="Changed By"
                                    ColumnPriority="Desktop" />
                                <Rock:RockBoundField
                                    DataField="OriginalStatus"
                                    SortExpression="OriginalStatus"
                                    HeaderText="Original Status"
                                    ColumnPriority="Desktop" />
                                <Rock:RockBoundField
                                    DataField="UpdatedStatus"
                                    SortExpression="UpdatedStatus"
                                    HeaderText="Updated Status"
                                    ColumnPriority="Desktop" />

                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

