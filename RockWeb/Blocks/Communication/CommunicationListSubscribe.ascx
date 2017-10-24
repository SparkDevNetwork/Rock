<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationListSubscribe.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationListSubscribe" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bullhorn"></i>Email Preferences</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNoCommunicationLists" runat="server" NotificationBoxType="Info" Text="You are not subscribed to any communication lists." Visible="false" />
                
                <asp:Panel ID="pnlCommunicationPreferences" runat="server">
                    <div class="grid grid-panel margin-all-sm">
                        <asp:Repeater ID="rptCommunicationLists" runat="server" OnItemDataBound="rptCommunicationLists_ItemDataBound">
                            <ItemTemplate>
                                <div class="row">
                                    <div class="col-xs-6">
                                        <asp:HiddenField ID="hfGroupId" runat="server" />
                                        <Rock:RockCheckBox ID="cbCommunicationListIsSubscribed" runat="server" AutoPostBack="true" OnCheckedChanged="cbCommunicationListIsSubscribed_CheckedChanged" />
                                        <Rock:NotificationBox ID="nbGroupNotification" runat="server" Visible="false" />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:Toggle ID="tglCommunicationPreference" runat="server" OnText="Email" OffText="SMS" OnCheckedChanged="tglCommunicationPreference_CheckedChanged" />
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
