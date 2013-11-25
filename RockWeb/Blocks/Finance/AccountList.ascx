<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountList.ascx.cs" Inherits="RockWeb.Blocks.Finance.AccountList" %>

<asp:UpdatePanel ID="pnlAccountListUpdatePanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <Rock:GridFilter ID="rAccountFilter" runat="server" >
            <Rock:RockTextBox ID="txtAccountName" runat="server" Label="Name" />
            <Rock:RockDropDownList ID="ddlIsActive" runat="server" Label="Active">
                <asp:ListItem Text="" Value="" />
                <asp:ListItem Text="Yes" Value="Yes" />
                <asp:ListItem Text="No" Value="No" />
            </Rock:RockDropDownList>
            <Rock:RockDropDownList ID="ddlIsTaxDeductible" runat="server" Label="Tax Deductible">
                <asp:ListItem Text="" Value="" />
                <asp:ListItem Text="Yes" Value="Yes" />
                <asp:ListItem Text="No" Value="No" />
            </Rock:RockDropDownList>
        </Rock:GridFilter>
        
        <Rock:Grid ID="rGridAccount" runat="server"  RowItemText="Account" OnRowSelected="rGridAccount_Edit" TooltipField="Description">
            <Columns>
                <Rock:ReorderField />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="PublicName" HeaderText="Public Name" SortExpression="PublicName" />
                <Rock:BoolField DataField="IsTaxDeductible" HeaderText="Tax Deductible" SortExpression="IsTaxDeductible" />
                <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                <asp:BoundField DataField="StartDate" HeaderText="Starts On" SortExpression="StartDate" DataFormatString="{0:d}" />
                <asp:BoundField DataField="EndDate" HeaderText="Ends On" SortExpression="EndDate" DataFormatString="{0:d}" />
                <Rock:DeleteField OnClick="rGridAccount_Delete" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
