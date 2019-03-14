<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LavaTester.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Utility.LavaTester" %>

<script>

    function pageLoad()
    {
        EnableTooltips();
    }

    function EnableTooltips()
    {
        $(document).ready(function ()
        {
            $('[data-toggle="tooltip"]').tooltip();
        });
    }

</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Lava Tester <small><a href="http://rockrms.com/Lava" target="_blank">view Lava documentation</a></small></h1>
                <div class="panel-labels">
                    <Rock:RockCheckBox ID="cbEnableDebug" runat="server" Checked="false" Text="Enable debug?" CssClass="pull-right" Help="If enabled, extra Lava debug information will be included at the bottom but it will slow down quite a bit. You can also use the <a href='http://www.churchitnetwork.com/ModelMap'>ModelMap block</a> to see which properties exist on various entities."/>
                </div>
            </div>
            <div class="panel-body form-group">
                <fieldset>

                    <Rock:NotificationBox ID="nbInstructions" runat="server" Title="Instructions" Text="Select one of the entities below, type some lava, and press the Test button." NotificationBoxType="Info" Dismissable="true" />

                    <div class="row">
                        <div class="col-md-3">
                            <Rock:PersonPicker ID="ppPerson" OnSelectPerson="ppPerson_SelectPerson" Label="Person" runat="server" Help="The item you choose will be set to a Lava object called 'Person' If nothing is selected, the Current Person will be put in the 'Person' object."/>
                        </div>
                        <div class="col-md-9">
                            <Rock:GroupPicker ID="gpGroups" OnSelectItem="gpGroups_SelectItem" runat="server" Label="Group" Help="The item you choose will be set to a Lava object called 'Group'."/>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <div class="row">
                                <div class="col-md-3"><Rock:WorkflowTypePicker ID="wfpWorkflowType" runat="server" Label="Workflow Type" OnSelectItem="wfpWorkflowType_SelectItem" /></div>
                                <div class="col-md-4"><Rock:DataDropDownList ID="ddlWorkflows" runat="server" EnhanceForLongLists="true" Label="Workflow (instances)" Help="The item you choose will be set to a Lava object called 'Workflow'." SourceTypeName="Rock.Model.Workflow, Rock" DataTextField="Name" DataValueField="Id" PropertyName="Name" Visible="false" OnSelectedIndexChanged="ddlWorkflows_SelectedIndexChanged" AutoPostBack="true" /><Rock:RockCheckBox ID="cbIncludeInactive" runat="server" Checked="false" Text="Include inactive?" Help="If checked, inactive workflow instances will be included." AutoPostBack="true" OnCheckedChanged="cbIncludeInactive_CheckedChanged"/></div>
                                <div class="col-md-4"><Rock:DataDropDownList ID="ddlWorkflowActivities" runat="server" EnhanceForLongLists="true" Label="Activity" Help="The item you choose will be set to a Lava object called 'Activity'." SourceTypeName="Rock.Model.WorkflowActivity, Rock" DataTextField="Name" DataValueField="Id" PropertyName="Id" Visible="false" OnSelectedIndexChanged="ddlWorkflowActivities_SelectedIndexChanged" AutoPostBack="true" CausesValidation="false" /></div>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <div class="row">
                                <div class="col-md-6"><Rock:DataDropDownList ID="ddlRegistrationInstances" runat="server" EnhanceForLongLists="true" Label="Registration (instances)" Help="The item you choose will be set to a Lava object called 'RegistrationInstance'." SourceTypeName="Rock.Model.RegistrationInstance, Rock" Required="false" DataTextField="Name" DataValueField="Id" PropertyName="Name" Visible="true" AutoPostBack="true" OnSelectedIndexChanged="ddlRegistrationInstances_SelectedIndexChanged" /></div>
                                <div class="col-md-6"><Rock:DataDropDownList ID="ddlRegistrations" runat="server" EnhanceForLongLists="true" Label="Registration" Help="The item you choose will be set to a Lava object called 'Registration'." SourceTypeName="Rock.Model.Registration, Rock"  Required="false" DataTextField="FirstName" DataValueField="Id" PropertyName="FirstName" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="ddlRegistrations_SelectedIndexChanged" /></div>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceLava" runat="server" Label="Lava" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300" Placeholder="test" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-2"></div>
                        <div class="col-md-2 text-right">
                            <asp:LinkButton data-toggle="tooltip" data-html="true" title="Loads your saved Lava from the selected slot into the editor." ID="lbLoadLava" runat="server" CssClass="btn btn-sm btn-default pull-right" Text="Load" OnClick="lbLoadLava_Click"></asp:LinkButton>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlSaveSlot" runat="server" CssClass="form-control input-sm" EnhanceForLongLists="true">
                                <asp:ListItem Text="empty save slot 1" Value="0"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 2" Value="1"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 3" Value="2"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 4" Value="3"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 5" Value="4"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 6" Value="5"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 7" Value="6"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 8" Value="7"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 9" Value="8"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 10" Value="9"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 11" Value="10"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 12" Value="11"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 13" Value="12"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 14" Value="13"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 15" Value="14"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 16" Value="15"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 17" Value="16"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 18" Value="17"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 19" Value="18"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 20" Value="19"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 21" Value="20"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 22" Value="21"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 23" Value="22"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 24" Value="23"></asp:ListItem>
                                <asp:ListItem Text="empty save slot 25" Value="24"></asp:ListItem>
                            </Rock:RockDropDownList>
                        </div>
                        <div class="col-md-1">
                            <asp:LinkButton ID="lbSave" data-toggle="tooltip" data-placement="top" data-html="true" title="<p>Saves the Lava in the editor into the selected save slot.</p> <p><i>NOTE: If you put an HTML comment on the first line it will be used for the slot name.</i></p>" runat="server" CssClass="btn btn-sm btn-default pull-left" Text="Save Lava" OnClick="lbSave_Click"></asp:LinkButton>
                        </div>
                        <div class="col-md-1">
                            <Rock:NumberBox ID="nbHeight" runat="server" CssClass="input-sm pull-right"  Placeholder="editor height" AutoPostBack="true" OnTextChanged="nbHeight_TextChanged"  data-toggle="tooltip" data-placement="top" data-html="true" title="Used to change the height of the editor" />
                        </div>
                    </div>

                    <div class="actions">
                        <Rock:BootstrapButton ID="bbTest" runat="server" CssClass="btn btn-primary" OnClick="bbTest_Click">Test</Rock:BootstrapButton>
                    </div>

                    <h3>Output</h3>
                    <div class="well" style="background-color: #fff">
                        <asp:Literal ID="litOutput" runat="server"><i class="text-muted">nothing to display yet, try pressing the Test button ;)</i></asp:Literal>
                    </div>

                    <h3 runat="server" id="h3DebugTitle" visible="false">Lava Reference / Debug</h3>
                    <asp:Literal ID="litDebug" runat="server"></asp:Literal>
                </fieldset>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
