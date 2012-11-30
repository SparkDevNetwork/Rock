<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRoles.ascx.cs" Inherits="GroupRoles" %>

<asp:UpdatePanel ID="upGroupRoles" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">
            <Rock:NotificationBox ID="nbGridWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />
            <Rock:Grid ID="gGroupRoles" runat="server" AllowSorting="true">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                    <asp:BoundField DataField="GroupType.Name" HeaderText="Group Type" SortExpression="GroupType.Name" />
                    <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                    <Rock:EditField OnClick="gGroupRoles_Edit" />
                    <Rock:DeleteField OnClick="gGroupRoles_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfGroupRoleId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="failureNotification" />

            <fieldset>
                <legend>
                    <i id="iconIsSystem" runat="server" class="icon-eye-open"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>
                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Crm.GroupRole, Rock" PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Crm.GroupRole, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        <Rock:DataDropDownList ID="ddlGroupType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Crm.GroupType, Rock" PropertyName="Name" LabelText="Group Type" />
                    </div>
                    <div class="span6">
                        <Rock:DataTextBox ID="tbSortOrder" runat="server" SourceTypeName="Rock.Crm.GroupRole, Rock" PropertyName="SortOrder" LabelText="Sort Order" Required="true" />
                        <Rock:DataTextBox ID="tbMinCount" runat="server" SourceTypeName="Rock.Crm.GroupRole, Rock" PropertyName="MinCount" LabelText="Min Group Members with this Role" />
                        <Rock:DataTextBox ID="tbMaxCount" runat="server" SourceTypeName="Rock.Crm.GroupRole, Rock" PropertyName="MaxCount" LabelText="Max Group Members with this Role" />
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
