<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Admin.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Admin" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>
    <asp:HiddenField ID="hfLatitude" runat="server" />
    <asp:HiddenField ID="hfLongitude" runat="server" />
    <asp:HiddenField ID="hfKiosk" runat="server" />
    <asp:HiddenField ID="hfGroupTypes" runat="server" />
    <span style="display:none">
        <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click"></asp:LinkButton>
        <asp:LinkButton ID="lbCheckGeoLocation" runat="server" OnClick="lbCheckGeoLocation_Click"></asp:LinkButton>
    </span>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row checkin-header">
        <div class="col-md-12">
            <h1>Check-in Configuration</h1>
        </div>
    </div>
    
    <asp:Panel runat="server" ID="pnlManualConfig" Visible="false">

    <div class="row checkin-body">
        <div class="col-md-12">

            <Rock:RockDropDownList ID="ddlKiosk" runat="server" CssClass="input-xlarge" Label="Kiosk Device" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" ></Rock:RockDropDownList>
            <Rock:RockCheckBoxList ID="cblGroupTypes" runat="server" Label="Group Type(s)" DataTextField="Name" DataValueField="Id" ></Rock:RockCheckBoxList>

        </div>
    </div>
    </asp:Panel>

    <div class="checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-primary" ID="lbOk" runat="server" OnClick="lbOk_Click" Text="OK" Visible="false" />
            <a class="btn btn-primary" runat="server" ID="lbRetry" visible="false" href="javascript:window.location.href=window.location.href" >Retry</a>
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
