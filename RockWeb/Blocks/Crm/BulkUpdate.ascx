<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkUpdate.ascx.cs" Inherits="RockWeb.Blocks.Crm.BulkUpdate" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfSelectedItems" runat="server"  />

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-truck"></i> Update Individuals</h1>
            </div>

            <div class="panel-body">

                <asp:Panel ID="pnlEntry" runat="server">

                    <asp:ValidationSummary ID="ValidationSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

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

                    <Rock:PanelWidget ID="pwWorkFlows" runat="server" Title="Workflows" TitleIconCssClass="fa fa-cogs"  Expanded="false">
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockListBox ID="rlbWorkFlowType" runat="server" DisplayDropAsAbsolute="true" Placeholder="Select a workflow..." />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwIndividualDetails" runat="server" Title="Individual Details" TitleIconCssClass="fa fa-user" Expanded="false" CssClass="fade-inactive">

                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:DefinedValuePicker ID="dvpTitle" runat="server" />
                                <Rock:DefinedValuePicker ID="dvpConnectionStatus" runat="server" />
                                <Rock:RockDropDownList ID="ddlGender" runat="server" >
                                    <asp:ListItem Text="Male" Value="Male" />
                                    <asp:ListItem Text="Female" Value="Female" />
                                    <asp:ListItem Text="Unknown" Value="Unknown" />
                                </Rock:RockDropDownList>
                                <Rock:DefinedValuePicker ID="dvpMaritalStatus" runat="server" />
                                <div class="row">
                                    <div class="col-xs-5">
                                        <Rock:GradePicker ID="ddlGradePicker" runat="server" UseAbbreviation="true" UseGradeOffsetAsValue="true" Label="" />
                                    </div>
                                    <div class="col-xs-7">
                                        <Rock:YearPicker ID="ypGraduation" runat="server" Label="Graduation Year" Help="High School Graduation Year." />
                                    </div>
                                </div>
                                <Rock:CampusPicker ID="cpCampus" runat="server" />
                                <Rock:RockDropDownList ID="ddlCommunicationPreference" runat="server" Label="Communication Preference">
                                    <asp:ListItem Text="Email" Value="1" />
                                    <asp:ListItem Text="SMS" Value="2" />
                                </Rock:RockDropDownList>
                            </div>
                            <div class="col-sm-6">
                                <Rock:DefinedValuePicker ID="dvpSuffix" runat="server" />

                                <Rock:DefinedValuePicker ID="dvpRecordStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" />
                                <Rock:DefinedValuePicker ID="dvpInactiveReason" runat="server" Visible="false" Label="Inactive Reason" FormGroupCssClass="bulk-item-visible" />
                                <Rock:RockTextBox ID="tbInactiveReasonNote" runat="server" TextMode="MultiLine" Rows="2" Visible="false" Label="Inactive Reason Note" FormGroupCssClass="bulk-item-visible" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlIsEmailActive" runat="server" >
                                    <asp:ListItem Text="Active" Value="Active" />
                                    <asp:ListItem Text="Inactive" Value="Inactive" />
                                </Rock:RockDropDownList>
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlEmailPreference" runat="server" >
                                    <asp:ListItem Text="Email Allowed" Value="EmailAllowed" />
                                    <asp:ListItem Text="No Mass Emails" Value="NoMassEmails" />
                                    <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                                </Rock:RockDropDownList>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-sm-12">
                                <Rock:RockTextBox ID="tbEmailNote" runat="server" TextMode="MultiLine" Rows="2"></Rock:RockTextBox>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlFollow" runat="server" >
                                    <asp:ListItem Text="Add to Following" Value="Add" />
                                    <asp:ListItem Text="Remove Following" Value="Remove" />
                                </Rock:RockDropDownList>
                                <Rock:RockTextBox ID="tbSystemNote" runat="server" TextMode="MultiLine" Rows="2"></Rock:RockTextBox>
                            </div>
                            <div class="col-sm-6">
                                <Rock:DefinedValuePicker ID="dvpReviewReason" runat="server" Enabled="false"
                                    Label="<span class='js-select-item'><i class='fa fa-circle-o'></i></span> Review Reason" />
                                <Rock:RockTextBox ID="tbReviewReasonNote" runat="server" Enabled="false"
                                    Label="<span class='js-select-item'><i class='fa fa-circle-o'></i></span> Review Reason Note" TextMode="MultiLine" Rows="2"></Rock:RockTextBox>
                            </div>
                        </div>

                    </Rock:PanelWidget>

                    <div class="row fade-inactive">
                        <div class="col-sm-6">
                            <asp:PlaceHolder ID="phAttributesCol1" runat="server" />
                        </div>
                        <div class="col-sm-6">
                            <asp:PlaceHolder ID="phAttributesCol2" runat="server" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="pwNote" runat="server" Title="Add note" TitleIconCssClass="fa fa-file-text-o" Expanded="false">
                        <div class="panel-noteentry">
                            <Rock:RockDropDownList ID="ddlNoteType" runat="server" Label="Note Type" />
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
                                <Rock:RockDropDownList ID="ddlGroupAction" runat="server" Label="Action" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupAction_SelectedIndexChanged" >
                                    <asp:ListItem Value="Add" Text="Add To Group" />
                                    <asp:ListItem Value="Remove" Text="Remove From Group" />
                                    <asp:ListItem Value="Update" Text="Update In Group" />
                                </Rock:RockDropDownList>
                                <Rock:GroupPicker ID="gpGroup" runat="server" Label="Group" OnSelectItem="gpGroup_SelectItem" />
                                <Rock:NotificationBox ID="nbGroupMessage" runat="server" NotificationBoxType="Warning" Visible="false"></Rock:NotificationBox>
                            </div>
                            <asp:Panel ID="pnlGroupMemberStatus" runat="server" CssClass="col-sm-6">
                                <Rock:RockDropDownList ID="ddlGroupRole" runat="server" Label="Role" DataTextField="Name" DataValueField="Id" Visible="false" />
                                <Rock:RockDropDownList ID="ddlGroupMemberStatus" runat="server" Label="Member Status" Visible="false" />
                            </asp:Panel>
                        </div>
                        <asp:Panel ID="pnlGroupMemberAttributes" runat="server" CssClass="row">
                            <div class="col-sm-12">
                                <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                            </div>
                        </asp:Panel>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwTag" runat="server" Title="Tag" TitleIconCssClass="fa fa-tags" Expanded="false">
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockDropDownList ID="ddlTagAction" runat="server" Label="Action" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupAction_SelectedIndexChanged" >
                                    <asp:ListItem Value="Add" Text="Add To Tag" />
                                    <asp:ListItem Value="Remove" Text="Remove From Tag" />
                                </Rock:RockDropDownList>
                                <Rock:RockDropDownList ID="ddlTagList" runat="server" Label="Tag" />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <asp:CustomValidator ID="cvSelection" runat="server" OnServerValidate="cvSelection_ServerValidate" Display="None" ErrorMessage="You have not selected anything to update." />

                    <div class="actions">
                        <asp:LinkButton ID="btnComplete" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnComplete_Click" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlConfirm" runat="server" Visible="false" CssClass="js-panel-confirm">

                    <asp:PlaceHolder id="phConfirmation" runat="server" />

                    <div class="actions">
                        <asp:LinkButton ID="btnBack" runat="server" Text="Back" CssClass="btn btn-link" OnClick="btnBack_Click" />
                        <asp:LinkButton ID="btnConfirm" runat="server" Text="Confirm" CssClass="btn btn-primary" OnClick="btnConfirm_Click" />
                    </div>

                </asp:Panel>

                <Rock:TaskActivityProgressReporter ID="tapReporter" runat="server" />
                <Rock:NotificationBox ID="nbTapReportFailed" runat="server" Visible="false" NotificationBoxType="Info" Text="Bulk update task started. It should be completed shortly."></Rock:NotificationBox>
            </div>
        </div>

        <script type="text/javascript">
            function ResetScrollPosition() {
                setTimeout("window.scrollTo(0,0)", 0);
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>


