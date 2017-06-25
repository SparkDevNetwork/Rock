<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingProgress.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingProgress" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">
                        <i class="fa fa-certificate"></i>
                        <asp:Literal ID="lTitle" runat="server" />
                    </h1>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-md-11 center-block" style="float: none;">
                            <div class="row" style="background-color: #edeae6; border-radius: 10px;">
                                <div class="col-md-12">
                                    <b>Total Individual Goals</b>
                                    <p class="pull-right"><b>$<%=this.GroupContributionTotal%>/$<%=this.GroupIndividualFundraisingGoal%></b></p>
                                </div>
                                <br />
                                <div class="col-md-12">
                                    <div class=" progress">
                                        <div class="progress-bar progress-bar-<%=this.ProgressCssClass %>" role="progressbar" aria-valuenow="<%=this.PercentComplete%>" aria-valuemin="0" aria-valuemax="100" style="width: <%=this.PercentComplete%>%;">
                                            <%=this.PercentComplete%>% Complete
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div style="height: 20px;"></div>
                            <asp:Repeater ID="rptFundingProgress" runat="server">
                                <ItemTemplate>
                                    <div class="row" style="border-bottom: 1px solid black;">
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
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
