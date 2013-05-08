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


    <div class="row-fluid checkin-header">
        <div class="span3"></div>
        <div class="span6">
            <h1>Admin</h1>
        </div>
        <div class="span3"></div>
    </div>


    <div class="row-fluid checkin-body">
        <div class="span12">

            <Rock:LabeledDropDownList ID="ddlKiosk" runat="server" CssClass="input-xlarge" LabelText="Kiosk Device" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" ></Rock:LabeledDropDownList>
            <Rock:LabeledCheckBoxList ID="cblGroupTypes" runat="server" LabelText="Ministry Type(s)" DataTextField="Name" DataValueField="Id" ></Rock:LabeledCheckBoxList>
            <asp:LinkButton ID="lbSelectMinistry" runat="server" CssClass="btn btn-small" Text="Select" OnClick="lbSelectMinistry_Click" />
            <Rock:LabeledCheckBoxList ID="cblRoomTypes" runat="server" LabelText="Room Type(s)" DataTextField="Value" DataValueField="Key" ></Rock:LabeledCheckBoxList>
            <%--<Rock:GroupPicker ID="gpMinistryGroups" runat="server" Required="false" LabelText="Ministry Types(s)" OnSelectItem="ddlMinistryGroups_SelectedIndexChanged" AllowMultiSelect="true"/>--%>
            <%--<Rock:GroupPicker ID="gpMinistryGroups" runat="server" Required="false" LabelText="Ministry Types(s)" AllowMultiSelect="true"/>--%>

        </div>
    </div>



   <div class="row-fluid checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-primary" ID="lbOk" runat="server" OnClick="lbOk_Click" Text="OK" Visible="false" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
