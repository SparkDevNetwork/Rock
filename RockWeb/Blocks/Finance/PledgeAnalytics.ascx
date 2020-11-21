<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Finance.PledgeAnalytics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server">

            <div class="panel panel-block panel-analytics">
                <div class="panel-heading panel-follow">
                    <h1 class="panel-title">
                        <i class="fa fa-list"></i>
                        Pledge Analytics
                    </h1>

                    <div class="panel-labels">
                    </div>
                    <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
                </div>

                <div class="panel-body">
                    <div class="row row-eq-height-md">

                        <div class="col-md-3 filter-options">
                            <Rock:AccountPicker ID="apAccount" runat="server" Label="Account" Required="true" />

                            <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" Label="Date Range" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange"/>

                            <Rock:NumberRangeEditor ID="nrePledgeAmount" runat="server" Label="Pledge Amount" />

                            <Rock:NumberRangeEditor ID="nrePercentComplete" runat="server" Label="% Complete" />

                            <Rock:NumberRangeEditor ID="nreAmountGiven" runat="server" Label="Amount Given" />

                            <Rock:RockRadioButtonList ID="rblInclude" runat="server" Label="Show">
                                <asp:ListItem Text="Only those with pledges" Value="0" Selected="True" />
                                <asp:ListItem Text="Only those with gifts" Value="1" />
                                <asp:ListItem Text="Those with gifts or pledges" Value="2" />
                            </Rock:RockRadioButtonList>
                        </div>

                        <div class="col-md-9">
                            <div class="row analysis-types">
                                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                                <div class="col-md-12">
                                    <div class="actions text-right">
                                        <asp:LinkButton ID="btnApply" runat="server" OnClick="btnApply_Click" CssClass="btn btn-primary" ToolTip="Update the chart"><i class="fa fa-refresh"></i> Update</asp:LinkButton>
                                    </div>
                                </div>
                            </div>

                            <asp:Panel ID="pnlUpdateMessage" runat="server" Visible="true">
                                <Rock:NotificationBox ID="nbUpdateMessage" runat="server" NotificationBoxType="Default" CssClass="text-center padding-all-lg" Heading="Confirm Settings"
                                    Text="<p>Confirm your settings and select the Update button to display your results.</p>" />
                            </asp:Panel>

                            <asp:Panel ID="pnlResults" runat="server" Visible="false">

                                <Rock:Grid ID="gList" runat="server" AllowSorting="true" PersonIdField="Id" OnRowSelected="gList_RowSelected"
                                    ExportSource="ColumnOutput" ExportFilename="PledgeAnalytics">
                                    <Columns>
                                        <Rock:SelectField />
                                        <Rock:RockTemplateField HeaderText="Person" SortExpression="LastName,NickName">
                                            <ItemTemplate><%# FormatName( Eval("LastName"), Eval("NickName") ) %></ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:RockBoundField DataField="Email" HeaderText="Email" SortExpression="Email" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                        <Rock:CurrencyField DataField="PledgeAmount" HeaderText="Pledge Total" SortExpression="PledgeAmount" />
                                        <Rock:CurrencyField DataField="GiftAmount" HeaderText="Total Giving Amount" SortExpression="GiftAmount" />
                                        <Rock:RockBoundField DataField="PercentComplete" HeaderText="Percent Complete" SortExpression="PercentComplete" DataFormatString="{0:P0}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                        <Rock:RockBoundField DataField="GiftCount" HeaderText="Giving Count" SortExpression="GiftCount" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right"/>
                                    </Columns>
                                </Rock:Grid>

                            </asp:Panel>

                        </div>
                    </div>
                </div>
            </div>


        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {
                Rock.controls.fullScreen.initialize();
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
