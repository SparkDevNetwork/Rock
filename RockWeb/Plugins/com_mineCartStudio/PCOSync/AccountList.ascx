<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountList.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.PCOSync.AccountList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-check-o"></i> Accounts</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gAccount" runat="server" AllowSorting="true" OnRowSelected="gAccount_Edit" TooltipField="Description" RowItemText="Account">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="GroupCount" HeaderText ="Active Groups" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" />
                            <Rock:RockBoundField DataField="MemberCount" HeaderText ="Active Members" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" />
                            <Rock:RockBoundField DataField="WelcomeEmailTemplate" HeaderText="Welcome Email" SortExpression="WelcomeEmailTemplate" />
                            <Rock:DeleteField OnClick="gAccount_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
