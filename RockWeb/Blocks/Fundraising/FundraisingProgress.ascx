<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingProgress.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingProgress" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />
            <asp:HiddenField ID="hfGroupMemberId" runat="server" />
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">
                        <i class="fa fa-certificate"></i>
                        <asp:Literal ID="lTitle" runat="server" />
                    </h1>
                </div>
                
                <asp:Panel ID="pnlHeader" runat="server" class="bg-color padding-t-md padding-l-md padding-r-md padding-b-sm">
                    <div class="clearfix">
                        <b>Total Individual Goals</b>
                        <p class="pull-right"><strong>$<%=this.GroupContributionTotal%>/$<%=this.GroupIndividualFundraisingGoal%></strong></p>
                    </div>

                    <div class="progress">
                        <div class="progress-bar progress-bar-<%=this.ProgressCssClass %>" role="progressbar" aria-valuenow="<%=this.PercentComplete%>" aria-valuemin="0" aria-valuemax="100" style="width: <%=this.PercentComplete%>%;">
                            <%=this.PercentComplete%>% Complete
                        </div>
                    </div>
                </asp:Panel>
                    
                <ul class="list-group">
                    <asp:Repeater ID="rptFundingProgress" runat="server">
                        <ItemTemplate>
                            <li class="list-group-item">
                                <div class="row">
                                <div class="col-md-12">
                                    <p class="pull-right">$<%#Eval("ContributionTotal") %>/$<%#Eval("IndividualFundraisingGoal") %></p>

                                </div>
                                <div class="col-md-4">
                                    <%# Eval("FullName") %>
                                </div>
                                <div class="col-md-8 col-xs-12">
                                    <div class="progress">
                                        <div class="progress-bar progress-bar-<%#Eval("CssClass") %>" role="progressbar" aria-valuenow="<%#Eval( "Percentage" )%>" aria-valuemin="0" aria-valuemax="100" style="width: <%#Eval( "ProgressBarWidth" )%>%;">
                                            <%#Eval("Percentage") %>% Complete
                                        </div>
                                    </div>
                                </div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
