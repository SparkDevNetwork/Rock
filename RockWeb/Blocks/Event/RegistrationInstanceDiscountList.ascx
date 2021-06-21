<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceDiscountList.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceDiscountList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">


                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />

                <asp:Panel ID="pnlDiscounts" runat="server" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-gift"></i>
                            Discounts
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdDiscountsGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fDiscounts" runat="server" OnDisplayFilterValue="fDiscounts_DisplayFilterValue" OnClearFilterClick="fDiscounts_ClearFilterClick">
                                <Rock:SlidingDateRangePicker ID="sdrpDiscountDateRange" runat="server" Label="Discount Date Range" Help="To filter based on when the discount code was used." />
                                <Rock:RockDropDownList ID="ddlDiscountCode" runat="server" Label="Discount Code" AutoPostBack="true" OnSelectedIndexChanged="ddlDiscountCode_SelectedIndexChanged" EnhanceForLongLists="true"></Rock:RockDropDownList>
                                <Rock:RockTextBox ID="tbDiscountCodeSearch" runat="server" Label="Code Search" Help="Enter a search parameter. Cannot be used with the 'Discount Code' list." />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gDiscounts" runat="server" DisplayType="Full" AllowSorting="true" RowItemText="Discount" ExportSource="DataSource">
                                <Columns>
                                    <Rock:RockBoundField HeaderText="Registration ID" DataField="RegistrationId" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                    <Rock:RockBoundField HeaderText="Registered By" DataField="RegisteredByName" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" SortExpression="RegisteredByName" />
                                    <Rock:DateField HeaderText="Registration Date" DataField="RegistrationDate" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" SortExpression="RegistrationDate" />
                                    <Rock:RockBoundField HeaderText="Registrant Count" DataField="RegistrantCount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="RegistrantCount" />
                                    <Rock:RockBoundField HeaderText="Discount Code" DataField="DiscountCode" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" SortExpression="DiscountCode" />
                                    <Rock:RockBoundField HeaderText="Discount" DataField="Discount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="Discount" HtmlEncode="false" />
                                    <Rock:CurrencyField HeaderText="Total Cost" DataField="TotalCost" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="TotalCost" />
                                    <Rock:CurrencyField HeaderText="Discount Qualified Cost" DataField="DiscountQualifiedCost" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="DiscountQualifiedCost" />
                                    <Rock:CurrencyField HeaderText="Total Discount" DataField="TotalDiscount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="TotalDiscount" />
                                    <Rock:CurrencyField HeaderText="Registration Cost" DataField="RegistrationCost" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="RegistrationCost" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                        <br />
                        <div class="row">
                            <div class="col-md-4 col-md-offset-8 margin-t-md">
                                <asp:Panel ID="pnlDiscountSummary" runat="server" CssClass="panel panel-block">
                                    <div class="panel-heading">
                                        <h1 class="panel-title">Total Results</h1>
                                    </div>
                                    <div class="panel-body">
                                        <div class="row">
                                            <div class="col-xs-8">Total Cost</div>
                                            <div class='col-xs-4 text-right'>
                                                <asp:Literal ID="lTotalTotalCost" runat="server" /></div>
                                        </div>
                                        <div class="row">
                                            <div class="col-xs-8">Discount Qualified Cost</div>
                                            <div class='col-xs-4 text-right'>
                                                <asp:Literal ID="lTotalDiscountQualifiedCost" runat="server" /></div>
                                        </div>
                                        <div class="row">
                                            <div class="col-xs-8">Total Discount</div>
                                            <div class='col-xs-4 text-right'>
                                                <asp:Literal ID="lTotalDiscounts" runat="server" /></div>
                                        </div>
                                        <div class="row">
                                            <div class="col-xs-8">Registration Cost</div>
                                            <div class='col-xs-4 text-right'>
                                                <asp:Literal ID="lTotalRegistrationCost" runat="server" /></div>
                                        </div>
                                        <br />
                                        <div class="row">
                                            <div class="col-xs-8">Total Registrations</div>
                                            <div class='col-xs-4 text-right'>
                                                <asp:Literal ID="lTotalRegistrations" runat="server" /></div>
                                        </div>
                                        <div class="row">
                                            <div class="col-xs-8">Total Registrants</div>
                                            <div class='col-xs-4 text-right'>
                                                <asp:Literal ID="lTotalRegistrants" runat="server" /></div>
                                        </div>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                </asp:Panel>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
