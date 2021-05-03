<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddGroup.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.AddGroup" %>

<asp:UpdatePanel ID="upAddGroup" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plus-square-o"></i>
                    <asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <asp:CustomValidator ID="cvGroupMember" runat="server" Display="None" />
                <Rock:NotificationBox ID="nbValidation" runat="server" Heading="Please correct the following:" NotificationBoxType="Danger" />

                <asp:Panel ID="pnlGroupData" runat="server">

                    <div class="row" id="divGroupName" runat="server">
                        <div class="col-md-4">
                            <Rock:RockTextBox ID="tbGroupName" runat="server" Label="Group Name" Required="true" />
                        </div>
                        <div class="col-md-8">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <h4><%=_groupTypeName %> Members</h4>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:NewGroupMembers ID="nfmMembers" runat="server" OnAddGroupMemberClick="nfmMembers_AddGroupMemberClick" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" />
                            <Rock:DefinedValuePicker ID="dvpMaritalStatus" runat="server" Label="Marital Status of Adults"
                                Help="The marital status to use for the adults in this family." />
                        </div>

                        <div class="col-md-8">
                            <Rock:AddressControl ID="acAddress" Label="Address" runat="server" UseStateAbbreviation="false" UseCountryAbbreviation="false" />
                            <Rock:RockCheckBox ID="cbHomeless" runat="server" Text="Family is Homeless" Visible="false" />
                        </div>
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlAddressInUseWarning" runat="server" Visible="false">
                    <Rock:HiddenFieldWithClass ID="hfSelectedGroupAtAddressGroupId" runat="server" CssClass="js-selected-group-at-address-choice" />
                    <div class="alert alert-warning">
                        <h4>Address Already In Use</h4>
                        <p>
                            <asp:Literal ID="lAlreadyInUseWarning" runat="server" /></p>
                        <p></p>
                        <div class="row">
                            <div class="col-md-4">
                                <Rock:RockRadioButton ID="rbNewGroup" runat="server" CssClass="js-group-at-address-choice" GroupName="groupAtAddressChoice" Checked="true" DisplayInline="false" />
                                <strong>New Family</strong>
                                <br />
                            </div>
                            <asp:Repeater ID="rptGroupsAtAddress" runat="server" OnItemDataBound="rptGroupsAtAddress_ItemDataBound">
                                <ItemTemplate>
                                    <div class="col-md-4">
                                        <Rock:RockRadioButton ID="rbGroupToUse" runat="server" CssClass="js-group-at-address-choice" GroupName="groupAtAddressChoice" DisplayInline="false" />
                                        <strong>
                                            <asp:Literal runat="server" ID="lGroupTitle" /></strong>
                                        <br />
                                        <asp:Literal runat="server" ID="lGroupLocationHtml" />
                                        <asp:Literal runat="server" ID="lGroupMembersHtml" />
                                    </div>
                                    <asp:Literal ID="lNewRowHtml" runat="server" />
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                        <div class="row">
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlContactInfo" runat="server" Visible="false">
                    <Rock:NewGroupContactInfo ID="nfciContactInfo" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlAdvanceInfo" runat="server" Visible="false">
                    <Rock:NewGroupAdvanceInfo ID="nfaiAdvanceInfo" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlAttributes" runat="server" Visible="false">
                </asp:Panel>

                <asp:Panel ID="pnlDuplicateWarning" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbDuplicateWarning" runat="server" NotificationBoxType="Warning" Title="Possible Duplicates"
                        Text="<div>One or more of the people you are adding may already exist. Please confirm that none of the existing people below are the same person as someone that you are adding.</div>" />
                    <div>
                        <asp:PlaceHolder ID="phDuplicates" runat="server" />
                    </div>
                </asp:Panel>

                <Rock:NotificationBox ID="nbMessages" runat="server"></Rock:NotificationBox>

                <div class="actions clearfix">
                    <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary pull-right" OnClick="btnNext_Click" />
                    <asp:LinkButton ID="btnPrevious" runat="server" Text="Previous" CssClass="btn btn-link pull-right" OnClick="btnPrevious_Click" Visible="false" CausesValidation="false" />
                </div>
            </div>
        </div>

        <script type="text/javascript">
            $(document).ready(function () {
                Sys.Application.add_load(function () {

                     <%-- workaround for RadioButtons in Repeaters https://stackoverflow.com/a/16793570/1755417 --%>
                    $('.js-group-at-address-choice').attr('Name', 'groupAtAddressChoice');

                    $('.js-group-at-address-choice').on('click', function (a, b, c) {
                        $('.js-group-at-address-choice').not($(this)).prop('checked', false);

                        $('.js-selected-group-at-address-choice').val($(this).attr('data-groupid'));
                    });
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
