<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendanceDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupAttendanceDetail" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="lbPrintAttendanceRoster" />
    </Triggers>
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading clearfix">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-check-square-o"></i>
                    <asp:Literal ID="lHeading" runat="server" Text="Group Attendance" />
                </h1>
                <Rock:ButtonDropDownList ID="bddlCampus" runat="server" FormGroupCssClass="panel-options pull-right" Title="All Campuses" SelectionStyle="Checkmark"
                    OnSelectionChanged="bddlCampus_SelectionChanged" DataTextField="Name" DataValueField="Id" />
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbNotice" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <asp:CustomValidator ID="cvAttendance" runat="server" Display="None" />

                <asp:Panel ID="pnlDetails" runat="server">

                    <div class="row">
                        <div class="col-sm-3">
                            <Rock:RockLiteral ID="lOccurrenceDate" runat="server" Label="Attendance For" />
                            <Rock:DatePicker ID="dpOccurrenceDate" runat="server" Label="Attendance For" AllowFutureDateSelection="false" Required="true" />
                        </div>
                        <div class="col-sm-3">
                            <Rock:RockLiteral ID="lLocation" runat="server" Label="Location" />
                            <Rock:RockDropDownList ID="ddlLocation" runat="server" Label="Location" DataValueField="Key" DataTextField="Value"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlLocation_SelectedIndexChanged" />
                        </div>
                        <div class="col-sm-3">
                            <Rock:RockLiteral ID="lSchedule" runat="server" Label="Schedule" />
                            <Rock:RockDropDownList ID="ddlSchedule" runat="server" Label="Schedule" DataValueField="Key" DataTextField="Value" />
                        </div>
                        <div class="col-sm-3">
                            <Rock:RockLiteral ID="lDidAttendCount" runat="server" Label="Attendance Count" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:RockCheckBox ID="cbDidNotMeet" runat="server" Text="We Did Not Meet" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">

                            <div class="js-roster">
                                <div class="panel-labels clearfix">
                                    <div class="pull-left">
                                        <h4 class="js-members-label">
                                            <asp:Literal ID="lMembers" runat="server" />
                                        </h4>
                                    </div>
                                    <div class="pull-right">
                                        <Rock:Toggle ID="tglSort" runat="server" OnText="Last Name" OnCssClass="btn-primary" OffCssClass="btn-outline-primary" ActiveButtonCssClass="btn-primary" ButtonSizeCssClass="btn-xs" OffText="First Name" AutoPostBack="true" OnCheckedChanged="tglSort_CheckedChanged" Checked="true" Label="Sort by:" />
                                    </div>
                                </div>
                                <asp:ListView ID="lvMembers" runat="server">
                                    <ItemTemplate>
                                        <asp:HiddenField ID="hfMember" runat="server" Value='<%# Eval("PersonId") %>' />
                                        <Rock:RockCheckBox ID="cbMember" runat="server" Checked='<%# Eval("Attended") %>' OnDataBinding="cbMember_DataBinding"/>
                                    </ItemTemplate>
                                </asp:ListView>
                                <div class="pull-left margin-b-md margin-r-md">
                                    <Rock:PersonPicker ID="ppAddPerson" runat="server" OnSelectPerson="ppAddPerson_SelectPerson" />
                                </div>
                                <div class="pull-left margin-b-lg">
                                    <asp:LinkButton ID="lbAddMember" runat="server" CssClass="btn btn-default" OnClick="lbAddMember_Click" CausesValidation="false" Visible="false"><i class="fa fa-plus"></i> Add Group Member</asp:LinkButton>
                                </div>
                            </div>

                        </div>
                        <div class="col-md-6">

                            <asp:Panel ID="pnlPendingMembers" runat="server" Visible="false">
                                <h4>
                                    <asp:Literal ID="lPendingMembers" runat="server" /></h4>
                                <asp:ListView ID="lvPendingMembers" runat="server" OnItemCommand="lvPendingMembers_ItemCommand">
                                    <ItemTemplate>
                                        <div class="form-group">
                                            <asp:HiddenField ID="hfMember" runat="server" Value='<%# Eval("Id") %>' />
                                            <asp:Label ID="lName" runat="server" Text='<%# Eval("FullName") %>' />
                                            <asp:LinkButton ID="lbAdd" runat="server" ToolTip="Add Person to Group" CausesValidation="false" CommandName="Add" CommandArgument='<%# Eval("Id") %>' CssClass="js-add-member"><i class="fa fa-plus"></i></asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:ListView>
                            </asp:Panel>

                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="dtNotes" runat="server" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" SourceTypeName="Rock.Model.AttendanceOccurrence, Rock" PropertyName="Notes"></Rock:DataTextBox>
                        </div>
                    </div>


                    <Rock:NotificationBox ID="nbPrintRosterWarning" runat="server" NotificationBoxType="Warning" />

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                        <asp:LinkButton ID="lbCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false"></asp:LinkButton>
                        <asp:LinkButton ID="lbPrintAttendanceRoster" runat="server" ToolTip="Print Attendance Roster" CssClass="btn btn-default btn-sm pull-right" OnClick="lbPrintAttendanceRoster_Click" CausesValidation="false"><i class="fa fa-clipboard"></i></asp:LinkButton>
                    </div>

                </asp:Panel>

            </div>

        </div>

        <script>
            Sys.Application.add_load(function () {
                // toggle all checkboxes
                $('.js-members-label').on('click', function (e) {

                    var container = $(this).parent();
                    var isChecked = container.hasClass('all-checked');

                    container.find('input:checkbox').each(function () {
                        $(this).prop('checked', !isChecked);
                    });

                    if (isChecked) {
                        container.removeClass('all-checked');
                    }
                    else {
                        container.addClass('all-checked');
                    }

                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
