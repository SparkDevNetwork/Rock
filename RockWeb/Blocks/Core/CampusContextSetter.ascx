<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusContextSetter.ascx.cs" Inherits="RockWeb.Blocks.Core.CampusContextSetter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="ddlCampus" />
    </Triggers>
    <ContentTemplate>
        <input type="hidden" id="rock_page_campus_context" name="context" />

        <div class="campus-context-setter">
            <Rock:RockDropDownList runat="server" ID="ddlCampus" AutoPostBack="true" onchange="$('#rock_page_campus_context').val($(this).val())" OnSelectedIndexChanged="ddlCampus_SelectedIndexChanged" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
