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
                            <Rock:RockCheckBox ID="cbEnableDebug" runat="server" Checked="false" Label="Enable Debug?" />
                            <Rock:PersonPicker ID="ppPerson" Label="Person" runat="server" />
                            <Rock:GroupPicker ID="gpGroups" runat="server" Label="Group"/>
                            <div class="row">
                                <div class="col-md-3"><Rock:WorkflowTypePicker ID="wfpWorkflowType" runat="server" Label="Workflow Type" OnSelectItem="wfpWorkflowType_SelectItem" /></div>
                                <div class="col-md-9"><Rock:DataDropDownList ID="ddlWorkflows" runat="server" Label="Workflow (instances)" SourceTypeName="Rock.Model.Workflow, Rock" DataTextField="Name" DataValueField="Id" PropertyName="Name" Visible="false" OnSelectedIndexChanged="ddlWorkflows_SelectedIndexChanged" /></div>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceLava" runat="server" Label="Lava" EditorMode="Lava" EditorTheme="Rock" EditorHeight="150" AutoCompleteType="Email" />
                        </div>
                    </div>

                    <div class="actions">
                        <Rock:BootstrapButton ID="bbTest" runat="server" CssClass="btn btn-primary" OnClick="bbTest_Click">Test</Rock:BootstrapButton>
                    </div>

                    <h2>Output</h2>
                    <div class="alert alert-info">
                        <asp:Literal ID="litOutput" runat="server"></asp:Literal>
                    </div>

                    <h3 runat="server" id="h3DebugTitle" visible="false">Lava Reference / Debug</h3>
                    <asp:Literal ID="litDebug" runat="server"></asp:Literal>
                </fieldset>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
