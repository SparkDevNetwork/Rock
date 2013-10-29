<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupTypes" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfGroupTypeId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
                <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
            </div>

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <fieldset>
                            <legend>Behavior</legend>
                            <Rock:DataTextBox ID="tbGroupTerm" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="GroupTerm" Required="true" ValidationGroup="GroupTypeDetail" />
                            <Rock:DataTextBox ID="tbGroupMemberTerm" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="GroupMemberTerm" Required="true" ValidationGroup="GroupTypeDetail" />
                            <Rock:RockDropDownList ID="ddlGroupTypePurpose" runat="server" Label="Purpose" Help="Define a specific purpose for this group type" ValidationGroup="GroupTypeDetail" />
                            <Rock:RockDropDownList ID="ddlDefaultGroupRole" runat="server" DataTextField="Name" DataValueField="Id" Label="Default Group Role" ValidationGroup="GroupTypeDetail" />
                            <Rock:RockControlWrapper ID="rcGroupTypes" runat="server" Label="Child Group Types" ValidationGroup="GroupTypeDetail"
                                Help="This defines what types of child groups can be added to groups of this type. This is used to define the group hierarchy. To allow an unlimited hierarchy add this type as an allowed child group type.">
                                <Rock:Grid ID="gChildGroupTypes" runat="server" DisplayType="Light" ShowHeader="false" RowItemText="Group Type">
                                    <Columns>
                                        <asp:BoundField DataField="Value" />
                                        <Rock:DeleteField OnClick="gChildGroupTypes_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </Rock:RockControlWrapper>
                        </fieldset>
                    </div>
                    <div class="col-md-6">
                        <fieldset>
                            <legend>Display</legend>
                            <Rock:RockCheckBox ID="cbShowInGroupList" runat="server" Label="Show in Group Lists" Text="Yes" Help="Check this option to include groups of this type in the GroupList block's list of groups." ValidationGroup="GroupTypeDetail" />
                            <Rock:RockCheckBox ID="cbShowInNavigation" runat="server" Label="Show in Navigation" Text="Yes" Help="Check this option to include groups of this type in the GroupTreeView block's navigation control." ValidationGroup="GroupTypeDetail" />
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="IconCssClass" ValidationGroup="GroupTypeDetail" />
                            <Rock:ImageUploader ID="imgIconSmall" runat="server" Label="Small Icon Image" ValidationGroup="GroupTypeDetail" />
                            <Rock:ImageUploader ID="imgIconLarge" runat="server" Label="Large Icon Image" ValidationGroup="GroupTypeDetail" />
                        </fieldset>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <fieldset>
                            <legend>Locations</legend>
                            <Rock:RockCheckBox ID="cbAllowMultipleLocations" runat="server" Label="Multiple Locations" Text="Allow" ValidationGroup="GroupTypeDetail" />
                            <Rock:RockDropDownList ID="ddlLocationSelectionMode" runat="server" Label="Location Selection Mode" Help="The selection mode to use when adding locations to groups of this type" ValidationGroup="GroupTypeDetail" />
                            <Rock:RockControlWrapper ID="rcLocationTypes" runat="server" Label="Location Types" ValidationGroup="GroupTypeDetail"
                                Help="Groups can have one or more location types attached to them.  For instance you may want to have a meeting location and an assignment target location.">
                                <Rock:Grid ID="gLocationTypes" runat="server" DisplayType="Light" ShowHeader="false" RowItemText="Location">
                                    <Columns>
                                        <asp:BoundField DataField="Value" />
                                        <Rock:DeleteField OnClick="gLocationTypes_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </Rock:RockControlWrapper>
                        </fieldset>
                    </div>
                    <div class="col-md-6">
                        <fieldset>
                            <legend>Check In</legend>
                            <Rock:RockCheckBox ID="cbTakesAttendance" runat="server" Label="Takes Attendance" Text="Yes" Help="Check this option if groups of this type should allow taking of attendance." ValidationGroup="GroupTypeDetail" />
                            <Rock:RockDropDownList ID="ddlAttendanceRule" runat="server" Label="Attendance Rule" ValidationGroup="GroupTypeDetail" />
                            <Rock:RockDropDownList ID="ddlAttendancePrintTo" runat="server" Label="Attendance Print To" ValidationGroup="GroupTypeDetail" />
                        </fieldset>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <fieldset>
                            <legend>Attributes</legend>
                            <Rock:GroupTypePicker ID="gtpInheritedGroupType" runat="server" Label="Inherited Group Type" Help="Group Type to inherit attributes from" AutoPostBack="true" OnSelectedIndexChanged="gtpInheritedGroupType_SelectedIndexChanged" ValidationGroup="GroupTypeDetail" />
                        </fieldset>
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockControlWrapper ID="rcGroupTypeAttributesInherited" runat="server" Label="Inherited Group Type Attributes" ValidationGroup="GroupTypeDetail"
                            Help="Inherited Attributes for this group type.">
                            <Rock:Grid ID="gGroupTypeAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Attribute">
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <span class='text-muted'>
                                                <%# Eval("Name") %><br />
                                                <small>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a>)</small>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </Rock:Grid>
                        </Rock:RockControlWrapper>
                        <Rock:RockControlWrapper ID="rcGroupTypeAttributes" runat="server" Label="Group Type Attributes" ValidationGroup="GroupTypeDetail"
                            Help="Attributes for this group type. Every group of this type will have the same value that is set here.">
                            <Rock:Grid ID="gGroupTypeAttributes" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <asp:BoundField DataField="Name" />
                                    <Rock:EditField OnClick="gGroupTypeAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupTypeAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </Rock:RockControlWrapper>
                    </div>
                    <div class="col-md-4">
                        <Rock:RockControlWrapper ID="rcGroupAttributesInherited" runat="server" Label="Inherited Group Attributes" ValidationGroup="GroupTypeDetail"
                            Help="Inherited Attributes for groups of this type.">
                            <Rock:Grid ID="gGroupAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Attribute">
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <span class='text-muted'>
                                                <%# Eval("Name") %><br />
                                                <small>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a>)</small>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </Rock:Grid>
                        </Rock:RockControlWrapper>
                        <Rock:RockControlWrapper ID="rcGroupAttributes" runat="server" Label="Group Attributes" ValidationGroup="GroupTypeDetail"
                            Help="Attributes for groups of this type. Each group can have different values (i.e. Meeting Time) that are set on the group detail.">
                            <Rock:Grid ID="gGroupAttributes" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <asp:BoundField DataField="Name" />
                                    <Rock:EditField OnClick="gGroupAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </Rock:RockControlWrapper>
                    </div>
                    <div class="col-md-4">
                        <Rock:RockControlWrapper ID="rcGroupMemberAttributesInherited" runat="server" Label="Inherited Group Member Attributes" ValidationGroup="GroupTypeDetail"
                            Help="Inherited Attributes for members in a group of this type.">
                            <Rock:Grid ID="gGroupMemberAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Attribute">
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <span class='text-muted'>
                                                <%# Eval("Name") %><br />
                                                <small>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a>)</small>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </Rock:Grid>
                        </Rock:RockControlWrapper>
                        <Rock:RockControlWrapper ID="rcGroupMemberAttributes" runat="server" Label="Group Member Attributes" ValidationGroup="GroupTypeDetail"
                            Help="Attributes for members in a group of this type. Each member can have different values (i.e. Hours Serving) that are set on the member detail.">
                            <Rock:Grid ID="gGroupMemberAttributes" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <asp:BoundField DataField="Name" />
                                    <Rock:EditField OnClick="gGroupMemberAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupMemberAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </Rock:RockControlWrapper>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" ValidationGroup="GroupTypeDetail" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <p class="description">
                    <asp:Literal ID="lGroupTypeDescription" runat="server"></asp:Literal>
                </p>

                <div class="row">
                    <asp:Literal ID="lblMainDetails" runat="server" />
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-sm" CausesValidation="false" OnClick="btnEdit_Click" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-action btn-sm" CausesValidation="false" OnClick="btnDelete_Click" />
                </div>

            </fieldset>

        </asp:Panel>

        <Rock:ModalAlert ID="modalAlert" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgChildGroupType" runat="server" OnSaveClick="dlgChildGroupType_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ChildGroupType">
            <Content>
                <Rock:RockDropDownList ID="ddlChildGroupType" runat="server" DataTextField="Name" DataValueField="Id" Label="Child Group Type" ValidationGroup="ChildGroupType" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgLocationType" runat="server" OnSaveClick="dlgLocationType_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="LocationType">
            <Content>
                <Rock:RockDropDownList ID="ddlLocationType" runat="server" DataTextField="Name" DataValueField="Id" Label="Location Type" ValidationGroup="LocationType" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupTypeAttribute" runat="server" OnSaveClick="dlgGroupTypeAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupTypeAttribute">
            <Content>
                <Rock:AttributeEditor ID="edtGroupTypeAttributes" runat="server" ShowActions="false" ValidationGroup="GroupTypeAttribute" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupAttribute" runat="server" OnSaveClick="dlgGroupAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupAttribute">
            <Content>
                <Rock:AttributeEditor ID="edtGroupAttributes" runat="server" ShowActions="false" ValidationGroup="GroupAttribute" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupMemberAttribute" runat="server" OnSaveClick="dlgGroupMemberAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupMemberAttribute">
            <Content>
                <Rock:AttributeEditor ID="edtGroupMemberAttributes" runat="server" ShowActions="false" ValidationGroup="GroupMemberAttribute" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
