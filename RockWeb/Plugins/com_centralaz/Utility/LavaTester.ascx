<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LavaTester.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Utility.LavaTester" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Lava Tester</h1>
            </div>
            <div class="panel-body form-group">
                <fieldset>
                    <div class="row">
                        <div class="col-md-12">
                            <h2>Entity <small>(currently only supports Person and Group)</small></h2>
                            <Rock:PersonPicker ID="ppPerson" Label="Person" runat="server" />
                            <Rock:GroupPicker ID="gpGroups" runat="server" Label="Group"/>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceLava" runat="server" Label="Lava" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300" AutoCompleteType="Email" />
                        </div>
                    </div>

                    <div class="actions">
                        <Rock:BootstrapButton ID="bbTest" runat="server" CssClass="btn btn-primary" OnClick="bbTest_Click">Test</Rock:BootstrapButton>
                    </div>

                    <h2>Output</h2>
                    <div class="alert alert-info">
                        <asp:Literal ID="litOutput" runat="server"></asp:Literal>
                    </div>

                    <h3>Lava Reference</h3>
                    <asp:Literal ID="litDebug" runat="server"></asp:Literal>
                </fieldset>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
