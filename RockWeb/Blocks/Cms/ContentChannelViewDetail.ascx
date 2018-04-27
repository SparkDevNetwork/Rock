<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelViewDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelViewDetail" %>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <%-- Custom Block Settings --%>
        <asp:Panel ID="pnlSettings" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdSettings" runat="server" OnSaveClick="mdSettings_SaveClick">
                <Content>
                    <div class="row">
                        <div class="col-md-6">
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>
                </Content>

            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
