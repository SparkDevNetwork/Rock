<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRoleChanger.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Groups.GroupRoleChanger" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox runat="server" ID="nbSuccess" Visible="false" NotificationBoxType="Success" Text="GroupTypeRole successfully removed."></Rock:NotificationBox>
        <Rock:NotificationBox runat="server" ID="nbWarning" Visible="false" NotificationBoxType="Warning" Text=""></Rock:NotificationBox>
        <Rock:NotificationBox runat="server" ID="nbError" Visible="false" NotificationBoxType="Danger" Text=""></Rock:NotificationBox>

        <Rock:RockDropDownList runat="server" ID="ddlGroupTypes" Label="Group Type" DataValueField="Id" DataTextField="Name"
            AutoPostBack="true" OnSelectedIndexChanged="ddlGroupTypes_SelectedIndexChanged" />
        <div class="row">
            <div class="col-sm-6">
                <Rock:RockDropDownList ID="ddlOriginalRole" runat="server" Label="Group Role to Remove" DataValueField="Id" DataTextField="Name" AutoPostBack="true" OnSelectedIndexChanged="ddlOriginalRole_SelectedIndexChanged" />
                <Rock:RockLiteral ID="lMemberSummary" runat="server" />
            </div>
            <div class="col-sm-6">
                <Rock:RockDropDownList ID="ddlNewRole" runat="server" Label="Substitute Group Role" DataValueField="Id" DataTextField="Name" AutoPostBack="true" OnSelectedIndexChanged="ddlNewRole_SelectedIndexChanged" />
            </div>
        </div>

        <Rock:BootstrapButton runat="server" ID="btnSave" CssClass="btn btn-success" Text="Save"
            Enabled="false" OnClick="btnSave_Click"></Rock:BootstrapButton>

    </ContentTemplate>
</asp:UpdatePanel>
