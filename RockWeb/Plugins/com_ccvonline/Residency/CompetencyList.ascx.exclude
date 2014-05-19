<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <asp:HiddenField ID="hfTrackId" runat="server" />
        <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit" DataKeyNames="Id" ShowConfirmDeleteDialog="false">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <Rock:DeleteField OnClick="gList_Delete" />
            </Columns>
        </Rock:Grid>
        <script>
            $('.btn-danger').on('click', function (e) {
                // turn off the built-in ShowConfirmDeleteDelete so that we can put a little more info in the confirmDelete message
                Rock.controls.grid.confirmDelete(e, "competency and all its projects and assessments")
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
