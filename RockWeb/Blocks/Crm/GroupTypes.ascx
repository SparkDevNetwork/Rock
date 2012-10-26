<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypes.ascx.cs" Inherits="GroupTypes" %>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">
            <Rock:Grid ID="gGroupType" runat="server" AllowSorting="true">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                    <asp:BoundField DataField="Groups.Count" HeaderText="Group Count" SortExpression="Groups.Count" />
                    <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                    <Rock:EditField OnClick="gGroupType_Edit" />
                    <Rock:DeleteField OnClick="gGroupType_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfGroupTypeId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="failureNotification" />

            <fieldset>
                <legend>
                    <i id="iconIsSystem" runat="server" class="icon-eye-open"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>
                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Crm.GroupType, Rock" PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Crm.GroupType, Rock" PropertyName="Description" />
                        <Rock:DataDropDownList ID="ddlDefaultGroupRole" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Crm.GroupRole, Rock" PropertyName="Name" LabelText="Default Group Role" />
                        </div>
                    <div class="span6">
                        <Rock:Grid ID="gChildGroupTypes" runat="server" AllowPaging="false" ShowActionExcelExport="false">
                            <Columns>
                                <asp:BoundField DataField="Value" HeaderText="Child Group Types" />
                                <Rock:DeleteField OnClick="gChildGroupTypes_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
