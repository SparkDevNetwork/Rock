<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailForm.ascx.cs" Inherits="RockWeb.Blocks.Cms.EmailForm" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lError" runat="server" />

        <asp:Panel ID="pnlEmailForm" runat="server" CssClass="emailform">
            <asp:Literal ID="lEmailForm" runat="server" />

            <div class="emailform-messages"></div>
            
            <asp:Panel ID="pnlCaptcha" runat="server" CssClass="captcha-panel">
                <Rock:Captcha ID="cpCaptcha" runat="server" OnTokenReceived="cpCaptcha_TokenReceived" />
            </asp:Panel>
			
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
 
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            var formData = {};
            var formAttachments = {};

            if (!prm._handlersAlreadyAdded) {
                prm.add_beginRequest(maintainFormData);
                prm.add_endRequest(restoreFormData);
                prm._handlersAlreadyAdded = true;
            }

            function maintainFormData(sender, args) {
                formData = {};
                formAttachments = {};
                $(".emailform input[type='text'], .emailform input[type='email'], .emailform input[type='password'], .emailform textarea, .emailform select").each(function () {
                    var name = $(this).attr('name');
                    if (name) {
                        formData[name] = $(this).val();
                    }
                });
                $(".emailform input[type='checkbox']").each(function () {
                    var name = $(this).attr('name');
                    if (name) {
                        if (!formData[name]) formData[name] = [];
                        if ($(this).prop('checked')) {
                            formData[name].push($(this).val());
                        }
                    }
                });
                $(".emailform input[type='radio']").each(function () {
                    if ($(this).prop('checked')) {
                        var name = $(this).attr('name');
                        if (name) {
                            formData[name] = $(this).val();
                        }
                    }
                });
                $(".emailform input[type='file']").each(function () {
                    var name = $(this).attr('name');
                    if (name && this.files.length > 0) {
                        formAttachments[name] = Array.from(this.files);
                    }
                });
            }

            function restoreFormData(sender, args) {
                for (var name in formData) {
                    var val = formData[name];
                    if (Array.isArray(val)) {
                        $(".emailform input[type='checkbox'][name='" + name + "']").prop('checked', false);
                        for (var i = 0; i < val.length; i++) {
                            $(".emailform input[type='checkbox'][name='" + name + "'][value='" + val[i] + "']").prop('checked', true);
                        }
                    } else {
                        var input = $(".emailform [name='" + name + "']");
                        if (input.is(':radio')) {
                            input.prop('checked', false);
                            $(".emailform input[type='radio'][name='" + name + "'][value='" + val + "']").prop('checked', true);
                        } else {
                            input.val(val);
                        }
                    }
                }
                for (var name in formAttachments) {
                    var files = formAttachments[name];
                    if (files && files.length > 0) {
                        var dt = new window.DataTransfer();
                        files.forEach(function (file) {
                            dt.items.add(file);
                        });
                        try {
                            $(".emailform input[type='file'][name='" + name + "']")[0].files = dt.files;
                        } catch (err) {
                            console.error(err.message);
                        }
                    }
                }
            }
        </script>

    </ContentTemplate>
    <Triggers>
        <asp:PostBackTrigger ControlID="btnSubmit" />
    </Triggers>
</asp:UpdatePanel>
