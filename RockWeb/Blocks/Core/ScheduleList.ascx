<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleList.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduleList" %>

<asp:UpdatePanel ID="upScheduleList" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="grid">
            <Rock:Grid ID="gSchedules" runat="server" AllowSorting="true" OnRowSelected="gSchedules_Edit">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:BoundField DataField="CategoryName" HeaderText="Category" SortExpression="CategoryName" />
                    <Rock:DeleteField OnClick="gSchedules_Delete" />
                </Columns>
            </Rock:Grid>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
