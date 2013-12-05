<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upGroupList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lGroupIconHtml" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>

                <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />

            </div>

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <Rock:NotificationBox ID="nbRoleLimitWarning" runat="server" NotificationBoxType="Warning" Heading="Role Limit Warning" />
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                    </div>
                </div>

                <Rock:PanelWidget ID="wpGeneral" runat="server" title="General">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlGroupType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" Label="Group Type" AutoPostBack="true" />
                            <Rock:GroupPicker ID="gpParentGroup" runat="server" Required="false" Label="Parent Group" OnSelectItem="ddlParentGroup_SelectedIndexChanged" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlCampus" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" Label="Campus" />
                            <Rock:RockCheckBox ID="cbIsSecurityRole" runat="server" Label="Security Role" Text="Yes"  />
                        </div>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="wpLocations" runat="server">
                    <Rock:Grid ID="gLocations" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Location">
                        <Columns>
                            <asp:BoundField DataField="Location" HeaderText="Location" />
                            <asp:BoundField DataField="LocationTypeValue.Name" HeaderText="Type" />
                            <Rock:DeleteField OnClick="gLocations_Delete"  />
                        </Columns>
                    </Rock:Grid>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="wpGroupAttributes" runat="server" Title="Group Attribute Values">
                    <asp:PlaceHolder ID="phGroupAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="wpGroupMemberAttributes" runat="server" Title="Member Attributes" CssClass="group-type-attribute-panel">
                    <Rock:NotificationBox ID="nbGroupMemberAttributes" runat="server" NotificationBoxType="Info" 
                        Text="Member Attributes apply to members in this group.  Each member will have their own value for these attributes" />
                    <Rock:RockControlWrapper ID="rcGroupMemberAttributesInherited" runat="server" Label="Inherited Attribute(s)">
                        <Rock:Grid ID="gGroupMemberAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Member Attribute">
                            <Columns>
                                <asp:BoundField DataField="Name" />
                                <asp:BoundField DataField="Description" />
                                <asp:TemplateField>
                                    <ItemTemplate>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a>)</ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </Rock:Grid>
                    </Rock:RockControlWrapper>
                    <Rock:Grid ID="gGroupMemberAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Member Attribute" ShowConfirmDeleteDialog="false">
                        <Columns>
                            <Rock:ReorderField />
                            <asp:BoundField DataField="Name" HeaderText="Attribute" />
                            <asp:BoundField DataField="Description" HeaderText="Description" />
                            <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                            <Rock:EditField OnClick="gGroupMemberAttributes_Edit" />
                            <Rock:DeleteField OnClick="gGroupMemberAttributes_Delete" />
                        </Columns>
                    </Rock:Grid>
                </Rock:PanelWidget>


                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary btn-sm" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn  btn-sm" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <p class="description">
                    <asp:Literal ID="lGroupDescription" runat="server"></asp:Literal></p>

                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                        <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                    </div>
                    <div class="col-md-6 location-maps">
                        <asp:PlaceHolder ID="phMaps" runat="server" />
                    </div>
                </div>


                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-sm" OnClick="btnEdit_Click" CausesValidation="false" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-action btn-sm" OnClick="btnDelete_Click" CausesValidation="false" />
                    <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm pull-right" />
                </div>

            </fieldset>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgGroupMemberAttribute" runat="server" Title="Group Member Attributes" OnSaveClick="dlgGroupMemberAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupMemberAttribute">
            <Content>
                <Rock:AttributeEditor ID="edtGroupMemberAttributes" runat="server" ShowActions="false" ValidationGroup="GroupMemberAttribute" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgLocations" runat="server" Title="Group Location" OnSaveClick="dlgLocations_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Location">
            <Content>

                <asp:HiddenField ID="hfAddLocationGroupGuid" runat="server" />

                <asp:ValidationSummary ID="valLocationSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Location" />

                <ul id="ulNav" runat="server" class="nav nav-pills" >
                    <asp:Repeater ID="rptLocationTypes" runat="server" >
                        <ItemTemplate >
                            <li class='<%# GetTabClass(Container.DataItem) %>'>
                                <asp:LinkButton ID="lbLocationType" runat="server" Text='<%# Container.DataItem %>' OnClick="lbLocationType_Click" CausesValidation="false">
                                </asp:LinkButton> 
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>

                <div class="tabContent" >
                    <asp:Panel ID="pnlMemberSelect" runat="server" Visible="true" >
                        <Rock:RockDropDownList ID="ddlMember" runat="server" Label="Member" ValidationGroup="Location" />
                    </asp:Panel>
                    <asp:Panel ID="pnlLocationSelect" runat="server" Visible="false">
                        <Rock:LocationPicker ID="locpGroupLocation" runat="server" Label="Location" ValidationGroup="Location" />
                    </asp:Panel>
                </div>

                <Rock:RockDropDownList ID="ddlLocationType" runat="server" Label="Type" DataValueField="Id" DataTextField="Name" ValidationGroup="Location" />

            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
