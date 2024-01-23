<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventCalendarItemPersonalizedRegistration.ascx.cs" Inherits="RockWeb.Blocks.Event.EventCalendarItemPersonalizedRegistration" %>

<style>
.eventitem {
  cursor: pointer;
}

.eventitem-select {
  font-size: 38px;
}

.eventitem-select::after {
  content: "\f0c8";
}

.eventitem.selected .eventitem-select::after {
  content: "\f14a";
}
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfSelectedEventId" ClientIDMode="Static" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-circle"></i> <asp:Literal ID="lBlockTitle" runat="server" /></h1>

                <div class="panel-labels form-horizontal">
                    <Rock:CampusPicker ID="cpCampus" FormGroupCssClass="form-group-sm" Style="width: 170px;" runat="server" AutoPostBack="true" OnSelectedIndexChanged="cpCampus_SelectedIndexChanged" />
                </div>
            </div>
            <div class="panel-body">
                <asp:Literal ID="lErrors" runat="server" />

                <asp:Panel ID="pnlRegistrationForm" runat="server">

                    <asp:Literal ID="lEventIntro" runat="server" />

                    <asp:Repeater ID="rptEvents" runat="server" OnItemDataBound="rptEvents_ItemDataBound">
                         <ItemTemplate>
                             <div class="well eventitem js-eventitem <%# Container.ItemIndex == 0 ? "selected" : "" %>">
                                 <div class="d-flex">
                                     <div class="flex-shrink-0 mr-3">
                                         <div class="eventitem-select fa"></div>
                                     </div>
                                     <div class="flex-fill">
                                         <div class="row">
                                             <div class="col-md-4 col-sm-8">
                                                 <h5 class="margin-t-none"><%# DateTime.Parse(Eval("StartDate").ToString()).ToString("d") + " " + DateTime.Parse(Eval("StartDate").ToString()).ToString(" (dddd)") %></h5>

                                                 <p>
                                                    <%# string.Format(Eval("Location").ToString().Length > 0 ? "Location: {0}" : "", Eval("Location").ToString())  %>
                                                    <%# string.Format(Eval("StartDate").ToString().Length > 0 ? "Start Time: {0}" : "", Eval("StartDate", "{0:h:mm tt}"))  %>
                                                 </p>
                                            </div>
                                            <div class="col-md-4 col-sm-8">
                                                <p>
                                                    <%# (Eval("ContactName").ToString().Length > 0 || Eval("ContactEmail").ToString().Length > 0 || Eval("ContactPhone").ToString().Length > 0) ? "<strong>Contact:</strong><br />" : ""  %>

                                                    <%# Eval("ContactName") %><br />
                                                    <%# Eval("ContactEmail") %><br />
                                                    <%# Eval("ContactPhone") %>
                                                 </p>
                                            </div>

                                             <div class="col-sm-4">
                                                 <div class="pull-right label label-campus" id="campusLabel" runat="server">
                                                     <%# Eval("Campus") %>
                                                 </div>
                                             </div>
                                         </div>

                                         <%# string.Format(Eval("Note").ToString().Length > 0 ? "<strong>Note:</strong><p>{0}</p>" : "",  Eval("Note")) %>
                                         <asp:HiddenField ID="hfEventId" runat="server" Value='<%# Eval("Id") %>' />
                                     </div>
                                 </div>
                             </div>
                         </ItemTemplate>
                    </asp:Repeater>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBoxList ID="cblRegistrants" runat="server" Label="Register" RepeatDirection="Horizontal" RepeatColumns="2" />
                        </div>
                        <div class="col-md-6">
                            <Rock:EmailBox ID="ebEmailReminder" runat="server" Label="Email Reminder Address" />
                        </div>
                    </div>

                    <asp:LinkButton ID="lbRegister" runat="server" Text="Start Registration" CssClass="btn btn-primary margin-b-md margin-t-md" OnClick="lbRegister_Click" />

                    <asp:Literal ID="lMessages" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlComplete" runat="server">
                    <asp:Literal id="lCompleteMessage" runat="server" />
                </asp:Panel>
            </div>

            <script>
                Sys.Application.add_load(function () {
                    $(".js-eventitem").on("click", function () {
                        $('.js-eventitem').removeClass('selected');
                        $(this).addClass('selected');
                        var eventId = $(this).find("input[type='hidden']").val();
                        $('#hfSelectedEventId').val(eventId);
                    });
                });
            </script>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
