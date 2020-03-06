<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberListLava.ascx.cs" Inherits="com.bemaservices.Theme.Agency.GroupMemberListLava" %>

<script type="text/javascript">
    function clearDialog() {
        $('#rock-config-cancel-trigger').click();
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">
            <Rock:NotificationBox ID="nbContentError" runat="server" Dismissable="true" Visible="false" />
            <asp:PlaceHolder ID="phContent" runat="server" />
            <asp:Literal ID="lDebug" runat="server" />
        </asp:Panel>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModal" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Channel Configuration" OnCancelScript="clearDialog();">
                <Content>

                    <asp:UpdatePanel ID="upnlEdit" runat="server">
                        <ContentTemplate>

                            <Rock:NotificationBox ID="nbError" runat="server" Heading="Error" Title="Query Error!" NotificationBoxType="Danger" Visible="false" />

                            <div class="row">
                                <div class="col-md-5">
                                    <Rock:RockDropDownList ID="ddlGroup" runat="server" Required="true" Label="Group"
                                        DataTextField="Name" DataValueField="Guid" AutoPostBack="true" OnSelectedIndexChanged="ddlGroup_SelectedIndexChanged"
                                        Help="The channel to display items from." />
                                </div>
                                
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:CodeEditor ID="ceTemplate" runat="server" EditorHeight="200" EditorMode="Lava" EditorTheme="Rock" Label="Format"
                                        Help="The template to use when formatting the list of items." />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:NumberBox ID="nbItemCacheDuration" runat="server" CssClass="input-width-sm" Label="Item Cache Duration"
                                        Help="Number of seconds to cache the content items returned by the selected filter (use '0' for no caching)." />
                                    <Rock:NumberBox ID="nbOutputCacheDuration" runat="server" CssClass="input-width-sm" Label="Output Cache Duration"
                                        Help="Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value. (use '0' for no caching)." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:PagePicker ID="ppDetailPage" runat="server" Label="Detail Page" />
                                </div>
                            </div>

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
