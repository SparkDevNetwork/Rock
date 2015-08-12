<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerbookHomePageSidebar.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Prayerbook.PrayerbookHomePageSidebar" %>

<Rock:RockLiteral ID="lActiveBook" runat="server" Text="" Label="Now accepting submissions for: " />

<div class="actions">
    <asp:Button ID="bbtnAddNewRequest" runat="server" Text="" OnClick="bbtnAddNewRequest_Click" CssClass="btn btn-primary btn-sm" />
</div>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:RockDropDownList ID="ddlBooks" runat="server" Label="View entries for book: "
            DataTextField="Name" DataValueField="Id"
            OnSelectedIndexChanged="ddlBooks_SelectedIndexChanged" AutoPostBack="true" />

        <Rock:Grid ID="gBookEntries" runat="server" Caption="Select row to edit or view." EmptyDataText="There are currently no requests."
            DataKeyNames="Id" ShowActionRow="false" OnRowSelected="gBookEntries_RowSelected">
            <Columns>
                <Rock:RockBoundField DataField="ModifiedDateTime" HeaderText="Last Modified" DataFormatString="{0:d}" />
                <Rock:RockBoundField DataField="Person.FullName" HeaderText="Author" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
