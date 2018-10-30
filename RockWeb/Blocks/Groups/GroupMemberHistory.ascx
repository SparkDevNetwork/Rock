<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberHistory.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMemberHistory" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfGroupMemberId" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-history"></i>
                    <asp:Literal ID="lGroupTitle" runat="server" />
                    <asp:Literal ID="lGroupGridTitle" runat="server" Text=" | Historical Group Members" Visible="false" />
                    <%-- lGroupMemberPreHtml and lGroupMemberTitle will be Visible=true when a GroupMember is selected  --%>
                    <asp:Literal ID="lGroupMemberPreHtml" runat="server" Text=" | " Visible="false" />
                    <asp:Literal ID="lGroupMemberTitle" runat="server" Visible="false" />
                </h1>
            </div>

            <asp:Panel ID="pnlMembers" runat="server" Visible="false">
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfGroupMembers" runat="server" OnDisplayFilterValue="gfGroupMembers_DisplayFilterValue" OnClearFilterClick="gfGroupMembers_ClearFilterClick" OnApplyFilterClick="gfGroupMembers_ApplyFilterClick">
                            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                            <Rock:SlidingDateRangePicker ID="sdrDateAdded" runat="server" Label="Date Added" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                            <Rock:SlidingDateRangePicker ID="sdrDateRemoved" runat="server" Label="Date Removed" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                            <Rock:RockCheckBoxList ID="cblRole" runat="server" Label="Last Role" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                            <Rock:RockCheckBoxList ID="cblGroupMemberStatus" runat="server" Label="Last Status" RepeatDirection="Horizontal" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gGroupMembers" runat="server" AllowSorting="true" DataKeyNames="Id" CssClass="js-grid-group-members"
                            OnRowSelected="gGroupMembers_RowSelected" OnRowDataBound="gGroupMembers_RowDataBound">
                            <Columns>
                                <Rock:RockLiteralField ID="lPersonNameHtml" HeaderText="Name" SortExpression="Person.LastName,Person.NickName" />
                                <Rock:PersonField DataField="Person" HeaderText="Name" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                <Rock:DateField DataField="DateTimeAdded" HeaderText="Date Added" SortExpression="DateTimeAdded" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left"/>
                                <Rock:DateField DataField="ArchivedDateTime" HeaderText="Date Removed" SortExpression="ArchivedDateTime" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                                <Rock:RockBoundField DataField="GroupRole.Name" HeaderText="Last Role" SortExpression="GroupRole.Name" />
                                <Rock:EnumField DataField="GroupMemberStatus" HeaderText="Last Status" SortExpression="GroupMemberStatus" />
                                <Rock:RockLiteralField ID="lPersonProfileLink" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>

                <script>

                    Sys.Application.add_load(function () {
                        $("div.photo-icon").lazyload({
                            effect: "fadeIn"
                        });

                        // person-link-popover
                        $('.js-person-popover').popover({
                            placement: 'right',
                            trigger: 'manual',
                            delay: 500,
                            html: true,
                            content: function () {
                                var dataUrl = Rock.settings.get('baseUrl') + 'api/People/PopupHtml/' + $(this).attr('personid') + '/false';

                                var result = $.ajax({
                                    type: 'GET',
                                    url: dataUrl,
                                    dataType: 'json',
                                    contentType: 'application/json; charset=utf-8',
                                    async: false
                                }).responseText;

                                var resultObject = jQuery.parseJSON(result);

                                return resultObject.PickerItemDetailsHtml;

                            }
                        }).on('mouseenter', function () {
                            var _this = this;
                            $(this).popover('show');
                            $(this).siblings('.popover').on('mouseleave', function () {
                                $(_this).popover('hide');
                            });
                        }).on('mouseleave', function () {
                            var _this = this;
                            setTimeout(function () {
                                if (!$('.popover:hover').length) {
                                    $(_this).popover('hide')
                                }
                            }, 100);
                        });
                    });
                </script>
            </asp:Panel>


            <asp:Literal ID="lTimelineHtml" runat="server" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
