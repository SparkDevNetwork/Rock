<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypeSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.GroupTypeSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').on('click', function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maWarning" runat="server" />

        <div class="checkin-header">
            <h1><asp:Literal ID="lTitle" runat="server" /></h1>
        </div>

        <div class="checkin-body">

            <div class="checkin-scroll-panel">
                <div class="scroller">

                    <div class="control-group checkin-body-container">
                        <label class="control-label"><asp:Literal ID="lCaption" runat="server" /></label>
                        <div class="controls">
                            <asp:Panel ID="pnlNoOptions" runat="server" Visible="false">
                                <h4><asp:Literal ID="lNoOptions" runat="server" /></h4>
                                <div class="actions">
                                    <asp:LinkButton CssClass="btn btn-primary btn-checkin btn-footer" ID="btnNoOptionOk" runat="server" OnClick="btnNoOptionOk_Click" Text="Ok" />
                                </div>
                            </asp:Panel>
                            <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand">
                                <ItemTemplate>
                                    <Rock:BootstrapButton ID="lbSelect" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("GroupType.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" DataLoadingText="Loading..." />
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>

                </div>
            </div>

        </div>

        <div class="checkin-footer">
            <div class="checkin-actions">
                <asp:LinkButton CssClass="btn btn-default btn-back" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
                <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
