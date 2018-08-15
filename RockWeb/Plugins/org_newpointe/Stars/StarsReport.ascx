<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarsReport.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Stars.StarsReport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlSearch" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Super Stars Report</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="starsFilters" runat="server" OnApplyFilterClick="filters_ApplyFilterClick">
                        <Rock:MonthYearPicker runat="server" ID="mypMonth" Label="Month / Year" />
                        <Rock:CampusesPicker runat="server" ID="cpCampus" Label="Campus" />
                        <Rock:RockDropDownList runat="server" ID="ddlStars" Label="Stars Level">
                            <asp:ListItem Text="All" Value=""></asp:ListItem>
                            <asp:ListItem Text="10" Value="10"></asp:ListItem>
                            <asp:ListItem Text="20" Value="20"></asp:ListItem>
                            <asp:ListItem Text="30" Value="30"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>

                    <Rock:Grid ID="gStars" runat="server" AllowSorting="true" AllowPaging="True" EmptyDataText="No Data Found" RowClickEnabled="True" OnRowSelected="gStars_OnRowSelected" EntityTypeId="15" DataKeyNames="Id" OnGridRebind="gStars_GridRebind">
                        <Columns>
                            <asp:BoundField DataField="Campus" HeaderText="Campus" />
                            <asp:BoundField DataField="Person" HeaderText="Person" />
                            <asp:BoundField DataField="Person.GradeFormatted" HeaderText="GradeFormatted" />
                            <asp:BoundField DataField="Sum" HeaderText="Stars" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
