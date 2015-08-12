<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountabilityGroupMemberList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.AccountabilityGroupMemberList" %>

<asp:UpdatePanel ID="upnlList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlGroupMembers" runat="server">

                <div class="panel panel-block">
                
                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Group Members" />
                        </h1>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="maGridWarning" runat="server" />

                        <Rock:NotificationBox ID="nbRoleWarning" runat="server" NotificationBoxType="Warning" Title="No roles!" Visible="false" />

                        <div class="grid grid-panel">
                            <Rock:Grid ID="gGroupMembers" runat="server" DisplayType="Full" AllowSorting="true" OnRowDataBound="gGroupMembers_RowDataBound" OnRowSelected="gGroupMembers_View">
                                <Columns>

                                    <asp:BoundField DataField="Person.FullName" HeaderText="Name" SortExpression="Person.NickName" />
                                    
                                    <asp:TemplateField SortExpression="FirstReport" HeaderText="First Report">
                                        <ItemTemplate>
                                            <asp:Literal ID="lFirstReport" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    
                                    <asp:TemplateField SortExpression="LastReport" HeaderText="Last Report">
                                        <ItemTemplate>
                                            <asp:Literal ID="lLastReport" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    
                                    <asp:TemplateField SortExpression="WeeksSinceLast" HeaderText="Weeks Since Last">
                                        <ItemTemplate>
                                            <asp:Literal ID="lWeeksSinceLast" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    
                                    <asp:TemplateField SortExpression="ReportsOpportunities" HeaderText="Reports / Opportunities">
                                        <ItemTemplate>
                                            <asp:Literal ID="lReportsOpportunities" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                   
                                     <asp:TemplateField SortExpression="PercentSubmitted" HeaderText="% Submitted">
                                        <ItemTemplate>
                                            <asp:Literal ID="lPercentSubmitted" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    
                                    <asp:TemplateField SortExpression="Score" HeaderText="Score">
                                        <ItemTemplate>
                                            <asp:Literal ID="lScore" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>