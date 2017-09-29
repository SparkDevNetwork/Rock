<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LavaShortcodeList.ascx.cs" Inherits="RockWeb.Blocks.Cms.LavaShortcodeList" %>

<asp:UpdatePanel ID="upLavaShortcode" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <script>
            Sys.Application.add_load(function () {
                $('.js-shortcode-toggle').on('click', function () {
                    $(this).closest('.panel').find('.panel-body').slideToggle();
                    $(this).find('.example-toggle i').toggleClass('fa-circle-o fa-circle');
                });
            });
        </script>

        <asp:Panel ID="pnlLavaShortcodeList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-cube"></i> Lava Shortcodes</h1>
                <div class="pull-right">
                    <Rock:Toggle ID="tglShowActive" runat="server" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-warning" OnText="Active" OffText="Inactive" CssClass="btn" Checked="true" OnCheckedChanged="tglShowActive_CheckedChanged" />
                    <asp:LinkButton ID="btnAddShortcut" runat="server" CssClass="btn btn-xs btn-action" OnClick="btnAddShortcut_Click" Text="Add Shortcut"><i class="fa fa-plus"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">
                <asp:Repeater ID="rptShortcodes" runat="server" OnItemDataBound="rptShortcodes_ItemDataBound">
                    <ItemTemplate>
                       
                        <div class="panel panel-widget panel-shortcodeitem">
                          <div class="panel-heading js-shortcode-toggle clearfix">
                            <div class="row">
                              <div class="col-md-8">
		                        <a name="<%# Eval("TagName").ToString().ToLower() %>"></a>
		                        <h1 class="panel-title"><%# Eval("Name") %></h1>
                              </div>
                              <div class="col-md-4">
                                <div class="pull-right">
          
		                          <div class="example-toggle">
                                    <i class="fa fa-circle-o"></i> Show Details
                                  </div>
		                          <div class="pull-right margin-t-sm">
                                      <%# Boolean.Parse(Eval("IsSystem").ToString()) ?
                                        "<span class='label label-default pull-right'>System</span>" : "" %>
		                          </div>
                                </div>
                              </div>
                            </div>
                            <p class="description">
                              <%# Eval("Description") %>
                            </p>
                          </div>
                          <div class="panel-body" style="display: none;">
                                <%# Eval("Documentation") %>
                              
                                <div id="divEditPanel" runat="server" class="pull-right">
                                    <asp:HiddenField ID="hfShortcodeId" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "Id").ToString() %>' />
                                    <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnEdit_Click"><i class="fa fa-pencil"></i></asp:LinkButton>
                                    <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-danger btn-xs" OnClick="btnDelete_Click"><i class="fa fa-times"></i></asp:LinkButton>
                                </div>
                          </div>
                        </div>

                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
