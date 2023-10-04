<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HealKeywordStatistics.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Reporting.HealKeywordStatistics" %>
<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Info" Visible="false" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fas fa-comment-alt-lines"></i><asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-xs-12">
                        <Rock:GridFilter ID="gfNeedTypes" runat="server">
                            <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                            <%--<Rock:RockCheckBoxList ID="cblRequests" runat="server" Label="Response Type" />--%>
                        </Rock:GridFilter>
                    </div>
                </div>

                <div class="row" style="padding-top: 40px;">
                    <div class="col-sm-2">
                        <span class="control-label">Submissions</span>
                    </div>
                    <div class="col-sm-10">
                        <asp:Literal ID="lSubmissions" runat="server" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-2 control-label ">
                        Multiple Items Selected
                    </div>
                    <div class="col-sm-10">
                        <asp:Literal ID="lMultipleItems" runat="server" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-2 control-label">
                        No Boxes Checked
                    </div>
                    <div class="col-sm-10">
                        <asp:Literal ID="lNoBoxesChecked" runat="server" />
                    </div>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gNeedTypes" runat="server" DataKeyNames="Guid" EmptyDataText="No Responses Submitted" RowItemText="Response" ExportSource="ColumnOutput">
                        <Columns>
                            <Rock:RockBoundField DataField="Guid" HeaderText="DefinedValueGuid" Visible="false" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Response Type" />
                            <Rock:RockBoundField DataField="Count" HeaderText="Responses" />
                        </Columns>
                    </Rock:Grid>

                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
