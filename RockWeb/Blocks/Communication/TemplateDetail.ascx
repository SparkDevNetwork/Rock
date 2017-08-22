<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Communication.TemplateDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-text-o"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:HiddenField ID="hfCommunicationTemplateId" runat="server" />

                <asp:ValidationSummary ID="ValidationSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.CommunicationTemplate, Rock" PropertyName="Name" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.CommunicationTemplate, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:ImageUploader ID="imgTemplatePreview" runat="server" Label="Template Preview Image" Help="The preview of this template to show when selecting a template for a new communication" />
                    </div>
                </div>


                <Rock:RockControlWrapper ID="rcwMediumType" runat="server" Label="Select the communication medium that this template supports.">
                    <div class="controls">
                        <div class="js-mediumtype">
                            <Rock:HiddenFieldWithClass ID="hfMediumType" CssClass="js-hidden-selected" runat="server" />
                            <div class="btn-group">
                                <a id="btnMediumRecipientPreference" runat="server" class="btn btn-info btn-sm active js-medium-recipientpreference" data-val="0">Recipient Preference</a>
                                <a id="btnMediumEmail" runat="server" class="btn btn-default btn-sm" data-val="1">Email</a>
                                <a id="btnMediumSMS" runat="server" class="btn btn-default btn-sm" data-val="2">SMS</a>
                            </div>
                        </div>
                    </div>
                </Rock:RockControlWrapper>

                <asp:Panel ID="pnlEmailTemplate" CssClass="js-email-template" runat="server">
                    <h2>Email</h2>
                    <Rock:CodeEditor ID="ceEmailTemplate" runat="server" Label="Message Template" EditorHeight="600" EditorMode="Html"  />
                </asp:Panel>

                <asp:Panel ID="pnlSMSTemplate" CssClass="js-sms-template" runat="server">
                    <h2>SMS</h2>
                </asp:Panel>


                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

        <script>
            Sys.Application.add_load(function ()
            {
                $('.js-mediumtype .btn').on('click', function (e)
                {
                    setActiveMediumTypeButton($(this));
                });

                setActiveMediumTypeButton($('.js-mediumtype').find("[data-val='" + $('.js-mediumtype .js-hidden-selected').val() + "']"));
            });

            function setActiveMediumTypeButton($activeBtn)
            {
                $activeBtn.addClass('active').addClass('btn-info').removeClass('btn-default');
                $activeBtn.siblings('.btn').removeClass('active').removeClass('btn-info').addClass('btn-default')
                $activeBtn.closest('.btn-group').siblings('.js-hidden-selected').val($activeBtn.data('val'));

                $pnlEmail = $('#<%=pnlEmailTemplate.ClientID%>');
                $pnlSMS = $('#<%=pnlSMSTemplate.ClientID%>');

                if ($('#<%=btnMediumEmail.ClientID%>').hasClass('active') || $('.js-medium-recipientpreference').hasClass('active')) {
                    $pnlEmail.show();
                }
                else {
                    $pnlEmail.hide();
                }

                if ($('#<%=btnMediumSMS.ClientID%>').hasClass('active') || $('.js-medium-recipientpreference').hasClass('active')) {
                    $pnlSMS.show();
                }
                else {
                    $pnlSMS.hide();
                }
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>


