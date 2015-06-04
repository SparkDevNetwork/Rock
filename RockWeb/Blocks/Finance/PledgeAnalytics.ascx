<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Finance.PledgeAnalytics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server">
        
            <div class="panel panel-block panel-analytics">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-list"></i> Pledge Analytics</h1>

                    <div class="panel-labels">
                       
                    </div>

                </div>
                <div class="panel-body">
                    <div class="row row-eq-height-md">
                        <div class="col-md-3 filter-options">
                            <Rock:AccountPicker ID="apAccount" runat="server" Label="Account" />

                            <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />

                            <Rock:NumberRangeEditor ID="nrePledgeAmount" runat="server" Label="Pledge Amount" />

                            <Rock:NumberRangeEditor ID="nrePercentComplete" runat="server" Label="% Complete" />

                            <Rock:NumberRangeEditor ID="nreAmountGiven" runat="server" Label="Amount Given" />

                            <Rock:RockRadioButtonList ID="rblInclude" runat="server" Label="Include">
                                <asp:ListItem id="liEveryoneWithPledge" Text="Everyone with a pledge" Value="0" />
                                <asp:ListItem id="liEveryoneWithAGift" Text="Everyone with a gift" Value="1" />
                                <asp:ListItem id="liEveryoneWithAGiftOrPledge" Text="Everyone with a gift or pledge" Value="2" />
                            </Rock:RockRadioButtonList>
                        </div>
                        <div class="col-md-9">
                            <div class="row analysis-types">
                                <div class="col-md-12">
                                    <div class="actions text-right">
                                        <asp:LinkButton ID="btnApply" runat="server" CssClass="btn btn-primary" ToolTip="Update the chart"><i class="fa fa-refresh"></i> Update</asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                            <div class="grid grid-panel">
                                <Rock:Grid ID="gList" runat="server" AllowSorting="true">
                                    <Columns>
                                        <Rock:RockBoundField DataField="GivingId" HeaderText="GivingId" SortExpression="GivingId" />
                                        <Rock:RockBoundField DataField="PledgeTotal" HeaderText="Pledge Total" SortExpression="PledgeTotal" />
                                        <Rock:RockBoundField DataField="AccountName" HeaderText="Account" SortExpression="AccountName" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
