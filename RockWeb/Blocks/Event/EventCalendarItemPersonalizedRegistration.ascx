<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventCalendarItemPersonalizedRegistration.ascx.cs" Inherits="RockWeb.Blocks.Event.EventCalendarItemPersonalizedRegistration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Registration</h1>

                <div class="panel-labels form-horizontal">
                    <Rock:CampusPicker ID="cpCampus" FormGroupCssClass="form-group-sm" Style="width: 170px;" runat="server" />
                </div>
            </div>
            <div class="panel-body">
                <asp:Literal ID="lErrors" runat="server" />
                
                <h1 class="margin-t-none"><asp:Literal ID="lName" runat="server" /></h1>

                <Rock:RockCheckBoxList ID="cblRegistrants" runat="server" Label="Register" RepeatDirection="Horizontal" />

                <asp:Literal ID="lEventIntro" runat="server" />
                <asp:PlaceHolder ID="phEvents" runat="server" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
