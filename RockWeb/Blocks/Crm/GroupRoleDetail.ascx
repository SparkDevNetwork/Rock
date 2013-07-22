<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRoleDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupRoleDetail" %>

<asp:UpdatePanel ID="upGroupRoles" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfGroupRoleId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        <Rock:DataDropDownList ID="ddlGroupType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" LabelText="Group Type" />
                    </div>
                    <div class="span6">
                        <Rock:DataTextBox ID="tbSortOrder" runat="server" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="SortOrder" LabelText="Sort Order" Required="true" />
                        <Rock:DataTextBox ID="tbMinCount" runat="server" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="MinCount" LabelText="Min Group Members with this Role" />
                        <Rock:DataTextBox ID="tbMaxCount" runat="server" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="MaxCount" LabelText="Max Group Members with this Role" />
                        <asp:PlaceHolder ID="phAttributes" runat="server" />
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
