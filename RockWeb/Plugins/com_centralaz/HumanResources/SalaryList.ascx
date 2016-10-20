<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SalaryList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.HumanResources.SalaryList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">


            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-folder-open"></i>Salary History</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gSalaries" runat="server" RowItemText="Salary" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Amount" HeaderText="Amount" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="HousingAllowance" HeaderText="Housing Allowance" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="FuelAllowance" HeaderText="Fuel Allowance" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="PhoneAllowance" HeaderText="Phone Allowance" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="EffectiveDate" HeaderText="Effective Date" />
                            <Rock:RockBoundField DataField="ReviewedDate" HeaderText="Reviewed Date" />
                            <Rock:EditField OnClick="gSalaries_Edit" />
                            <Rock:DeleteField OnClick="gSalaries_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdDetails" runat="server" Title="Salary" ValidationGroup="Salary">
            <Content>

                <asp:HiddenField ID="hfIdValue" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbSalaried" Label="Salaried Employee" runat="server" Required="true" />
                        <Rock:DatePicker ID="dpEffectiveDate" Label="Effective Date" runat="server" Required="true" />
                        <Rock:DatePicker ID="dpReviewedDate" Label="Reviewed Date" runat="server" />
                    </div>
                    <div class="col-md-6">
                        <Rock:CurrencyBox ID="nbAmount" runat="server" NumberType="Currency" MinimumValue="0" SourceTypeName="com.centralaz.HumanResources.Salary, com.centralaz.HumanResources" PropertyName="Amount" Required="true" Label="Amount" />
                        <Rock:CurrencyBox ID="nbHousing" runat="server" NumberType="Currency" MinimumValue="0" SourceTypeName="com.centralaz.HumanResources.Salary, com.centralaz.HumanResources" PropertyName="HousingAllowance" Label="Housing Allowance" />
                        <Rock:CurrencyBox ID="nbFuel" runat="server" NumberType="Currency" MinimumValue="0" SourceTypeName="com.centralaz.HumanResources.Salary, com.centralaz.HumanResources" PropertyName="FuelAllowance" Label="Fuel Allowance" />
                        <Rock:CurrencyBox ID="nbPhone" runat="server" NumberType="Currency" MinimumValue="0" SourceTypeName="com.centralaz.HumanResources.Salary, com.centralaz.HumanResources" PropertyName="PhoneAllowance" Label="Phone Allowance" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
