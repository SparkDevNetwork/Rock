<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaInteractionList.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WistiaIntegration.MediaInteractionList" %>

<asp:UpdatePanel ID="upnlProjects" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-play"></i> Media Interactions</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server">
                            <Rock:SlidingDateRangePicker ID="sdrpFilterDateRange" runat="server" Label="Date Range" />
                            <Rock:PersonPicker ID="ppFilterPerson" runat="server" />
                        </Rock:GridFilter>
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gInteractions" runat="server" AllowSorting="true" ShowConfirmDeleteDialog="true" OnRowDataBound="gMedia_RowDataBound">
                            <Columns>
                                <Rock:RockBoundField DataField="InteractionDateTime" HeaderText="Date / Time" SortExpression="InteractionDateTime" />
                                <Rock:PersonField HeaderText="Viewer" DataField="PersonAlias.Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                                <asp:TemplateField HeaderText="Thumbnail">
                                    <ItemTemplate>
                                        <asp:Literal ID="lThumbnail" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Location">
                                    <ItemTemplate>
                                        <asp:Literal ID="lLocation" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Client">
                                    <ItemTemplate>
                                        <asp:Literal ID="lClient" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Organization">
                                    <ItemTemplate>
                                        <asp:Literal ID="lOrganization" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
