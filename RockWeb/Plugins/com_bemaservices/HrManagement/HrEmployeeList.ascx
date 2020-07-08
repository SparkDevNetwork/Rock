<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HrEmployeeList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.HrManagement.HrEmployeeList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlEmployeeList" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-clock"></i>&nbsp;HR Employee List</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:ModalAlert ID="maWarningDialog" runat="server" />
                        <Rock:GridFilter ID="gfEmployeeFilter" runat="server">
                            <Rock:NumberBox ID="nbFiscalYearEnd" runat="server" Label="Fiscal Year End" NumberType="Integer" MaximumValue="3000" MinimumValue="1900" />
                            <Rock:RockDropDownList ID="ddlPtoType" runat="server" Label="Pto Type" DataValueField="Id" DataTextField="Name" />
                            <Rock:RockTextBox ID="tbMinistryArea" runat="server" Label="Ministry Area" />
                            <Rock:PersonPicker ID="ppSupervisor" runat="server" Label="Supervisor" />
                        </Rock:GridFilter>

                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gEmployeeList" runat="server" RowItemText="Request" OnRowSelected="gEmployeeList_Edit" AllowSorting="true" CssClass="js-grid-allocation-list" OnRowDataBound="gEmployeeList_RowDataBound">
                            <Columns>
                                <Rock:RockLiteralField ID="lName" HeaderText="Name" SortExpression="Name" />
                                <Rock:RockLiteralField ID="lAllocation" HeaderText="Allocations" SortExpression="Allocations" />
                                <Rock:RockLiteralField ID="lTotalAccrued" HeaderText="Accrued Hours" SortExpression="TotalAccrued" />
                                <Rock:RockLiteralField ID="lTotalTaken" HeaderText="Taken Hours" SortExpression="TotalTaken" />
                                <Rock:RockLiteralField ID="lRemaining" HeaderText="Remaining Hours" SortExpression="Remaining" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
