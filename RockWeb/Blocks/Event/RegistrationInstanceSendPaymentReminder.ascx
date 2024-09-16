<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceSendPaymentReminder.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstancePaymentReminder" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlSend" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bell-o"></i> Send Payment Reminders</h1>
            </div>
            <div class="panel-body">
                <asp:Panel ID="pnlPreview" runat="server">
                    <h4></h4>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="txtFromName" runat="server" Label="From Name" Required="true" />
                            <Rock:RockTextBox ID="txtFromEmail" runat="server" Label="From Email" Required="true" />
                            <Rock:RockTextBox ID="txtFromSubject" runat="server" Label="Subject" Required="true" />
                        </div>
                        <div class="col-md-12">
                            <label>Message</label>
                            <Rock:Toggle ID="tglEmailBodyView" runat="server" CssClass="pull-right" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" OnText="Preview" OffText="Source" Checked="true" OnCheckedChanged="tglEmailBodyView_CheckedChanged" />
                            <iframe id="ifEmailPreview" runat="server" style="width: 100%; height: 400px;" />
                            <Rock:CodeEditor ID="ceEmailMessage" runat="server" EditorHeight="400" Visible="false" />
                        </div>
                    </div>
                    
                    <hr />
                    <h4>Outstanding Balances</h4>
                    <asp:Literal ID="lBalanceInstructions" runat="server" />
                    <Rock:Grid ID="gRegistrations" runat="server" AllowPaging="false" RowItemText="Registration" AllowSorting="true" DataKeyNames="Id">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockTemplateField HeaderText="Name" SortExpression="Name">
                                <ItemTemplate>
                                    <%# Eval("LastName") %>, <%# Eval("FirstName") %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Date Registered" SortExpression="CreatedDateTime">
                                <ItemTemplate>
                                    <%# ((DateTime)Eval("CreatedDateTime")).ToShortDateString() %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="ConfirmationEmail" HeaderText="Email" />
                            <Rock:RockTemplateField HeaderText="Last Reminder Sent" SortExpression="LastPaymentReminderDateTime">
                                <ItemTemplate>
                                    <%# DaysSinceLastReminder((DateTime)Eval("LastPaymentReminderDateTime")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            
                            <Rock:CurrencyField DataField="TotalCost" HeaderText="Cost" />
                            <Rock:CurrencyField DataField="TotalPaid" HeaderText="Amount Paid" />
                            <Rock:CurrencyField DataField="BalanceDue" HeaderText="Balance Due" SortExpression="BalanceDue" />
                        </Columns>
                    </Rock:Grid>
                    
                    <asp:LinkButton ID="btnSendReminders" runat="server" Text="Send Reminders" CssClass="btn btn-primary margin-t-md" OnClick="btnSendReminders_Click" />
                </asp:Panel>

                <Rock:NotificationBox ID="nbMessages" runat="server" NotificationBoxType="Info" />
            </div>
        
        </asp:Panel>

        <asp:Panel ID="pnlComplete" runat="server" Visible="false">
            <Rock:NotificationBox ID="nbResult" NotificationBoxType="Success" runat="server" />
        </asp:Panel>


        <script type="text/javascript">
          Sys.WebForms.PageRequestManager.getInstance().add_endRequest(pageLoaded);

          function pageLoaded(sender, args) {
             window.scrollTo(0,0);
          }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
