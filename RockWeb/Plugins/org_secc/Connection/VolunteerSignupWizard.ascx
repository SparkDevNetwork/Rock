<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VolunteerSignupWizard.ascx.cs" Inherits="org.secc.Connection.VolunteerSignupWizard" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

       <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />
        
        <asp:Literal ID="lBody" runat="server"></asp:Literal> 
        <asp:Literal ID="lDebug" runat="server" Visible="false"></asp:Literal>
        <asp:PlaceHolder ID="phTimings" runat="server"></asp:PlaceHolder>

        
        <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="mdEdit_SaveClick" Title="Edit Signup Settings">
            <Content>
                <asp:UpdatePanel ID="upEditControls" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <style>
                            .panel.panel-block {
                                border: 1px solid #6aa3d5;
                            }
                            .panel.panel-block > .panel-heading {
                                display: block;
                            }
                            .panel.panel-block > .panel-body {
                                padding: 15px;
                            }
                        </style>
                        <div class="container-fluid">
                            <div style="margin-bottom: 10px">
                                <ul class="nav nav-pills">
                                    <li class="active" id="liSettings" runat="server">
                                        <asp:LinkButton ID="lbSettings" runat="server" Text="Settings" OnClick="lbSettings_Click"></asp:LinkButton>
                                    </li>
                                    <li id="liLava" runat="server">
                                        <asp:LinkButton ID="lbLava" runat="server" Text="Lava" OnClick="lbLava_Click"></asp:LinkButton>
                                    </li>
                                    <li id="liCounts" runat="server">
                                        <asp:LinkButton ID="lbCounts" runat="server" Text="Totals" OnClick="lbCounts_Click"></asp:LinkButton>
                                    </li>
                                </ul>
                            </div>
                            
                            <asp:Panel id="pnlSettings" runat="server">
                                <div class="panel panel-block" >
                                    <div class="panel-heading">
                                        <h1 class="panel-title">Sign-up Settings</h1>
                                    </div>
                                    <div class="panel-body">
                                        <div class="row">
                                            <div class="col-md-3">
                                                <Rock:PagePicker ID="ppSignupPage" runat="server" Required="true" Label="Connections Signup Page" Help='The page the user should be sent to when they click the "Signup" button' OnSelectItem="ppSignupPage_SelectItem" />
                                            </div>
                                            <div class="col-md-4">
                                                <Rock:RockDropDownList ID="rddlType" Label="Select the type of Volunteer Signup" runat="server" OnSelectedIndexChanged="RddlType_SelectedIndexChanged" AutoPostBack="true">
                                                    <asp:ListItem Value="">Select One . . .</asp:ListItem>
                                                    <asp:ListItem Value="Connection">Connection</asp:ListItem>
                                                </Rock:RockDropDownList>
                                            </div>
                                            <div class="col-md-5">
                                                <Rock:RockDropDownList ID="rddlConnection" Label="Select the Connection Opportunity" runat="server" Visible="false" OnSelectedIndexChanged="ConnectionRddl_SelectedIndexChanged" AutoPostBack="true" />
                                                <Rock:GroupPicker ID="gpGroup" Label="Select the Group" runat="server" Visible="false" OnSelectItem="GPicker_SelectItem" />
                                            </div>
                                        </div>
                                        <asp:PlaceHolder ID="phEditControls" runat="server" EnableViewState="false" />
                                    </div>
                                </div>
                                <asp:HiddenField ID="hdnSettings" runat="server" />
                                <asp:Panel class="panel panel-block show-heading" id="pnlPartitions" runat="server" visible="false" EnableViewState="false">
                                    <div class="panel-heading">
                                        <h1 class="panel-title">Partitions</h1>
                                        <div class="pull-right" style="margin: -8px -10px">
                                            <asp:LinkButton ID="btnAddPartition" runat="server" CssClass="hidden" OnClick="btnAddPartition_Click" />
                                            <asp:HiddenField ID="hdnPartitionType" runat="server" />
                                            <asp:DropDownList ID="bddlAddPartition" OnSelectedIndexChanged="BddlAddPartition_SelectionChanged" runat="server" AutoPostBack="true" CssClass="form-control">
                                                <asp:ListItem Value="">New Partition</asp:ListItem>
                                                <asp:ListItem Value="Campus">Campus</asp:ListItem>
                                                <asp:ListItem Value="DefinedType">Defined Type</asp:ListItem>
                                                <asp:ListItem Value="Schedule">Schedule</asp:ListItem>
                                                <asp:ListItem Value="Role">Group Type Role</asp:ListItem>
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="panel-body">
                                        <asp:Repeater ID="rptPartions" runat="server" OnItemDataBound="rptPartions_ItemDataBound">
                                            <ItemTemplate>
                                                <div class="row">
                                                    <div class="col-sm-8 col-xs-10">
                                                        <strong><%# Eval("PartitionType") %></strong><br />
                                                        <asp:PlaceHolder ID="phPartitionControl" runat="server">
                                                    </asp:PlaceHolder></div>
                                                    <div class="col-sm-3 col-xs-10"><Rock:RockTextBox ID="tbAttributeKey" runat="server" Placeholder="Parameter Name" Label="Parameter Name" Help="This is used to name the QueryString URL parameter which is used for the end-user's selection." OnTextChanged="tbAttributeKey_TextChanged" AutoPostBack="true" /></div>
                                                    <div class="col-sm-1 col-xs-2"><asp:LinkButton ID="bbPartitionDelete" runat="server" OnClick="bbPartitionDelete_Click" ToolTip="Remove" CssClass="btn btn-danger" OnClientClick="Rock.dialogs.confirmPreventOnCancel(event, 'Making changes to partition settings will clear all existing counts!  Are you sure you want to proceed?');"><i class="fa fa-remove"></i></asp:LinkButton></div>
                                                </div>
                                            </ItemTemplate>
                                            <SeparatorTemplate>
                                                <hr />
                                            </SeparatorTemplate>
                                        </asp:Repeater>
                                        <asp:PlaceHolder ID="phPartitionControls" runat="server" EnableViewState="false" />
                                    </div>
                                </asp:Panel>
                            </asp:Panel>
                            <asp:Panel class="panel panel-block" id="pnlCounts" runat="server" visible="false">
                                <div class="panel-heading">
                                    <h1 class="panel-title">Volunteers Needed</h1>
                                </div>
                                <div class="panel-body">
                                    <style>
                                        div[id$=pnlCounts] .grid-filter { min-height: 32px; }
                                        div[id$=pnlCounts] .grid-actions { display: none; }
                                    </style>
                                    <Rock:GridFilter ID="gFilter" runat="server" OnApplyFilterClick="gFilter_ApplyFilterClick" OnClearFilterClick="gFilter_ClearFilterClick">
                                        
                                    </Rock:GridFilter>
                                    <Rock:Grid ID="gCounts" runat="server" AllowPaging="false" DataKeyNames="RowId" AllowSorting="false" ShowActionRow="false" OnRowDataBound="gCounts_RowDataBound" ViewStateMode="Disabled" EnableResponsiveTable="false" ShowActionsInHeader="false">
                                        <Columns>
                                            <asp:TemplateField HeaderText="Group" ItemStyle-Width="250px"></asp:TemplateField>
                                            <asp:TemplateField HeaderText="Total" ItemStyle-Width="75px"></asp:TemplateField>
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </asp:Panel>
                            <asp:Panel class="panel panel-block" id="pnlLava" runat="server" visible="false">
                                <div class="panel-heading">
                                    <h1 class="panel-title">Display Lava</h1>
                                </div>
                                <div class="panel-body">
                                    <Rock:CodeEditor ID="ceLava" runat="server" EditorMode="Lava" Rows="100" EditorHeight="400"></Rock:CodeEditor>
                                </div>
                            </asp:Panel>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
