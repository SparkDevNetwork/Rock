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
                <Rock:NumberRangeEditor ID="nreMembersAllowed" runat="server" NumberType="Integer" Label="Number Allowed in Group" 
                    Help="The minimum and/or maximum number of members allowed with this role in each group" ValidationGroup="GroupRoleDetail" />
                <asp:CustomValidator ID="cvAllowed" runat="server" Display="None" OnServerValidate="cvAllowed_ServerValidate" 
                    ValidationGroup="GroupRoleDetail" ErrorMessage="The Number Allowed in Group should have a minimum number allowed that is less than the maximum number allowed." />
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
