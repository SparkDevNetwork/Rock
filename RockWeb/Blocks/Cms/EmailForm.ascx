<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailForm.ascx.cs" Inherits="RockWeb.Blocks.Cms.EmailForm" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lError" runat="server" />

        <asp:Panel ID="pnlEmailForm" runat="server" CssClass="emailform">
            <asp:Literal ID="lEmailForm" runat="server" />

            <div class="emailform-messages"></div>
			
			<div id="divButtonWrap" runat="server">
				<asp:Button ID="btnSubmit" CssClass="btn btn-primary" runat="server" Text="Submit" OnClientClick="return validateForm();" OnClick="btnSubmit_Click" />
			</div>
        </asp:Panel>

        <asp:Literal ID="lDebug" runat="server" />
        
        <div class="response-message js-response-message">
            <asp:Literal ID="lResponse" runat="server" Visible="false" />
        </div>

        <script>

            Sys.Application.add_load(function () {
                var checkFindings = '';
                var controlCount = 0;

                $(".emailform :text").each(function () {
                    if ($(this).attr("name") == null) {
                        checkFindings = "There are email form fields without 'name' attributes."
                        console.log($(this));
                    }
                    controlCount++;
                });
            });

            function scrollToMessage() {
                if (!$('.js-response-message').visible(true)) {
                    $('html, body').animate({
                        scrollTop: $('.js-response-message').offset().top + 'px'
                    }, 'fast');
                }
            }

            function validateForm() {

                var responseText = '';
                var i = 0;
                var n = $("input:checked").length; //number of checked 

                $(".emailform :radio").each(function () {
                    if ($(this).attr("required")) {
                        // only add to the response text for the radio group not each item
                        i++;
                        if (n < i && i < 2) {
                            responseText += 'Please make a selection for ' + $(this).attr("name") + '\n';
                        }
                    }
                });

                $(".emailform :text").each(function () {
                    if ($(this).attr("required") != null) {
                        if ($(this).val() == '') {
                            var controlLabel = $(this).siblings('label').text();
                            if (controlLabel == null || controlLabel == '') {
                                controlLabel = $(this).attr("name");
                            }
                            responseText += 'Please provide a value for ' + controlLabel + '\n';
                        }
                    }
                });

                if (responseText != '') {
                    alert("A few things before we're done:\n\n" + responseText);
                    return false;
                }

                return true;

            }
        </script>

    </ContentTemplate>
    <Triggers>
        <asp:PostBackTrigger ControlID="btnSubmit" />
    </Triggers>
</asp:UpdatePanel>
