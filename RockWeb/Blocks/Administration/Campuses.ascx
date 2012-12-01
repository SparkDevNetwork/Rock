<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Campuses.ascx.cs" Inherits="RockWeb.Blocks.Administration.Campuses" %>

<asp:UpdatePanel ID="upCampuses" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server">
            <Rock:Grid ID="gCampuses" runat="server" AllowSorting="true" OnEditRow="gCampuses_Edit">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <Rock:DeleteField OnClick="gCampuses_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfCampusId" runat="server" />

            <asp:ValidationSummary runat="server" CssClass="failureNotification" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>
                <Rock:DataTextBox ID="tbCampusName" runat="server" SourceTypeName="Rock.Crm.Campus, Rock" PropertyName="Name" />
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>

