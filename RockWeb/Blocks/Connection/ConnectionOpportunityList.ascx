﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionOpportunityList.ascx.cs" Inherits="RockWeb.Blocks.Connection.ConnectionOpportunityList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlConnectionOpportunities" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-list"></i>
                            Connection Opportunities
                        </h1>

                        <div class="panel-labels">
                            <Rock:HighlightLabel ID="hlSyncStatus" runat="server" LabelType="Info" Visible="false" Text="<i class='fa fa-exchange'></i>" />
                        </div>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                                <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" RepeatDirection="Horizontal" />
                                <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gConnectionOpportunities" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gConnectionOpportunities_Edit">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                    <Rock:RockBoundField DataField="Summary" HeaderText="Summary" TruncateLength="300"  />
                                    <Rock:RockTemplateField HeaderText="Status" SortExpression="IsActive">
                                        <ItemTemplate>
                                            <%# (bool)Eval("IsActive") ? "<span class='label label-success'>Active</span>" : "<span class='label label-default'>Inactive</span>" %>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
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
