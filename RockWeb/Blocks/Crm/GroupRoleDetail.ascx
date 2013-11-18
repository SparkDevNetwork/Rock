<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRoleDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupRoleDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfGroupTypeId" runat="server" />
        <asp:HiddenField ID="hfRoleId" runat="server" />

        <div class="banner">
            <h1><asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
        </div>

        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

        <asp:ValidationSummary ID="valGroupRoleDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="GroupRoleDetail" />

        <div class="row">
            <div class="col-md-6">
                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.GroupTypeRole, Rock" PropertyName="Name" ValidationGroup="GroupRoleDetail" />
            </div>
            <div class="col-md-6">
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.GroupTypeRole, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" ValidationGroup="GroupRoleDetail" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-6">
                <Rock:NumberBox ID="nbMinimumRequired" runat="server" NumberType="Integer" Label="Minimum Required" />
                <Rock:NumberBox ID="nbMaximumAllowed" runat="server" NumberType="Integer" Label="Maximum Allowed" />
                <asp:CustomValidator ID="cvAllowed" runat="server" Display="None" OnServerValidate="cvAllowed_ServerValidate" 
                    ValidationGroup="GroupRoleDetail" ErrorMessage="The Minimum Required should be less than Maximum Allowed." />
             </div>
            <div class="col-md-6">
                <asp:PlaceHolder ID="phAttributes" runat="server" />
            </div>
        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" ValidationGroup="GroupRoleDetail" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
