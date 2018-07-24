<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationEntry.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationEntry" %>

<script>
    //Sys.WebForms.PageRequestManager.getInstance().add_endRequest(scrollToGrid);
    function scrollToResults() {
        
            $('html, body').animate({
                scrollTop: $('.js-pnl-result')
            }, 'fast');
        
    }
</script>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
 
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment-o"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
                
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlEdit" runat="server">

                    <asp:HiddenField ID="hfCommunicationId" runat="server" />
                    <asp:HiddenField ID="hfMediumId" runat="server" />

                    <asp:ValidationSummary ID="ValidationSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:CustomValidator ID="cvDelayDateTime" runat="server" />

                    <div class="well well-pillwrap">
                        <div id="divMediums" runat="server">
                            <ul class="nav nav-pills">
                                <asp:Repeater ID="rptMediums" runat="server">
                                    <ItemTemplate>
                                        <li class='<%# (int)Eval("Key") == MediumEntityTypeId ? "active" : "" %>'>
                                            <asp:LinkButton ID="lbMedium" runat="server" Text='<%# Eval("Value") %>' CommandArgument='<%# Eval("Key") %>' OnClick="lbMedium_Click" CausesValidation="false">
                                            </asp:LinkButton>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </div>
                    </div>

                    <Rock:NotificationBox ID="nbInvalidTransport" runat="server" NotificationBoxType="Warning" Dismissable="true" Title="Warning" Visible="false" />

                    <div class="row">
                        <div class="col-md-12">
                            <div class="pull-right">
                                <Rock:RockCheckBox ID="cbBulk" runat="server" Text="Bulk Communication" CssClass="js-bulk-option"
                                  Help="Select this option if you are sending this email to a group of people.  This will include the option for recipients to unsubscribe and will not send the email to any recipients that have already asked to be unsubscribed." />
                            </div>
                        </div>
                    </div>

                    <div class="panel panel-widget recipients">
                        <div class="panel-heading clearfix">
                            <div class="control-label pull-left">
                                To: <asp:Literal ID="lNumRecipients" runat="server" />
                            </div> 
                    
                            <div class="pull-right">
                                <Rock:PersonPicker ID="ppAddPerson" runat="server" CssClass="picker-menu-right" PersonName="Add Person" OnSelectPerson="ppAddPerson_SelectPerson" />
                            </div>

                            <asp:CustomValidator ID="valRecipients" runat="server" OnServerValidate="valRecipients_ServerValidate" Display="None" ErrorMessage="At least one recipient is required." />
                
                         </div>   
                
                         <div class="panel-body">

                                <ul class="recipients list-unstyled">
                                    <asp:Repeater ID="rptRecipients" runat="server" OnItemCommand="rptRecipients_ItemCommand" OnItemDataBound="rptRecipients_ItemDataBound">
                                        <ItemTemplate>
                                            <li class='recipient <%# Eval("Status").ToString().ToLower() %>'><asp:Literal id="lRecipientName" runat="server"></asp:Literal> <asp:LinkButton ID="lbRemoveRecipient" runat="server" CommandArgument='<%# Eval("PersonId") %>' CausesValidation="false"><i class="fa fa-times"></i></asp:LinkButton></li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>

                                <div class="pull-right">
                                    <asp:LinkButton ID="lbShowAllRecipients" runat="server" CssClass="btn btn-action btn-xs" Text="Show All" OnClick="lbShowAllRecipients_Click" CausesValidation="false"/>
                                    <asp:LinkButton ID="lbRemoveAllRecipients" runat="server" Text="Remove All Pending Recipients" CssClass="remove-all-recipients btn btn-action btn-xs" OnClick="lbRemoveAllRecipients_Click" CausesValidation="false"/>
                                </div>

                        </div>
                    </div>

                    <Rock:RockDropDownList ID="ddlTemplate" runat="server" Label="Template" AutoPostBack="true" OnSelectedIndexChanged="ddlTemplate_SelectedIndexChanged" EnhanceForLongLists="true" />

                    <asp:PlaceHolder ID="phContent" runat="server" />

                    <Rock:DateTimePicker ID="dtpFutureSend" runat="server" Label="Delay Send Until" SourceTypeName="Rock.Model.Communication" PropertyName="FutureSendDateTime" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSubmit" runat="server" Text="Submit" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
                        <asp:LinkButton ID="btnTest" runat="server" Text="Send Test" CssClass="btn btn-link" OnClick="btnTest_Click" />
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save as Draft" CssClass="btn btn-link" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" />
                    </div>

                    <Rock:NotificationBox ID="nbTestResult" CssClass="margin-t-md" runat="server" Visible="false" />

                </asp:Panel>

                <asp:Panel ID="pnlResult" runat="server" Visible="false" CssClass="js-pnl-result">
                    <Rock:NotificationBox ID="nbResult" runat="server" NotificationBoxType="Success" />
                    <br />
                    <asp:HyperLink ID="hlViewCommunication" runat="server" Text="View Communication" />
                </asp:Panel>

            </div>
        </div>

        

        <script type="text/javascript">
            Sys.Application.add_load(function () {

                // Set all recipients tooltip
                $('.recipient span').tooltip();

                // Set the display of any recipients that have preference of NoBulkEmail based on if this is a bulk communication
                $('.js-bulk-option').click(function () {
                    if ($(this).is(':checked')) {
                        $('.js-no-bulk-email').addClass('text-danger');
                    } else {
                        $('.js-no-bulk-email').removeClass('text-danger');
                    }
                });
            })
        </script>


    </ContentTemplate>
</asp:UpdatePanel>


