<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InternalCommunicationView.ascx.cs" Inherits="RockWeb.Blocks.Utility.InternalCommunicationView" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lBlockTitleIcon" runat="server" /> 
                    <asp:Literal ID="lBlockTitle" runat="server" />
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessages" runat="server" />
                
                <asp:Literal ID="lBlockBody" runat="server" />

                <asp:HiddenField ID="hfCurrentPage" runat="server" />

                <div class="actions">
                    <asp:LinkButton ID="btnPrevious" runat="server" OnClick="btnPrevious_Click" CssClass="btn btn-xs btn-default"><i class="fa fa-chevron-left"></i> Prev</asp:LinkButton>
                    <asp:LinkButton ID="btnNext" runat="server" OnClick="btnNext_Click" CssClass="btn btn-xs btn-default pull-right">Next <i class="fa fa-chevron-right"></i></asp:LinkButton>
                </div>
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>