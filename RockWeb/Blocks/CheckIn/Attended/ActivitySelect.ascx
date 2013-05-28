<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ActivitySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.ActivitySelect" %>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row-fluid attended-checkin-header">
        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbBack" CssClass="btn btn-primary btn-large" runat="server" OnClick="lbBack_Click" Text="Back"/>
        </div>

        <div class="span6">
            <h1><asp:Label ID="lblPersonName" runat="server"></asp:Label></h1>
        </div>

        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbNext" CssClass="btn btn-primary btn-large last" runat="server" OnClick="lbNext_Click" Text="Next"/>
        </div>
    </div>
                
    <div class="row-fluid attended-checkin-body">

        <div class="span3">
            <div class="attended-checkin-body-container">
                <h3>Ministry</h3>
                <asp:Repeater ID="rMinistry" runat="server" OnItemCommand="rMinistry_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectMinistry" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div class="span3">
            <div class="attended-checkin-body-container">
                <h3>Time</h3>
                <asp:Repeater ID="rTime" runat="server" OnItemCommand="rTime_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectTime" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <asp:HiddenField ID="hfTimes" runat="server"></asp:HiddenField>
        </div>

        <div class="span3">
            <div class="attended-checkin-body-container">
                <h3>Activity</h3>
                <asp:Repeater ID="rActivity" runat="server" OnItemCommand="rActivity_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectActivity" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div class="span3">
<%--        Haven't completely decided what we're going to do with this column yet. I say that we list the choices that the user makes and allow them to make changes. Maybe if the user
            selects a block here, it highlights the current selections allowing them to edit? And then have a "X" button attached to each block allowing them to delete? Or is that getting too
            complicated for what we need?--%>
            <div class="attended-checkin-body-container">
                <h3>Selected</h3>
                <Rock:Grid ID="gActivityList" runat="server" AllowSorting="true" AllowPaging="false" ShowActionRow="false" ShowHeader="false">
                    <Columns>
                        <asp:BoundField DataField="ListId" Visible="false" />
                        <asp:BoundField DataField="Time" />
                        <asp:BoundField DataField="AssignedTo" />
                        <Rock:DeleteField OnClick="gActivityList_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>
        </div>

    </div>   

</ContentTemplate>
</asp:UpdatePanel>
