<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceGroupPlacement.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceGroupPlacement" %>

<style>
    .registrant-gender-boy {
        background-color: lightblue;
        border-color: blue;
        color: blue;
    }

    .registrant-gender-girl {
        background-color: lightpink;
        border-color: deeppink;
        color: deeppink;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ButtonGroup ID="bgRegistrationTemplatePlacement" runat="server" Label="Select Placement Type" AutoPostBack="true" OnSelectedIndexChanged="bgRegistrationTemplatePlacement_SelectedIndexChanged" />
        <Rock:NotificationBox ID="nbConfigurationError" runat="server" />
        <Rock:HiddenFieldWithClass ID="hfRegistrationTemplateId" runat="server" CssClass="js-registration-template-id" />
        <Rock:HiddenFieldWithClass ID="hfRegistrationInstanceId" runat="server" CssClass="js-registration-instance-id" />
        <Rock:HiddenFieldWithClass ID="hfRegistrationTemplatePlacementId" runat="server" CssClass="js-registration-template-placement-id" />
        <Rock:HiddenFieldWithClass ID="hfRegistrationTemplatePlacementGroupTypeId" runat="server" CssClass="js-registration-template-placement-grouptype-id" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block panel-group-placement">

            <%-- Panel Header --%>
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lGroupPlacementGroupTypeIconHtml" runat="server">
                        <i class="fa fa-star"></i>
                    </asp:Literal>
                    <asp:Literal ID="lGroupPlacementGroupTypeName" Text="###GroupType.Term###.Pluralized()" runat="server" />
                </h1>

                <div class="panel-labels">
                    <asp:LinkButton ID="btnAddPlacementGroup" runat="server" CssClass="js-autoschedule btn btn-default btn-xs" OnClick="btnAddPlacementGroup_Click">
                        <asp:Literal ID="lAddPlacementGroupButtonIconHtml" runat="server">
                            <i class="fa fa-star"></i>
                        </asp:Literal>
                        <asp:Literal ID="lAddPlacementGroupButtonText" Text="Add (###GroupType.Term Name###)" runat="server" />
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnConfiguration" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnConfiguration_Click"><i class="fa fa-gear"></i></asp:LinkButton>
                </div>
            </div>

            <%-- Panel Body --%>
            <div class="panel-body">
                <asp:Panel ID="pnlGroupPlacementContainer" runat="server">
                    <asp:Panel ID="pnlGroupPlacement" runat="server">



                        <div class="row row-eq-height">
                            <div class="col-md-4 hidden-xs">

                                <div class="group-placement-registrantlist">

                                    <div class="panel panel-block registrant-list">

                                        <div class="panel-heading">
                                            <h1 class="panel-title">
                                                <i class="fa fa-user"></i>
                                                Registrants
                                            </h1>

                                            <div class="panel-labels">
                                                <div class="js-toggle-registrant-details btn btn-xs btn-default">
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
                                                <asp:Panel ID="pnlRegistrantListContainer" CssClass="js-group-placement-registrant-container registrant-container dropzone" runat="server">
                                                </asp:Panel>
                                            </div>
                                        </div>

                                    </div>
                                </div>

                            </div>

                            <div class="col-md-8">
                                <%-- containers for each placement group (for example:bus) that registrants can be dragged into --%>
                                <div class="placement-groups js-placement-groups">
                                    <asp:Repeater ID="rptPlacementGroups" runat="server" OnItemDataBound="rptPlacementGroups_ItemDataBound">
                                        <ItemTemplate>

                                            <asp:Panel ID="pnlPlacementGroup" runat="server" CssClass="placement-group js-placement-group">
                                                <Rock:HiddenFieldWithClass ID="hfPlacementGroupId" runat="server" CssClass="js-placement-group-id" />
                                                <Rock:HiddenFieldWithClass ID="hfPlacementGroupCapacity" runat="server" CssClass="js-placement-group-capacity" />

                                                <div class="panel panel-block placement-group">
                                                    <div class="panel-heading">
                                                        <h1 class="panel-title">
                                                            <asp:Literal ID="lGroupName" runat="server" />
                                                        </h1>

                                                        <asp:Panel ID="pnlGroupStatusLabels" runat="server" CssClass="panel-labels">
                                                            ##TODO Group Status ##
                                                        </asp:Panel>
                                                    </div>

                                                    <div class="panel-body">

                                                        <asp:Repeater ID="rptPlacementGroupRole" runat="server" OnItemDataBound="rptPlacementGroupRole_ItemDataBound" Visible="true">
                                                            <ItemTemplate>
                                                                <asp:Panel ID="pnlGroupRoleMembers" runat="server" CssClass="group-role-members js-group-role-members">
                                                                    <Rock:HiddenFieldWithClass ID="hfGroupTypeRoleId" runat="server" CssClass="js-grouptyperole-id" />

                                                                    <div class="panel panel-block">
                                                                        <div class="panel-heading">

                                                                            <h1 class="panel-title">
                                                                                <asp:Literal ID="lGroupRoleName" runat="server" />
                                                                            </h1>

                                                                            <asp:Panel ID="pnlGroupRoleStatusLabels" runat="server" CssClass="panel-labels">
                                                                                ##TODO Role Status##
                                                                                <div class="js-grouptyperole-statuslabels-container grouptyperole-statuslabels-container"></div>
                                                                            </asp:Panel>
                                                                        </div>
                                                                        <div class="panel-body">
                                                                            <div class="placement-target-container js-placement-target-container dropzone"></div>
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
            <Rock:ModalDialog ID="mdAddPlacementGroup" runat="server" ValidationGroup="vgAddPlacementGroup" Title="Add Placement Group/Add Shared Placement Group" OnSaveClick="mdAddPlacementGroup_SaveClick">
                <Content>
                    <Rock:ButtonGroup ID="bgAddNewOrExistingPlacementGroup" runat="server" OnSelectedIndexChanged="bgAddNewOrExistingPlacementGroup_SelectedIndexChanged" AutoPostBack="true">
                        
                    </Rock:ButtonGroup>
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
                                <Rock:CampusPicker ID="cpNewPlacementGroupCampus" runat="server" Label="Campus" ValidationGroup="vgAddPlacementGroup"/>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbGroupCapacity" runat="server" Label="Group Capacity" NumberType="Integer" MinimumValue="0" ValidationGroup="vgAddPlacementGroup"/>
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="tbNewPlacementGroupDescription" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" ValidationGroup="vgAddPlacementGroup"/>
                                <Rock:AttributeValuesContainer ID="avcNewPlacementGroupAttributeValues" runat="server" ValidationGroup="vgAddPlacementGroup"/>
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlAddExistingPlacementGroup">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NotificationBox ID="nbAddExistingPlacementGroupWarning" runat="server" NotificationBoxType="Warning" Dismissable="true" />
                                <Rock:GroupPicker ID="gpAddExistingPlacementGroup" runat="server" Label="Group" ValidationGroup="vgAddPlacementGroup"  OnSelectItem="gpAddExistingPlacementGroup_SelectItem"/>
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
            <Rock:ModalDialog ID="mdPlacementConfiguration" runat="server" Title="Placement Configuration" OnSaveClick="mdPlacementConfiguration_SaveClick">
                <Content>
                    <Rock:CampusPicker ID="cpConfigurationCampusPicker" runat="server" Label="Campus Filter" />

                    <%-- This will only be shown when in Registration Template mode --%>
                    <Rock:PanelWidget ID="pwRegistrationInstanceConfiguration" runat="server" Title="Registration Instance Configuration">
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockCheckBox ID="cbShowRegistrationInstanceName" runat="server" Label="Show Registration Instance Name" Help="When enabled, the registration instance name will be included in the details of each registrant in the Registrants list" Checked="true" />

                                <Rock:RockCheckBoxList ID="cblRegistrationInstances" runat="server" Label="Registration Instances" Help="Set the registration instances to include. If none are selected, then all will be included." />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pnlRegistrationConfiguration" runat="server" Title="Registrant Configuration">
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
                                <Rock:RockDropDownList ID="ddlDisplayedRegistrantAttributes" EnhanceForLongLists="true" runat="server" Label="Displayed Registration (did you mean Registrant or both??) Attributes" />

                                <Rock:RockControlWrapper ID="rcwRegistrantFilters" runat="server" Label="Registrant Filters">
                                    ##TODO##
                                </Rock:RockControlWrapper>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwGroupConfiguration" runat="server" Title="Group Configuration">
                        ##TODO##
                    </Rock:PanelWidget>

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
