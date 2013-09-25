<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupTypes" %>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfGroupTypeId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Description" />
                        <Rock:DataTextBox ID="tbGroupTerm" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="GroupTerm" />
                        <Rock:DataTextBox ID="tbGroupMemberTerm" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="GroupMemberTerm" />
                        <Rock:DataDropDownList ID="ddlDefaultGroupRole" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="Name" Label="Default Group Role" />
                        <Rock:RockCheckBox ID="cbShowInGroupList" runat="server" Label="Show in Group Lists" />
                        <Rock:RockCheckBox ID="cbShowInNavigation" runat="server" Label="Show in Navigation" Help="Check the box to show in navigation controls such as TreeViews and Menus." />
                        <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="IconCssClass" />
                        <Rock:ImageUploader ID="imgIconSmall" runat="server" Label="Small Icon Image" />
                        <Rock:ImageUploader ID="imgIconLarge" runat="server" Label="Large Icon Image" />
                    </div>
                    <div class="span6">
                        <Rock:RockCheckBox ID="cbTakesAttendance" runat="server" Label="Takes Attendance" />
                        <Rock:RockDropDownList ID="ddlAttendanceRule" runat="server" Label="Attendance Rule" />
                        <Rock:RockDropDownList ID="ddlAttendancePrintTo" runat="server" Label="Attendance Print To" />
                        <Rock:RockCheckBox ID="cbAllowMultipleLocations" runat="server" Label="Allow Multiple Locations" />
                        <Rock:RockDropDownList ID="ddlLocationSelectionMode" runat="server" Label="Location Selection Mode" Help="The selection mode to use when adding locations to groups of this type" />
                        <Rock:RockDropDownList ID="ddlGroupTypePurpose" runat="server" Label="Purpose" Help="Define a specific purpose for this group type <span class='label'>Defined Value</span>" />
                        <Rock:GroupTypePicker ID="gtpInheritedGroupType" runat="server" Label="Inherited Group Type" Help="Group Type to inherit properties and attributes from" AutoPostBack="true" OnSelectedIndexChanged="gtpInheritedGroupType_SelectedIndexChanged" />
                        <div class="control-group">
                            <h5>Group Type Attributes
                            </h5>
                            <p>
                                Global attributes provide values for each and every group of this type. Every group of this type will be guaranteed to have the same value.
                            </p>
                            <Rock:Grid ID="gGroupTypeAttributes" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false">
                                <Columns>
                                    <asp:BoundField DataField="Name" />
                                    <Rock:EditField OnClick="gGroupTypeAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupTypeAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                        <h5>Group Attributes
                        </h5>
                        <p>
                            Group attributes allow for providing different values for each group. Examples  would be 'small group topic', 'meeting night', etc.
                        </p>
                        <Rock:Grid ID="gGroupAttributes" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false">
                            <Columns>
                                <asp:BoundField DataField="Name" />
                                <Rock:EditField OnClick="gGroupAttributes_Edit" />
                                <Rock:DeleteField OnClick="gGroupAttributes_Delete" />
                            </Columns>
                        </Rock:Grid>

                        <h5>Location Types
                        </h5>
                        <p>
                            Groups can have several locations attached to them.  For instance you may want to have a meeting location and a assignment target location.
                        </p>
                        <Rock:Grid ID="gLocationTypes" runat="server" DisplayType="Light" ShowHeader="false">
                            <Columns>
                                <asp:BoundField DataField="Value" />
                                <Rock:DeleteField OnClick="gLocationTypes_Delete" />
                            </Columns>
                        </Rock:Grid>

                        <h5>Allowed Child Groups
                        </h5>
                        <p>
                            This defines what types of groups can be added as children. This helps to define what hierarchy of groups you wish to support. To create allow for an unlimited hierarchy allow child groups of this group type.
                        </p>
                        <Rock:Grid ID="gChildGroupTypes" runat="server" DisplayType="Light" ShowHeader="false">
                            <Columns>
                                <asp:BoundField DataField="Value" />
                                <Rock:DeleteField OnClick="gChildGroupTypes_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" /> 
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlChildGroupTypePicker" runat="server" Visible="false">
            <Rock:DataDropDownList ID="ddlChildGroupType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" Label="Select Child Group Type" />

            <div class="actions">
                <asp:LinkButton ID="btnAddChildGroupType" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAddChildGroupType_Click"></asp:LinkButton> 
                <asp:LinkButton ID="btnCancelAddChildGroupType" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancelAddChildGroupType_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlLocationTypePicker" runat="server" Visible="false">
            <Rock:DataDropDownList ID="ddlLocationType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Name" Label="Select Location Type" />

            <div class="actions">
                <asp:LinkButton ID="btnAddLocationType" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAddLocationType_Click"></asp:LinkButton> 
                <asp:LinkButton ID="btnCancelAddLocationType" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancelAddLocationType_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlGroupTypeAttribute" runat="server" Visible="false">
            <Rock:AttributeEditor ID="edtGroupTypeAttributes" runat="server" OnSaveClick="btnSaveGroupTypeAttribute_Click" OnCancelClick="btnCancelGroupTypeAttribute_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlGroupAttribute" runat="server" Visible="false">
            <Rock:AttributeEditor ID="edtGroupAttributes" runat="server" OnSaveClick="btnSaveGroupAttribute_Click" OnCancelClick="btnCancelGroupAttribute_Click" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
