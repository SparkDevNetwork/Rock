<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionOpportunityList.ascx.cs" Inherits="RockWeb.Blocks.Involvement.ConnectionOpportunityList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlConnectionOpportunities" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-long-arrow-right"></i>
                            Connection Opportunities
                        </h1>

                        <div class="panel-labels">
                            <Rock:HighlightLabel ID="hlSyncStatus" runat="server" LabelType="Info" Visible="false" Text="<i class='fa fa-exchange'></i>" />
                            &nbsp;
                        </div>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                                <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" RepeatDirection="Horizontal" />
                                <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gConnectionOpportunities" runat="server" DisplayType="Full" AllowSorting="false" OnRowSelected="gConnectionOpportunities_Edit">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="GroupType" HeaderText="Campuses" />
                                    <Rock:RockBoundField DataField="Active" HeaderText="Status" HtmlEncode="false" />
                                    <Rock:DeleteField OnClick="DeleteConnectionOpportunity_Click" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
