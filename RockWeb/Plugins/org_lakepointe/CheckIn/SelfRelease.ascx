<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SelfRelease.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.CheckIn.SelfRelease" %>
<style type="text/css">
    .selfReleaseChild .btn-sm {
        padding: 5px 10px;
        font-size: 12px;
        line-height: 1.5;
        border-radius: 3px;
    }

    .selfReleaseChild .btn-danger {
        color: #fff;
        background-color: #c9302c !important;
        border-color: #ac2925 !important;
    }

    .selfReleaseChild .btn-success {
        color: #fff;
        background-color: #449d44 !important;
        border-color: #398439 !important;
    }


    .selfReleaseChild {
        text-align: center;
    }

        .selfReleaseChild .btn-primary::after, .selfReleaseChild .btn-link::after, .selfReleaseChild .btn-action::after, .selfReleaseChild .btn-default::after, .selfReleaseChild .new-btn::after,
        .selfReleaseChild .btn-danger::after, .selfReleaseChild .btn-success::after {
            line-height: 1.5;
            font-size: 12px;
            padding: inherit;
            border-radius: 3px;
            content: inherit;
            font-family: inherit;
            content: "";
        }

        .selfReleaseChild .photo img {
            max-width: 150px;
            display: block;
            margin-left: auto;
            margin-right: auto;
        }
        .selfReleaseChild .label {
            font-size:14px;
        }
    .commands {
        padding-top:30px;
    }
</style>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Danger" />
        <asp:Literal ID="lIntroduction" runat="server" Visible="false" />
        <asp:Literal ID="lConfirmation" runat="server" Visible="false" />
        <asp:Panel ID="pnlSelfRelease" runat="server" Visible="false">
            <div class="row">
                <asp:Repeater ID="rptSelfReleaseChildren" runat="server">
                    <ItemTemplate>
                        <div class="selfReleaseChild col-md-4">
                            <asp:HiddenField ID="hfPersonGuid" runat="server" />
                            <h3 style="text-align: center;">
                                <asp:Literal ID="lName" runat="server" /></h3>
                            <div class="row">
                                <div class="col-xs-12">
                                    <div class="photo">
                                        <asp:Literal ID="lFamilyMemberPhoto" runat="server" />
                                    </div>
                                </div>
                                <div class="col-xs-12">
                                    <ul class="person-demographics list-unstyled">
                                        <li>
                                            <asp:Literal ID="lAge" runat="server" /></li>
                                        <li>
                                            <asp:Literal ID="lGrade" runat="server" /></li>
                                    </ul>
                                </div>

                                <asp:Panel id="pnlSelfReleaseEdit" runat="server" Visible="true" CssClass="col-xs-12">
                                    <Rock:Toggle ID="tglSelfRelease" runat="server" OnCssClass="btn-success" ActiveButtonCssClass="btn-primary" OffCssClass="btn-danger" ButtonSizeCssClass="btn-sm" OnText="Yes" OffText="No" Label="Self Checkout" />
                                </asp:Panel>
                                <asp:Panel ID="pnlSelfReleaseView" runat="server" Visible="false" CssClass="col-xs-12">
                                    <label class="control-label">Self Checkout</label><br />
                                    <asp:Literal ID="lSelfReleaseView" runat="server" />
                                </asp:Panel>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="row commands">
                <div class="col-xs-12">
                    <span class="pull-right"><asp:LinkButton ID="lbNext" runat="server" CssClass="btn-primary">Next</asp:LinkButton></span>
                    <asp:LinkButton ID="lbBack" runat="server" Visible="false" CssClass="btn-default">Back</asp:LinkButton>
                    <span class="pull-right"><asp:LinkButton ID="lbSave" runat="server" Visible="false" CssClass="btn-primary">Save</asp:LinkButton></span>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
