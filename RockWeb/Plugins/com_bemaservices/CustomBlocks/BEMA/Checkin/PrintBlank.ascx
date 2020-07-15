<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrintBlank.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.CustomBlocks.BEMA.Checkin.PrintBlank" %>

<meta name="viewport" content="width=device-width, initial-scale=1">


<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfActiveTab" runat="server" />
        <asp:HiddenField ID="hfActiveDialog" runat="server" />
        <asp:HiddenField ID="hfAttendanceId" runat="server" />
        <asp:HiddenField ID="hfPersonId" runat="server" />
        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfGroupTypeId" runat="server" />
        <asp:HiddenField ID="hfCampusId" runat="server" />
        <asp:HiddenField ID="hfEnRouteId" runat="server" />
        <asp:HiddenField ID="hfStaying" runat="server" Value="False" />
        <asp:HiddenField runat="server" id="hfMessages"/>
        <asp:HiddenField runat="server" id="hfFromNumber"/>
        <asp:HiddenField ID="hfMoveAttendanceId" runat="server" />

        <Rock:RockDropDownList ID="rddlLabel" runat="server" DataTextField="Name" DataValueField="Guid" EnableViewState="true" />
        <label class="control-label">Label Printer<Rock:HelpBlock Text="Choose where to reprint the tag" runat="server" /></label>
                <div class="input-group">
                    <span class="input-group-addon"><i class="fa fa-id-card-o" aria-hidden="true"></i></span>
                    <Rock:RockDropDownList ID="ddlKiosk" runat="server" CssClass="input-xlarge" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" />
                    <span class="input-group-btn">
                        <asp:LinkButton CssClass="btn btn-primary" ID="btnPrint" OnClick="btnPrint_Click" runat="server" Text="Reprint Tag" />
                    </span>
                </div>
                <br />


        
        

    </ContentTemplate>
</asp:UpdatePanel>
