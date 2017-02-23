<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountabilityReportForm.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.AccountabilityReportForm" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel panel-block">

                <div class="panel-heading">
                    <h1 class="panel-title">Accountability Report</h1>
                </div>

                <div class="panel-body">
                    <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />

                    <div class="row">
                        <div class="col-md-10">
                            <b>Report for week ending on date:</b>
                        </div>
                        <div class="col-md-2">
                            <Rock:RockDropDownList ID="ddlSubmitForDate" runat="server"></Rock:RockDropDownList>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-3 col-md-offset-9">
                            Comments (300 chars max)
                        </div>
                    </div>

                    <asp:PlaceHolder ID="phQuestions" runat="server" />
                    <Rock:RockTextBox ID="tbComments" Label="Comments (250 chars max)" SourceTypeName="com.centralaz.Accountability.Model.ResponseSet, com.centralaz.Accountability" PropertyName="Comments" TextMode="MultiLine" Rows="5" runat="server" />
                    <asp:LinkButton ID="lbSubmit" class="btn btn-primary" Text="Submit" runat="server" OnClick="lbSubmit_OnClick" />
                    <asp:LinkButton ID="lbCancel" class="btn btn-link" Text="Cancel" runat="server" OnClick="lbCancel_OnClick" />

                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
    <script type="text/javascript">
        $(function () {
            var limit = '<%= GetAttributeValue( "MaximumCommentLength" )%>';
            $('textarea[id$=tbComments]').keyup(function() {
                    var len = $(this).val().length;
                    if (len > limit) {this.value = this.value.substring(0, limit);}
            })
        });
</script>
