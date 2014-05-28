<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupContextSetter.ascx.cs" Inherits="RockWeb.Blocks.Core.GroupContextSetter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <input type="hidden" id="rock_group_context" name="context" />

        <div class="group-context-setter" style="width:300px">

            <Rock:NotificationBox runat="server" ID="nbSelectGroupTypeWarning" NotificationBoxType="Warning" Text="Select a group type or root group from the block settings" />

            <Rock:RockDropDownList runat="server" ID="ddlGroup" AutoPostBack="true" onchange="$('#rock_group_context').val($(this).val())" OnSelectedIndexChanged="ddlGroup_SelectedIndexChanged" />

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
