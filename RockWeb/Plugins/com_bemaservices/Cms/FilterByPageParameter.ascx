<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FilterByPageParameter.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Cms.FilterByPageParameter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h4 class="panel-title"><asp:Literal ID="lHeadingIcon" runat="server" />&nbsp;<asp:Literal ID="lHeading" runat="server" /></h4>
                <div class="pull-right">
                    <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-xs btn-square btn-default" OnClick="lbEditFilters_Click"><i class="fa fa-gear"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">

                <asp:Panel runat="server" ID="pnlFilters">
                    <div class="row">
                        <div class="col-md-12">
                            <asp:PlaceHolder ID="phAttributes" runat="server" />
                        </div>
                    </div>

                    <div class="pull-right">
                        <Rock:BootstrapButton ID="btnResetFilters" runat="server" Text="Reset Filters" CssClass="btn btn-default" OnClick="btnResetFilters_Click" />
                        <Rock:BootstrapButton ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlEdit" runat="server" Visible="false">
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gFilters" runat="server" AllowSorting="false" RowItemText="filter" TooltipField="Description" OnGridReorder="gFilters_GridReorder" OnRowDataBound="gFilters_RowDataBound">
                            <Columns>
                                <Rock:ReorderField Visible="true" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                <Rock:RockBoundField DataField="FieldType" HeaderText="Filter Type" />
                                <Rock:RockTemplateField ID="rtDefaultValue">
                                    <HeaderTemplate>Default Value</HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Literal ID="lDefaultValue" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:EditField OnClick="gFilters_Edit" />
                                <Rock:SecurityField TitleField="Name" />
                                <Rock:DeleteField OnClick="gFilters_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                    <br />
                    <div class="row">
                        <div class="col-md-12">
                            <asp:LinkButton ID="lbClose" runat="server" CssClass="btn btn-primary" OnClick="lbClose_Click">Close</asp:LinkButton>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <Rock:ModalDialog ID="mdFilter" runat="server" Title="Filter" SaveButtonText="Save">
            <Content>
                <Rock:AttributeEditor ID="edtFilter" runat="server" ShowActions="false" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
