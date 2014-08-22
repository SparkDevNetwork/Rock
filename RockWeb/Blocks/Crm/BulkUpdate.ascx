<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkUpdate.ascx.cs" Inherits="RockWeb.Blocks.Crm.BulkUpdate" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i>Bulk Update</h1>
            </div>

            <div class="panel-body">

                <asp:Panel ID="pnlEntry" runat="server">

                    <asp:ValidationSummary ID="ValidationSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="panel panel-widget individuals">
                        <div class="panel-heading clearfix">
                            <div class="control-label pull-left">
                                Selected Individuals:
                                <asp:Literal ID="lNumIndividuals" runat="server" />
                            </div>

                            <div class="pull-right">
                                <Rock:PersonPicker ID="ppAddPerson" runat="server" PersonName="Add Person" OnSelectPerson="ppAddPerson_SelectPerson" />
                            </div>

                            <asp:CustomValidator ID="valIndividuals" runat="server" OnServerValidate="valIndividuals_ServerValidate" Display="None" ErrorMessage="At least one individual is required." />

                        </div>

                        <div class="panel-body">

                            <ul class="individuals list-unstyled">
                                <asp:Repeater ID="rptIndividuals" runat="server" OnItemCommand="rptIndividuals_ItemCommand">
                                    <ItemTemplate>
                                        <li class='individual'><%# Eval("PersonName") %>
                                            <asp:LinkButton ID="lbRemoveIndividual" runat="server" CommandArgument='<%# Eval("PersonId") %>' CausesValidation="false"><i class="fa fa-times"></i></asp:LinkButton></li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>

                            <div class="pull-right">
                                <asp:LinkButton ID="lbShowAllIndividuals" runat="server" CssClass="btn btn-action btn-xs" Text="Show All" OnClick="lbShowAllIndividuals_Click" CausesValidation="false" />
                                <asp:LinkButton ID="lbRemoveAllIndividuals" runat="server" Text="Remove All Individuals" CssClass="remove-all-individuals btn btn-action btn-xs" OnClick="lbRemoveAllIndividuals_Click" CausesValidation="false" />
                            </div>

                        </div>
                    </div>

                    <Rock:PanelWidget ID="pwIndividualDetails" runat="server" Title="Individual Details" TitleIconCssClass="fa fa-user" Expanded="false">

                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlTitle" runat="server" Label="Title" />
                                <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Connection Status" />
                                <Rock:RockDropDownList ID="ddlGender" runat="server" Label="Gender">
                                    <asp:ListItem Text="" Value="" />
                                    <asp:ListItem Text="Male" Value="Male" />
                                    <asp:ListItem Text="Female" Value="Female" />
                                    <asp:ListItem Text="Unknown" Value="Unknown" />
                                </Rock:RockDropDownList>
                                <Rock:RockDropDownList ID="ddlMaritalStatus" runat="server" Label="Marital Status" />
                                <div class="row">
                                    <div class="col-xs-5">
                                        <Rock:RockDropDownList ID="ddlGrade" runat="server" Label="Grade" />
                                    </div>
                                    <div class="col-xs-7">
                                        <Rock:YearPicker ID="ypGraduation" runat="server" Label="Graduation Year" Help="High School Graduation Year." />
                                    </div>
                                </div>
                                <Rock:CampusPicker ID="cpCampus" runat="server" />
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlSuffix" runat="server" Label="Suffix" />
                                <Rock:RockDropDownList ID="ddlRecordStatus" runat="server" Label="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" />
                                <Rock:RockDropDownList ID="ddlInactiveReason" runat="server" Label="Inactive Reason" Visible="false"></Rock:RockDropDownList>
                                <Rock:RockTextBox ID="tbInactiveReasonNote" runat="server" Label="Inactive Reason Note" TextMode="MultiLine" Rows="2" Visible="false"></Rock:RockTextBox>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlIsEmailActive" runat="server" Label="Email Status">
                                    <asp:ListItem Text="" Value="" />
                                    <asp:ListItem Text="Active" Value="Active" />
                                    <asp:ListItem Text="Inactive" Value="Inactive" />
                                </Rock:RockDropDownList>
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlEmailPreference" runat="server" Label="Email Preference">
                                    <asp:ListItem Text="" Value="" />
                                    <asp:ListItem Text="Email Allowed" Value="EmailAllowed" />
                                    <asp:ListItem Text="No Mass Emails" Value="NoMassEmails" />
                                    <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                                </Rock:RockDropDownList>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-sm-12">
                                <Rock:RockTextBox ID="tbEmailNote" runat="server" Label="Email Note" TextMode="MultiLine" Rows="2"></Rock:RockTextBox>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlFollow" runat="server" Label="Follow">
                                    <asp:ListItem Text="" Value="" />
                                    <asp:ListItem Text="Add to Following" Value="Add" />
                                    <asp:ListItem Text="Remove Following" Value="Remove" />
                                </Rock:RockDropDownList>
                                <Rock:RockTextBox ID="tbSystemNote" runat="server" Label="System Note" TextMode="MultiLine" Rows="2"></Rock:RockTextBox>
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlReviewReason" runat="server" Label="Review Reason" />
                                <Rock:RockTextBox ID="tbReviewReasonNote" runat="server" Label="Review Reason Note" TextMode="MultiLine" Rows="2"></Rock:RockTextBox>
                            </div>
                        </div>

                    </Rock:PanelWidget>

                    <div class="row">
                        <div class="col-sm-6">
                            <asp:PlaceHolder ID="phAttributesCol1" runat="server" />
                        </div>
                        <div class="col-sm-6">
                            <asp:PlaceHolder ID="phAttributesCol2" runat="server" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="pwNote" runat="server" Title="Add note" TitleIconCssClass="fa fa-file-text-o" Expanded="false">
                        <div class="panel-noteentry">
                            <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="3" />
                            <div class="settings clearfix">
                                <div class="options pull-left">
                                    <Rock:RockCheckBox ID="cbIsAlert" runat="server" Text="Alert" />
                                    <Rock:RockCheckBox ID="cbIsPrivate" runat="server" Text="Private" />
                                </div>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwGroup" runat="server" Title="Group" TitleIconCssClass="fa fa-users" Expanded="false">
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:ButtonDropDownList ID="bddlGroupAction" runat="server" Label="Action" AutoPostBack="true" OnSelectionChanged="bddlGroupAction_SelectionChanged">
                                    <asp:ListItem Value="Add" Text="Add To Group" />
                                    <asp:ListItem Value="Remove" Text="Remove From Group" />
                                    <asp:ListItem Value="Update" Text="Update In Group" />
                                </Rock:ButtonDropDownList>
                                <Rock:GroupPicker ID="gpGroup" runat="server" Label="Group" OnSelectItem="gpGroup_SelectItem" />
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlGroupRole" runat="server" Label="Role" DataTextField="Name" DataValueField="Id" />
                                <Rock:RockDropDownList ID="ddlGroupMemberStatus" runat="server" Label="Member Status" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-sm-12">
                                <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnComplete" runat="server" Text="Complete" CssClass="btn btn-primary" OnClick="btnComplete_Click" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlResult" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbResult" runat="server" NotificationBoxType="Success" />
                </asp:Panel>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>


