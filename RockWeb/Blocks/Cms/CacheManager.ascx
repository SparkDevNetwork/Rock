<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CacheManager.ascx.cs" Inherits="CacheManager" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:HiddenField ID="hfActiveDialog" runat="server" />

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-tachometer"></i>Cache Manager</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:Grid ID="gCacheTagList" runat="server" AllowSorting="true" EmptyDataText="No Tags Found">
                        <Columns>
                            <Rock:RockBoundField DataField="TagName" HeaderText="Tag Name" SortExpression="TagName" />
                            <Rock:RockBoundField DataField="TagDescription" HeaderText="Description" SortExpression="TagDescription" TruncateLength="255" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="LinkedKeys" HeaderText="Linked Keys" SortExpression="LinkedKeys" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:LinkButtonField Text="<i class='fa fa-eraser'></i>" CssClass="btn btn-default btn-sm btn-square" OnClick="gCacheTagList_ClearCacheTag" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                        </Columns>
                    </Rock:Grid>
                    </div>
                    <div class="col-md-3">
                        <asp:Literal ID="lCacheStatistics" runat="server"></asp:Literal>
                    </div>
                    <div class="col-md-3"> 
                        <p class="clearfix">
                            <asp:LinkButton ID="btnClearCache" runat="server" CssClass="btn btn-action btn-sm pull-right" OnClick="btnClearCache_Click" CausesValidation="false">
                                <i class="fa fa-repeat"></i> Clear Cache
                            </asp:LinkButton>
                        </p>
                        <Rock:RockDropDownList ID="ddlCacheTypes" runat="server" DataTextField="Name" DataValueField="Id" Label="Cache Types" OnSelectedIndexChanged="ddlCacheTypes_SelectedIndexChanged" AutoPostBack="true" />
                    </div>
                </div>

            </div>
        </div>

        <Rock:ModalDialog ID="dlgAddTag" runat="server" Title="Add Tag" OnSaveClick="dlgAddTag_SaveClick" OnCancelScript="clearActiveDialog();" >
            <Content>
                <Rock:NotificationBox ID="nbModalMessage" runat="server" Visible="false" />
                <div class="row">
                    <div class="col-md-2"></div>
                    <div class="col-md-10">
                        <div class="row">
                            <div class="col-md-4">
                                Tag Name
                                <asp:TextBox ID="tbTagName" runat="server" onkeypress="this.value = this.value.toLowerCase();" Style="text-transform: lowercase;" Width="100%"/>
                            </div>
                        </div> 
                        <br />
                        <div class="row">
                            <div class="col-md-8">
                                Description
                                <asp:TextBox ID="tbTagDescription" runat="server" TextMode="MultiLine" Rows="3" Width="100%"/>
                            </div>
                        </div>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>