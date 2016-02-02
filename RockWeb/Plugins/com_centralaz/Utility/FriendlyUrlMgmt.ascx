<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FriendlyUrlMgmt.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Utility.FriendlyUrlMgmt" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlEdit" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-desktop"></i>Add/Edit a Friendly Url</h1>
            </div>
            <div class="panel-body">
                <h3>Add or Edit a Friendly Url</h3>
                <Rock:RockDropDownList ID="ddlFriendlyUrls" AutoPostBack="true" runat="server" AppendDataBoundItems="True"
                    OnSelectedIndexChanged="ddlFriendlyUrls_SelectedIndexChanged" Label="FriendlyUrl" />
                <asp:Label ID="lblOutput" runat="server"></asp:Label>
                <hr />
                <div class="col-md-6">
                    <Rock:RockTextBox ID="FriendlyUrlName" runat="server" Label="Friendly Url Name:" />
                    <Rock:RockTextBox ID="RedirectDestination" runat="server" Label="Redirect Destination URL:" />
                    <asp:CheckBox ID="ExactDestination" runat="server" Checked="True" Text="Redirect all requests to exact destination (instead of relative to destination)" />
                    <asp:CheckBox ID="ChildOnly" runat="server" Text="Only redirect requests to content in this directory (not subdirectories)" />
                    <Rock:RockDropDownList ID="ddlStatusCode" runat="server" Label="Status Code:">
                        <asp:ListItem Value="Permanent">Permanent (301)</asp:ListItem>
                        <asp:ListItem Value="Found">Found (302)</asp:ListItem>
                        <asp:ListItem Value="Temporary">Temporary (307)</asp:ListItem>
                    </Rock:RockDropDownList>

                    <div class="actions">
                        <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" CssClass="btn btn-primary" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" CssClass="btn btn-default" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" OnClientClick="return confirm('Are you sure you want to delete the selected Friendly URL?')"
                            OnClick="btnDelete_Click" />
                    </div>
                </div>
                <asp:Label ID="lblProcessReport" runat="server" Text=""></asp:Label>

            </div>
        </asp:Panel>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Friendly Url List</h1>

            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gVirtualDirectory" runat="server" RowItemText="Group" AllowSorting="true">
                        <Columns>
                            <asp:BoundField DataField="FriendlyURL" HeaderText="Friendly URL" SortExpression="FriendlyURL" />
                            <%--<asp:HyperLinkField DataTextField="Destination" HeaderText="Destination" SortExpression="Destination"
                                DataNavigateUrlFields="Destination" DataNavigateUrlFormatString="{0}" />--%>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
