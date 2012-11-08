<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Bio.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.Bio" %>

<aside class="bio">             
    <div class="content">

        <asp:PlaceHolder ID="phImage" runat="server"></asp:PlaceHolder>

        <section class="group">
            <span class="member-status"><asp:Literal ID="lPersonStatus" runat="server" /></span>
            <span class="record-status<%= (RecordStatus == "Inactive" ? " inactive" : "") %>"><%= RecordStatus %></span>

            <span class="campus"><asp:Literal ID="lCampus" runat="server"></asp:Literal></span>
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

