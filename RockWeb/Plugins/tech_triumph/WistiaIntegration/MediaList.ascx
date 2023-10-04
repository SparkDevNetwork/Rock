<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaList.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WistiaIntegration.MediaList" %>

<asp:UpdatePanel ID="upnlProjects" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-play"></i> Media List</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server">
                            <Rock:RockDropDownList ID="ddlSection" runat="server" Label="Section" EnhanceForLongLists="true" />
                        </Rock:GridFilter>
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gMedia" runat="server" AllowSorting="true" TooltipField="Name" ShowConfirmDeleteDialog="true" OnRowSelected="gMedia_Edit">
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                <Rock:RockBoundField DataField="Section" HeaderText="Section" SortExpression="Section" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" TruncateLength="250" SortExpression="Description" HeaderStyle-Width="50%" />
                                <Rock:RockBoundField DataField="DurationFormatted" HeaderText="Duration" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                <Rock:RockBoundField DataField="PlayCount" HeaderText="Play Count" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" SortExpression="PlayCount" HeaderStyle-HorizontalAlign="Right" />
                                <Rock:RockBoundField DataField="HoursWatched" HeaderText="Hours Watched" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:#,##0.0}" SortExpression="HoursWatched" HeaderStyle-HorizontalAlign="Right" />
                         
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
