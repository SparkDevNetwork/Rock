<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinScheduleBuilder.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.CheckinScheduleBuilder" %>

<asp:UpdatePanel ID="upCheckinScheduleBuilder" runat="server">
    <ContentTemplate>
        
        <Rock:CategoryPicker ID="pCategory" runat="server" AllowMultiSelect="false" Label="Schedule Category" OnSelectItem="pCategory_SelectItem" />
        <Rock:NotificationBox ID="nbNotification" runat="server" NotificationBoxType="Warning" />

        <div class="grid">
            <Rock:GridFilter ID="rFilter" runat="server">
                <Rock:GroupTypePicker ID="ddlGroupType" runat="server" />
                <Rock:LocationItemPicker ID="pkrParentLocation" runat="server" Label="Parent Location" />
            </Rock:GridFilter>
            <Rock:Grid ID="gGroupLocationSchedule" runat="server" AllowSorting="true" AllowPaging="false" OnRowDataBound="gGroupLocationSchedule_RowDataBound" >
                <Columns>
                    <asp:BoundField DataField="GroupName" HeaderText="Group" SortExpression="Group.Name" />
                    <asp:BoundField DataField="LocationName" HeaderText="Location" SortExpression="Location.Name" />
                </Columns>
            </Rock:Grid>
        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

