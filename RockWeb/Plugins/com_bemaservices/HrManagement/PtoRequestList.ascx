<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PtoRequestList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.HrManagement.PtoRequestList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlPtoRequestList" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-clock"></i>&nbsp;PTO Request List</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:ModalAlert ID="maWarningDialog" runat="server" />
                        <Rock:GridFilter ID="gfPtoRequestFilter" runat="server">
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                            <Rock:DateRangePicker ID="drpRequestDate" runat="server" Label="Date Range" />
                            <Rock:RockDropDownList ID="ddlPtoType" runat="server" Label="Pto Type" />
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                        </Rock:GridFilter>

                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gPtoRequestList" runat="server" RowItemText="Request" OnRowSelected="gPtoRequestList_Edit" AllowSorting="true" CssClass="js-grid-allocation-list">
                            <Columns>
                                <Rock:SelectField />
                                <Rock:RockBoundField DataField="Name" HeaderText="Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.LastName" />
                                <Rock:RockBoundField DataField="PtoType" HeaderText="PTO Type" SortExpression="PtoType.Name" />
                                <Rock:RockBoundField DataField="RequestDate" HeaderText="Request Date" SortExpression="RequestDate" />
                                <Rock:RockBoundField DataField="Reason" HeaderText="Reason" SortExpression="Reason" />
                                <Rock:RockBoundField DataField="Hours" HeaderText="Total Hours" SortExpression="Hours" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                <Rock:RockLiteralField HeaderText="Status" ID="lRequestStatus" SortExpression="Status" HeaderStyle-CssClass="grid-columnstatus" ItemStyle-CssClass="grid-columnstatus" FooterStyle-CssClass="grid-columnstatus" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" OnDataBound="lRequestStatus_DataBound" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
