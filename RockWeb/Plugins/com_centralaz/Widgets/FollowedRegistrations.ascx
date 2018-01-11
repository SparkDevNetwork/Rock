<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FollowedRegistrations.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.FollowedRegistrations" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class='panel panel-block'>
            <div class='panel-heading'>
                <h4 class='panel-title'><i class='fa fa-flag'></i> Followed Registrations</h4>
                <div class="pull-right">
                    <asp:LinkButton ID="lbEdit" runat="server" CssClass="fa fa-cog" OnClick="lbEdit_Click" />
                </div>
            </div>

            <Rock:NotificationBox runat="server" ID="nbError" NotificationBoxType="Danger"></Rock:NotificationBox>
            <asp:Panel ID="pnlView" runat="server" Visible="true" class='panel-body'>

                <asp:Literal ID="lContent" runat="server"></asp:Literal>

                <asp:Literal ID="lDebug" runat="server" Visible="false"></asp:Literal>

            </asp:Panel>
            <asp:Panel ID="pnlEdit" runat="server" Visible="false" CssClass="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <h3>Name</h3>
                    </div>
                    <div class="col-md-6">
                        <h3>Fields to subtotal</h3>
                    </div>
                </div>
                <asp:Repeater ID="rptFollowedRegistrations" runat="server" OnItemDataBound="rptFollowedRegistrations_ItemDataBound">
                    <ItemTemplate>
                        <div class="row">
                            <asp:HiddenField ID="hfRegistrationId" runat="server" />
                            <asp:HiddenField ID="hfTemplateId" runat="server" />
                            <div class="col-md-6">
                                <asp:Literal ID="lName" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <asp:CheckBoxList ID="cblValues" runat="server" RepeatDirection="Vertical" />
                                <asp:CheckBoxList ID="cblFeeValues" runat="server" RepeatDirection="Vertical" />
                            </div>
                        </div>
                        <br />
                    </ItemTemplate>
                </asp:Repeater>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>
            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
