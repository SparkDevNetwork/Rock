<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRoleDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupRoleDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfGroupTypeId" runat="server" />
        <asp:HiddenField ID="hfRoleId" runat="server" />

        <div class="banner">
            <h1><asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
        </div>

        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

        <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" ValidationGroup="GroupRoleDetail" />

        <div class="row">
            <div class="col-md-6">
                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="Name" ValidationGroup="GroupRoleDetail" />
            </div>
            <div class="col-md-6">
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" ValidationGroup="GroupRoleDetail" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-6">
                <Rock:DataTextBox ID="tbMinCount" runat="server" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="MinCount" Label="Minimum Allowed" Help="The minumum number of group members in a group that should have this role" ValidationGroup="GroupRoleDetail" />
                <Rock:DataTextBox ID="tbMaxCount" runat="server" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="MaxCount" Label="Maximum Allowed" Help="The maximum number of group members in a group that are allowed to have this role" ValidationGroup="GroupRoleDetail" />
            </div>
            <div class="col-md-6">
                <asp:PlaceHolder ID="phAttributes" runat="server" />
            </div>
        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" ValidationGroup="GroupRoleDetail" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-default" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
