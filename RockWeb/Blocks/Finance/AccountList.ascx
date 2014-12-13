<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountList.ascx.cs" Inherits="RockWeb.Blocks.Finance.AccountList" %>

<asp:UpdatePanel ID="pnlAccountListUpdatePanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i> Account List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rAccountFilter" runat="server" >
                        <Rock:RockTextBox ID="txtAccountName" runat="server" Label="Name" />
                        <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" Visible="false" />
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
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="PublicName" HeaderText="Public Name" SortExpression="PublicName" />
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus" Visible="false" />
                            <Rock:BoolField DataField="IsTaxDeductible" HeaderText="Tax Deductible" SortExpression="IsTaxDeductible" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:RockBoundField DataField="StartDate" HeaderText="Starts On" SortExpression="StartDate" DataFormatString="{0:d}" />
                            <Rock:RockBoundField DataField="EndDate" HeaderText="Ends On" SortExpression="EndDate" DataFormatString="{0:d}" />
                            <Rock:DeleteField OnClick="rGridAccount_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
