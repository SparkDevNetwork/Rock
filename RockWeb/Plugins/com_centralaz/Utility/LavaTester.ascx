<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LavaTester.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Utility.LavaTester" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i>Lava Tester</h1>
            </div>
            <div class="panel-body form-group">
                <fieldset>
                    <div class="row">
                        <div class="col-md-12">
                            <h2>Entity (only supports Person for now)</h2>

                            <Rock:PersonPicker ID="ppPerson" runat="server" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockTextBox ID="txtLava" runat="server" TextMode="MultiLine" Rows="10"></Rock:RockTextBox>
                        </div>
                    </div>

                    <div class="actions">
                        <Rock:BootstrapButton ID="bbTest" runat="server" CssClass="btn btn-primary" OnClick="bbTest_Click">Test</Rock:BootstrapButton>
                    </div>

                    <h2>Output</h2>
                    <code>
                        <asp:Literal ID="litOutput" runat="server"></asp:Literal>
                    </code>
                </fieldset>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
