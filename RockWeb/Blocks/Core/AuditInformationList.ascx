<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AuditInformationList.ascx.cs" Inherits="RockWeb.Blocks.Core.AuditInformationList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server" Visible="true">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check"></i> Audit Records</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:EntityTypePicker ID="etpEntityTypeFilter" runat="server" Label="Entity Type" IncludeGlobalOption="false" />
                        <Rock:RockTextBox ID="tbEntityIdFilter" runat="server" Label="Entity Id" />
                        <Rock:PersonPicker ID="ppWhoFilter" runat="server" Label="Who" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gAuditInformationList" runat="server" AllowSorting="true" OnRowSelected="gAuditInformationList_Select">
                        <Columns>
                            <Rock:EnumField DataField="AuditType" HeaderText="Action" SortExpression="AuditType" />
                            <asp:BoundField DataField="EntityType" HeaderText="Entity Type" SortExpression="EntityType" />
                            <asp:BoundField DataField="EntityId" HeaderText="Entity Id" SortExpression="EntityId" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="Title" HeaderText="Entity Description" SortExpression="Title" TruncateLength="80" HtmlEncode="true" />
                            <asp:BoundField DataField="Properties" HeaderText="Properties Updated" SortExpression="Properties" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:DateTimeField DataField="DateTime" HeaderText="When" SortExpression="DateTime" />
                            <asp:HyperLinkField DataTextField="PersonName" DataNavigateUrlFields="PersonId" SortExpression="PersonName" DataNavigateUrlFormatString="~/Person/{0}" HeaderText="Who" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="mdProperties" runat="server" Title="Properties Updated" >
            <Content>
                
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-check"></i> Audit Information</h1>
                    </div>
                    <div class="panel-body">

                        <div class="grid grid-panel">
                            <Rock:Grid ID="gProperties" runat="server" AllowSorting="false" EmptyDataText="No Properties" ShowActionRow="false" AllowPaging="false">
                                <Columns>
                                    <asp:BoundField DataField="Property" HeaderText="Property" SortExpression="EntityType" />
                                    <asp:BoundField DataField="OriginalValue" HeaderText="Original Value" SortExpression="OriginalValue" />
                                    <asp:BoundField DataField="CurrentValue" HeaderText="New Value" SortExpression="CurrentValue" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
