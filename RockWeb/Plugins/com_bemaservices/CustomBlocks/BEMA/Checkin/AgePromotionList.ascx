<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AgePromotionList.ascx.cs" Inherits="RockWeb.com_bemaservices.CheckIn.AgePromotionList" %>
<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function ()
    {
        var proxy = $.connection.rockMessageHub;

        proxy.client.receiveNotification = function (name, message, results)
        {
            if (name == '<%=this.SignalRNotificationKey %>') {
                $('#<%=pnlProgress.ClientID%>').show();

                if (message) {
                    $('#<%=lProgressMessage.ClientID %>').html(message);
                }

                if (results) {
                    $('#<%=lProgressResults.ClientID %>').html(results);
                }
            }
        }

        proxy.client.showButtons = function (name, visible)
        {
            if (name == '<%=this.SignalRNotificationKey %>') {

                if (visible) {
                    $('#<%=pnlActions.ClientID%>').show();
                }
                else {
                    $('#<%=pnlActions.ClientID%>').hide();
                }
            }
        }

        $.connection.hub.start().done(function ()
        {
            // hub started... do stuff here if you want to let the user know something
        });
    })
</script>

<asp:UpdatePanel ID="upnlGroupList" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i runat="server" id="iIcon"></i> <asp:Literal ID="lTitle" runat="server" Text="Group List" /></h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                        <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Label="Group Type" />
                        <Rock:RockDropDownList ID="ddlGroupTypePurpose" runat="server" Label="Group Type Purpose"/>
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                            <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                            <asp:ListItem Text="Active" Value="active"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="inactive"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gGroups" runat="server" RowItemText="Group" AllowSorting="true" OnRowSelected="gGroups_Edit" CssClass="js-grid-group-list" OnRowDataBound="gGroups_RowDataBound">
                        <Columns>
                            <Rock:SelectField></Rock:SelectField>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockTemplateField HeaderText="Name" SortExpression="Name" Visible="false">
                                <ItemTemplate>
                                    <%# Eval("Name") %><br /><small><%# Eval("Path") %></small>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="GroupTypeName" HeaderText="Group Type" SortExpression="GroupTypeName"/>
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" ColumnPriority="Desktop" SortExpression="Description" />
                            <Rock:RockBoundField DataField="GroupRole" HeaderText="Role" SortExpression="Role" />
                            <Rock:RockBoundField DataField="ActiveMemberCount" HeaderText="Active Members" SortExpression="ActiveMemberCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="InactiveMemberCount" HeaderText="Inactive Members" SortExpression="InactiveMemberCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:DateTimeField DataField="DateAdded" HeaderText="Added" SortExpression="DateAdded" FormatAsElapsedTime="true" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActiveOrder" />
                            <Rock:RockBoundField DataField="StartDate" HeaderText="Start date" SortExpression="StartDate" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <Rock:RockBoundField DataField="EndDate" HeaderText="End date" SortExpression="EndDate" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                        </Columns>
                    </Rock:Grid>

                    <asp:Panel ID="pnlActions" runat="server" CssClass="actions">
                    <div class="row">
                        <div class="col-md-12">
                            <div class="actions">
                                <asp:LinkButton ID="btnPromote" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Promote Members in Selected Groups" CssClass="btn btn-primary" OnClick="btnPromote_Click" />
                                <asp:LinkButton ID="btnUndo" runat="server" AccessKey="z" ToolTip="Alt+z" Text="Undo Last Promotion" CssClass="btn btn-cancel" OnClick="btnUndo_Click" />
                             </div>
                        </div>
                    </div>
                    <div>&nbsp;</div>
                    <div class="row">
                        <div class="col-md-12">
                             <div class="actions">
                                <asp:LinkButton ID="btnBackup" runat="server"  Text="Backup Selected Groups" CssClass="btn btn-primary" OnClick="btnBackup_Click" />
                                <asp:LinkButton ID="btnPurgeBackup" runat="server" Text="Purge Backup Groups" CssClass="btn btn-cancel" OnClick="btnPurge_Click" />
                           </div>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlProgress" runat="server" CssClass="js-messageContainer" Visible="false"> <%-- // Style="display: none"> --%>
                    <strong>Progress</strong></br>
                    <div class="alert alert-info">
                        <asp:Label ID="lProgressMessage" CssClass="js-progressMessage" runat="server" />
                    </div>

                    <strong>Details</strong><br />
                    <div class="alert alert-info">
                        <pre><asp:Label ID="lProgressResults" CssClass="js-progressResults" runat="server" /></pre>
                    </div>
                    <asp:LinkButton ID="btnDone" runat="server" AccessKey="d" ToolTip="Alt+d" Text="Done" CssClass="btn btn-primary" OnClick="btnDone_Click" />
                </asp:Panel>

                </div>

                <Rock:ModalAlert ID="maWarning" runat="server" />



            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
