<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RetirementFundList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.HumanResources.RetirementFundList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">


            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-folder-open"></i>Retirement Funds</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gRetirementFunds" runat="server" RowItemText="Retirement Fund" TooltipField="Description" OnRowDataBound="gRetirementFunds_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="EmployeeAmount" HeaderText="Employee Amount" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="EmployerAmount" HeaderText="Employer Amount" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="ActiveDate" HeaderText="Active Date" />
                            <Rock:RockBoundField DataField="InactiveDate" HeaderText="Inactive Date" />
                            <Rock:EditField OnClick="gRetirementFunds_Edit" />
                            <Rock:DeleteField OnClick="gRetirementFunds_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdDetails" runat="server" Title="Retirement Fund" ValidationGroup="RetirementFund">
            <Content>

                <asp:HiddenField ID="hfIdValue" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DefinedValuePicker ID="dvpFund" Label="Retirement Fund" runat="server" Required="true" />
                        <Rock:DatePicker ID="dpActiveDate" Label="Active Date" runat="server" Required="true" />
                        <Rock:DatePicker ID="dpInactiveDate" Label="Inactive Date" runat="server" />
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbEmployeeAmount" runat="server" MinimumValue="0" SourceTypeName="com.centralaz.HumanResources.RetirementFund, com.centralaz.HumanResources" PropertyName="EmployeeAmount" Required="true" Label="Employee Amount" />
                        <Rock:NumberBox ID="nbEmployerAmount" runat="server" MinimumValue="0" SourceTypeName="com.centralaz.HumanResources.RetirementFund, com.centralaz.HumanResources" PropertyName="EmployerAmount" Label="Employer Amount" />
                        <Rock:RockCheckBox ID="cbFixedAmount" Label="Fixed Amount" runat="server" Required="true" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
