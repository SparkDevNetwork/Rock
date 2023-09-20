<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunionList.ascx.cs" Inherits="RockWeb.Plugins.org_secc.PastoralCare.CommunionList" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>
        
        <asp:Panel runat="server" ID="pnlInfo" Visible="false">
            <div class="panel-heading">
                <asp:Literal Text="Information" runat="server" ID="ltHeading" />
            </div>
            <div class="panel-body">
                <asp:Literal Text="" runat="server" ID="ltBody" />
            </div>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlMain" Visible="true">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-certificate"></i> Communion List</h1>
                </div>
                <Rock:GridFilter ID="gFilter" runat="server" OnApplyFilterClick="gFilter_ApplyFilterClick">
                    <Rock:CampusesPicker ID="cpCampus" runat="server" Label="Campus" />
                    <Rock:RockCheckBoxList id="cblState" runat="server" RepeatDirection="Horizontal" Label="State"/>
                </Rock:GridFilter>
                <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" ExportFilename="CommunionList" ShowActionRow="false">
                    <Columns>
                        <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus" ExcelExportBehavior="AlwaysInclude"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Location" HeaderText="Location" SortExpression="Location" ExcelExportBehavior="AlwaysInclude"></Rock:RockBoundField>
                        <Rock:PersonField DataField="Person" HeaderText="Person" SortExpression="Person" ExcelExportBehavior="AlwaysInclude"/>
                        <Rock:RockBoundField DataField="Age" HeaderText="Age" SortExpression="Person.Age" ExcelExportBehavior="NeverInclude"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Address" HeaderText="Street Address" ExcelExportBehavior="NeverInclude"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="City" HeaderText="City" ExcelExportBehavior="NeverInclude"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="State" HeaderText="State" ExcelExportBehavior="NeverInclude"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="PostalCode" HeaderText="Zip" SortExpression="PostalCode" ExcelExportBehavior="AlwaysInclude"></Rock:RockBoundField>
                        <Rock:RockTemplateField HeaderText="Status" ExcelExportBehavior="NeverInclude">
                            <ItemTemplate>
                                <span class="label label-success"><%# Eval("Status") %></span>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:BoolField DataField="Communion" HeaderText="Communion" ExcelExportBehavior="NeverInclude"/>
                        <Rock:RockTemplateField HeaderText="Actions" ExcelExportBehavior="NeverInclude">
                            <ItemTemplate>
                                <a href="<%# "https://maps.google.com/?q="+Eval("Address") %>" target="_blank" class="btn btn-default"><i class="fa fa-map-o" title="View Map"></i></a>                                </ItemTemplate>
                        </Rock:RockTemplateField>
                    </Columns>
                </Rock:Grid>
                <asp:LinkButton id="excel" runat="server" OnClick="Actions_ExcelExportClick" CssClass="btn btn-default pull-right" style="margin-top: 5px;" ><i class="fa fa-table"></i></asp:LinkButton>
            </div>
        </asp:Panel>
    </ContentTemplate>
    <Triggers>
        <asp:PostBackTrigger ControlID="excel" />
    </Triggers>
</asp:UpdatePanel>
