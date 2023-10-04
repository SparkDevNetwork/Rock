<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GLTGroupRoster.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Groups.GLTGroupRoster" %>
<asp:UpdatePanel ID="upGroupRoster" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <style type="text/css">
            .editHeader {
                color: #ffffff;
                text-align: center;
                background-color: #f04b28;
                font-size: 48px;
                font-weight: bold;
                line-height: 2em;
            }

            .gmCard {
                /*overflow: hidden;*/
                /*min-height: 200px;*/
                padding-top: 10px;
                padding-bottom: 10px;
            }

                .gmCard .gmCard-header {
                    background-color: #f04b28;
                    color: #ffffff;
                    font-weight: 600;
                    font-size: 18pt;
                    height: 60px;
                    vertical-align: middle;
                }

                .gmCard .gmCard-header {
                    text-align: center;
                }

                    .gmCard .gmCard-header img {
                        width: 60px;
                        height: 60px;
                    }

                .gmCard .gmCard-body {
                    /*overflow: hidden;*/
                    line-height: 1.4;
                    /*font-size: 14px;*/
                    min-height: 250px;
                }

                .gmCard .gmCard-footer {
                    background-color: #f04b28;
                    text-align: center;
                    height: 30px;
                }

            .gmCard-footer input.largeCB {
                width: 20px;
                height: 20px;
            }

            .grid-filter header button.btn-xs {
                font-size: 12px;
                padding-top: 1px;
                padding-bottom: 1px;
                padding-left: 15px;
                padding-right: 15px;
                line-height: 1.5;
                background-color: #f04b28;
                margin-bottom: 15px;
            }

            .grid-filter header .btn-xs.is-open {
                background-color: #f04b28;
            }

            .grid-filter-overview {
                padding-top: 10px;
            }

            .grid-filter .grid-filter-entry {
                padding-top: 45px;
            }

            .grid-actions .btn {
                background: #f04b28;
            }

            .btn-action {
                background: #f04b28;
                color: #fff;
            }

            .grid-filter-overview {
                overflow: hidden;
                min-height: 50px;
            }

            .btn-xs {
                padding: 1px 5px;
                font-size: 12px;
                line-height: 1.5;
            }

                .btn-xs:hover {
                    padding: 1px 10px;
                    font-size: 12px;
                    line-height: 1.5;
                    border-width: 6px 14px 6px 8px !important;
                }

            .btn-memberEdit::after, .btn-memberEdit:after {
                content: none;
            }

            .btn-memberEdit:hover {
                padding: 0.3em 1em;
                border: 2px solid;
                border-color: transparent !important;
                letter-spacing: 1px;
            }

            .btn-floater {
                margin: 1em 1em 1em 1em;
            }
        </style>
        <Rock:NotificationBox ID="nbGroupMembers" runat="server" NotificationBoxType="Warning" />
        <asp:Panel ID="pnlGroupMembers" runat="server" Visible="false">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title pull-left">
                        <i class="fas fa-user-friends"></i>
                        <asp:Literal ID="lHeading" runat="server" Text="Group Members"></asp:Literal>
                    </h1>
                </div>
                <div class="panel-body">
                    <asp:Panel ID="pnlCommandsTop" runat="server" CssClass="row">
                        <div class="col-md-12">
                            <span class="pull-right">
                                <asp:LinkButton ID="lbSendCommunicationTop" CssClass="btn btn-xs btn-default btn-floater" runat="server" OnClick="lbSendCommunication_Click"><i class="far fa-envelope"></i> Send Communication</asp:LinkButton>
                            </span>
                            <span class="pull-right">
                                <asp:LinkButton ID="lbSendParentCommunication" CssClass="btn btn-xs btn-default btn-floater" runat="server" OnClick="lbSendParentCommunication_Click"><i class="far fa-envelope"></i> Send Parent Communication</asp:LinkButton>
                            </span>
                        </div>
                    </asp:Panel>
                    <Rock:GridFilter ID="gfGroupMember" runat="server" OnApplyFilterClick="gfGroupMember_ApplyFilterClick" OnDisplayFilterValue="gfGroupMember_DisplayFilterValue" OnClearFilterClick="gfGroupMember_ClearFilterClick" Visible="false">

                        <Rock:RockCheckBoxList ID="cblGender" runat="server" Label="Gender" RepeatDirection="Horizontal" Visible="false">
                            <asp:ListItem Text="Male" Value="1" />
                            <asp:ListItem Text="Female" Value="2" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblGroupRole" runat="server" Label="Role" RepeatDirection="Horizontal" Visible="false" />
                        <Rock:RockCheckBoxList ID="cblStatus" runat="server" Label="Status" Visible="false" />

                    </Rock:GridFilter>
                    <asp:Repeater ID="rptGroupMember" runat="server" OnItemDataBound="rptGroupMember_ItemDataBound" OnItemCommand="rptGroupMember_ItemCommand">
                        <ItemTemplate>
                            <asp:Literal ID="lRowHeader" runat="server" />
                            <div class="col-md-6 gmCard">
                                <div class="gmCard-header">
                                    <div class="row">
                                        <div class="col-sm-12">
                                            <asp:Panel ID="pnlImage" runat="server" Style="width: 60px; margin-right: 15px; float: left;">
                                                <asp:Image ID="imgMember" runat="server" />
                                            </asp:Panel>
                                            <asp:Panel ID="pnlMemberHeader" runat="server" Visible="true">
                                                <span class="pull-right">
                                                    <asp:LinkButton ID="lbEditMember" runat="server" CssClass="btn btn-md btn-link btn-memberEdit" CommandName="EditMember"><i class="far fa-edit"></i>                                                   </asp:LinkButton></span>
                                                <asp:Literal ID="lName" runat="server" /><br />
                                                <asp:Literal ID="lBadges" runat="server" />
                                            </asp:Panel>
                                        </div>

                                    </div>

                                </div>
                                <div class="gmCard-body">
                                    <div class="row">
                                        <div class="col-xs-4">
                                            <label>Role</label>
                                        </div>
                                        <div class="col-xs-8">
                                            <asp:Literal ID="lRole" runat="server" />
                                        </div>
                                    </div>
                                    <asp:Panel ID="pnlEmail" CssClass="row" runat="server" Visible="false">
                                        <div class="col-xs-4">
                                            <label>Email</label>
                                        </div>
                                        <div class="col-xs-8">
                                            <asp:Literal ID="lEmail" runat="server" />
                                        </div>
                                    </asp:Panel>
                                    <asp:Panel ID="pnlBirthDate" CssClass="row" runat="server" Visible="false">
                                        <div class="col-xs-4">
                                            <label>Birth Date</label>
                                        </div>
                                        <div class="col-xs-8">
                                            <asp:Literal ID="lBirthDate" runat="server" />
                                        </div>
                                    </asp:Panel>
                                    <asp:Panel ID="pnlAddress" CssClass="row" runat="server" Visible="false">
                                        <div class="col-xs-4">
                                            <label>Address</label>
                                        </div>
                                        <div class="col-xs-8">
                                            <asp:Literal ID="lAddress" runat="server" />
                                        </div>
                                    </asp:Panel>
                                    <asp:Panel ID="pnlPhones" runat="server" Visible="false">
                                        <asp:Literal ID="lPhones" runat="server" />
                                    </asp:Panel>
                                    <asp:Panel ID="pnlSchool" CssClass="row" runat="server" Visible="false">
                                        <div class="col-xs-4">
                                            <label>School</label>
                                        </div>
                                        <div class="col-xs-8">
                                            <asp:Literal ID="lSchool" runat="server" />
                                        </div>
                                    </asp:Panel>
                                    <asp:Panel ID="pnlFirstAttended" CssClass="row" runat="server" Visible="false">
                                        <div class="col-xs-4">
                                            <label>First Attended</label>
                                        </div>
                                        <div class="col-xs-8">
                                            <asp:Literal ID="lFirstAttended" runat="server" />
                                        </div>
                                    </asp:Panel>
                                    <asp:Panel ID="pnlLastAttended" CssClass="row" runat="server" Visible="false">
                                        <div class="col-xs-4">
                                            <label>Last Attended</label>
                                        </div>
                                        <div class="col-xs-8">
                                            <asp:Literal ID="lLastAttended" runat="server" />
                                        </div>
                                    </asp:Panel>
                                    <asp:Panel ID="pnlParent1" CssClass="card-body" runat="server" Visible="false">
                                        <asp:Panel ID="pnlParent1Name" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Name</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent1Name" runat="server" />
                                            </div>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlParent1Mobile" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Mobile</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent1Mobile" runat="server" />
                                            </div>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlParent1Email" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Email</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent1Email" runat="server" />
                                            </div>
                                        </asp:Panel>
                                    </asp:Panel>
                                    <asp:Panel ID="pnlParent2" CssClass="card-body" runat="server" Visible="false">
                                        <asp:Panel ID="pnlParent2Name" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Name</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent2Name" runat="server" />
                                            </div>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlParent2Mobile" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Mobile</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent2Mobile" runat="server" />
                                            </div>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlParent2Email" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Email</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent2Email" runat="server" />
                                            </div>
                                        </asp:Panel>
                                    </asp:Panel>
                                    <asp:Panel ID="pnlParent3" CssClass="card-body" runat="server" Visible="false">
                                        <asp:Panel ID="pnlParent3Name" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Name</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent3Name" runat="server" />
                                            </div>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlParent3Mobile" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Mobile</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent3Mobile" runat="server" />
                                            </div>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlParent3Email" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Email</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent3Email" runat="server" />
                                            </div>
                                        </asp:Panel>
                                    </asp:Panel>
                                    <asp:Panel ID="pnlParent4" CssClass="card-body" runat="server" Visible="false">
                                        <asp:Panel ID="pnlParent4Name" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Name</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent4Name" runat="server" />
                                            </div>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlParent4Mobile" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Mobile</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent4Mobile" runat="server" />
                                            </div>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlParent4Email" CssClass="row" runat="server" Visible="false">
                                            <div class="col-xs-4">
                                                <label>Parent Email</label>
                                            </div>
                                            <div class="col-xs-8">
                                                <asp:Literal ID="lParent4Email" runat="server" />
                                            </div>
                                        </asp:Panel>
                                    </asp:Panel>
                                </div>
                                <div class="gmCard-footer">
                                    <Rock:RockCheckBox ID="cbGroupMember" CssClass="largeCB" runat="server" />
                                    <asp:HiddenField ID="hfGroupMemberId" runat="server" />
                                </div>
                            </div>
                            <asp:Literal ID="lRowFooter" runat="server" />
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlEditGroupMember" runat="server" Visible="false">
            <div class="editHeader" style="margin-bottom:25px;">
                <div class="row">
                    <asp:Panel ID="pnlEditPhoto" runat="server" CssClass="editPhoto" style="padding-left:0px;" >
                        <asp:Image id="imgEditPhoto" runat="server" />
                    </asp:Panel>
                    <asp:Panel ID="pnlEditTitle" runat="server" style="height:100px; display:table-cell; vertical-align:middle;overflow:hidden;">
                        <asp:Literal ID="lName" runat="server" />
                    </asp:Panel>
                </div>
            </div>
            <div class="editBody">
                <asp:HiddenField ID="hfGroupMemberId" runat="server" />
                <asp:ValidationSummary ID="vsMemberEdit" runat="server" ValidationGroup="vgEditMember" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbRoleNotEditable" runat="server" NotificationBoxType="Info"  Visible="false">
                    <strong>Note</strong> Editing is restricted for some leader roles. To update a leader's role please contact your group coach or ministry contact.
                </Rock:NotificationBox>
                <div class="row">
                    <div class="col-sm-12 col-md-4">
                        <Rock:RockLiteral ID="lGroupMemberRole" runat="server" Label="Group Role" Visible="false" />
<%--                        <Rock:RockDropDownList ID="ddlGroupMemberRole" Required="true" runat="server" Label="Group Role" RequiredErrorMessage="Group Member Role is required." ValidationGroup="vgEditMember" Visible="false" />--%>
                        <Rock:RockCheckBoxList ID="cblGroupMemberRole" RepeatDirection="Horizontal" Required="true" runat="server" Label="Group Role" RequiredErrorMessage="Group Member Role is required." ValidationGroup="vgEditMember" Visible="false" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-12 col-md-6">
                        <Rock:RockRadioButtonList ID="rblGroupMemberStatus" runat="server" RepeatDirection="Horizontal" Label="Group Member Status" Required="true" RequiredErrorMessage="Group Member Status is required." ValidationGroup="vgEditMember" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <asp:LinkButton ID="lbSaveMemberUpdates" runat="server" CssClass="btn btn-primary" ValidationGroup="vgEditMember" CausesValidation="true" OnClick="lbSaveMemberUpdates_Click"> Save</asp:LinkButton>
                        <asp:LinkButton ID="lbCancelMemberUpdates" runat="server" CssClass="btn btn-default" OnClick="lbCancelMemberUpdates_Click"> Cancel</asp:LinkButton>
                    </div>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
