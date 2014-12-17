﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupTypes" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                </div>
            </div>
            <div class="panel-body">

                <asp:HiddenField ID="hfGroupTypeId" runat="server" />

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lGroupTypeDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="cold-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                </div>

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valGroupTypeDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

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

                    <Rock:PanelWidget ID="wpGeneral" runat="server" Title="General">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlGroupTypePurpose" runat="server" Label="Purpose" 
                                    Help="An optional field used to qualify what the over-all purpose of this group type is for.  Additional values can be added by editing the 'Group Type Purpose' Defined Type."/>
                                <Rock:RockControlWrapper ID="rcGroupTypes" runat="server" Label="Child Group Types"
                                    Help="The types of child groups that can be added to groups of this type. This is used to define the group hierarchy. To allow an unlimited hierarchy add this type as an allowed child group type.">
                                    <div class="grid">
                                        <Rock:Grid ID="gChildGroupTypes" runat="server" DisplayType="Light" ShowHeader="false" RowItemText="Group Type">
                                            <Columns>
                                                <Rock:RockBoundField DataField="Value" />
                                                <Rock:DeleteField OnClick="gChildGroupTypes_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                </Rock:RockControlWrapper>
                                <Rock:RockCheckBox ID="cbTakesAttendance" runat="server" Label="Takes Attendance" Text="Yes" 
                                    Help="Check this option if groups of this type should support taking and tracking attendance." />
                                <Rock:RockDropDownList ID="ddlAttendanceRule" runat="server" Label="Check-in Rule"
                                    Help="The rule that check in should use when a person attempts to check in to a group of this type.  If 'None' is selected, user will not be added to group and is not required to belong to group.  If 'Add On Check In' is selected, user will be added to group if they don't already belong.  If 'Already Belongs' is selected, user must already be a member of the group or they will not be allowed to check in." />
                                <Rock:RockDropDownList ID="ddlPrintTo" runat="server" Label="Print Using" 
                                    Help="When printing check-in labels, should the device's printer or the location's printer be used?  Note: the device has a similiar setting which takes precedence over this setting.">
                                    <asp:ListItem Text="Device Printer" Value="1" />
                                    <asp:ListItem Text="Location Printer" Value="2" />
                                </Rock:RockDropDownList>
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBoxList ID="cblLocationSelectionModes" runat="server" Label="Location Selection Modes"
                                    Help="The location selection modes to allow when adding locations to groups of this type."/>
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbAllowMultipleLocations" runat="server" Label="Multiple Locations" Text="Allow" 
                                            Help="Check this option if more than one location should be allowed for groups of this type." />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbEnableLocationSchedules" runat="server" Label="Enable Location Schedules" Text="Yes" 
                                            Help="Check this option if group locations should be associated with one or more pre-defined schedules." />
                                    </div>
                                </div>
                                <Rock:RockControlWrapper ID="rcLocationTypes" runat="server" Label="Location Types"
                                    Help="Groups can have one or more location types attached to them.  For instance you may want to have a meeting location and an assignment target location.">
                                    <div class="grid">
                                        <Rock:Grid ID="gLocationTypes" runat="server" DisplayType="Light" ShowHeader="false" RowItemText="Location Type">
                                            <Columns>
                                                <Rock:RockBoundField DataField="Value" />
                                                <Rock:DeleteField OnClick="gLocationTypes_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                </Rock:RockControlWrapper>
                                <Rock:GroupTypePicker ID="gtpInheritedGroupType" runat="server" Label="Inherited Group Type" 
                                    Help="Group Type to inherit attributes from" AutoPostBack="true" OnSelectedIndexChanged="gtpInheritedGroupType_SelectedIndexChanged" />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpRoles" runat="server" Title="Roles">
                        <div class="grid">
                            <Rock:Grid ID="gGroupTypeRoles" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Role">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:RockBoundField DataField="MinCount" HeaderText="Minimum Required" DataFormatString="{0:N0}" />
                                    <Rock:RockBoundField DataField="MaxCount" HeaderText="Maximum Allowed" DataFormatString="{0:N0}" />
                                    <Rock:RockTemplateField HeaderText="Default">
                                        <ItemTemplate>
                                            <input type="radio" value='<%# Eval( "Guid" ) %>' name="GroupTypeDefaultRole" <%# ((Guid)Eval("Guid")).Equals(DefaultRoleGuid) ? "checked" : "" %> />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:EditField OnClick="gGroupTypeRoles_Edit" />
                                    <Rock:DeleteField OnClick="gGroupTypeRoles_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpGroupMemberAttributes" runat="server" Title="Member Attributes" CssClass="group-type-attribute-panel">
                        <Rock:NotificationBox ID="nbGroupMemberAttributes" runat="server" NotificationBoxType="Info" 
                            Text="Member Attributes apply to all of the group members in every group of this type.  Each member will have their own value for these attributes" />
                        <Rock:RockControlWrapper ID="rcGroupMemberAttributesInherited" runat="server" Label="Inherited Attribute(s)">
                            <div class="grid">
                                <Rock:Grid ID="gGroupMemberAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Member Attribute">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" />
                                        <Rock:RockBoundField DataField="Description" />
                                        <Rock:RockTemplateField>
                                            <ItemTemplate>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a>)</ItemTemplate>
                                        </Rock:RockTemplateField>
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:RockControlWrapper>
                        <div class="grid">
                            <Rock:Grid ID="gGroupMemberAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Member Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:EditField OnClick="gGroupMemberAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupMemberAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpGroupAttributes" runat="server" Title="Group Attributes" CssClass="group-type-attribute-panel">
                        <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" 
                            Text="Group Attributes apply to all of the groups of this type.  Each group will have its own value for these attributes" />
                        <Rock:RockControlWrapper ID="rcGroupAttributesInherited" runat="server" Label="Inherited Attribute(s)">
                            <div class="grid">
                                <Rock:Grid ID="gGroupAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Group Attribute">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" />
                                        <Rock:RockBoundField DataField="Description" />
                                        <Rock:RockTemplateField>
                                            <ItemTemplate>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a>)</ItemTemplate>
                                        </Rock:RockTemplateField>
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:RockControlWrapper>
                        <div class="grid">
                            <Rock:Grid ID="gGroupAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:EditField OnClick="gGroupAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpGroupTypeAttributes" runat="server" Title="Group Type Attributes" CssClass="group-type-attribute-panel">
                        <Rock:NotificationBox ID="NotificationBox2" runat="server" NotificationBoxType="Info" 
                            Text="Group Type Attributes apply to all of the groups of this type.  Each group will have the same value equal to what is set as the default value here." />
                        <Rock:RockControlWrapper ID="rcGroupTypeAttributesInherited" runat="server" Label="Inherited Attribute(s)">
                            <div class="grid">
                                <Rock:Grid ID="gGroupTypeAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Group Type Attribute">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" />
                                        <Rock:RockBoundField DataField="Description" />
                                        <Rock:RockTemplateField>
                                            <ItemTemplate>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a>)</ItemTemplate>
                                        </Rock:RockTemplateField>
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:RockControlWrapper>
                        <div class="grid">
                            <Rock:Grid ID="gGroupTypeAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group Type Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:EditField OnClick="gGroupTypeAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupTypeAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpDisplay" runat="server" Title="Display Options">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbGroupTerm" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="GroupTerm" Required="true" 
                                    Help ="The term to use for groups of this group type."/>
                                <Rock:DataTextBox ID="tbGroupMemberTerm" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="GroupMemberTerm" Required="true" 
                                    Help="The term to use for members in groups of this group type."/>
                                <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="IconCssClass"
                                    Help="The Font Awesome icon class to use when displaying groups of thie group type." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbShowInGroupList" runat="server" Label="Show in Group Lists" Text="Yes" 
                                    Help="Check this option to include groups of this type in the GroupList block's list of groups." />
                                <Rock:RockCheckBox ID="cbShowInNavigation" runat="server" Label="Show in Navigation" Text="Yes" 
                                    Help="Check this option to include groups of this type in the GroupTreeView block's navigation control." />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>
            </div>

    </asp:Panel>


        <Rock:ModalAlert ID="modalAlert" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgGroupTypeRoles" runat="server" OnSaveClick="gGroupTypeRoles_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Roles">
            <Content>
                <asp:HiddenField ID="hfRoleGuid" runat="server" />
                <asp:ValidationSummary ID="vsRoles" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Roles" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbRoleName" runat="server" SourceTypeName="Rock.Model.GroupTypeRole, Rock" PropertyName="Name" ValidationGroup="Roles" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbRoleDescription" runat="server" SourceTypeName="Rock.Model.GroupTypeRole, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" ValidationGroup="Roles" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbMinimumRequired" runat="server" NumberType="Integer" Label="Minimum Required" />
                        <Rock:NumberBox ID="nbMaximumAllowed" runat="server" NumberType="Integer" Label="Maximum Allowed" />
                        <asp:CustomValidator ID="cvAllowed" runat="server" Display="None" OnServerValidate="cvAllowed_ServerValidate" 
                            ValidationGroup="Roles" ErrorMessage="The Minimum Required should be less than Maximum Allowed." />
                     </div>
                    <div class="col-md-6">
                        <asp:PlaceHolder ID="phGroupTypeRoleAttributes" runat="server" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgChildGroupType" runat="server" OnSaveClick="dlgChildGroupType_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ChildGroupTypes">
            <Content>
                <Rock:RockDropDownList ID="ddlChildGroupType" runat="server" DataTextField="Name" DataValueField="Id" Label="Child Group Type" ValidationGroup="ChildGroupTypes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgLocationType" runat="server" OnSaveClick="dlgLocationType_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="LocationType">
            <Content>
                <Rock:RockDropDownList ID="ddlLocationType" runat="server" DataTextField="Value" DataValueField="Id" Label="Location Type" ValidationGroup="LocationType" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupTypeAttribute" runat="server" Title="Group Type Attributes" OnSaveClick="dlgGroupTypeAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupTypeAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtGroupTypeAttributes" runat="server" ShowActions="false" ValidationGroup="GroupTypeAttributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupAttribute" runat="server" Title="Group Attributes" OnSaveClick="dlgGroupAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtGroupAttributes" runat="server" ShowActions="false" ValidationGroup="GroupAttributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupMemberAttribute" runat="server" Title="Group Member Attributes" OnSaveClick="dlgGroupMemberAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupMemberttributes">
            <Content>
                <Rock:AttributeEditor ID="edtGroupMemberAttributes" runat="server" ShowActions="false" ValidationGroup="GroupMemberttributes" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
