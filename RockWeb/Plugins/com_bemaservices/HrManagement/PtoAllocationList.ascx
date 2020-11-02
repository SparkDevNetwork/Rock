<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PtoAllocationList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.HrManagement.PtoAllocationList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlPtoAllocationList" runat="server">
            <asp:HiddenField ID="hfAction" runat="server" />
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-archive"></i>&nbsp;PTO Allocation List</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:ModalAlert ID="maWarningDialog" runat="server" />
                        <Rock:GridFilter ID="gfPtoAllocationFilter" runat="server">
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                            <Rock:DateRangePicker ID="drpAllocationDate" runat="server" Label="Date Range" />
                            <Rock:RockDropDownList ID="ddlPtoType" runat="server" Label="PTO Type" DataValueField="Id" DataTextField="Name" />
                            <Rock:RockDropDownList ID="ddlSourceType" runat="server" Label="Source" />
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                        </Rock:GridFilter>

                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gPtoAllocationList" runat="server" RowItemText="Allocation" OnRowSelected="gPtoAllocationList_Edit" AllowSorting="true" CssClass="js-grid-allocation-list">
                            <Columns>
                                <Rock:SelectField />
                                <Rock:RockBoundField DataField="Name" HeaderText="Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.LastName" />
                                <Rock:RockBoundField DataField="PtoType" HeaderText="PTO Type" SortExpression="PtoType.Name" />
                                <Rock:RockBoundField DataField="SourceType" HeaderText="Source" SortExpression="PtoAllocationSourceType" />
                                <Rock:RockBoundField DataField="Hours" HeaderText="Total Hours" SortExpression="Hours" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                <Rock:DateField DataField="StartDate" HeaderText="Start Date" SortExpression="StartDate" />
                                <Rock:DateField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" />
                                <Rock:RockBoundField DataField="AccrualSchedule" HeaderText="Schedule" SortExpression="PtoAccrualSchedule" Visible="false" />
                                <Rock:RockLiteralField HeaderText="Status" ID="lAllocationStatus" SortExpression="Status" HeaderStyle-CssClass="grid-columnstatus" ItemStyle-CssClass="grid-columnstatus" FooterStyle-CssClass="grid-columnstatus" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" OnDataBound="lAllocationStatus_DataBound" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>

            <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" Dismissable="true"></Rock:NotificationBox>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
