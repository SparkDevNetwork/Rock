<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NcoaResults.ascx.cs" Inherits="RockWeb.Blocks.Crm.NcoaResults" %>

<asp:UpdatePanel id="upNcoaResults" runat="server">
    <ContentTemplate>
        <asp:Panel id="pnlDetails" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <div class="pull-left">
                    <h1 class="panel-title">
                        <i class="fa fa-people-carry"></i>
                        NCOA Results
                    </h1>
                </div>
                <div class="pull-right">
                    <asp:Literal id="lTotal" runat="server" />
                </div>
                <div class="clearfix"></div>
            </div>

            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter id="gfNcoaFilter" runat="server">
                        <Rock:RockDropDownList id="ddlProcessed" runat="server" Label="Processed" />
                        <Rock:SlidingDateRangePicker id="sdpMoveDate" runat="server" Label="Move Date" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                        <Rock:SlidingDateRangePicker id="sdpProcessedDate" runat="server" Label="NCOA Processed Date" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                        <Rock:RockDropDownList id="ddlMoveType" runat="server" Label="Move Type" />
                        <Rock:RockDropDownList id="ddlAddressStatus" runat="server" Label="Address Status" />
                        <Rock:RockDropDownList id="ddlInvalidReason" runat="server" Label="Invalid Reason" />
                        <Rock:NumberBox id="nbMoveDistance" runat="server" NumberType="Double" Label="Move Distance" Help="Maximum distance that a person or family moved" AppendText="miles"></Rock:NumberBox>
                        <Rock:RockTextBox id="tbLastName" runat="server" Label="Last Name" />
                        <Rock:CampusPicker id="cpCampus" Label="Campus" runat="server" />
                    </Rock:GridFilter>
                </div>

                <div class="panel-body margin-t-md">

                    <asp:Repeater id="rptNcoaResultsFamily" runat="server" OnItemDataBound="rptNcoaResultsFamily_ItemDataBound">
                        <ItemTemplate>
                            <h3><%# Eval("Key") %></h3>
                            <asp:Repeater id="rptNcoaResults" runat="server" OnItemDataBound="rptNcoaResults_ItemDataBound" OnItemCommand="rptNcoaResults_ItemCommand">
                                <ItemTemplate>
                                    <div class="well">
                                        <div class="row">
                                            <div class="col-md-4 col-xs-12">
                                                <div class="row">
                                                    <div class="col-lg-6 col-md-12">
                                                        <h4>
                                                            <label class='label <%# Eval("TagLineCssClass") %>'><%# Eval("TagLine") %></label></h4>
                                                        <dl>
                                                            <dt><%# Eval("MoveDate") == null ? string.Empty:"Move Date" %></dt>
                                                            <dd><%# string.Format("{0:d}", (DateTime?) Eval("MoveDate")) %></dd>
                                                        </dl>
                                                    </div>
                                                    <div class="col-lg-6 col-md-12">
                                                        <asp:Literal id="lMembers" runat="server" />
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-4 col-xs-12">
                                                <div class="row">
                                                    <div class="col-lg-6 col-md-12">
                                                        <dl>
                                                            <dt>Original Address </dt>
                                                            <dd><%# Eval("OriginalAddress") %> </dd>
                                                        </dl>
                                                    </div>
                                                    <div class="col-lg-6 col-md-12">
                                                        <dl>
                                                            <dt><%# Eval("NewAddress") == null ? string.Empty:"New Address" %></dt>
                                                            <dd><%# Eval("NewAddress") %> </dd>
                                                        </dl>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-4 col-xs-12">
                                                <div class="row">
                                                    <div class="col-lg-5 col-md-12">
                                                        <dl>
                                                            <dt><%# Eval("MoveDistance") == null ? string.Empty:"Move Distance" %></dt>
                                                            <dd><%# Eval("MoveDistance") %></dd>
                                                        </dl>
                                                    </div>
                                                    <div class="col-lg-7 col-md-12 margin-b-sm">
                                                        <label class='label <%# Eval("StatusCssClass") %>'><%# Eval("Status") %></label>
                                                        <h4>
                                                            <div class="pull-right">
                                                                <a class="btn btn-default" href='<%# string.Format( "{0}{1}", ResolveRockUrl( "~/Person/" ), Eval("HeadOftheHousehold.Id") ) %>'><i class="fa fa-users"></i></a>
                                                            </div>
                                                        </h4>
                                                    </div>
                                                    <div class="col-lg-7 col-md-12 col-lg-offset-5">
                                                        <asp:LinkButton id="lbAction" CommandArgument='<%# Eval("Id") %>' CommandName='<%# Eval("CommandName") %>' Visible='<%# (bool) Eval("ShowButton") %>' runat="server" Text='<%# Eval("CommandText") %>' CssClass="btn btn-default" />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-xs-12">
                                                <span class="text-warning"><asp:Literal id="lWarning" runat="server"></asp:Literal></span>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <hr />
                        </ItemTemplate>
                    </asp:Repeater>

                    <div class="clearfix">
                        <asp:HyperLink id="hlNext" CssClass="btn btn-primary pull-right" runat="server" Text="Next <i class='fa fa-chevron-right'></i>" />
                        <asp:HyperLink id="hlPrev" CssClass="btn btn-primary pull-left" runat="server" Text="<i class='fa fa-chevron-left'></i> Prev" />
                    </div>

                </div>


            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
