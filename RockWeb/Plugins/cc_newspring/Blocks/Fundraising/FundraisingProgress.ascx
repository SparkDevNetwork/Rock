<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingProgress.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.Fundraising.FundraisingProgress" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <section class="bg-white soft xs-soft-half hard-bottom clearfix push-bottom xs-push-half-bottom rounded shadowed">
            <h2>Fundraising Progress</h2>
        <asp:Panel ID="pnlView" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />
            <asp:HiddenField ID="hfGroupMemberId" runat="server" />
            <div class="panel panel-block">
                <div class="panel-heading display-none">
                    <h1 class="panel-title">
                        <i class="fa fa-certificate"></i>
                        <asp:Literal ID="lTitle" runat="server" />
                    </h1>
                </div>
                
                <asp:Panel ID="pnlHeader" runat="server" class="bg-color padding-t-md padding-l-md padding-r-md padding-b-sm">
                    <div class="clearfix">
                        <p class="pull-right">$<%=this.GroupContributionTotal%> <span class="italic">&nbsp;of&nbsp;</span> <strong>$<%=this.GroupIndividualFundraisingGoal%></strong></p>
                        <h3 class="h4">Total Progress</h3>
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
                                    <div class="col-xs-12 col-sm-12 col-md-12 soft-half-top">
                                        <p class="pull-right">$<%#Eval("ContributionTotal") %> <span class="italic">&nbsp;of&nbsp;</span> <span class="stronger">$<%#Eval("IndividualFundraisingGoal") %></span></p>
                                        <h3 class="h4 push-half-bottom"><%# Eval("FullName") %></h3>
                                    </div><div class="col-xs-12 col-sm-12 col-md-12">
                                        <div class="progress">
                                            <div class="progress-bar progress-bar-<%#Eval("CssClass") %>" role="progressbar" aria-valuenow="<%#Eval( "Percentage" )%>" aria-valuemin="0" aria-valuemax="100" style="width: <%#Eval( "ProgressBarWidth" )%>%;">
                                                <%#Eval("Percentage") %>% Complete
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </div>
        </asp:Panel>
        </section>
    </ContentTemplate>
</asp:UpdatePanel>
