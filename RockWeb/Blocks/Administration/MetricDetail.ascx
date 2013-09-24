<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.MetricDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>                
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfMetricId" runat="server" />
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

            <fieldset>    
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row-fluid">
                    <div class="span6">
                         <Rock:DataTextBox ID="tbCategory" runat="server"
                            SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Category" />
                        <Rock:DataTextBox ID="tbTitle" runat="server"
                            SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Title" />
                        <Rock:DataTextBox ID="tbSubtitle" runat="server"
                            SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Subtitle" />
                        <Rock:DataTextBox ID="tbDescription" runat="server"
                            SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                    <div class="span6">
                        <Rock:LabeledDropDownList ID="ddlCollectionFrequency" runat="server"
                            LabelText="Collection Frequency"  />
                        <Rock:DataTextBox ID="tbMinValue" runat="server" LabelText="Minimum Value"
                            SourceTypeName="Rock.Model.Metric, Rock" PropertyName="MinValue" />
                        <Rock:DataTextBox ID="tbMaxValue" runat="server" LabelText="Maximum Value"
                            SourceTypeName="Rock.Model.Metric, Rock" PropertyName="MaxValue" />
                        <Rock:DataTextBox ID="tbSource" runat="server" LabelText="Data Source"
                            SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Source" />
                        <Rock:DataTextBox ID="tbSourceSQL" runat="server" LabelText="Source SQL"
                            SourceTypeName="Rock.Model.Metric, Rock" TextMode="MultiLine" Rows="3"
                            PropertyName="SourceSQL" />
                        <Rock:LabeledCheckBox ID="cbType" runat="server" LabelText="Allow Multiple Values" />
                    </div>
                </div>
            </fieldset>            

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>        

    </ContentTemplate>
</asp:UpdatePanel>
