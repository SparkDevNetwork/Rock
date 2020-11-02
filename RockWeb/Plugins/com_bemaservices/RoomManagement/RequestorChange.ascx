<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RequestorChange.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.RoomReservation.RequestorChange" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Room Reservation Requester Change
                </h1>
            </div>
            <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

            <div class="panel-body">

                <div class="alert alert-info">
                    <p>This block is used to move Room Reservations from one requestor to another.</p>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppOld" runat="server" Label="Old Person" />
                        <asp:Label AssociatedControlID="cblRole" Text="Update the following Role(s)" runat="server" /><br />
                        <asp:CheckBoxList ID="cblRole" RepeatDirection="Horizontal" runat="server">
                            <asp:ListItem Text="Requester" Value="Requester" Selected="True" />
                            <asp:ListItem Text="Event Contact" Value="EventContact" />
                            <asp:ListItem Text="Admin Contact" Value="AdminContact" />
                        </asp:CheckBoxList></br>
                    </div>
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppNew" runat="server" Label="New Person" />
                        <asp:Label AssociatedControlID="cblMinistry" Text="Filter by Ministry" runat="server" /><br />
                        <asp:CheckBoxList ID="cblMinistry" RepeatDirection="Horizontal" runat="server" DataTextField="Name" DataValueField="Id" /></br>
                    </div>
                </div>
                </br>
                <asp:LinkButton ID="btnChange" Text="Change Requestors" CssClass="btn btn-primary" runat="server" OnClick="btnChange_Click" />

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
