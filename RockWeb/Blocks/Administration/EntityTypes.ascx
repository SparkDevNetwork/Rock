<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EntityTypes.ascx.cs" Inherits="EntityTypes" %>

<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gEntityTypes" runat="server" AllowSorting="true">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Entity Type" SortExpression="Name" />
                    <asp:BoundField DataField="FriendlyName" HeaderText="Friendly Name" SortExpression="FriendlyName" />
                    <Rock:BoolField DataField="IsCommon" HeaderText="Common" SortExpression="IsCommon" />
                    <asp:TemplateField>
                        <HeaderStyle CssClass="span1" />
                        <ItemStyle HorizontalAlign="Center"/>
                        <ItemTemplate>
                            <a id="aSecure" runat="server" class="btn btn-default btn-sm" height="500px"><i class="icon-lock"></i></a>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfEntityTypeId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <fieldset id="fieldsetEditDetails" runat="server">
                <legend><asp:Literal ID="lActionTitle" runat="server" /></legend>
                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.EntityType, Rock" PropertyName="Name" LabelText="Entity Type Name" />
                <Rock:DataTextBox ID="tbFriendlyName" runat="server" SourceTypeName="Rock.Model.EntityType, Rock" PropertyName="FriendlyName" LabelText="Friendly Name" />
                <Rock:LabeledCheckBox ID="cbCommon" runat="server" Text="Common" Help="There are various places that a user is prompted for an entity type.  'Common' entities will be listed first for the user to easily find them" />
            </fieldset>

            <div class="actions" id="pnlEditDetailsActions" runat="server">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>