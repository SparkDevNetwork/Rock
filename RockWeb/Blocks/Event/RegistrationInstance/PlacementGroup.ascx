<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PlacementGroup.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstance.PlacementGroup" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block panel-placementgroup">

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

                        <Rock:HiddenFieldWithClass ID="hfRegistrationTemplateId" runat="server" CssClass="js-registration-template-id" />
                        <Rock:HiddenFieldWithClass ID="hfRegistrationInstanceId" runat="server" CssClass="js-registration-instance-id" />

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
                                                <Rock:HiddenFieldWithClass ID="hfPlacementGroupid" runat="server" CssClass="js-placement-group-id" />
                                                <Rock:HiddenFieldWithClass ID="hfPlacementGroupCapacity" runat="server" CssClass="js-placement-group-capacity" />

                                                <div class="panel panel-block">
                                                    <div class="panel-heading">
                                                        <h1 class="panel-title">
                                                            <asp:Literal ID="lGroupName" runat="server" />
                                                        </h1>

                                                        <asp:Panel ID="pnlGroupStatusLabels" runat="server" CssClass="panel-labels">
                                                            ##TODO Group Status ##
                                                        </asp:Panel>

                                                        <asp:Repeater ID="rptPlacementGroupRole" runat="server">
                                                            <ItemTemplate>
                                                                <div class="panel panel-block">
                                                                    <div class="panel-heading">
                                                                        <h1 class="panel-title">
                                                                            <asp:Literal ID="lGroupRoleName" runat="server" />
                                                                        </h1>

                                                                        <asp:Panel ID="pnlGroupRoleStatusLabels" runat="server" CssClass="panel-labels">
                                                                            ##TODO Role Status##
                                                                        </asp:Panel>
                                                                    </div>
                                                                </div>

                                                                <div class="panel-body">
                                                                    <div class="group-group-role-placement-target-container js-group-group-role-placement-target-container dropzone"></div>
                                                                </div>
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

    </ContentTemplate>
</asp:UpdatePanel>
