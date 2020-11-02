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
                            <Rock:RockDropDownList ID="ddlFiscalYearEnd" runat="server" Label="Fiscal Year End" />
                            <Rock:RockDropDownList ID="ddlPtoType" runat="server" Label="PTO Type" DataValueField="Id" DataTextField="Name" />
                            <Rock:RockTextBox ID="tbMinistryArea" runat="server" Label="Ministry Area" />
                            <Rock:PersonPicker ID="ppSupervisor" runat="server" Label="Supervisor" />
                            <Rock:RockCheckBox ID="cbShowUnallocatedPtoTypes" runat="server" Label="Show Unallocated PTO Types" />
                        </Rock:GridFilter>

                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gEmployeeList" runat="server" RowItemText="Request" OnRowSelected="gEmployeeList_Edit" AllowSorting="true" CssClass="js-grid-allocation-list" OnRowDataBound="gEmployeeList_RowDataBound">
                            <Columns>
                                <Rock:RockLiteralField ID="lName" HeaderText="Name" SortExpression="Name" />
                                <Rock:RockLiteralField ID="lSupervisor" HeaderText="Supervisor" />
                                <Rock:RockLiteralField ID="lMinistryArea" HeaderText="Ministry Area" />
                                <Rock:RockLiteralField ID="lAllocation" HeaderText="Allocations" />
                                <Rock:RockLiteralField ID="lTotalAccrued" HeaderText="Accrued Hours" />
                                <Rock:RockLiteralField ID="lTotalTaken" HeaderText="Taken Hours" />
                                <Rock:RockLiteralField ID="lRemaining" HeaderText="Remaining Hours" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
