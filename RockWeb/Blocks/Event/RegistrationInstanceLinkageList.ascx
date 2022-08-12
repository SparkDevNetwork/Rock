<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceLinkageList.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceLinkageList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />

                <asp:Panel ID="pnlLinkages" runat="server" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-link"></i>
                            Linkages
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdLinkagesGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fLinkages" runat="server" OnDisplayFilterValue="fLinkages_DisplayFilterValue" OnClearFilterClick="fLinkages_ClearFilterClick">
                                <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gLinkages" runat="server" DisplayType="Full" AllowSorting="true" RowItemText="Linkage" ExportSource="ColumnOutput">
                                <Columns>
                                    <Rock:RockTemplateFieldUnselected HeaderText="Calendar Item">
                                        <ItemTemplate>
                                            <asp:Literal ID="lCalendarItem" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <asp:BoundField HeaderText="Campus" DataField="Campus.Name" SortExpression="Campus.Name" />
                                    <asp:HyperLinkField HeaderText="Group" DataTextField="Group" DataNavigateUrlFields="GroupID" SortExpression="Group" />
                                    <Rock:RockTemplateFieldUnselected HeaderText="Content Item">
                                        <ItemTemplate>
                                            <asp:Literal ID="lContentItem" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <asp:BoundField HeaderText="Public Name" DataField="PublicName" SortExpression="PublicName" />
                                    <asp:BoundField HeaderText="URL Slug" DataField="UrlSlug" SortExpression="UrlSlug" />
                                    <Rock:EditField OnClick="gLinkages_Edit" />
                                    <Rock:DeleteField OnClick="gLinkages_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
