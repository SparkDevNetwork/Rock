<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Person.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Person" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-cancel-checkin').click(function (event) {
            event.stopImmediatePropagation();
            var personName = $('H4.js-checkin-person-name').first().text();
            return Rock.dialogs.confirmDelete(event, 'Checkin for ' + personName);
        });
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <h4 class="js-checkin-person-name"><asp:Literal ID="lName" runat="server"></asp:Literal></h4>

        <div class="row margin-b-sm">
            <div class="col-sm-3">
                <asp:Literal ID="lPhoto" runat="server" />
            </div>
            <div class="col-sm-6">
                <ul class="list-unstyled">
                    <li><asp:Literal ID="lGender" runat="server" /></li>
                    <li><asp:Literal ID="lAge" runat="server" /></li>
                    <li><asp:Literal ID="lGrade" runat="server" /></li>
                </ul>
                <Rock:RockControlWrapper ID="rcwPhone" runat="server" Label="Phone(s)">
                    <ul class="list-unstyled list-horizontal">
                        <asp:Repeater ID="rptrPhones" runat="server">
                            <ItemTemplate>
                                <li><a class="btn btn-default" href='tel:<%# Eval("Number") %>' ><i class="fa fa-phone-square"></i> <%# Eval("NumberFormatted") %> <small>(<%# Eval("NumberTypeValue.Value") %>)</small></a></li>                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </Rock:RockControlWrapper>

                <Rock:RockLiteral ID="lEmail" runat="server" Label="Email" />
            </div>
            <div class="col-sm-2">
                <Rock:NotificationBox ID="nbResult" CssClass="margin-t-md" runat="server" Visible="false" Dismissable="true" />

                <!-- Initiates entry -->
                <asp:LinkButton runat="server" Visible="false" ID="btnSms" CssClass="btn btn-xs btn-info js-btn-sms" OnClick="btnSms_Click" >Send SMS</asp:LinkButton>

                <!-- During entry -->
                <textarea runat="server" rows="4" cols="30" id="tbSmsMessage" visible="false" placeholder="Your message here" class="js-sms-message"></textarea> <br />
                <asp:LinkButton runat="server" Visible="false" ID="btnSmsSend" CssClass="btn btn-xs btn-default js-btn-send" OnClick="btnSend_Click">Send</asp:LinkButton>
                <asp:LinkButton runat="server" Visible="false" ID="btnSmsCancel" OnClick="btnSmsCancel_Click">Cancel</asp:LinkButton>
            </div>
        </div>

        <Rock:RockControlWrapper ID="rcwFamily" runat="server" Label="Family" CssClass="list-unstyled">
            <ul class="list-unstyled list-horizontal">
                <asp:Repeater ID="rptrFamily" runat="server" OnItemDataBound="rptrFamily_ItemDataBound">
                    <ItemTemplate>
                        <li><a class="btn btn-action" href='<%# Eval("Url") %>' ><asp:Literal ID="lFamilyIcon" runat="server" /> <%# Eval("FullName") %> <small><%# Eval("Note") %></small></a> </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </Rock:RockControlWrapper>
        
        <Rock:RockControlWrapper ID="rcwRelationships" runat="server" Label="Related People" CssClass="list-unstyled">
            <ul class="list-unstyled list-horizontal">
                <asp:Repeater ID="rptrRelationships" runat="server" OnItemDataBound="rptrRelationships_ItemDataBound">
                    <ItemTemplate>
                        <li><a class="btn btn-action" href='<%# Eval("Url") %>' ><asp:Literal ID="lRelationshipsIcon" runat="server" /> <%# Eval("FullName") %> <small><%# Eval("Note") %></small></a> </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </Rock:RockControlWrapper>

        <Rock:RockControlWrapper ID="rcwCheckinHistory" runat="server" Label="Check-in History">
            <Rock:Grid ID="gHistory" runat="server" DisplayType="Light" AllowPaging="false" CssClass="table-condensed">
                <Columns>
                    <Rock:RockTemplateField HeaderText="When">
                        <ItemTemplate>
                            <%# ((DateTime)Eval("Date")).ToShortDateString() %><br />
                            <%# Eval("Schedule") %>
                            <asp:Literal id="lWhoCheckedIn" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </Rock:RockTemplateField>
                    <Rock:RockTemplateField HeaderText="Where">
                        <ItemTemplate>
                            <%# Eval("Location") %><br />
                            <%# Eval("Group") %>
                        </ItemTemplate>
                    </Rock:RockTemplateField>
                    <Rock:RockTemplateField HeaderText="Code">
                        <ItemTemplate>
                            <asp:Literal id="lActive" runat="server"></asp:Literal><br />
                            <%# Eval("Code") %>
                        </ItemTemplate>
                    </Rock:RockTemplateField>
                    <Rock:DeleteField OnClick="gHistory_Delete" ButtonCssClass="js-cancel-checkin btn btn-xs btn-danger" Tooltip="Delete This Checkin" />
                </Columns>
            </Rock:Grid>
        </Rock:RockControlWrapper>

		<%-- Ensures that a blank SMS cannot be sent --%>
        <script type="text/javascript">
            function setSmsSendDisabled(boolean) {
                $('.js-btn-send').attr('disabled', boolean);
            }

            Sys.Application.add_load(function () {
                setSmsSendDisabled(true);
                $('.js-sms-message').on('input', function (e) {
                    var tbValue = $(this).val();
                    setSmsSendDisabled(!tbValue.trim());
                });

                $('.js-btn-sms').on('click', function (e) {
                    setSmsSendDisabled(true);
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
