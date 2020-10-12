<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupDetailLava.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupDetailLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlGroupView" runat="server">
            <div class="communication-preference pull-right" runat="server" id="dCommunicationsPreference">
                <span>Your communication preference:&nbsp;</span><Rock:Toggle ID="tglCommunicationPreference" CssClass="pull-right" runat="server" OnText="SMS" OffText="Email" ButtonSizeCssClass="btn-xs" OnCssClass="btn-info" OffCssClass="btn-info" OnCheckedChanged="tglCommunicationPreference_CheckedChanged" />
            </div>

            <asp:Literal ID="lContent" runat="server"></asp:Literal>

            <asp:Literal ID="lDebug" runat="server"></asp:Literal>
        </asp:Panel>

        <asp:Panel ID="pnlGroupEdit" runat="server" Visible="false">

            <asp:Literal ID="lGroupEditPreHtml" runat="server" />

            <asp:ValidationSummary ID="vsGroupEdit" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

            <div class="row">
                <div class="col-md-6">
                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" />
                </div>
                <div class="col-md-6">
                    <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" CssClass="js-isactivegroup" />
                    <Rock:RockCheckBox ID="cbIsPublic" runat="server" Text="Public" />
                </div>
            </div>

            <div class="row js-inactivateoptions">
                <div class="col-md-6 pull-right">
                    <%-- Inactive Reason ddl this isn't a defined value picker since the values can be filtered by group type --%>
                    <Rock:RockDropDownList ID="ddlInactiveReason" runat="server" Visible="false" Label="Inactive Reason" />
                </div>
            </div>

            <div class="row js-inactivateoptions">
                <div class="col-md-6 pull-right">
                    <%-- Inactive note multi line --%>
                    <Rock:DataTextBox ID="tbInactiveNote" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="InactiveReasonNote" TextMode="MultiLine" Rows="4" Visible="false" Label="Inactive Note" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 pull-right">
                    <%-- Inactivate child groups checkbox --%>
                    <Rock:RockCheckBox ID="cbInactivateChildGroups" runat="server" Text="Inactivate Child Groups" ContainerCssClass="js-inactivatechildgroups" Style="display: none" />
                    <Rock:HiddenFieldWithClass ID="hfHasChildGroups" runat="server" CssClass="js-haschildgroups" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-12">
                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                </div>
            </div>

            <asp:Panel ID="pnlSchedule" runat="server" Visible="false" CssClass="row">
                <div class="col-sm-6">
                    <Rock:DayOfWeekPicker ID="dowWeekly" runat="server" CssClass="input-width-md" Label="Day of the Week" />
                </div>
                <div class="col-sm-6">
                    <Rock:TimePicker ID="timeWeekly" runat="server" Label="Time of Day" />
                </div>
            </asp:Panel>

            <div class="row">
                <div class="col-md-6">
                    <Rock:NumberBox ID="nbGroupCapacity" runat="server" Label="Group Capacity" NumberType="Integer" Visible="false" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-12">
                    <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                </div>
            </div>

            <asp:Panel ID="pnlGroupEditLocations" runat="server">
                <div class="well">
                    <h4>Locations</h4>
                    <ul id="ulNav" runat="server" class="nav nav-pills margin-b-md">
                        <asp:Repeater ID="rptLocationTypes" runat="server">
                            <ItemTemplate>
                                <li class='<%# GetLocationTabClass(Container.DataItem) %>'>
                                    <asp:LinkButton ID="lbLocationType" runat="server" Text='<%# Container.DataItem %>' OnClick="lbLocationType_Click" CausesValidation="false">
                                    </asp:LinkButton>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>

                    <div class="tabContent">
                        <asp:Panel ID="pnlMemberSelect" runat="server" Visible="true">
                            <Rock:RockDropDownList ID="ddlMember" runat="server" Label="Member" ValidationGroup="Location" />
                        </asp:Panel>
                        <asp:Panel ID="pnlLocationSelect" runat="server" Visible="false">
                            <Rock:LocationPicker ID="locpGroupLocation" runat="server" Label="Location" ValidationGroup="Location" />
                        </asp:Panel>
                    </div>

                    <Rock:RockDropDownList ID="ddlLocationType" runat="server" Label="Type" DataValueField="Id" DataTextField="Value" ValidationGroup="Location" />
                </div>
            </asp:Panel>

            <div class="actions">
                <asp:Button ID="btnSaveGroup" runat="server" AccessKey="s" ToolTip="Alt+s" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveGroup_Click" />
                <asp:LinkButton id="lbCancelGroup" runat="server" AccessKey="c" ToolTip="Alt+c" CssClass="btn btn-link" OnClick="lbCancelGroup_Click" CausesValidation="false">Cancel</asp:LinkButton>
            </div>

            <asp:Literal ID="lGroupEditPostHtml" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlEditGroupMember" runat="server" Visible="false">

            <asp:Literal ID="lGroupMemberEditPreHtml" runat="server" />

            <asp:ValidationSummary ID="vsEditGroupMember" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
            <asp:CustomValidator ID="cvEditGroupMember" runat="server" Display="None" />
            <Rock:NotificationBox ID="nbGroupMemberErrorMessage" runat="server" NotificationBoxType="Danger" />

            <div class="row">
                <div class="col-md-6">
                    <Rock:PersonPicker runat="server" ID="ppGroupMemberPerson" Label="Person" Required="true"/>
                </div>
                <div class="col-md-6">
                    <Rock:RockRadioButtonList ID="rblStatus" runat="server" Label="Member Status" RepeatDirection="Horizontal" />
                </div>
            </div>

            <div class="row">
                <asp:Panel ID="pnlGroupMemberRole" runat="server" CssClass="col-md-6">
                    <Rock:RockDropDownList runat="server" ID="ddlGroupRole" DataTextField="Name" DataValueField="Id" Label="Role" Required="true" />
                </asp:Panel>
                <div class="col-md-6">
                    <Rock:RockRadioButtonList ID="rblCommunicationPreference" runat="server" RepeatDirection="Horizontal" Label="Communication Preference">
                        <asp:ListItem Text="Email" Value="1" />
                        <asp:ListItem Text="SMS" Value="2" />
                    </Rock:RockRadioButtonList>
                    <asp:Panel ID="pnlGroupMemberAttributes" runat="server">
                        <Rock:DynamicPlaceHolder ID="phGroupMemberAttributes" runat="server" />
                    </asp:Panel>
                </div>
            </div>

            <div class="actions">
                <asp:Button ID="btnSaveGroupMember" runat="server" AccessKey="s" ToolTip="Alt+s" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveGroupMember_Click" />
                <asp:LinkButton id="btnCancelGroupMember" runat="server" AccessKey="c" ToolTip="Alt+c" CssClass="btn btn-link" OnClick="btnCancelGroupMember_Click" CausesValidation="false">Cancel</asp:LinkButton>
            </div>

            <asp:Literal ID="lGroupMemberEditPostHtml" runat="server" />

        </asp:Panel>

        <Rock:ModalDialog ID="mdConfirmDelete" runat="server" Title="Please Confirm" SaveButtonText="Yes" OnSaveClick="mdConfirmDelete_Click">
            <Content>
                <asp:Literal ID="lConfirmDeleteMsg" runat="server" />
            </Content>
        </Rock:ModalDialog>

        <script>
            Sys.Application.add_load(function () {
                function setIsActiveControls(activeCheckbox) {
                    // if isactive was toggled from Active to Inactive and the group has child groups, show the inactivate child groups checkbox
                    var hasChildren = $('.js-haschildgroups').val();
                    var rfvId = "<%= ddlInactiveReason.ClientID %>" + "_rfv";

                    if ($(activeCheckbox).is(':checked')) {
                        $('.js-inactivateoptions').hide();
                        $('.js-inactivatechildgroups').hide();
                        enableRequiredField(rfvId, false);
                    }
                    else {
                        $('.js-inactivateoptions').show();
                        enableRequiredField(rfvId, true);

                        if (hasChildren === "true") {
                            $('.js-inactivatechildgroups').show();
                        }
                    }
                }

                function enableRequiredField(validatorId, enable) {
                    var jqObj = $('#' + validatorId);
                    if (jqObj != null) {
                        var domObj = jqObj.get(0);
                        if (domObj != null) {
                            console.log(validatorId + ': found');
                            ValidatorEnable(domObj, enable);
                        } else {
                            console.log(validatorId + ': NOT found');
                        }
                    }
                }

                $('.js-isactivegroup').on('click', function () {
                    setIsActiveControls(this);
                });

                $('.js-isactivegroup').each(function (i) {
                    setIsActiveControls(this);
                });

            });

        </script>
    </ContentTemplate>
</asp:UpdatePanel>
