<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceFeeList.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceFeeList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />

                <asp:Panel ID="pnlFees" runat="server" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-money"></i>
                            Fees
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdFeesGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fFees" runat="server" OnDisplayFilterValue="fFees_DisplayFilterValue" OnClearFilterClick="fFees_ClearFilterCick">
                                <Rock:SlidingDateRangePicker ID="sdrpFeeDateRange" runat="server" Label="Fee Date Range" />
                                <Rock:RockDropDownList ID="ddlFeeName" runat="server" Label="Fee Name" AutoPostBack="true" OnSelectedIndexChanged="ddlFeeName_SelectedIndexChanged"></Rock:RockDropDownList>
                                <Rock:RockCheckBoxList ID="cblFeeOptions" runat="server" Label="Fee Options"></Rock:RockCheckBoxList>
                            </Rock:GridFilter>
                            <Rock:Grid ID="gFees" runat="server" DisplayType="Full" AllowSorting="true" RowItemText="Fee" ExportSource="DataSource">
                                <Columns>
                                    <Rock:RockBoundField HeaderText="Registration ID" DataField="RegistrationId" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                    <Rock:DateField HeaderText="Registration Date" DataField="RegistrationDate" SortExpression="RegistrationDate" />
                                    <Rock:RockBoundField HeaderText="Registered By" DataField="RegisteredByName" SortExpression="RegisteredByName" />
                                    <Rock:RockBoundField HeaderText="Registrant" DataField="RegistrantName" SortExpression="RegistrantName" />
                                    <Rock:RockBoundField HeaderText="Registrant ID" DataField="RegistrantId" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                    <Rock:RockBoundField HeaderText="Fee Name" DataField="FeeName" SortExpression="FeeName" />
                                    <Rock:RockBoundField HeaderText="Option" DataField="FeeItemName" SortExpression="FeeItemName" />
                                    <Rock:RockBoundField HeaderText="Quantity" DataField="Quantity" SortExpression="Quantity" />
                                    <Rock:CurrencyField HeaderText="Cost" DataField="Cost" SortExpression="Cost" />
                                    <Rock:CurrencyField HeaderText="Fee Total" DataField="FeeTotal" SortExpression="FeeTotal" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
