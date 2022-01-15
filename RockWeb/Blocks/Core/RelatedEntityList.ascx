<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RelatedEntityList.ascx.cs" Inherits="RockWeb.Blocks.Core.RelatedEntityList" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lIcon" runat="server" /> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            
            <div class="panel-body">

                <Rock:NotificationBox id="nbMessages" runat="server" NotificationBoxType="Warning" />

                <asp:Literal ID="lContent" runat="server" />

                

            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="mdConfirmDelete" runat="server" Title="Please Confirm" SaveButtonText="Yes" OnSaveClick="mdConfirmDelete_Click">
            <Content>
                <asp:Literal ID="lConfirmDeleteMsg" runat="server" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdConfirmAdd" runat="server" Title="Please Confirm" SaveButtonText="Yes" OnSaveClick="mdConfirmAdd_SaveClick">
            <Content>
                <asp:Literal ID="lConfirmAddMsg" runat="server" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>


