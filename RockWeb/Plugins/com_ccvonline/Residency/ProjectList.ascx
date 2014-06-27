<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProjectList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ProjectList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlList" runat="server">
            <h4>Projects</h4>
            
            <asp:HiddenField ID="hfCompetencyId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            
            <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit" DataKeyNames="Id" OnRowCommand="gList_RowCommand">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                    <asp:BoundField DataField="MinAssessmentCountDefault" HeaderText="Default # of Assessments" SortExpression="MinAssessmentCountDefault" />
                    <asp:TemplateField ItemStyle-CssClass="grid-col-actions" HeaderStyle-CssClass="grid-col-actions" HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="btnClone" CssClass="btn btn-action" CausesValidation="false" CommandName="clone" CommandArgument="<%# Container.DataItemIndex %>"><i class="fa fa-files-o"></i> Clone</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <Rock:DeleteField OnClick="gList_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
