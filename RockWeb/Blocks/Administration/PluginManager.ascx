<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PluginManager.ascx.cs" Inherits="RockWeb.Blocks.Administration.PluginManager" %>

<asp:UpdateProgress id="updateProgress" runat="server">
		<ProgressTemplate>
		    <div id="updateProgress">
                <img src="<%= CurrentTheme %>/Assets/Images/waiting.gif" alt="Loading ..." title="Loading ...">
		    </div>
		</ProgressTemplate>
</asp:UpdateProgress>
<asp:UpdatePanel ID="upRockPackages" runat="server">
<ContentTemplate>
    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />
    <asp:Panel id="pnlPackageList" runat="server" DefaultButton="ibSearch">
    <asp:HiddenField runat="server" ID="hfViewing" />
        <div class="container-fluid">
            <div class="row">
                <div class="span9">
                    <ul class="nav nav-pills">
                        <li runat="server" id="liInstalled"><asp:LinkButton id="btnInstalled" CssClass="btn-large" runat="server" Text="Installed" onclick="btnInstalled_Click" /></li>
                        <li runat="server" id="liAvailable"><asp:LinkButton id="btnAvailable" CssClass="btn-large" runat="server" Text="Available" onclick="btnAvailable_Click" /></li>
                    </ul>
                </div>
                <div class="span3">
                    <asp:TextBox runat="server" ID="txtSearch" class="pull-right search-query" placeholder="SEARCH"></asp:TextBox>
                    <asp:ImageButton ID="ibSearch" runat="server" style="display:none;" OnClick="bSearch_Click" Width="0px" Height="0px" />
                </div>
            </div>
            <div class="row">
                <Rock:Grid ID="gPackageList" runat="server" AllowPaging="false" EmptyDataText="No plugins found"
                AlternatingRowStyle-BackColor="#f3f3f3"
                onrowdatabound="gPackageList_RowDataBound" DataKeyNames="Id,Version" GridLines="none"
                onrowcommand="gPackageList_RowCommand" >
                    <Columns>
                        <asp:ImageField DataImageUrlField="IconUrl" NullImageUrl="http://quarry.rockchms.com/Content/Images/packageDefaultIcon1.png" ItemStyle-VerticalAlign="Top" ItemStyle-Width="80" ItemStyle-Height="80" AlternateText="Plugin Icon"></asp:ImageField>
                        <asp:TemplateField ShowHeader="False" ItemStyle-VerticalAlign="Top" >
                            <ItemTemplate>
                                <h3><asp:LinkButton runat="server" ID="lbView" CommandName="view"><%# Eval("Title") %></asp:LinkButton></h3>
                                <p>by <asp:Literal runat="server" ID="lblAuthors"></asp:Literal></p>
                                <p><asp:Literal runat="server" ID="lblVersion" Visible="false" Text="Version "></asp:Literal>
                                    <asp:Literal runat="server" ID="lblLatestVersion" Visible="false" Text="Latest Version "></asp:Literal>
                                    <asp:Literal runat="server" ID="lblInstalledVersion"  Visible="false" Text="Installed Version "></asp:Literal>
                                </p>
                                <div>
                                    <p><%# Eval("Description") %></p>
                                </div>
                                <asp:LinkButton CssClass="btn" ID="lbCommand" runat="server" />
                                <asp:LinkButton CssClass="btn btn-primary" ID="lbUpdate" CommandName="update" Text="Update" runat="server" /> &nbsp;
                                <a runat="server" id="lProjectUrl" href="#">Project Website</a>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </Rock:Grid>            
            </div>
        </div>
    </asp:Panel>

    <asp:Panel runat="server" ID="pnlPackage" Visible="false" >
        <div class="container-fluid">
            <div class="row">
                <div class="span12">
                    <ul class="nav nav-pills">
                        <li class="active"><asp:LinkButton id="lbBack" CssClass="btn-large" runat="server" Text="Back" onclick="lbBack_Click" /></li>
                    </ul>
                </div>
            </div>
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
                    <p><asp:Literal runat="server" ID="lTags"></asp:Literal></p>

                    <asp:LinkButton CssClass="btn btn-warning" ID="lbPackageUninstall" 
                        Text="<i class='icon-remove'></i> &nbsp; Uninstall" runat="server" 
                        CommandName="uninstall" OnCommand="lbPackageUninstall_Click" />

                    <asp:GridView runat="server" ID="gvPackageVersions" DataKeyNames="Id,Version" GridLines="None" AutoGenerateColumns="false"
                     onrowdatabound="gvPackageVersions_RowDataBound" onrowcommand="gvPackageVersions_RowCommand">
                    <Columns>
                        <asp:BoundField HeaderText="Version" DataField="Version" />
                        <asp:BoundField HeaderText="Last updated" DataField="LastUpdated" DataFormatString="{0:MM/dd/yyyy}" />
                        <asp:TemplateField ShowHeader="False" ItemStyle-VerticalAlign="Top" >
                            <ItemTemplate>
                                <i runat="server" ID="iInstalledIcon" visible="false" class="icon-ok" title="this version is installed"></i>
                                <asp:LinkButton CssClass="btn" ID="lbInstall" CommandName="Install" Text="<i class='icon-download-alt'></i> &nbsp; Install" runat="server" />
                                <asp:LinkButton CssClass="btn btn-primary" ID="lbUpdate" CommandName="Update" Visible="false" Text="<i class='icon-download-alt'></i> &nbsp; Update" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField> 
                    </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>

