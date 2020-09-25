<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BemaReportConfiguration.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.CustomBlocks.BEMA.Reporting.BemaReportConfiguration" %>
<style>
    .btn-add-margin {
        margin-top: 5px;
    }
</style>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:PlaceHolder ID="phContent" runat="server"></asp:PlaceHolder>
        <div class="row">
            <asp:Repeater ID="rptReports" runat="server" OnItemDataBound="rptReports_ItemDataBound" OnItemCommand="rptReports_ItemCommand">
                <ItemTemplate>
                    <%# (Container.ItemIndex != 0 && Container.ItemIndex % 4 == 0) ? @"</div><div class='row'>" : string.Empty %>
                    <div class="col-md-3 col-sm-6">
                        <div class="portfolio-caption well">
                            <h4><%# Eval("PageTitle") %></h4>
                            <p><%# Eval("Description") %></p>
                            <asp:LinkButton ID="lbAdd" runat="server" Text="Add" CssClass="btn btn-success btn-add-margin" Width="100%" CommandArgument='<%# Eval("Guid") %>' CommandName="Add" />
                            <asp:LinkButton ID="lbView" runat="server" Text="View" CssClass="btn btn-info btn-add-margin" Width="100%" CommandArgument='<%# Eval("Guid") %>' CommandName="View" />
                            <asp:LinkButton ID="lbRemove" runat="server" Text="Remove" CssClass="btn btn-warning btn-add-margin" Width="100%" CommandArgument='<%# Eval("Guid") %>' CommandName="Remove" />
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

