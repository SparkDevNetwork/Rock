<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Residency.CompetencyList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-lightbulb-o"></i> Competencies</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit" DataKeyNames="Id" ShowConfirmDeleteDialog="false" TooltipField="Description">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

            <asp:HiddenField ID="hfTrackId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            

        </asp:Panel>

        <script>
            $('.btn-danger').on('click', function (e) {
                // turn off the built-in ShowConfirmDeleteDialog so that we can put a little more info in the confirmDelete message
                Rock.dialogs.confirmDelete(e, "competency and all its projects and assessments")
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
