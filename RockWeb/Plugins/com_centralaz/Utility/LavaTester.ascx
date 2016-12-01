<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LavaTester.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Utility.LavaTester" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Lava Tester</h1>
                <div class="panel-labels">
                    <Rock:RockCheckBox ID="cbEnableDebug" runat="server" Checked="false" Text="Enable Debug?" CssClass="pull-right" Help="If enabled, extra Lava debug information will be included at the bottom but it will slow down tremendously. You can also use the <a href='http://www.churchitnetwork.com/ModelMap'>ModelMap block</a> to see which properties exist on various entities."/>
                </div>
            </div>
            <div class="panel-body form-group">
                <fieldset>

                    <Rock:NotificationBox ID="nbInstructions" runat="server" Title="Instructions" Text="Select one of the entities below, type some lava, and press the Test button." NotificationBoxType="Info" Dismissable="true" />

                    <div class="row">
                        <div class="col-md-3">
                            <Rock:PersonPicker ID="ppPerson" Label="Person" runat="server" />
                        </div>
                        <div class="col-md-9">
                            <Rock:GroupPicker ID="gpGroups" runat="server" Label="Group"/>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <div class="row">
                                <div class="col-md-3"><Rock:WorkflowTypePicker ID="wfpWorkflowType" runat="server" Label="Workflow Type" OnSelectItem="wfpWorkflowType_SelectItem" /></div>
                                <div class="col-md-9"><Rock:DataDropDownList ID="ddlWorkflows" runat="server" Label="Workflow (instances)" SourceTypeName="Rock.Model.Workflow, Rock" DataTextField="Name" DataValueField="Id" PropertyName="Name" Visible="false" OnSelectedIndexChanged="ddlWorkflows_SelectedIndexChanged" /></div>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceLava" runat="server" Label="Lava" EditorMode="Lava" EditorTheme="Rock" EditorHeight="150" Placeholder="test" />
                        </div>
                    </div>

                    <div class="actions">
                        <Rock:BootstrapButton ID="bbTest" runat="server" CssClass="btn btn-primary" OnClick="bbTest_Click">Test</Rock:BootstrapButton>
                    </div>

                    <h3>Output</h3>
                    <div class="well" style="background-color: #fff">
                        <asp:Literal ID="litOutput" runat="server"></asp:Literal>
                    </div>

                    <h3 runat="server" id="h3DebugTitle" visible="false">Lava Reference / Debug</h3>
                    <asp:Literal ID="litDebug" runat="server"></asp:Literal>
                </fieldset>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
