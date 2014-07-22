<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/Themes/Rock/Styles/bootstrap.css" visible="false" />
<link rel="stylesheet" type="text/css" href="~/Themes/Rock/Styles/theme.css" visible="false" />

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonId" runat="server" />
            <asp:HiddenField ID="hfPersonId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlEditDetails" runat="server">

                    <asp:Panel ID="pnlCompetencyLabels" runat="server">
                        <Rock:RockLiteral ID="lblPeriod" runat="server" Label="Period" />
                        <Rock:RockLiteral ID="lblTrack" runat="server" Label="Track" />
                        <Rock:RockLiteral ID="lblCompetency" runat="server" Label="Competency" />
                    </asp:Panel>

                    <asp:Panel ID="pnlCompetencyDropDownLists" runat="server">
                        <Rock:DataDropDownList ID="ddlPeriod" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="com.ccvonline.Residency.Model.Period, com.ccvonline.Residency" PropertyName="Name" AutoPostBack="true" OnSelectedIndexChanged="ddlPeriod_SelectedIndexChanged" Label="Period" />
                        <Rock:DataDropDownList ID="ddlTrack" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="com.ccvonline.Residency.Model.Track, com.ccvonline.Residency" PropertyName="Name" AutoPostBack="true" OnSelectedIndexChanged="ddlTrack_SelectedIndexChanged" Label="Track" />
                        <Rock:DataDropDownList ID="ddlCompetency" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="Name" Label="Competency" CssClass="input-xlarge" />
                        <Rock:NotificationBox ID="nbAllCompetenciesAlreadyAdded" runat="server" NotificationBoxType="Info" Text="All competencies for this track have already been assigned to this resident." />
                    </asp:Panel>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
