<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SiteList.ascx.cs" Inherits="RockWeb.Blocks.Cms.SiteList" %>

<asp:UpdatePanel ID="upSites" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lBlockIcon" runat="server" /> <asp:Literal ID="lBlockTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilterSite" runat="server" OnApplyFilterClick="rFilterSite_ApplyFilterClick">
                        <Rock:RockCheckBox ID="cbShowInactive" runat="server" Checked="false" Label="Include Inactive" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gSites" runat="server" AllowSorting="true" OnRowSelected="gSites_Edit" CssClass="js-grid-site-list">
                        <Columns>
                            <Rock:RockBoundField ItemStyle-CssClass="js-name" DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField HeaderText="Description" DataField="Description" SortExpression="Description" />
                            <Rock:RockTemplateField HeaderText="Domain(s)" ID="colDomains">
                                <ItemTemplate><%# GetDomains( (int)Eval("Id") ) %></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField HeaderText="Theme" DataField="Theme" SortExpression="Theme" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:SecurityField TitleField="Name" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

