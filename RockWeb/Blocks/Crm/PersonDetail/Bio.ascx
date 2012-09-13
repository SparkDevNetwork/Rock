<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Bio.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.Bio" %>

<aside class="bio">             
    <div class="content">

<%--        <img src="/RockWeb/Assets/Mockup/jon.jpg" alt="Jon Edmiston" />--%>

        <section class="group">
            <span class="member-status"><asp:Literal ID="lPersonStatus" runat="server" /></span>
            <span class="record-status<%= (RecordStatus == "Inactive" ? " inactive" : "") %>"><%= RecordStatus %></span>

            <span class="campus">TODO: Campus</span>
            <span class="area">TODO: Area</span>
        </section>

        <section class="group">
            <span class="age"><asp:Literal ID="lAge" runat="server" /></span>
            <span class="gender"><asp:Literal ID="lGender" runat="server" /></span>
            <span class="marital-status"><asp:Literal ID="lMaritalStatus" runat="server" /> <asp:Literal ID="lAnniversary" runat="server" /></span>
        </section>
    </div>
    <footer>
        <div class="left"></div>
        <div class="center"></div>
        <div class="right"></div>
    </footer>
</aside>

