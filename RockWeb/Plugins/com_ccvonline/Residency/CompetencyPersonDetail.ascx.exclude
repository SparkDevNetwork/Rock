<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/CSS/bootstrap.min.css" visible="false" />

<asp:UpdatePanel ID="upCompetencyPersonDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonId" runat="server" />
            <asp:HiddenField ID="hfPersonId" runat="server" />

            <div id="pnlEditDetails" runat="server" class="well">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <Rock:LabeledText ID="lblPersonName" runat="server" LabelText="Resident" />
                    
                    <asp:Panel ID="pnlCompetencyLabels" runat="server">
                        <Rock:LabeledText ID="lblPeriod" runat="server" LabelText="Period" />
                        <Rock:LabeledText ID="lblTrack" runat="server" LabelText="Track" />
                        <Rock:LabeledText ID="lblCompetency" runat="server" LabelText="Competency" />
                    </asp:Panel>

                    <asp:Panel ID="pnlCompetencyDropDownLists" runat="server">
                        <Rock:DataDropDownList ID="ddlPeriod" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="com.ccvonline.Residency.Model.Period, com.ccvonline.Residency" PropertyName="Name" AutoPostBack="true" OnSelectedIndexChanged="ddlPeriod_SelectedIndexChanged" LabelText="Period" />
                        <Rock:DataDropDownList ID="ddlTrack" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="com.ccvonline.Residency.Model.Track, com.ccvonline.Residency" PropertyName="Name" AutoPostBack="true" OnSelectedIndexChanged="ddlTrack_SelectedIndexChanged" LabelText="Track"/>
                        <Rock:DataDropDownList ID="ddlCompetency" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="Name" LabelText="Competency" CssClass="input-xlarge"/>
                        <Rock:NotificationBox ID="nbAllCompetenciesAlreadyAdded" runat="server" NotificationBoxType="Info" Text="All competencies for this track have already been assigned to this resident."/>
                    </asp:Panel>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <legend>Resident Competency - Projects
                </legend>
                <div class="well">
                    <div class="row-fluid">
                        <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                    </div>
                </div>

            </fieldset>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
