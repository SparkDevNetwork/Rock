<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceGroupPlacement.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceGroupPlacement" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <%-- Prompt for RegistrationTemplatePlacement. This will only be visible if using this as a standalone block (if RegistrationTemplatePlacementId isn't specified) --%>
        <Rock:RockControlWrapper ID="rcwSelectRegistrationTemplatePlacement" runat="server" Visible="false">
            <ul class="nav nav-tabs margin-b-md">
                <asp:Repeater ID="rptSelectRegistrationTemplatePlacement" runat="server" OnItemDataBound="rptSelectRegistrationTemplatePlacement_ItemDataBound">
                    <ItemTemplate>
                        <asp:Literal ID="lTabHtml" runat="server" />
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </Rock:RockControlWrapper>

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
                    <asp:LinkButton ID="btnConfiguration" runat="server" CssClass="btn btn-default btn-square btn-xs" OnClick="btnConfiguration_Click"><i class="fa fa-gear"></i></asp:LinkButton>
                    <button type="button" class="btn btn-default btn-xs btn-square js-toggle-group-details">
                        <i class="fa fa-angle-double-up"></i>
                    </button>
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
                        <Rock:HiddenFieldWithClass ID="hfOptionsDisplayRegistrantAttributes" runat="server" CssClass="js-options-display-registrant-attributes" />
                        <Rock:HiddenFieldWithClass ID="hfOptionsApplyRegistrantFilters" runat="server" CssClass="js-options-apply-registrant-filters" />
                        <Rock:HiddenFieldWithClass ID="hfOptionsRegistrantPersonDataViewFilterId" runat="server" CssClass="js-options-registrant-person-dataviewfilter-id" />
                        <Rock:HiddenFieldWithClass ID="hfOptionsDisplayedRegistrantAttributeIds" runat="server" CssClass="js-options-displayed-registrant-attribute-ids" />
                        <Rock:HiddenFieldWithClass ID="hfOptionsDisplayedGroupMemberAttributeIds" runat="server" CssClass="js-options-displayed-groupmember-attribute-ids" />

                        <Rock:HiddenFieldWithClass ID="hfOptionsFilterFeeId" runat="server" CssClass="js-options-filter-fee-id" />
                        <Rock:HiddenFieldWithClass ID="hfOptionsFilterFeeItemIds" runat="server" CssClass="js-options-filter-fee-item-ids" />

                        <div class="row row-eq-height">
                            <div class="col-md-4 col-lg-3">

                                <div class="resource-list js-group-placement-registrant-list group-placement-registrant-list">

                                    <div class="js-registrant-template" style="display: none">
                                        <%-- template that groupPlacement.js uses to populate available registrants --%>

                                        <div class="js-registrant registrant person unselectable" data-person-gender="" data-registrant-id="" data-person-id="">

                                            <span class="person-name js-registrant-name"></span>

                                            <div class="details-container small js-registrant-details hide-dragging">
                                                <div class="registration-instance-name-container js-registration-instance-name-container">
                                                    <dl>
                                                        <dt>Instance</dt>
                                                        <dd class="registrant-registrationinstance-name js-registrant-registrationinstance-name"></dd>
                                                    </dl>
                                                </div>

                                                <div class="registrant-attributes-container js-registrant-attributes-container">
                                                </div>

                                                <div class="registrant-fees-container js-registrant-fees-container">
                                                </div>
                                            </div>

                                        </div>
                                    </div>

                                    <div class="panel panel-block registrant-list resource-list">

                                        <div class="panel-heading">
                                            <h1 class="panel-title">
                                                <i class="fa fa-user"></i>
                                                Registrants
                                            </h1>

                                            <div class="panel-labels">
                                                <div class="btn btn-xs btn-square btn-default js-toggle-registrant-details toggle-registrant-details ">
                                                    <i class="fa fa-angle-double-down"></i>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="panel-body padding-all-none">
                                            <Rock:RockTextBox ID="sfRegistrant" runat="server" CssClass="registrant-search js-registrant-search" PrependText="<i class='fa fa-search'></i>" Placeholder="Search" spellcheck="false" />

                                            <div class="scroll-list">
                                                <%-- loading indicator --%>
                                                <i class="fa fa-refresh fa-spin margin-l-md js-loading-notification" style="display: none; opacity: .4;"></i>

                                                <%-- container for list of registrants --%>
                                                <asp:Panel ID="pnlRegistrantListContainer" CssClass="js-group-placement-registrant-container group-placement-registrant-container dropzone" data-empty-label="No People Available" runat="server">
                                                </asp:Panel>
                                            </div>
                                        </div>

                                    </div>
                                </div>

                            </div>

                            <div class="col-md-8 col-lg-9">
                                <%-- containers for each placement group (for example:bus) that registrants can be dragged into --%>
                                <div class="placement-groups js-placement-groups">

                                    <div class="js-group-member-template" style="display: none">
                                        <%-- template that groupPlacement.js uses to populate group member divs --%>

                                        <div class="js-group-member person groupmember unselectable" data-person-gender="" data-groupmember-id="" data-person-id="">
                                            <a class="js-person-id-anchor person-id-anchor"></a>
                                            <div class="person-container">
                                                <span class="person-name js-groupmember-name"></span>
                                                <div class="dropdown js-groupmember-actions hide-dragging">
                                                    <button class="btn btn-overflow" type="button" data-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
                                                    <ul class="dropdown-menu">
                                                        <li><a class="js-edit-group-member">Edit</a></li>
                                                        <li><a class="js-remove-group-member">Remove</a></li>
                                                    </ul>
                                                </div>
                                            </div>

                                            <div class="details-container js-groupmember-details hide-dragging">
                                                <div class="groupmember-attributes-container js-groupmember-attributes-container">
                                                </div>
                                                <div class="registrant-attributes-container js-registrant-attributes-container">
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <asp:Repeater ID="rptPlacementGroups" runat="server" OnItemDataBound="rptPlacementGroups_ItemDataBound">
                                        <ItemTemplate>

                                            <asp:Panel ID="pnlPlacementGroup" runat="server" CssClass="placement-group js-placement-group">
                                                <Rock:HiddenFieldWithClass ID="hfPlacementGroupId" runat="server" CssClass="js-placement-group-id" />
                                                <Rock:HiddenFieldWithClass ID="hfPlacementGroupCapacity" runat="server" CssClass="js-placement-capacity" />
                                                <Rock:HiddenFieldWithClass ID="hfPlacementGroupRegistrationInstanceId" runat="server" CssClass="js-placement-group-registrationinstanceid" />

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
                                                            <div class="dropdown js-group-actions hide-dragging pull-right">
                                                                <div class="btn btn-square btn-overflow btn-xs" data-toggle="dropdown">
                                                                    <i class="fa fa-ellipsis-v"></i>
                                                                </div>
                                                                <ul class="dropdown-menu">
                                                                    <li><a class="js-edit-group">Edit</a></li>
                                                                    <li><a id="detachPlacementGroup" runat="server" class="js-detach-placement-group">Detach</a></li>
                                                                    <li id="actionSeparator" runat="server" role="separator" class="divider"></li>
                                                                    <li><a id="deleteGroup" runat="server" class="dropdown-item-danger js-delete-group">Delete</a></li>
                                                                </ul>
                                                            </div>
                                                            <div class="btn btn-default btn-xs btn-square pull-right js-placement-group-toggle-visibility placement-group-toggle-visibility">
                                                                <i class="fa fa-chevron-up"></i>
                                                            </div>
                                                            <div class="placement-status-labels pull-right">
                                                                <Rock:HighlightLabel runat="server" LabelType="Info" ID="hlInstanceName" CssClass="margin-r-sm" Visible="false" />
                                                                <Rock:HighlightLabel runat="server" LabelType="Default" ID="hlRegistrationTemplatePlacementName" CssClass="margin-r-sm" Visible="false" />
                                                                <Rock:HighlightLabel runat="server" LabelType="Campus" ID="hlGroupCampus" CssClass="margin-r-sm" Visible="false" />
                                                                <span class="label label-custom placement-capacity-label js-placement-capacity-label margin-r-sm" data-status="none"></span>
                                                            </div>

                                                        </div>
                                                    </div>

                                                    <div class="panel-body padding-all-none js-group-details">

                                                        <asp:Panel Id="pnlGroupAttributes" runat="server" CssClass="panel-drawer p-3 pb-0 border-bottom border-panel">
                                                            <Rock:AttributeValuesContainer ID="avcGroupAttributes" runat="server" NumberOfColumns="2" />
                                                        </asp:Panel>

                                                        <asp:Repeater ID="rptPlacementGroupRole" runat="server" OnItemDataBound="rptPlacementGroupRole_ItemDataBound" Visible="true">
                                                            <ItemTemplate>
                                                                <asp:Panel ID="pnlGroupRoleMembers" runat="server" CssClass="group-role-members js-group-role-members">
                                                                    <Rock:HiddenFieldWithClass ID="hfGroupTypeRoleId" runat="server" CssClass="js-grouptyperole-id" />
                                                                    <Rock:HiddenFieldWithClass ID="hfGroupTypeRoleMaxMembers" runat="server" CssClass="js-grouptyperole-max-members" />

                                                                    <div class="panel panel-inline">

                                                                        <div class="panel-heading">

                                                                            <h1 class="panel-title d-inline-block">
                                                                                <asp:Literal ID="lGroupRoleName" runat="server" />
                                                                            </h1>

                                                                            <asp:Panel ID="pnlGroupRoleStatusLabels" runat="server" CssClass="panel-labels">
                                                                                <div class="js-grouptyperole-statuslabels-container grouptyperole-status-labels">
                                                                                    <span class='badge badge-info'><asp:Literal ID="lGroupRoleCount" runat="server" /></span>
                                                                                    <span class="label label-custom placement-capacity-label js-grouptyperole-max-members-label" data-status="none">1:1</span>
                                                                                </div>
                                                                            </asp:Panel>                                                                        </div>
                                                                        <div class="alert alert-danger js-alert js-placement-place-registrant-error margin-all-md" style="display: none">
                                                                            <button type="button" class="close js-hide-alert" aria-hidden="true"><i class="fa fa-times"></i></button>
                                                                            <span class="js-placement-place-registrant-error-text"></span>
                                                                        </div>
                                                                        <div class="panel-body js-group-role-container group-role-container dropzone droppable" data-empty-label="Drag and Drop Here"></div>
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
                                <Rock:GroupPicker ID="gpAddExistingPlacementGroup" runat="server" Label="Group" ValidationGroup="vgAddPlacementGroup" AllowMultiSelect="true" OnSelectItem="gpAddExistingPlacementGroup_SelectItem" />
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlAddMultipleGroups">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NotificationBox ID="nbAddExistingPlacementMultipleGroupsWarning" runat="server" NotificationBoxType="Warning" Dismissable="true" />
                                <Rock:GroupPicker ID="gpAddExistingPlacementGroupsFromParent" runat="server" Label="Parent Group" ValidationGroup="vgAddPlacementGroup" Help="Selecting a Parent Group will add all of its child groups." OnSelectItem="gpAddExistingPlacementGroupsFromParent_SelectItem" />
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
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:CampusPicker ID="cpConfigurationCampusPicker" runat="server" Label="Campus Filter" CssClass="input-width-xl" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbHighlightGenders" runat="server" Label="Highlight Genders" Help="Enable this to highlight each registrant to indicate their gender (pink, blue)." />
                        </div>
                    </div>
                    <%-- This will only be shown when in Registration Template mode --%>
                    <Rock:PanelWidget ID="pwRegistrationTemplateConfiguration" runat="server" Title="Registration Template Configuration">
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockCheckBox ID="cbShowRegistrationInstanceName" runat="server" Label="Show Registration Instance Name" Help="When enabled, the registration instance name will be included in the details of each registrant in the Registrants list" Checked="true" />

                                <Rock:RockCheckBoxList ID="cblRegistrationInstances" runat="server" Label="Registration Instances" Help="Set the registration instances to include. If none are selected, then all will be included." />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pnlRegistrantConfiguration" runat="server" Title="Registrant Configuration" Expanded="true">
                        <div class="row">
                            
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbShowFees" runat="server" Label="Show Fees" Help="Enable this to show any fees associated with each registrant." />
                            </div>
                        </div>

                        <Rock:RockListBox ID="cblDisplayedRegistrantAttributes" EnhanceForLongLists="true" runat="server" Label="Displayed Registrant Attributes" AutoPostBack="true" OnSelectedIndexChanged="cblDisplayedRegistrantAttributes_SelectedIndexChanged" />
                        <Rock:RockCheckBox ID="cbDisplayRegistrantAttributes" runat="server" Text="Display Registrant Attributes on Group Members" />
                        <hr />
                        <h3>Filters</h3>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockControlWrapper ID="rcwRegistrantFilters" runat="server" Label="Registrant Filters">
                                    <Rock:DynamicPlaceholder ID="phRegistrantFilters" runat="server" />
                                </Rock:RockControlWrapper>
                            </div>
                            <div class="col-md-6">
                                <Rock:RockControlWrapper ID="rcwFeeFilters" runat="server" Label="Fee Filters" Help="Select fees to limit registrants that have selected that fee.">
                                    <Rock:RockDropDownList ID="ddlFeeName" runat="server" Label="Fee" AutoPostBack="true" OnSelectedIndexChanged="ddlFeeName_SelectedIndexChanged" />
                                    <Rock:RockCheckBoxList ID="cblFeeOptions" runat="server" Label="Fee Options" />
                                </Rock:RockControlWrapper>
                            </div>
                        </div>

                        <Rock:RockControlWrapper ID="rcwPersonFilters" runat="server" Label="Person Filters">
                            <Rock:DynamicPlaceholder ID="phPersonFilters" runat="server" />
                        </Rock:RockControlWrapper>
                        <Rock:RockCheckBox ID="cbApplyRegistrantFilters" runat="server" Text="Apply Registrant Filters to Group Members" />
                        
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwGroupConfiguration" runat="server" Title="Group Configuration">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockListBox ID="cblDisplayedGroupAttributes" EnhanceForLongLists="true" runat="server" Label="Displayed Group Attributes" AutoPostBack="true" OnSelectedIndexChanged="cblDisplayedGroupAttributes_SelectedIndexChanged" />
                                <Rock:RockListBox ID="cblDisplayedGroupMemberAttributes" EnhanceForLongLists="true" runat="server" Label="Displayed Group Member Attributes" AutoPostBack="true" OnSelectedIndexChanged="cblDisplayedGroupMemberAttributes_SelectedIndexChanged" Help="Only group member attributes that are defined on the Group Type are currently supported." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbHideFullGroups" runat="server" Label="Hide Full Groups" Help="Enable this to hide placement groups that are at the group capacity." />
                            </div>
                        </div>
                        <hr />
                        <h3>Filters</h3>
                        <Rock:RockControlWrapper ID="rcwGroupFilters" runat="server" Label="Group Filters">
                            <Rock:DynamicPlaceholder ID="phGroupFilters" runat="server" />
                        </Rock:RockControlWrapper>
                        <Rock:RockControlWrapper ID="rcwGroupMemberFilters" runat="server" Label="Group Member Filters">
                            <Rock:DynamicPlaceholder ID="phGroupMemberFilters" runat="server" />
                        </Rock:RockControlWrapper>
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
