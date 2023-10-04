<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyPreRegistration.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Crm.FamilyPreRegistration" %>
<%@ Import Namespace="Rock" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />
        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Family Pre-Registration</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlVisit" runat="server" CssClass="panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Visit Information</h3>
                    </div>
                    <div class="panel-body">
                        <div class="row">
                            <asp:Panel CssClass="col-md-5" runat="server" ID="pnlCampus">
                                <Rock:CampusPicker ID="cpCampus" runat="server" CssClass="input-width-lg" Label="Campus" RequiredErrorMessage="Campus is required." />
                            </asp:Panel>
                            <asp:Panel CssClass="col-md-5" runat="server" ID="pnlPlannedDate">
                                <Rock:DatePicker ID="dpPlannedDate" runat="server" Label="Planned Visit Date" AllowPastDateSelection="false" RequiredErrorMessage="Planned Visit Date is required." />
                            </asp:Panel>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlParents" runat="server" CssClass="panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Adult Information</h3>
                    </div>
                    <div class="panel-body">

                        <asp:HiddenField ID="hfFamilyGuid" runat="server" />
                        <asp:HiddenField ID="hfAdultGuid1" runat="server" />
                        <asp:HiddenField ID="hfAdultGuid2" runat="server" />

                        <h4>First Adult</h4>
                        <div class="row">
                            <asp:HiddenField ClientIDMode="Static" ID="hfImage1" runat="server" />
                            <asp:HiddenField ClientIDMode="Static" ID="hfPersonNum" Value="1" runat="server" />

                            <div class="col-sm-3">
                                <Rock:RockLiteral ID="lFirstName1" runat="server" Label="First Name" Visible="false" />
                                <Rock:DataTextBox ID="tbFirstName1" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="NickName" Label="First Name" RequiredErrorMessage="First Name is required." />
                            </div>
                            <div class="col-sm-3">
                                <Rock:RockLiteral ID="lLastName1" runat="server" Label="Last Name" Visible="false" />
                                <Rock:DataTextBox ID="tbLastName1" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="LastName" RequiredErrorMessage="Last Name is required." />
                            </div>
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlSuffix1">
                                <Rock:DefinedValuePicker ID="dvpSuffix1" runat="server" Label="Suffix" RequiredErrorMessage="Suffix is required." />
                            </asp:Panel>
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlGender1">
                                <Rock:RockDropDownList ID="ddlGender1" runat="server" Label="Gender" RequiredErrorMessage="Gender is required." />
                            </asp:Panel>
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlBirthDate1">
                                <Rock:DatePicker ID="dpBirthDate1" runat="server" Label="Birthdate" AllowFutureDateSelection="False" RequireYear="True" ShowOnFocus="false" StartView="decade" RequiredErrorMessage="Birth Date is required." />
                            </asp:Panel>
                        </div>
                        <div class="row">
                            <asp:Panel CssClass="col-sm-6" runat="server" ID="pnlEmail1">
                                <Rock:EmailBox ID="tbEmail1" runat="server" Label="Email" RequiredErrorMessage="Email is required." />
                            </asp:Panel>
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlMobilePhone1">
                                <Rock:PhoneNumberBox ID="pnMobilePhone1" runat="server" Label="Mobile Phone" RequiredErrorMessage="Phone Number is required." />
                            </asp:Panel>
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlOkayTextBox1">
                                <Rock:RockCheckBox ID="cbPermissionToText1" runat="server" Text="Okay to Text?" Checked="true" Help="Is it okay if Lakepointe texts you at this number?" />
                            </asp:Panel>
                        </div>
                        <div class="row">
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlMaritalStatus1">
                                <Rock:DefinedValuePicker ID="dvpMaritalStatus1" runat="server" Label="Marital Status" RequiredErrorMessage="Marital Status is required." />
                            </asp:Panel>
                            <Rock:DynamicPlaceholder ID="phAttributes1" runat="server" />
                        </div>
                        <div class="row">
                            <asp:Panel CssClass="col-sm-12" runat="server" ID="pnlAPhoto1">
                                <div class="row">
                                    <div class="col-sm-6">
                                        <asp:LinkButton ID="lbCamera1" runat="server" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbCamera_Click" >
                                            <i class="fal fa-camera"></i> Take Photo&nbsp&nbsp
                                        </asp:LinkButton>
                                    </div>
                                    <div class="col-sm-6">
                                        <canvas id="canvas1" width=320 height=320></canvas>
                                    </div>
                                </div>
                            </asp:Panel>

                            <asp:Panel CssClass="col-sm-12" runat="server" ID="pnlA1UploadPhoto">
                                    <div class="col-md-3">
                                        <Rock:ImageEditor ID="uploadPhoto1" runat="server" Label="Photo" ButtonText="<i class='fal fa-pencil-alt'></i>" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                                    </div>
                            </asp:Panel>

                        </div>

                        <hr />

                        <h4>Second Adult</h4>
                        <div class="adult-2-fields">
                            <asp:HiddenField ID="hfSuffixRequired" runat="server" />
                            <asp:HiddenField ID="hfGenderRequired" runat="server" />
                            <asp:HiddenField ID="hfBirthDateRequired" runat="server" />
                            <asp:HiddenField ID="hfMaritalStatusRequired" runat="server" />
                            <asp:HiddenField ID="hfMobilePhoneRequired" runat="server" />
                            <asp:HiddenField ID="hfEmailRequired" runat="server" />

                            <div class="row">
                                <asp:HiddenField ClientIDMode="Static" ID="hfImage2" runat="server" />

                                <div class="col-sm-3">
                                    <Rock:RockLiteral ID="lFirstName2" runat="server" Label="First Name" Visible="false" />
                                    <Rock:DataTextBox ID="tbFirstName2" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="NickName" Label="First Name" />
                                </div>
                                <div class="col-sm-3">
                                    <Rock:RockLiteral ID="lLastName2" runat="server" Label="Last Name" Visible="false" />
                                    <Rock:DataTextBox ID="tbLastName2" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="LastName" />
                                </div>
                                <asp:Panel CssClass="col-sm-3 js-Adult2Required" runat="server" ID="pnlSuffix2">
                                    <Rock:DefinedValuePicker ID="dvpSuffix2" runat="server" Label="Suffix" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-3 js-Adult2Required" runat="server" ID="pnlGender2">
                                    <Rock:RockDropDownList ID="ddlGender2" runat="server" Label="Gender" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-3 js-Adult2Required" runat="server" ID="pnlBirthDate2">
                                    <Rock:DatePicker ID="dpBirthDate2" runat="server" Label="Birthdate" AllowFutureDateSelection="False" ShowOnFocus="false" StartView="decade" />
                                </asp:Panel>
                            </div>
                            <div class="row">
                                <asp:Panel CssClass="col-sm-6 js-Adult2Required" runat="server" ID="pnlEmail2">
                                    <Rock:EmailBox ID="tbEmail2" runat="server" Label="Email" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-3 js-Adult2Required" runat="server" ID="pnlMobilePhone2">
                                    <Rock:PhoneNumberBox ID="pnMobilePhone2" runat="server" Label="Mobile Phone" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlOkayTextBox2">
                                    <Rock:RockCheckBox ID="cbPermissionToText2" runat="server" Text="Okay to Text?" Checked="true" Help="Is it okay if Lakepointe texts you at this number?" />
                                </asp:Panel>
                            </div>
                            <div class="row">
                                <asp:Panel CssClass="col-sm-3 js-Adult2Required" runat="server" ID="pnlMaritalStatus2">
                                    <Rock:DefinedValuePicker ID="dvpMaritalStatus2" runat="server" Label="Marital Status" />
                                </asp:Panel>
                                <Rock:DynamicPlaceholder ID="phAttributes2" runat="server" />
                            </div>
                            <div class="row">
                                <asp:Panel CssClass="col-sm-12" runat="server" ID="pnlAPhoto2">
                                    <div class="row">
                                        <div class="col-sm-6">
                                            <asp:LinkButton ID="lbCamera2" runat="server" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbCamera_Click" >
                                                <i class="fal fa-camera"></i> Take Photo&nbsp&nbsp
                                            </asp:LinkButton>
                                        </div>
                                        <div class="col-sm-6">
                                            <canvas id="canvas2" width=320 height=320></canvas>
                                        </div>
                                    </div>
                                </asp:Panel>

                                <asp:Panel CssClass="col-sm-12" runat="server" ID="pnlA2UploadPhoto">
                                    <div class="col-md-3">
                                        <Rock:ImageEditor ID="uploadPhoto2" runat="server" Label="Photo" ButtonText="<i class='fal fa-pencil-alt'></i>" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                                    </div>
                                </asp:Panel>
                                
                                 
                            </div>
                        </div>

                        <hr />

                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:AddressControl ID="acAddress" Label="Address" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" />
                            </div>
                            <div class="col-sm-6">
                                <Rock:DynamicPlaceholder ID="phFamilyAttributes" runat="server" />
                            </div>
                        </div>
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlChildren" runat="server" CssClass="panel panel-default">
                    <div class="panel-heading">
                        <asp:Literal ID="lProgenyLabel" runat="server" Text="<h3 class='panel-title'>Children</h3>" />
                    </div>
                    <div class="panel-body">
                        <Rock:PreRegistrationChildren ID="prChildren" runat="server" OnAddChildClick="prChildren_AddChildClick" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlAcknowledgement" runat="server" Visible="false" >
                    <div class="row">
                        <div class="col-xs-12">
                            <asp:Literal ID="lAcknowledgement" runat="server" />
                        </div>
                    </div>
                    <Rock:RockCheckBox ID="rcbIAgree" runat="server" Visible="false" Required="true" />
                </asp:Panel>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"
                        OnClientClick="return preventSaveDoubleClick();" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

        <Rock:ModalDialog ID="dlgCamera" runat="server" Title="Take Person Photo" OnCancelScript="$('#capture').click();"
            ValidateRequestMode="Disabled" ValidationGroup="TakePicture" FormNoValidate="formnovalidate">
            <Content>
                <video style="transform: rotateY(180deg); -webkit-transform:rotateY(180deg); -moz-transform:rotateY(180deg);" ID="player" width=320 height=320 autoplay></video>
                <button ID="capture" hidden="hidden" onclick="return snap();" onclose="return loadImages();" formnovalidate="formnovalidate" autofocus="autofocus">Capture</button>
            </Content>
        </Rock:ModalDialog>

        <script type="text/javascript">
            translateAttributes();
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_endRequest(function (s, e) {
                translateAttributes();
            });
            function translateAttributes()
            {
                var schoolNameLabels = document.querySelectorAll("label[for$=attribute_field_39175]");
                for (var x = 0; x < schoolNameLabels.length; x++) {
                    schoolNameLabels[x].innerText = "<%=Translation.GetValueOrDefault( "LabelSchoolName", "School Name" )%>";
                }
                var selfReleaseLabels = document.querySelectorAll("label[for$=attribute_field_2298]");
                for (var x = 0; x < selfReleaseLabels.length; x++) {
                    selfReleaseLabels[x].innerText = "<%=Translation.GetValueOrDefault( "LabelSelfRelease", "Self Release" )%>";
                }
                var selfReleaseControls = document.querySelectorAll("select[id$=attribute_field_2298]");
                for (var x = 0; x < selfReleaseControls.length; x++) {
                    var selfReleaseOptions = selfReleaseControls[x].querySelectorAll("option");
                    for (var y = 0; y < selfReleaseOptions.length; y++) {
                        selfReleaseOptions[y].innerText = selfReleaseOptions[y].innerText.replace("Yes", "<%=Translation.GetValueOrDefault( "Yes", "Yes" )%>");
                        selfReleaseOptions[y].innerText = selfReleaseOptions[y].innerText.replace("No", "<%=Translation.GetValueOrDefault( "No", "No" )%>");
                    }
                }
                var messagesLanguageControls = document.querySelectorAll("select[id*=attribute_field_39182]");
                for (var x = 0; x < messagesLanguageControls.length; x++) {
                    var messagesLanguageOptions = messagesLanguageControls[x].querySelectorAll("option");
                    for (var y = 0; y < messagesLanguageOptions.length; y++) {
                        messagesLanguageOptions[y].innerText = messagesLanguageOptions[y].innerText.replace("Yes", "<%=Translation.GetValueOrDefault( "Yes", "Yes" )%>");
                        messagesLanguageOptions[y].innerText = messagesLanguageOptions[y].innerText.replace("No", "<%=Translation.GetValueOrDefault( "No", "No" )%>");
                    }
                }
            }
            function loadImages()
            {
                var canvases = document.querySelectorAll("canvas");
                for (var canvas of canvases)
                {
                    var hiddenFieldId = canvas.id.replace("canvas", "hfImage");
                    var hiddenField = document.querySelector("[id=" + hiddenFieldId + "]");

                    if (hiddenField != null && canvas != null) {
                        if (hiddenField.value != "") {
                            const image = new Image();
                            const context = canvas.getContext('2d');
                            image.onload = function () {
                                context.drawImage(image, 0, 0, canvas.width, canvas.height);
                            }
                            image.src = hiddenField.value;
                        }
                    }
                }
                return false;
            }

            var selectedCanvas = "";
            var selectedHiddenField = "";

            function snap()
            {
                const context = selectedCanvas.getContext('2d');
                const player = document.getElementById('player');

                // Draw the video frame to the canvas.
                context.drawImage(player, 0, 0, selectedCanvas.width, selectedCanvas.height);
                selectedHiddenField.value = selectedCanvas.toDataURL('image/jpeg');
                
                // Stop all video streams.
                if (player.srcObject != null) {
                    player.srcObject.getVideoTracks().forEach(track => track.stop());
                }

                return false;
            }

            function snapImage(hiddenField, canvas)
            {
                selectedCanvas = canvas;
                selectedHiddenField = hiddenField;
                const player = document.getElementById('player');
                const constraints = { video: { width: 320, height: 320 } };
                navigator.mediaDevices.getUserMedia(constraints)
                    .then((stream) => {
                        player.srcObject = stream;
                    });
            }

            function enableRequiredFields(enable)
            {
                $('.adult-2-fields').find("[id$='_rfv']").each(function ()
                {
                    var domObj = $(this).get(0);
                    if (domObj != null) {
                        domObj.enabled = (enable != false);
                    }
                });
            }

            function preventSaveDoubleClick() {
                var saveVal = $("[id$='lbSave']").val();
                var valGroup = "<%= ValGroupName %>";

                            if (saveVal === "Saving...") {
                                return false;
                            }

                            if (Page_ClientValidate(valGroup)) {
                                $("[id$='lbSave']").val("Saving...");
                                return true;
                            }

                            return false;
                        }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>