<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Person.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Person" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $(".photo a").fluidbox();

        $('.js-cancel-checkin').click(function (event) {
            event.stopImmediatePropagation();
            var personName = $('H4.js-checkin-person-name').first().text();
            return Rock.dialogs.confirmDelete(event, 'Checkin for ' + personName);
        });
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="row margin-b-sm">
            <div class="col-sm-3 col-md-2 xs-text-center">
                <asp:Literal ID="lPhoto" runat="server" />

            </div>
            <div class="col-sm-9 col-md-10 xs-text-center">
                <h1 class="title name js-checkin-person-name margin-t-none"><asp:Literal ID="lName" runat="server"></asp:Literal></h1>
                <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />
                <div class="row">
                    <div class="col-md-6">
                        <ul class="list-unstyled">
                            <li>
                                <asp:Literal ID="lGender" runat="server" />
                            </li>
                            <li>
                                <asp:Literal ID="lAge" runat="server" />
                            </li>
                            <li>
                                <asp:Literal ID="lGrade" runat="server" />
                            </li>
                        </ul>
                        <Rock:RockControlWrapper ID="rcwPhone" runat="server" Label="Phone(s)">
                            <ul class="list-unstyled list-horizontal">
                                <asp:Repeater ID="rptrPhones" runat="server">
                                    <ItemTemplate>
                                        <li><a class="btn btn-default" href='tel:<%# Eval("Number") %>'><i class="fa fa-phone-square"></i><%# Eval("NumberFormatted") %> <small>(<%# Eval("NumberTypeValue.Value") %>)</small></a></li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </Rock:RockControlWrapper>
                        <Rock:RockControlWrapper ID="rcwTextMessage" runat="server" Visible="true" Label="Text Message">
                            <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" Dismissable="true" />
                            <!-- Initiates entry -->
                            <asp:LinkButton runat="server" ID="btnSms" EnableViewState="true" CssClass="btn btn-default js-btn-sms" OnClick="btnSms_Click" />

                            <!-- During entry -->
                            <textarea runat="server" rows="3" cols="30" id="tbSmsMessage" visible="false" placeholder="Your message here..." class="form-control js-sms-message margin-b-sm"></textarea>
                            <asp:LinkButton runat="server" Visible="false" ID="btnSmsSend" CssClass="btn btn-xs btn-primary js-btn-send" OnClick="btnSend_Click">Send</asp:LinkButton>
                            <asp:LinkButton runat="server" Visible="false" ID="btnSmsCancel" CssClass="btn btn-xs btn-link" OnClick="btnSmsCancel_Click">Cancel</asp:LinkButton>
                        </Rock:RockControlWrapper>

                        <Rock:RockLiteral ID="lEmail" runat="server" Label="Email" />
                    </div>
                    <div class="col-md-6">
                        <div ID="pnlAdultFields" runat="server" class="adult-attributes">
                            <Rock:AttributeValuesContainer ID="avcAdultAttributes" runat="server" ShowCategoryLabel="false" />
                        </div>
                        <div ID="pnlChildFields" runat="server" class="child-attributes">
                            <Rock:AttributeValuesContainer ID="avcChildAttributes" runat="server" ShowCategoryLabel="false" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <Rock:RockControlWrapper ID="rcwFamily" runat="server" Label="Family" CssClass="list-unstyled">
            <ul class="list-unstyled list-horizontal">
                <asp:Repeater ID="rptrFamily" runat="server" OnItemDataBound="rptrFamily_ItemDataBound">
                    <ItemTemplate>
                        <li><a class="btn btn-action" href='<%# Eval("Url") %>'>
                            <asp:Literal ID="lFamilyIcon" runat="server" />
                            <%# Eval("FullName") %> <small><%# Eval("Note") %></small></a> </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </Rock:RockControlWrapper>
        <Rock:RockControlWrapper ID="rcwRelationships" runat="server" Label="Related People" CssClass="list-unstyled">
            <ul class="list-unstyled list-horizontal">
                <asp:Repeater ID="rptrRelationships" runat="server" OnItemDataBound="rptrRelationships_ItemDataBound">
                    <ItemTemplate>
                        <li><a class="btn btn-action" href='<%# Eval("Url") %>'>
                            <asp:Literal ID="lRelationshipsIcon" runat="server" />
                            <%# Eval("FullName") %> <small><%# Eval("Note") %></small></a> </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </Rock:RockControlWrapper>

        <!-- Reprint label functionality -->
        <Rock:NotificationBox ID="nbReprintMessage" runat="server" Visible="false"></Rock:NotificationBox>
        <Rock:ModalAlert ID="maNoLabelsFound" runat="server"></Rock:ModalAlert>
        <asp:HiddenField ID="hfCurrentAttendanceIds" runat="server" />
        <asp:HiddenField ID="hfPersonId" runat="server" />
        <asp:LinkButton ID="btnReprintLabels" runat="server" OnClick="btnReprintLabels_Click" CssClass="btn btn-default margin-b-md">Reprint Labels</asp:LinkButton>
        <Rock:ModalDialog ID="mdReprintLabels" runat="server" ValidationGroup="vgReprintLabels" Title="Label Reprints" OnSaveClick="mdReprintLabels_PrintClick" SaveButtonText="Print" Visible="false">
            <Content>
                <Rock:NotificationBox ID="nbReprintLabelMessages" runat="server" NotificationBoxType="Validation"></Rock:NotificationBox>
                <Rock:RockCheckBoxList ID="cblLabels" runat="server" Label="Labels" DataTextField="Name" DataValueField="FileGuid"></Rock:RockCheckBoxList>
                <Rock:RockDropDownList ID="ddlPrinter" runat="server" Label="Printers" DataTextField="Name" DataValueField="IPAddress"></Rock:RockDropDownList>
            </Content>
        </Rock:ModalDialog>

        <Rock:RockControlWrapper ID="rcwCheckinHistory" runat="server" Label="Check-in History">
            <Rock:Grid ID="gHistory" runat="server" DisplayType="Light" AllowPaging="false" CssClass="table-condensed">
                <Columns>
                    <Rock:RockTemplateField HeaderText="When">
                        <ItemTemplate>
                            <%# ((DateTime)Eval("Date")).ToShortDateString() %><br />
                            <%# Eval("Schedule") %>
                            <asp:Literal ID="lWhoCheckedIn" runat="server"></asp:Literal>
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
                            <asp:Literal ID="lActive" runat="server"></asp:Literal><br />
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
