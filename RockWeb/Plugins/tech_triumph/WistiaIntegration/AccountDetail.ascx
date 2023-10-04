<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountDetail.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WistiaIntegration.AccountDetail" %>

<asp:UpdatePanel ID="upnlAccounts" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-video-camera"></i>
                        <asp:Literal ID="lTitle" runat="server" /></h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Warning" Text="Inactive" />
                    </div>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <div id="pnlEditDetails" runat="server">
                        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                        <asp:HiddenField ID="hfAccountId" runat="server" />
                        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="tech.triumph.WistiaIntegration.Model.WistiaAccount, tech.triumph.WistiaIntegration" PropertyName="Name" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" Checked="true" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbApiKey" runat="server" SourceTypeName="tech.triumph.WistiaIntegration.Model.WistiaAccount, tech.triumph.WistiaIntegration" PropertyName="ApiKey" Label="API Key" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </div>

                    <fieldset id="fieldsetViewSummary" runat="server">
                        <div class="row">
                            <div class="col-md-6">
                                <asp:Literal ID="lblMainDetails" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-sm-4">
                                        <div class="metriccard metriccard-info" >
                                            <h1>Media Count</h1>
                                            <span class="value"><asp:Literal ID="lMediaCount" runat="server" /></span>
                                            <i class="fa fa-video-camera"></i>
                                        </div>
                                    </div>
                                     <div class="col-sm-4">
                                        <div class="metriccard metriccard-info">
                                            <h1>Load Count</h1>
                                            <span class="value"><asp:Literal ID="lLoadCount" runat="server" /></span>
                                            <i class="fa fa-desktop"></i>
                                        </div>
                                    </div>
                                     <div class="col-sm-4">
                                        <div class="metriccard metriccard-info">
                                            <h1>Play Count</h1>
                                            <span class="value"><asp:Literal ID="lPlayCount" runat="server" /></span>
                                            <i class="fa fa-play-circle-o"></i>
                                        </div>
                                    </div>
                                    <div class="col-sm-4">
                                        <div class="metriccard metriccard-success">
                                            <h1>Hours Watched</h1>
                                            <span class="value"><asp:Literal ID="lHoursWatched" runat="server" /></span>
                                            <i class="fa fa-clock-o"></i>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                            <!--
                                <asp:LinkButton ID="btnSync" runat="server" CssClass="btn btn-default pull-right" OnClick="btnSync_Click"><i class="fa fa-refresh"></i> Sync Account</asp:LinkButton>
                            -->
                        </div>
                    </fieldset>
                </div>
            </div>

            <Rock:ModalAlert ID="mdMessages" runat="server" />

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
