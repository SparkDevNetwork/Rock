<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledTransactionView.ascx.cs" Inherits="RockWeb.Blocks.Finance.ScheduledTransactionView" %>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-credit-card"></i> Scheduled Transaction Details
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server"/>
                </div>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <div class="row">
                            <div class="col-xs-6">
                                <asp:Literal ID="lDetailsLeft" runat="server" />
                            </div>
                            <div class="col-xs-6">
                                <asp:Literal ID="lDetailsRight" runat="server" />
                                <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click"><i class="fa fa-refresh"></i> Refresh Now</asp:LinkButton>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">

                        <Rock:Grid ID="gAccountsView" runat="server" EmptyDataText="No Account Details" RowItemText="Account" DisplayType="Light">
                            <Columns>
                                <Rock:RockTemplateField HeaderText="Accounts">
                                    <ItemTemplate><%# AccountName( (int)Eval("AccountId") ) %></ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="Amount" SortExpression="Amount" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C2}" />
                                <Rock:RockBoundField DataField="Summary" SortExpression="Summary" />
                            </Columns>
                        </Rock:Grid>

                        <dl>
                        <asp:Repeater ID="rptrNotes" runat="server">
                            <ItemTemplate>
                                <dt><%# Eval("Caption") %></dt>
                                <dd><%# Eval("Text") %>
                                    <small>- <%# Eval("Person") %> on <%# Eval("Date") %> at <%# Eval("Time") %></small>
                                </dd>
                            </ItemTemplate>
                        </asp:Repeater>
                        </dl>

                    </div>
                </div>

                <div class="actions margin-t-md">
                    <asp:LinkButton ID="lbUpdate" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbUpdate_Click" />
                    <asp:LinkButton ID="lbCancelSchedule" runat="server" Text="Cancel Schedule" CssClass="btn btn-danger js-cancel-txn" CausesValidation="false" OnClick="lbCancelSchedule_Click" Visible="false" />
                    <asp:LinkButton ID="lbReactivateSchedule" runat="server" Text="Reactivate Schedule" CssClass="btn btn-success js-reactivate-txn" CausesValidation="false" OnClick="lbReactivateSchedule_Click" Visible="false"/>
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" />
                </div>

                <Rock:NotificationBox ID="nbError" CssClass="margin-t-lg" runat="server" Visible="false" NotificationBoxType="Danger" Dismissable="true" />

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
