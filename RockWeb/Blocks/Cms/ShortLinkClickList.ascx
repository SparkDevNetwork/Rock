<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShortLinkClickList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ShortLinkClickList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <asp:HiddenField runat="server" ID="hfShortLinkId" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-link"></i> Clicks</h1>
            </div>
            <div class="panel-body">
            
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                
                <div class="grid grid-panel">
                    <Rock:Grid ID="gShortLinkClicks" runat="server" DisplayType="Full">
                        <Columns>
                            <Rock:DateTimeField DataField="InteractionDateTime" HeaderText="Date / Time" SortExpression="InteractionDateTime" />
                            <Rock:PersonField DataField="PersonAlias.Person" HeaderText="Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                            <Rock:RockBoundField DataField="InteractionSession.DeviceType.ClientType" HeaderText="Client Type" SortExpression="InteractionSession.DeviceType.ClientType" />
                            <Rock:RockBoundField DataField="InteractionSession.DeviceType.OperatingSystem" HeaderText="Operating System" SortExpression="InteractionSession.DeviceType.OperatingSystem" />
                            <Rock:RockBoundField DataField="InteractionSession.DeviceType.Application" HeaderText="Browswer" SortExpression="InteractionSession.DeviceType.Application" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
            
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
