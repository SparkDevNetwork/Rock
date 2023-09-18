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
                            <Rock:PersonField DataField="Person" HeaderText="Person" SortExpression="Person.LastName,Person.NickName" />
                            <Rock:RockBoundField DataField="ClientType" HeaderText="Client Type" SortExpression="ClientType" />
                            <Rock:RockBoundField DataField="OperatingSystem" HeaderText="Operating System" SortExpression="OperatingSystem" />
                            <Rock:RockBoundField DataField="Application" HeaderText="Browser" SortExpression="Application" />
                            <Rock:RockBoundField DataField="Source" HeaderText="UTM Source" SortExpression="Source" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
            
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
