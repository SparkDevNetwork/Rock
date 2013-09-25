<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PluginManager.ascx.cs" Inherits="RockWeb.Blocks.Administration.PluginManager" %>

<asp:UpdateProgress id="updateProgress" runat="server">
		<ProgressTemplate>
		    <div id="updateProgress" class="modal-backdrop"> <i class="icon-spinner icon-4x icon-spin" style="color: white;"></i> 
		    </div>
		</ProgressTemplate>
</asp:UpdateProgress>

<asp:UpdatePanel ID="upRockPackages" runat="server">
<ContentTemplate>
    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />
    <asp:Panel id="pnlPackageList" runat="server" DefaultButton="ibSearch">
        <div class="container">
            <div class="row">
                <div class="col-md-9">
                    <ul class="nav nav-pills">
                        <li runat="server" id="liInstalled"><asp:LinkButton id="btnInstalled" runat="server" Text="Installed" onclick="btnInstalled_Click" OnClientClick="$(this).button('loading')" /></li>
                        <li runat="server" id="liAvailable"><asp:LinkButton id="btnAvailable" runat="server" Text="Available" onclick="btnAvailable_Click" OnClientClick="$(this).button('loading')" /></li>
                    </ul>
                </div>
                <div class="col-md-3">
                    <asp:TextBox runat="server" ID="txtSearch" class="form-control pull-right" placeholder="SEARCH"></asp:TextBox>
                    <asp:ImageButton ID="ibSearch" runat="server" style="display:none;" OnClick="bSearch_Click" Width="0px" Height="0px" />
                </div>
            </div>
            <div class="row">
                <Rock:Grid ID="gPackageList" runat="server" DisplayType="Light" AllowPaging="false" EmptyDataText="No plugins found"
                AlternatingRowStyle-BackColor="#f3f3f3" OnGridRebind="gPackageList_GridRebind"  
                onrowdatabound="gPackageList_RowDataBound" DataKeyNames="Id,Version" GridLines="none"
                onrowcommand="gPackageList_RowCommand" >
                    <Columns>
                        <asp:TemplateField ItemStyle-VerticalAlign="Top">
                            <ItemTemplate>
                                <ul class="list-unstyled">
                                    <li><asp:Image runat="server" ID="imgIconUrl" alt="Plugin Icon" width="80" height="80" ImageUrl="http://quarry.rockchms.com/Content/Images/packageDefaultIcon1.png"  /></li>
                                    <li><a runat="server" id="lProjectUrl" href="#">Project Site</a></li>
                                </ul>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ShowHeader="False" ItemStyle-VerticalAlign="Top" >
                            <ItemTemplate>
                                <h3><asp:LinkButton runat="server" ID="lbView" CommandName="view"><%# Eval("Title") %></asp:LinkButton></h3>
                                <p>by <asp:Literal runat="server" ID="lblAuthors"></asp:Literal></p>
                                <p>
                                    <asp:Literal runat="server" ID="lblItemError"></asp:Literal>
                                    <asp:Literal runat="server" ID="lblVersion" Visible="false" Text="Version "></asp:Literal>
                                    <asp:Literal runat="server" ID="lblLatestVersion" Visible="false" Text="Latest Version "></asp:Literal>
                                    <asp:Literal runat="server" ID="lblInstalledVersion"  Visible="false" Text="Installed Version "></asp:Literal>
                                </p>
                                <div>
                                    <p><%# Eval("Summary") %></p>
                                </div>
                                <asp:LinkButton CssClass="btn btn-default" ID="lbCommand" runat="server" />
                                <asp:LinkButton CssClass="btn btn-primary" ID="lbUpdate" CommandName="update" Text="Update" runat="server" /> &nbsp;
                                
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </Rock:Grid>            
            </div>
        </div>
    </asp:Panel>

    <asp:Panel runat="server" ID="pnlPackage" Visible="false" >
        <div class="container">
            <div class="row">
                <div class="span2">
                    <asp:Image runat="server" ID="imgIcon" alt="Plugin Icon" width="128" height="128" />
                </div>
                <div class="span10">
                    <h3><asp:Literal runat="server" ID="lTitle"></asp:Literal></h3>
                    <p>by <asp:Literal runat="server" ID="lAuthors"></asp:Literal></p>
                    
                    <h5>Description</h5>
                    <p><asp:Literal runat="server" ID="lDescription"></asp:Literal></p>

                    <h5>Dependencies</h5>
                    <p><asp:Literal runat="server" ID="lDependencies"></asp:Literal></p>
                    <h5>Tags</h5>
                    <p><asp:Literal runat="server" ID="lTags"><i>none</i></asp:Literal></p>

                    <asp:LinkButton CssClass="btn btn-warning" ID="lbPackageUninstall" 
                        Text="<i class='icon-remove'></i> &nbsp; Uninstall" runat="server" 
                        CommandName="uninstall" OnCommand="lbPackageUninstall_Click" OnClientClick="$(this).button('loading')" data-loading-text="Uninstalling..."/>
 
                    <Rock:Grid runat="server" DisplayType="Light" ID="gvPackageVersions" DataKeyNames="Id,Version" GridLines="None" AutoGenerateColumns="false"
                     onrowdatabound="gvPackageVersions_RowDataBound" onrowcommand="gvPackageVersions_RowCommand">
                    <Columns>
                        <asp:BoundField HeaderText="Version" DataField="Version" />
                        <asp:BoundField HeaderText="Last updated" DataField="LastUpdated" DataFormatString="{0:MM/dd/yyyy}" />
                        <asp:TemplateField ShowHeader="False" ItemStyle-VerticalAlign="Top" >
                            <ItemTemplate>
                                <i runat="server" ID="iInstalledIcon" visible="false" class="icon-ok" title="this version is installed"></i>
                                <asp:LinkButton CssClass="btn btn-default" ID="lbInstall" CommandName="Install" Text="<i class='icon-download-alt'></i> &nbsp; Install" OnClientClick="$(this).button('loading')" data-loading-text="Installing..." runat="server" />
                                <asp:LinkButton CssClass="btn btn-primary" ID="lbUpdate" CommandName="Update" Visible="false" Text="<i class='icon-download-alt'></i> &nbsp; Update" OnClientClick="$(this).button('loading')" data-loading-text="Updating..." runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField> 
                    </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>

