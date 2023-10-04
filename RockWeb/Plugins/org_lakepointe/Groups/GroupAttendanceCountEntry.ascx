<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendanceCountEntry.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Groups.GroupAttendanceCountEntry" %>
<!-- TODO: remove btn-link and btn-xs styling updates-->
<style type="text/css">
    btn-link > a {
        revert;
    }
    .btn-link {
        font-weight: 400;
        color: #007acc !important;
        border-radius: 0
    }

        .btn-link, .btn-link:active, .btn-link.active, .btn-link[disabled], fieldset[disabled] .btn-link {
            background-color: transparent;
            -webkit-box-shadow: none;
            box-shadow: none
        }

            .btn-link, .btn-link:hover, .btn-link:focus, .btn-link:active {
                border-color: transparent;
                padding: 1px 5px

            }

                .btn-link:hover, .btn-link:focus {
                    color: #009ce3 !important;
                    text-decoration: none;
                    background: transparent !important;
                    background-color: transparent
                }

                .btn-link[disabled]:hover, fieldset[disabled] .btn-link:hover, .btn-link[disabled]:focus, fieldset[disabled] .btn-link:focus {
                    color: #d8d8d8;
                    text-decoration: none
                }

    .btn-xs, .btn-group-xs > .btn {
        padding: 1px 5px;
        font-size: 14px;
        line-height: 1.5;
        border-radius: 0
    }

</style>
<script type="text/javascript">
    var redirectTimeout;
    Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(function () {
        {
            clearTimeout(redirectTimeout);
        }
    });

    function submitRedirect(url, delay) {
        redirectTimeout = setTimeout(function () {
            window.location.href = url;
        }, delay);

    }
</script>
<asp:UpdatePanel ID="upAttendanceEntry" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbAlert" runat="server" NotificationBoxType="Default" Visible="false" />
        <asp:Panel ID="pnlGroupList" runat="server" Visible="false">
                <h2 >
                    <asp:Literal ID="lGroupListHeader" runat="server" /></h2>
                <asp:Literal ID="lListInstructions" runat="server" />
                <Rock:Grid ID="gGroupList" runat="server" DataKeyNames="GroupId" ShowFooter="false" ShowActionRow="false" AllowSorting="true" AllowPaging="true" RowItemText="Group" OnRowSelected="gGroupList_RowSelected" OnRowDataBound="gGroupList_RowDataBound">
                    <Columns>
                        <Rock:RockBoundField HeaderText="Id" DataField="Id" Visible="false" />
                        <Rock:RockBoundField HeaderText="Group Name" DataField="Group.Name" Visible="true" />
                        <Rock:RockBoundField HeaderText="Campus" DataField="Group.Campus.Name" />
                        <Rock:RockBoundField HeaderText="Meeting Time" DataField="MeetingTime" />
                        <Rock:RockBoundField HeaderText="Role" DataField="GroupRole.Name" Visible="true" />
                        <Rock:RockTemplateField HeaderText="Current Week" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:Literal ID="lThisWeek" runat="server" />
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:RockTemplateField HeaderText ="Previous Week" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:Literal ID="lPreviousWeek" runat="server" />
                            </ItemTemplate>

                        </Rock:RockTemplateField>


                        <%--<Rock:BoolField HeaderText="Attendance Submitted" DataField="HasAttendanceBeenSubmitted" Visible="true" />--%>
                    </Columns>
                </Rock:Grid>
        </asp:Panel>
        <asp:Panel ID="pnlGroupAttendance" runat="server" Visible="false">
            <div>
                    <asp:LinkButton ID="lbBackToList" runat="server" CssClass="btn btn-default" Text="Back to Group List" OnClick="lbBackToList_Click" />
            </div>
            </div>
            <h2>
                <asp:Literal ID="lGroupEntryTitle" runat="server" /></h2>
            <asp:HiddenField ID="hfGroupId" runat="server" />

            <div class="row">
                <div class="col-xs-12">
                    <asp:Literal ID="lInstructions" runat="server" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-4 col-xs-12">
                    <Rock:RockDropDownList ID="ddlClassDate" runat="server" Label="Class Date:" AutoPostBack="true" OnSelectedIndexChanged="ddlClassDate_SelectedIndexChanged" />
                    <Rock:NumberBox ID="tbAttendees" runat="server" Label="Attendee Count:" Visible="true" NumberType="Integer" />
                    <Rock:RockCheckBox ID="cbDidNotOccur" runat="server" Label="We Did Not Meet:" Visible="true" />
                </div>

            </div>
            <div class="row">
                <div class="col-xs-12">
                    <Rock:RockTextBox ID="tbNotes" runat="server" Label="Notes:" Visible="true" TextMode="MultiLine" Rows="4" />
                    <Rock:RockLiteral ID="lUpdatedBy" runat="server" Label=" Attendance Updated By:" Visible="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-xs-12">
                    <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" OnClick="lbSave_Click">Save</asp:LinkButton>
                    <asp:LinkButton ID="lReset" runat="server" CssClass="btn btn-default" OnClick="lReset_Click">Reset</asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
