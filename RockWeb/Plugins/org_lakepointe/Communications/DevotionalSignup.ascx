<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DevotionalSignup.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Communications.DevotionalSignup" %>


<asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <Rock:RockLiteral ID="lHeader" runat="server" />
        <asp:Panel ID="pnlView" runat="server">
            <Rock:NotificationBox ID="nbSubscriptions" runat="server" NotificationBoxType="Info" Visible="false" />
            <asp:Panel ID="pnlSubscriptions" runat="server">
                <div class="grid grid-panel margin-all-sm">
                    <asp:Repeater ID="rptSubscriptionGroups" runat="server" OnItemDataBound="rptSubscriptionGroups_ItemDataBound">
                        <ItemTemplate>
                            <div class="row">
                                <div class="col-xs-12">
                                    <asp:HiddenField ID="hfGroupId" runat="server" />
                                    <Rock:RockCheckBox ID="cbSubscriptionGroup" runat="server" AutoPostBack="true" OnCheckedChanged="cbSubscriptionGroup_CheckedChanged" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
