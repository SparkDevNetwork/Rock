<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MagnusSettings.ascx.cs" Inherits="RockWeb.Plugins.tech_Triumph.Magnus.MagnusSettings" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <style>
            .magnus-header{
                height: 200px;
                background: url('/Plugins/tech_triumph/Magnus/Assets/Images/magnus-panel-header.svg');
                background-repeat: no-repeat;
                background-size: cover;
                background-position: center;
            }

            @media(min-width:768px) {
                .magnus-header {
                    height: 300px;
                }
            }
        </style>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-pencil-ruler"></i> Magnus Settings</h1>

                <div class="panel-labels">
                    <asp:Literal ID="lHeaderLabels" runat="server" />
                </div>
            </div>

            <div class="magnus-header"></div>

            <div class="panel-body">
                <h2 class="mt-3">Editor Configuration</h2>
                <hr />

                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lDetailsLeft" runat="server" />
                    </div>

                    <div class="col-md-6">
                        <asp:Literal ID="lDetailsRight" runat="server" />
                        <asp:LinkButton ID="btnAddCurrentIp" runat="server" CssClass="btn btn-xs btn-default" Text="Add Current IP Address" OnClick="btnAddCurrentIp_Click" />
                    </div>

                </div>

                <div class="text-center">
                    <small>Made with <i class="fa fa-heart" style="color: #fb866a;"></i> by <a href="https://www.triumph.tech?utm_source=magnus-plugin">Triumph Tech</a> for our fellow Rockitects</a></small>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                </div>

            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-pencil-ruler"></i> Remote Edit Settings</h1>
            </div>
            <div class="magnus-header"></div>
            <div class="panel-body">
                <h2 class="mt-3">Edit Configuration</h2>
                <hr />

                <div class="row">
                    <div class="col-md-6">                        
                        <Rock:RockDropDownList ID="ddlSecurityRole" runat="server" Label="Security Role" Required="true" CssClass="input-width-xxl" DataTextField="Name" DataValueField="Id" EnhanceForLongLists="true" Help="The security role a person must belong to to use the plugin." />

                        <Rock:RockCheckBoxList ID="cblEnabledVirtualFilesystems" runat="server" Label="Enabled Virtual Filesystems" RepeatColumns="1" Help="Determines which virtual filesystems are enabled." />

                        <Rock:RockCheckBoxList ID="cblAllowedDirectories" runat="server" Label="Allowed Physical Directories" RepeatColumns="1" Help="Determines which physical directories on your server that allowed to be edited." />
                    </div>
                    <div class="col-md-6">
                        <Rock:KeyValueList ID="kvlIpAddressSubnets" runat="server" Required="true" Label="Allowed IP Subnets" Help="List of IP subnets that are allowed to access the remote share." KeyPrompt="Description" ValuePrompt="IP Configuration"  />

                        <div class="well">
                            <h5 class="mt-0">IP Filter Tips</h5>
                            <p class="small">
                                To maximize security, you will need to define which IP addresses or subnets are allowed to use the Remote Edit plugin. These filters accept several
                                different formats. Let’s look at the most common:
                            </p>

                            <ul class="small">
                                <li>Single IP Address – 192.168.100.12</li>
                                <li>IP Address Range – 192.168.100.1 – 192.168.100-255</li>
                                <li>Shortcut Range – 192.168.100.23-45</li>
                                <li>Subnet Mask – 192.168.100.1/255.255.255.0</li>
                                <li>CIDR Expression – 192.168.100.1/24</li>
                                <li>IPv6 - fe80::/10</li>
                            </ul>

                            <p class="small mb-0">
                                Your current IP address is: <asp:Literal ID="lCurrentIpAddress" runat="server" />
                            </p>
                        </div>

                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="mbAddIpAddress" runat="server" Title="Add Current IP Address" SaveButtonText="Save" OnSaveClick="mbAddIpAddress_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbLocationName" runat="server" Label="Location Name" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lCurrentIp" runat="server" Label="Current IP Address" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
