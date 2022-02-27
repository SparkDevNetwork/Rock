<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowLauncher.ascx.cs" Inherits="RockWeb.Plugins.org_secc.Workflow.WorkflowLauncher" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title pull-left">Workflow Launcher</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-lg-3">
                        <Rock:WorkflowTypePicker ID="wtpWorkflowType" runat="server" Label="Workflow" Help="Workflow to launch" />
                    </div>
                    <div class="col-lg-3">
                        <Rock:DataDropDownList ID="ddlEntityType" runat="server" Label="Entity Type" Help="Select the type of entity you want to launch workflows for." SourceTypeName="Rock.Model.EntityType, Rock" Required="false" DataTextField="FriendlyName" DataValueField="Name" PropertyName="Name" Visible="true" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                    </div>
                    <div class="col-lg-6" runat="server" visible="false" id="divRegistration">
                        <Rock:DataDropDownList ID="ddlRegistrationInstances" runat="server" Label="Registration (instances)" Help="Select a registration instance to launch a workflow for each existing registration." SourceTypeName="Rock.Model.RegistrationInstance, Rock" Required="false" DataTextField="Name" DataValueField="Id" PropertyName="Name" Visible="true" AutoPostBack="true" OnSelectedIndexChanged="ddlRegistrationInstances_SelectedIndexChanged" />
                        <Rock:DataDropDownList ID="ddlRegistrations" runat="server" Label="Registration (optional)" Help="Select a specific registration to use to launch the workflow." SourceTypeName="Rock.Model.Registration, Rock"  Required="false" DataTextField="EntityStringValue" DataValueField="Id" PropertyName="FirstName" Visible="false" AutoPostBack="true" />
                    </div>
                    <div class="col-lg-6" runat="server" visible="false" id="divGroup">
                        <div class="row">
                            <div class="col-md-3">
                                <Rock:GroupPicker ID="gpGroupPicker" runat="server" Label="Group" Help="Select a group to launch a workflow for each member in the group." OnSelectItem="gpGroupPicker_SelectItem" />
                            </div>
                            <div class="col-md-9">
                                <Rock:GroupMemberPicker ID="gmpGroupMemberPicker" runat="server" Label="Group Member (optional)" Help="Select a specific group member to run this workflow for." Visible="false" />
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-6" runat="server" visible="false" id="divDataView">
                        <div class="row">
                            <div class="col-md-3">
                                <Rock:DataViewPicker ID="dvpDataViewPicker" runat="server" Label="Data View" Help="Select a dataview to launch a workflow for each person." AutoPostBack="true" OnSelectedIndexChanged="dvpDataViewPicker_SelectedIndexChanged" EntityTypeId="15" />
                            </div>
                            <div class="col-md-9">
                                <Rock:DataDropDownList ID="ddlEntities" runat="server" Label="Person (optional)" Help="Select a specific person from the dataview to use to launch the workflow." SourceTypeName="Rock.Model.Person, Rock" Required="false" Visible="false" DataValueField="Id" DataTextField="EntityStringValue" PropertyName="FullName" />
                            </div>
                        </div>
                    </div>
                </div>
                <Rock:BootstrapButton ID="btnLaunch" runat="server" Text="Launch Workflow" CssClass="pull-right btn btn-primary" OnClick="Launch_Click"></Rock:BootstrapButton>
            
                <h3>Output</h3>
                <div class="well" style="background-color: #fff">
                    <asp:Literal ID="litOutput" runat="server"></asp:Literal>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
