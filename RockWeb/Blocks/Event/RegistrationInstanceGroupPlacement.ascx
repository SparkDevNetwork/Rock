<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceGroupPlacement.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceGroupPlacement" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <code>
            <asp:Literal ID="lRegistrationTemplateName" runat="server" />
            <asp:Literal ID="lRegistrationInstanceName" runat="server" />
        </code>

        <Rock:ButtonGroup ID="bgRegistrationTemplatePlacement" runat="server" Label="Select Placement Type" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="bgRegistrationTemplatePlacement_SelectedIndexChanged" />


        <Rock:NotificationBox ID="nbConfigurationError" runat="server" />


        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block panel-group-placement">

            <%-- Panel Header --%>
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lGroupPlacementGroupTypeIconHtml" runat="server">
                        <i class="fa fa-star"></i>
                    </asp:Literal>
                    <asp:Literal ID="lGroupPlacementGroupTypeName" Text="Group Type ..." runat="server" />
                </h1>

                <div class="panel-labels">
                    <asp:LinkButton ID="btnAddPlacementGroup" runat="server" CssClass="js-autoschedule btn btn-default btn-xs" OnClick="btnAddPlacementGroup_Click">
                        <asp:Literal ID="lAddPlacementGroupButtonIconHtml" runat="server">
                            <i class="fa fa-star"></i>
                        </asp:Literal>
                        <asp:Literal ID="lAddPlacementGroupButtonText" Text="Add ..." runat="server" />
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnConfiguration" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnConfiguration_Click"><i class="fa fa-gear"></i></asp:LinkButton>
                </div>
            </div>

            <%-- Panel Body --%>
            <div class="panel-body">
                <asp:Panel ID="pnlGroupPlacementContainer" runat="server">
                    <asp:Panel ID="pnlGroupPlacement" runat="server">

                        <Rock:HiddenFieldWithClass ID="hfBlockId" runat="server" CssClass="js-block-id" />
                        <Rock:HiddenFieldWithClass ID="hfRegistrationTemplateId" runat="server" CssClass="js-registration-template-id" />
                        <Rock:HiddenFieldWithClass ID="hfRegistrationTemplateShowInstanceName" runat="server" CssClass="js-registration-template-show-instance-name" />
                        <Rock:HiddenFieldWithClass ID="hfRegistrationTemplateInstanceIds" runat="server" CssClass="js-registration-template-instance-id-list" />
                        <Rock:HiddenFieldWithClass ID="hfRegistrationInstanceId" runat="server" CssClass="js-registration-instance-id" />
                        <Rock:HiddenFieldWithClass ID="hfRegistrantId" runat="server" CssClass="js-registrant-id" />
                        <Rock:HiddenFieldWithClass ID="hfRegistrationTemplatePlacementAllowMultiplePlacements" runat="server" CssClass="js-registration-template-placement-allow-multiple-placements" />
                        <Rock:HiddenFieldWithClass ID="hfRegistrationTemplatePlacementId" runat="server" CssClass="js-registration-template-placement-id" />
                        <Rock:HiddenFieldWithClass ID="hfRegistrationTemplatePlacementGroupTypeId" runat="server" CssClass="js-registration-template-placement-grouptype-id" />
                        <Rock:HiddenFieldWithClass ID="hfGroupDetailUrl" runat="server" CssClass="js-group-detail-url" />
                        <Rock:HiddenFieldWithClass ID="hfGroupMemberDetailUrl" runat="server" CssClass="js-group-member-detail-url" />

                        <Rock:HiddenFieldWithClass ID="hfOptionsIncludeFees" runat="server" CssClass="js-options-include-fees" />
                        <Rock:HiddenFieldWithClass ID="hfOptionsHighlightGenders" runat="server" CssClass="js-options-highlight-genders" />
                        <Rock:HiddenFieldWithClass ID="hfOptionsHideFullGroups" runat="server" CssClass="js-options-hide-full-groups" />
                        <Rock:HiddenFieldWithClass ID="hfOptionsRegistrantPersonDataViewFilterId" runat="server" CssClass="js-options-registrant-person-dataviewfilter-id" />
                        <Rock:HiddenFieldWithClass ID="hfOptionsDisplayedRegistrantAttributeIds" runat="server" CssClass="js-options-displayed-registrant-attribute-ids" />
                        <Rock:HiddenFieldWithClass ID="hfOptionsDisplayedGroupMemberAttributeKeys" runat="server" CssClass="js-options-displayed-groupmember-attribute-keys" />

                        <div class="row row-eq-height">
                            <div class="col-md-4 hidden-xs">

                                <div class="js-group-placement-registrant-list group-placement-registrant-list">

                                    <div class="js-registrant-template" style="display: none">
                                        <%-- template that groupPlacement.js uses to populate available registrants --%>

                                        <div class="js-registrant registrant unselectable clickable" data-person-gender="" data-registrant-id="" data-person-id="">

                                            <div class="panel panel-block">
                                                <div class="panel-heading">
                                                    <div class="panel-title">
                                                        <span class="registrant-name js-registrant-name"></span>
                                                    </div>
                                                </div>
                                                <div class="panel-body registrant-details js-registrant-details hide-transit">
                                                    <div class="registration-instance-name-container js-registration-instance-name-container form-group">
                                                        <label class="control-label">Instance</label>
                                                        <div class="registrant-registrationinstance-name js-registrant-registrationinstance-name"></div>
                                                    </div>

                                                    <div class="registrant-attributes-container js-registrant-attributes-container">
                                                    </div>

                                                    <div class="registrant-fees-container js-registrant-fees-container">
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="panel panel-block registrant-list">

                                        <div class="panel-heading">
                                            <h1 class="panel-title">
                                                <i class="fa fa-user"></i>
                                                Registrants
                                            </h1>

                                            <div class="panel-labels">
                                                <div class="js-toggle-registrant-details toggle-registrant-details btn btn-xs btn-default">
                                                    <i class="fa fa-angle-double-down"></i>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="panel-body padding-all-none">
                                            <Rock:RockTextBox ID="sfRegistrant" runat="server" CssClass="registrant-search padding-all-sm js-registrant-search" PrependText="<i class='fa fa-search'></i>" Placeholder="Search" spellcheck="false" />

                                            <div class="scroll-list">
                                                <%-- loading indicator --%>
                                                <i class="fa fa-refresh fa-spin margin-l-md js-loading-notification" style="display: none; opacity: .4;"></i>

                                                <%-- container for list of registrants --%>
                                                <asp:Panel ID="pnlRegistrantListContainer" CssClass="js-group-placement-registrant-container group-placement-registrant-container dropzone" runat="server">
                                                </asp:Panel>
                                            </div>
                                        </div>

                                    </div>
                                </div>

                            </div>

                            <div class="col-md-8">
                                <%-- containers for each placement group (for example:bus) that registrants can be dragged into --%>
                                <div class="placement-groups js-placement-groups">

                                    <div class="js-group-member-template" style="display: none">
                                        <%-- template that groupPlacement.js uses to populate group member divs --%>

                                        <div class="js-group-member groupmember unselectable clickable" data-person-gender="" data-groupmember-id="" data-person-id="">
                                            <div class="panel panel-block placement-group-member">
                                                <div class="panel-heading">
                                                    <div class="panel-title">
                                                        <span class="groupmember-name js-groupmember-name"></span>
                                                    </div>

                                                    <div class="panel-labels">
                                                        <div class="dropdown js-groupmember-actions hide-transit">
                                                            <button class="btn btn-link btn-overflow" type="button" data-toggle="dropdown"><i class="fas fa-ellipsis-h"></i></button>
                                                            <ul class="dropdown-menu">
                                                                <li>
                                                                    <a class="dropdown-item button btn-link js-remove-group-member">Remove</a>
                                                                    <a class="dropdown-item button btn-link js-edit-group-member">Edit</a>
                                                                </li>
                                                            </ul>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="panel-body js-groupmember-details hide-transit">
                                                    <div class="groupmember-attributes-container js-groupmember-attributes-container">
                                                    </div>
                                                </div>
                                            </div>

                                        </div>
                                    </div>

                                    <asp:Repeater ID="rptPlacementGroups" runat="server" OnItemDataBound="rptPlacementGroups_ItemDataBound">
                                        <ItemTemplate>

                                            <asp:Panel ID="pnlPlacementGroup" runat="server" CssClass="placement-group js-placement-group">
                                                <Rock:HiddenFieldWithClass ID="hfPlacementGroupId" runat="server" CssClass="js-placement-group-id" />
                                                <Rock:HiddenFieldWithClass ID="hfPlacementGroupCapacity" runat="server" CssClass="js-placement-capacity" />

                                                <div class="panel panel-block placement-group">
                                                    <div class="alert alert-danger js-alert js-placement-group-error" style="display: none">
                                                        <button type="button" class="close js-hide-alert" aria-hidden="true"><i class="fa fa-times"></i></button>
                                                        <span class="js-placement-group-error-text"></span>
                                                    </div>
                                                    <div class="panel-heading">
                                                        <h1 class="panel-title">
                                                            <asp:Literal ID="lGroupIconHtml" runat="server" />
                                                            <asp:Literal ID="lGroupName" runat="server" />
                                                        </h1>

                                                        <div class="panel-labels">
                                                            <div class="dropdown js-group-actions hide-transit pull-right">
                                                                <div class="btn btn-link btn-overflow btn-xs" data-toggle="dropdown">
                                                                    <i class="fas fa-ellipsis-v"></i>
                                                                </div>
                                                                <ul class="dropdown-menu">
                                                                    <li>
                                                                        <a class="dropdown-item button btn-link js-detach-placement-group">Detach from Placement</a>
                                                                        <a class="dropdown-item button btn-link js-delete-group">Delete Permanently</a>
                                                                        <a class="dropdown-item button btn-link js-edit-group">Edit Group</a>
                                                                    </li>
                                                                </ul>
                                                            </div>
                                                            <div class="btn btn-default btn-xs pull-right js-placement-group-toggle-visibility placement-group-toggle-visibility">
                                                                <i class="fa fa-chevron-up"></i>
                                                            </div>
                                                            <div class="placement-status-labels pull-right">
                                                                <Rock:HighlightLabel runat="server" LabelType="Info" ID="hlInstanceName" Visible="false" />
                                                                <Rock:HighlightLabel runat="server" LabelType="Default" ID="hlRegistrationTemplatePlacementName" Visible="false" />
                                                                <Rock:HighlightLabel runat="server" LabelType="Campus" ID="hlGroupCampus" CssClass="margin-r-sm" Visible="false" />
                                                                <span class="label label-custom placement-capacity-label js-placement-capacity-label margin-r-sm" data-status="none"></span>
                                                            </div>

                                                        </div>
                                                    </div>

                                                    <div class="panel-body js-group-details">
                                                        <Rock:AttributeValuesContainer ID="avcGroupAttributes" runat="server" />

                                                        <asp:Repeater ID="rptPlacementGroupRole" runat="server" OnItemDataBound="rptPlacementGroupRole_ItemDataBound" Visible="true">
                                                            <ItemTemplate>
                                                                <asp:Panel ID="pnlGroupRoleMembers" runat="server" CssClass="group-role-members js-group-role-members">
                                                                    <Rock:HiddenFieldWithClass ID="hfGroupTypeRoleId" runat="server" CssClass="js-grouptyperole-id" />
                                                                    <Rock:HiddenFieldWithClass ID="hfGroupTypeRoleMaxMembers" runat="server" CssClass="js-grouptyperole-max-members" />

                                                                    <div class="panel panel-block">

                                                                        <div class="panel-heading">

                                                                            <h1 class="panel-title">
                                                                                <asp:Literal ID="lGroupRoleName" runat="server" />
                                                                            </h1>

                                                                            <asp:Panel ID="pnlGroupRoleStatusLabels" runat="server" CssClass="panel-labels">
                                                                                <div class="js-grouptyperole-statuslabels-container grouptyperole-status-labels">
                                                                                    <span class="label label-custom grouptyperole-max-members-label js-grouptyperole-max-members-label" data-status="none">1:1</span>
                                                                                </div>
                                                                            </asp:Panel>
                                                                        </div>
                                                                        <div class="alert alert-danger js-alert js-placement-place-registrant-error" style="display: none">
                                                                            <button type="button" class="close js-hide-alert" aria-hidden="true"><i class="fa fa-times"></i></button>
                                                                            <span class="js-placement-place-registrant-error-text"></span>
                                                                        </div>
                                                                        <div class="panel-body">
                                                                            <div class="js-group-role-container group-role-container dropzone"></div>
                                                                        </div>
                                                                    </div>
                                                                </asp:Panel>
                                                            </ItemTemplate>
                                                        </asp:Repeater>

                                                    </div>

                                                </div>

                                            </asp:Panel>

                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                        </div>
                    </asp:Panel>
                </asp:Panel>
            </div>

        </asp:Panel>

        <%-- Add Group Placement Dialog --%>
        <asp:Panel ID="pnlAddPlacementGroup" runat="server">
            <Rock:ModalDialog ID="mdAddPlacementGroup" runat="server" ValidationGroup="vgAddPlacementGroup" CssClass="js-add-placement-group-modal" Title="Add Placement Group/Add Shared Placement Group" OnSaveClick="mdAddPlacementGroup_SaveClick">
                <Content>
                    <Rock:NotificationBox ID="nbNotAllowedToAddGroup" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="You are not authorized to save group with the selected group type and/or parent group." />

                    <Rock:ButtonGroup ID="bgAddNewOrExistingPlacementGroup" runat="server" CssClass="margin-b-sm" OnSelectedIndexChanged="bgAddNewOrExistingPlacementGroup_SelectedIndexChanged" AutoPostBack="true" />
                    <asp:Panel ID="pnlAddNewPlacementGroup" runat="server">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NotificationBox ID="nbNewPlacementGroupParentGroupWarning" runat="server" NotificationBoxType="Warning" Dismissable="true" />
                                <Rock:GroupPicker ID="gpNewPlacementGroupParentGroup" runat="server" Label="Parent Group" ValidationGroup="vgAddPlacementGroup" OnSelectItem="gpNewPlacementGroupParentGroup_SelectItem" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbNewPlacementGroupName" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" ValidationGroup="vgAddPlacementGroup" />
                            </div>
                            <div class="col-md-6">
                                <Rock:CampusPicker ID="cpNewPlacementGroupCampus" runat="server" Label="Campus" ValidationGroup="vgAddPlacementGroup" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbGroupCapacity" runat="server" Label="Group Capacity" NumberType="Integer" MinimumValue="0" ValidationGroup="vgAddPlacementGroup" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="tbNewPlacementGroupDescription" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" ValidationGroup="vgAddPlacementGroup" />
                                <Rock:AttributeValuesContainer ID="avcNewPlacementGroupAttributeValues" runat="server" ValidationGroup="vgAddPlacementGroup" />
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlAddExistingPlacementGroup">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NotificationBox ID="nbAddExistingPlacementGroupWarning" runat="server" NotificationBoxType="Warning" Dismissable="true" />
                                <Rock:GroupPicker ID="gpAddExistingPlacementGroup" runat="server" Label="Group" ValidationGroup="vgAddPlacementGroup" OnSelectItem="gpAddExistingPlacementGroup_SelectItem" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>
                    </asp:Panel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>


        <%-- Placement Configuration (User preferences) --%>
        <asp:Panel ID="pnlConfiguration" runat="server">
            <Rock:ModalDialog ID="mdPlacementConfiguration" runat="server" Title="Placement Configuration" CssClass=".js-configuration-modal" OnSaveClick="mdPlacementConfiguration_SaveClick">
                <Content>
                    <Rock:CampusPicker ID="cpConfigurationCampusPicker" runat="server" Label="Campus Filter" />

                    <%-- This will only be shown when in Registration Template mode --%>
                    <Rock:PanelWidget ID="pwRegistrationTemplateConfiguration" runat="server" Title="Registration Template Configuration">
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockCheckBox ID="cbShowRegistrationInstanceName" runat="server" Label="Show Registration Instance Name" Help="When enabled, the registration instance name will be included in the details of each registrant in the Registrants list" Checked="true" />

                                <Rock:RockCheckBoxList ID="cblRegistrationInstances" runat="server" Label="Registration Instances" Help="Set the registration instances to include. If none are selected, then all will be included." />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pnlRegistrantConfiguration" runat="server" Title="Registrant Configuration">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbHighlightGenders" runat="server" Label="Highlight Genders" Help="Enable this to highlight each registrant to indicate their gender (pink, blue)." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbShowFees" runat="server" Label="Show Fees" Help="Enable this to show any fees associated with each registrant." />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockListBox ID="cblDisplayedRegistrantAttributes" EnhanceForLongLists="true" runat="server" Label="Displayed Registrant Attributes" />

                                <Rock:RockControlWrapper ID="rcwRegistrantFilters" runat="server" Label="Registrant Filters">
                                    <Rock:DynamicPlaceholder ID="phFilters" runat="server" />
                                </Rock:RockControlWrapper>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwGroupConfiguration" runat="server" Title="Group Configuration">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockListBox ID="cblDisplayedGroupAttributes" EnhanceForLongLists="true" runat="server" Label="Displayed Group Attributes" />
                                <Rock:RockListBox ID="cblDisplayedGroupMemberAttributes" EnhanceForLongLists="true" runat="server" Label="Displayed Group Member Attributes" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbHideFullGroups" runat="server" Label="Hide Full Groups" Help="Enable this to hide placement groups that are at the group capacity." />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {

                var groupPlacementControlId = '<%=pnlGroupPlacement.ClientID%>';

                Rock.controls.groupPlacementTool.initialize({
                    id: groupPlacementControlId,
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
