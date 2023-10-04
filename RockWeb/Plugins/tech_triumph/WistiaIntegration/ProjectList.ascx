<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProjectList.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WistiaIntegration.ProjectList" %>

<asp:UpdatePanel ID="upnlProjects" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-folder-open"></i> Projects</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfProjectsFilter" runat="server">
                            <Rock:RockDropDownList ID="ddlShowInIntegration" runat="server" Label="Show In Integration">
                                <asp:ListItem Text="" Value="" />
                                <asp:ListItem Text="Yes" Value="Yes"/>
                                <asp:ListItem Text="No" Value="No" />
                            </Rock:RockDropDownList>
                        </Rock:GridFilter>
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gProject" runat="server" AllowSorting="true" TooltipField="Name" CssClass="js-grid-projects" OnRowSelected="gProject_Edit" >
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" TruncateLength="250" SortExpression="Description" HeaderStyle-Width="50%" />
                                <Rock:RockBoundField DataField="MediaCount" HeaderText="Media Files" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" />
                                <Rock:RockBoundField DataField="PlayCount" HeaderText="Play Count" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" SortExpression="PlayCount" />
                                <Rock:RockBoundField DataField="HoursWatched" HeaderText="Hours Watched" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:#,##0.0}" SortExpression="HoursWatched" />
                                <Rock:BoolField DataField="IsPublic" HeaderText="Is Public"  HeaderStyle-HorizontalAlign="Center" />
                                <Rock:BoolField DataField="ShowInIntegration" HeaderText="Show In Integration" HeaderStyle-HorizontalAlign="Center" />

                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
