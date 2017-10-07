<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationListSubscribe.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationListSubscribe" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bullhorn"></i>Email Preferences</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNoCommunicationLists" runat="server" NotificationBoxType="Info" Text="You are not subscribed to any communication lists." />

                <asp:Panel ID="pnlCommunicationPreferences" runat="server">
                    <div class="grid grid-panel margin-all-sm">
                        <div class="row">
                            <div class="col-xs-4">
                                <label>List</label>
                            </div>
                            <div class="col-xs-8">
                                <label>Communication Preference</label>
                            </div>
                        </div>

                        <asp:Repeater ID="rptCommunicationLists" runat="server" OnItemDataBound="rptCommunicationLists_ItemDataBound">
                            <ItemTemplate>
                                <div class="row">
                                    <div class="col-xs-4">
                                        <Rock:RockCheckBox ID="cbCommunicationListIsSubscribed" runat="server" />
                                    </div>
                                    <div class="col-xs-8">
                                        <Rock:RockRadioButtonList ID="rblCommunicationPreference" runat="server" RepeatDirection="Horizontal" CssClass="margin-t-sm">
                                            <asp:ListItem Text="Email" Value="1" />
                                            <asp:ListItem Text="SMS" Value="2" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                    </div>
                    
                </asp:Panel>

                <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" />

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
