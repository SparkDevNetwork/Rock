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
                        <Rock:DataDropDownList ID="ddlDefaultGroupRole" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="Name" LabelText="Default Group Role" />
                        <Rock:LabeledCheckBox ID="cbShowInGroupList" runat="server" LabelText="Show in Group Lists" />
                        <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="IconCssClass" />
                        <Rock:ImageSelector ID="imgIconSmall" runat="server" LabelText="Small Icon Image" />
                        <Rock:ImageSelector ID="imgIconLarge" runat="server" LabelText="Large Icon Image"/>
                    </div>
                    <div class="span6">
                        <Rock:LabeledCheckBox ID="cbTakesAttendance" runat="server" LabelText="Takes Attendance" />
                        <Rock:LabeledDropDownList ID="ddlAttendanceRule" runat="server" LabelText="Attendance Rule" />
                        <Rock:LabeledDropDownList ID="ddlAttendancePrintTo" runat="server" LabelText="Attendance Print To" />
                        <Rock:LabeledCheckBox ID="cbAllowMultipleLocations" runat="server" LabelText="Allow Multiple Locations" />
                        <Rock:Grid ID="gChildGroupTypes" runat="server" DisplayType="Light">
                            <Columns>
                                <asp:BoundField DataField="Value" HeaderText="Child Group Types" />
                                <Rock:DeleteField OnClick="gChildGroupTypes_Delete" />
                            </Columns>
                        </Rock:Grid>
                        <Rock:Grid ID="gLocationTypes" runat="server" DisplayType="Light">
                            <Columns>
                                <asp:BoundField DataField="Value" HeaderText="Location Types" />
                                <Rock:DeleteField OnClick="gLocationTypes_Delete" />
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
            <Rock:DataDropDownList ID="ddlChildGroupType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" LabelText="Select Child Group Type" />

            <div class="actions">
                <asp:LinkButton ID="btnAddChildGroupType" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAddChildGroupType_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAddChildGroupType" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancelAddChildGroupType_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlLocationTypePicker" runat="server" Visible="false">
            <Rock:DataDropDownList ID="ddlLocationType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Name" LabelText="Select Location Type" />

            <div class="actions">
                <asp:LinkButton ID="btnAddLocationType" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAddLocationType_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAddLocationType" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancelAddLocationType_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
