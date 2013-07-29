<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinScheduleBuilder.ascx.cs" Inherits="RockWeb.Blocks.Administration.CheckinScheduleBuilder" %>

<asp:UpdatePanel ID="upCheckinScheduleBuilder" runat="server">
    <ContentTemplate>
        
        <Rock:GridFilter ID="fGroupLocationSchedule" runat="server">
            <Rock:GroupTypePicker ID="ddlGroupType" runat="server" />
            <Rock:CategoryPicker ID="pCategory" runat="server" AllowMultiSelect="false"/>
            <Rock:LabeledDropDownList ID="ddlParentLocation" runat="server" LabelText="Parent Location" />
        </Rock:GridFilter>

        <Rock:Grid ID="gGroupLocationSchedule" runat="server" AllowSorting="true"  >
            <Columns>
                <asp:BoundField DataField="GroupNameLocationName" HeaderText="Checkin" SortExpression="GroupNameLocationName" />
                <asp:BoundField DataField="GroupName" HeaderText="Group" SortExpression="GroupName" />
                <asp:BoundField DataField="LocationName" HeaderText="Location" SortExpression="LocationName" />
            </Columns>
        </Rock:Grid>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

