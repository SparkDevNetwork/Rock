<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KnownRelationshipPhotos.ascx.cs" Inherits="RockWeb.Plugins.com_kfs.CheckIn.KnownRelationshipPhotos" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel-heading rollover-container clearfix">
                <h3 class="panel-title pull-left">
                    <asp:Literal ID="lGroupTypeIcon" runat="server"></asp:Literal>
                    <asp:Literal ID="lGroupName" runat="server"></asp:Literal></h3>
            </div>
        <div class="container-fluid">
            <asp:Literal ID="lAccessWarning" runat="server" />
            <asp:Repeater ID="rGroupMembers" runat="server">
                <ItemTemplate>
                    <div id="person" runat="server" class="col-sm-2 col-xs-6">
                        <a runat="server" id="personLink">
                            <div class="person-image"><img runat="server" id="imgPersonImage" class="img-responsive center-block" /></div>
                            <div class="person-info text-center h4"><%# Eval("Person.FullName") %></div>
                            <div class="person-role text-center"><%# ShowRole ? Eval("GroupRole.Name") : "" %></div>
                        </a>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
