<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarFilters.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Event.CalendarFilters" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col-md-12" style="padding-left:30px; padding-right:30px;">
                <asp:Panel ID="pnlAudience" runat="server" CssClass="col-md-6">
                    <Rock:RockDropDownList ID="ddlAudience" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlAudience_SelectedIndexChanged" />
                </asp:Panel>
                <asp:Panel ID="pnlCampus" runat="server" CssClass="col-md-6" >
                    <Rock:RockDropDownList ID="ddlCampus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlCampus_SelectedIndexChanged" />
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>