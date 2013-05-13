<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Admin.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Admin" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>
    <asp:HiddenField ID="hfKiosk" runat="server" />
    <asp:HiddenField ID="hfGroupTypes" runat="server" />
    <span style="display:none">
        <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click"></asp:LinkButton>
    </span>

    <Rock:ModalAlert ID="maWarning" runat="server" />


    <div class="row-fluid attended-checkin-header">
        <div class="span3 attended-checkin-actions"></div>
        <div class="span6">
            <h1>Admin</h1>
        </div>
        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbOk" runat="server" CssClass="btn btn-large last btn-primary" OnClick="lbOk_Click" Text="OK"></asp:LinkButton>
        </div>
    </div>

    <div class="row-fluid attended-checkin-admin-body">
        <div class="span3">
            <h3>Kiosk Device</h3>
            <Rock:LabeledDropDownList ID="ddlKiosk" runat="server" CssClass="input-xlarge" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" ></Rock:LabeledDropDownList>
        </div>
        <div class="span3">
            <h3>Ministry Types</h3>
            <asp:Repeater ID="rMinistries" runat="server" OnItemCommand="rMinistries_ItemCommand">
                <ItemTemplate>
                    <asp:LinkButton ID="lbSelectMinistries" runat="server" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ><%# Eval("Name") %></asp:LinkButton>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <div class="span3">
            <h3>Rooms</h3>
            <asp:Repeater ID="rRooms" runat="server" OnItemCommand="rRooms_ItemCommand">
                <ItemTemplate>
                    <asp:LinkButton ID="lbSelectRooms" runat="server" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ><%# Eval("Name") %></asp:LinkButton>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <div class="span3"></div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
