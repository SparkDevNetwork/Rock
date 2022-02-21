<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WebConnectionRequestListLava.ascx.cs" Inherits="RockWeb.Blocks.Connection.WebConnectionRequestListLava" %>

<style>
    .btn-previous-text {
        position: relative;
        left: 8px;
    }

    .btn-previous-icon {
        position: relative;
        right: 8px;
    }

    .btn-more-text {
        position: relative;
        right: 16px;
    }

    .btn-more-icon {
        position: relative;
        left: 18px;
    }
</style>

<asp:UpdatePanel ID="upConnectionSelectLava" runat="server">
    <ContentTemplate>
        <!-- Content -->
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-body">
                <div class="row">
                    <div class="col-xs-12">
                        <h2>
                            <asp:Literal ID="lTitle" runat="server"></asp:Literal>
                            <br />
                            <small>
                                <span class="text-muted">
                                    <asp:Literal ID="lSubTitle" runat="server"></asp:Literal>
                                </span>
                            </small>
                        </h2>
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12 pb-2">
                        <asp:LinkButton ID="lbOptions" runat="server" CssClass="text-muted text-semibold pull-right pr-1" OnClick="lbOptions_Click"><i class="fa fa-sliders"></i>&nbsp;&nbsp;Options</asp:LinkButton>
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Literal ID="lContent" runat="server"></asp:Literal>
                    </div>
                </div>
                <div class="row">
                    <div id="divLoadPrevious" runat="server" class="col-xs-6 col-md-2">
                        <asp:LinkButton ID="lbLoadPrevious" runat="server" Style="width: 120px;" CssClass="btn btn-primary" OnClick="lbLoadPrevious_Click"><i class="fa fa-chevron-circle-left btn-previous-icon"></i><span class="btn-previous-text">Previous</span></asp:LinkButton>
                    </div>
                    <div id="divLoadMore" runat="server" class="col-xs-6 col-md-2 ">
                        <asp:LinkButton ID="lbLoadMore" runat="server" Style="width: 120px;" CssClass="btn btn-primary" OnClick="lbLoadMode_Click"><span class="btn-more-text">More</span><i class="fa fa-chevron-circle-right btn-more-icon"></i></asp:LinkButton>
                    </div>
                </div>
            </div>
        </asp:Panel>
        <!-- Modal -->
        <Rock:ModalDialog ID="mdOptions" runat="server" Title="Options" SaveButtonText="Save" OnSaveClick="mdOptions_SaveClick">
            <Content>
                <div class="row">
                    <div class="col-xs-12">
                        <Rock:Switch
                            ID="swOnlyShowMyConnections"
                            runat="server"
                            Checked="true"
                            FormGroupCssClass="custom-switch-centered hide-label-sm"
                            Text="Only Show My Connections" />
                    </div>
                </div>
                <div class="row pt-2">
                    <div class="col-xs-8">
                        <Rock:RockCheckBoxList ID="cblStates" runat="server" RepeatDirection="Horizontal" Label="Request States" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
