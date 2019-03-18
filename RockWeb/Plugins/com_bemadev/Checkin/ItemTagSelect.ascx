<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ItemTagSelect.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.CheckIn.ItemTagSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').click(function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maWarning" runat="server" />

        <div class="checkin-header">
            <h1>
                How many item tags would you like?
            </h1>
        </div>

        <div class="checkin-body">

            <div class="checkin-scroll-panel">
                <div class="scroller">

                    <div class="control-group checkin-body-container">
                        <div class="controls">
                            <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand">
                                <ItemTemplate>
                                    <Rock:BootstrapButton ID="lbSelect" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Container.DataItem.ToString() %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" DataLoadingText="Loading..." />
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>

                </div>
            </div>
        </div>

        <div class="row-fluid checkin-footer">
            <div class="checkin-actions">
                <asp:LinkButton CssClass="btn btn-default btn-back" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
                <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
