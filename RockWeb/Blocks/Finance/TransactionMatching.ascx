﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionMatching.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionMatching" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <script>
            Sys.Application.add_load(function () {
                $(".transaction-image a").fluidbox();
                $("#account_entry").height($("#individual_details").height());
            });
        </script>
        <asp:HiddenField ID="hfBackNextHistory" runat="server" />
        <asp:HiddenField ID="hfHistoryPosition" runat="server" />
        <asp:HiddenField ID="hfTransactionId" runat="server" />
        <asp:HiddenField ID="hfBatchId" runat="server" />
        <asp:HiddenField ID="hfCheckMicrHashed" runat="server" />
        <asp:HiddenField ID="hfDoFadeIn" runat="server" />
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading panel-follow">
                <!-- <h1 class="panel-title">
                    <asp:Literal ID="lPanelTitle" runat="server" />
                </h1> -->

                <asp:Literal ID="lProgressBar" runat="server"></asp:Literal>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" Visible="false" />

                    <Rock:RockControlWrapper ID="rcwAddNewBusiness" runat="server" Visible="false">
                        <a id="hlAddNewBusiness" class="btn btn-default btn-xs" runat="server" href="#">Add Business</a>
                    </Rock:RockControlWrapper>

                    <Rock:RockControlWrapper ID="rcwAddNewFamily" runat="server" Visible="false">
                        <a id="hlAddNewFamily" class="btn btn-default btn-xs" runat="server" href="#">Add Family</a>
                    </Rock:RockControlWrapper>

                    <asp:LinkButton ID="btnFilter" runat="server" CssClass="btn btn-xs btn-square btn-default" OnClick="btnFilter_Click"><i class="fa fa-gear" title="Filter Accounts"></i></asp:LinkButton>
                </div>
                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>

            <div class="panel-body styled-scroll">
                <Rock:NotificationBox ID="nbNoUnmatchedTransactionsRemaining" runat="server" NotificationBoxType="Success" Text="<i class='fa fa-check-circle'></i> There are no more unmatched transactions in this batch. Click 'Done' to indicate that the batch is no longer pending and return to batch details." />
                <asp:LinkButton ID="lbFinish" runat="server" CssClass="btn btn-default" OnClick="lbFinish_Click">Done</asp:LinkButton>
                <asp:Panel ID="pnlEdit" runat="server">
                    <div class="row">
                        <div class="col-md-5 transaction-matching-image">
                            <Rock:NotificationBox ID="nbNoTransactionImageWarning" runat="server" NotificationBoxType="Warning" Text="Warning. No Images found for this transaction." />
                            <Rock:NotificationBox ID="nbIsInProcess" runat="server" NotificationBoxType="Warning" Text="Warning. This transaction is getting processed by ...." />
                            <div class="photo transaction-image">
                                <asp:Literal ID="lImage" runat="server" />
                            </div>
                            <div>
                                <asp:Repeater ID="rptrImages" runat="server">
                                    <ItemTemplate>
                                        <asp:Image ID="imgOther" ImageUrl='<%# ImageUrl( (int)Eval("BinaryFileId") ) %>' runat="server" CssClass="transaction-image-thumbnail" ToolTip="Click to toggle" />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                        <div class="col-md-7 transaction-matching-details">
                            <div class="header">
                                <div class="row">
                                    <div id="individual_details" class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlIndividual" runat="server" EnhanceForLongLists="true" Label="Individual" Help="Select a person that has previously been matched to the bank account. If the person isn't in this list, use the 'Assign to New' to select the matching person." AutoPostBack="true" OnSelectedIndexChanged="ddlIndividual_SelectedIndexChanged" />
                                        <span id="badgeIndividualCount" runat="server" class="pull-right badge badge-danger"
                                            style="position: relative; top: -58px; left: 10px"></span>

                                        <div>
                                            <Rock:PersonPicker ID="ppSelectNew" runat="server" Label="Assign to New" FormGroupCssClass="pull-left" Help="Select a new person to match to the bank account." IncludeBusinesses="true" OnSelectPerson="ppSelectNew_SelectPerson" ExpandSearchOptions="true" />
                                            <Rock:RockControlWrapper ID="rcwEnvelope" runat="server" Label="Envelope #" Help="Select a person based on their assigned envelope number">
                                                <Rock:RockTextBox ID="tbEnvelopeNumber" runat="server" CssClass="input-width-sm pull-left" />
                                                <asp:LinkButton ID="btnFindByEnvelopeNumber" runat="server" CssClass="btn btn-default margin-l-sm" Text="Find" OnClick="btnFindByEnvelopeNumber_Click" />
                                            </Rock:RockControlWrapper>
                                            <Rock:ModalDialog ID="mdEnvelopeSearchResults" runat="server" Title="Alert" OnSaveClick="mdEnvelopeSearchResults_SaveClick" ValidationGroup="vgEnvelopeSearchResults">
                                                <Content>
                                                    <asp:Literal ID="lEnvelopeSearchResults" runat="server" />
                                                    <br />
                                                    <Rock:RockRadioButtonList ID="cblEnvelopeSearchPersons" runat="server" ValidationGroup="vgEnvelopeSearchResults" Required="true" />
                                                </Content>
                                            </Rock:ModalDialog>
                                        </div>

                                        <asp:Panel ID="pnlPreview" CssClass="contents" runat="server">
                                            <asp:Literal ID="lPersonName" runat="server" />
                                            <asp:Literal ID="lSpouseName" runat="server" />
                                            <asp:Literal ID="lCampus" runat="server" />

                                            <!-- List of addresses associated with this person -->
                                            <ul class="list-unstyled">
                                                <asp:Repeater ID="rptrAddresses" runat="server">
                                                    <ItemTemplate>
                                                        <li class="address clearfix">

                                                            <strong><%# Eval("GroupLocationTypeValue.Value") %></strong>
                                                            <p>
                                                                <%# Eval("Location.FormattedHtmlAddress") %>
                                                            </p>
                                                        </li>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </ul>
                                            <ul class="list-unstyled address-extended" style="display: none">
                                                <asp:Repeater ID="rptPrevAddresses" runat="server">
                                                    <ItemTemplate>
                                                        <li class="address clearfix">

                                                            <strong><%# Eval("GroupLocationTypeValue.Value") %></strong>
                                                            <p>
                                                                <%# Eval("Location.FormattedHtmlAddress") %>
                                                            </p>
                                                        </li>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </ul>
                                            <ul class="list-unstyled">
                                                <li>
                                                    <a class="js-address-toggle btn btn-xs btn-link" id="btnMoreAddress" title="Show additional addresses" runat="server">Show More</a>
                                                </li>
                                            </ul>
                                        </asp:Panel>
                                    </div>

                                    <div id="account_entry" class="col-md-6 body">
                                        <Rock:RockControlWrapper ID="rcwAccountSplit" runat="server">
                                            <div class="form-horizontal label-auto js-accounts">
                                                <asp:Repeater ID="rptAccounts" runat="server">
                                                    <ItemTemplate>
                                                        <Rock:CurrencyBox ID="cbAccountAmount" runat="server" Label='<%#Eval( "Name" )%>' data-account-id='<%#Eval("Id")%>' CssClass="js-account-amount input-width-md" onkeydown="javascript:return handleAmountBoxKeyPress(this, event.keyCode);" onkeyup="javascript:handleAmountBoxKeyUp(event.keyCode)" />
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </div>
                                            <asp:Panel ID="pnlAddOptionalAccount" runat="server" CssClass="row" Visible="false">
                                                <div class="col-md-8">
                                                    <Rock:RockDropDownList ID="ddlAddAccount" runat="server" CssClass="js-add-account" EnhanceForLongLists="true" />
                                                </div>
                                                <div class="col-md-4" style="margin-left: -10px">
                                                    <Rock:CurrencyBox ID="cbOptionalAccountAmount" runat="server" CssClass="input-width-md" />
                                                </div>
                                            </asp:Panel>
                                        </Rock:RockControlWrapper>
                                    </div>
                                </div>

                            </div>


                            <div class="footer">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:DynamicPlaceholder ID="phPaymentAttributeEdits" runat="server" />
                                    </div>
                                </div>

                                <Rock:NotificationBox ID="nbSaveError" runat="server" NotificationBoxType="Danger" Dismissable="true" Text="Warning. Unable to save..." />

                                <%-- note: using disabled instead of readonly so that we can set the postback value in javascript --%>
                                <Rock:RockTextBox ID="tbTransactionCode" runat="server" Label="Transaction Code" />

                                <Rock:CurrencyBox ID="cbUnallocatedAmount" runat="server" Label="Unallocated Amount" FormGroupCssClass="js-unallocated-amount has-error" Help="The unallocated amount based on the original total amount." disabled="disabled" />
                                <Rock:CurrencyBox ID="cbTotalAmount" runat="server" Label="Total Amount" CssClass="js-total-amount" Help="Allocates amounts to the above account(s) until the total amount matches what is shown on the transaction image." disabled="disabled" Text="0.00"></Rock:CurrencyBox>
                                <Rock:HiddenFieldWithClass ID="hfOriginalTotalAmount" runat="server" CssClass="js-original-total-amount" />
                                <Rock:HiddenFieldWithClass ID="hfCurrencySymbol" runat="server" CssClass="js-currencysymbol" />

                                <Rock:RockTextBox ID="tbSummary" runat="server" Label="Summary" TextMode="MultiLine" Rows="2" />
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:LinkButton ID="btnPrevious" runat="server" CssClass="btn" OnClick="btnPrevious_Click">Previous</asp:LinkButton>
                            <div class="pull-right">
                                <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-default" Visible="false" OnClick="btnCancel_Click">Cancel</asp:LinkButton>
                                <asp:LinkButton ID="btnNext" runat="server" AccessKey="n" ToolTip="Alt+n" CssClass="btn btn-primary" OnClick="btnNext_Click">Next <i class="fa fa-chevron-right"></i></asp:LinkButton>
                            </div>
                            <div class="clearfix"></div>
                        </div>
                    </div>

                </asp:Panel>
            </div>

            <Rock:ModalDialog ID="mdAccountsPersonalFilter" runat="server" Title="Accounts Filter" OnSaveClick="mdAccountsPersonalFilter_SaveClick">
                <Content>
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:AccountPicker ID="apDisplayedPersonalAccounts" runat="server" AllowMultiSelect="true" Label="Displayed Accounts" DisplayActiveOnly="true" />
                            <Rock:RockCheckBox ID="cbOnlyShowSelectedAccounts" runat="server" Label="Limit to Existing" Text="Yes"
                                Help="If a transaction already has allocated amounts (i.e teller import), should only the accounts with values be displayed by default." />
                            <Rock:AccountPicker ID="apOptionalPersonalAccounts" runat="server" AllowMultiSelect="true" Label="Optional Accounts" DisplayActiveOnly="true" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:CampusPicker ID="cpAccounts" runat="server" Label="Campus" Help="Only display selected accounts that are associated with this campus (or not associated with a campus)." />
                            <Rock:RockCheckBox ID="cbIncludeChildAccounts" runat="server" Label="Include Child Accounts" Help="Checking this option will include all child and descendant accounts under all the selected accounts." />
                            <Rock:RockCheckBox ID="cbFilterAccountsByBatchsCampus" runat="server" Label="Filter Accounts By Batch Campus" Help="If checked, only accounts that have the same campus as the batch will be shown." />
                        </div>
                    </div>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

        <script>
            // update the Total Amount UI text as amounts are edited
            function updateRemainingAccountAllocation() {
                // do currency math in Cents instead of Dollars to avoid floating point math issues
                var transactionTotalAmountCents = null;

                $('#<%=pnlView.ClientID%> .js-account-amount :input').each(function (index, elem) {
                    var accountAmountDollar = $(elem).val();
                    if (!isNaN(accountAmountDollar) && accountAmountDollar != "") {
                        transactionTotalAmountCents = (transactionTotalAmountCents || 0.00) + Number(accountAmountDollar) * 100;
                    }
                });

                var transactionTotalAmountDollars = transactionTotalAmountCents != null ? (transactionTotalAmountCents / 100).toFixed(2) : null;

                $('#<%=pnlView.ClientID%>').find('.js-total-amount :input').val(transactionTotalAmountDollars);

                $unallocatedAmountEl = $('#<%=pnlView.ClientID%>').find('.js-unallocated-amount');

                var originalTotalAmountCents = Number($('#<%=pnlView.ClientID%>').find('.js-original-total-amount').val());
                var unallocatedAmountCents = 0;
                if (originalTotalAmountCents && originalTotalAmountCents > 0) {
                    unallocatedAmountCents = originalTotalAmountCents - (transactionTotalAmountCents || 0);
                }

                $unallocatedAmountEl.find(':input').val((unallocatedAmountCents / 100).toFixed(2));
                if (Math.round(unallocatedAmountCents) == 0) {
                    $unallocatedAmountEl.hide();
                }
                else {
                    $unallocatedAmountEl.show();
                }

            }

            Sys.Application.add_load(function () {
                Rock.controls.fullScreen.initialize();

                if ($('#<%=hfDoFadeIn.ClientID%>').val() == "1") {
                    $('#<%=pnlView.ClientID%>').rockFadeIn();
                }

                $('#<%=btnNext.ClientID%>').click(verifyUnallocated);

                updateRemainingAccountAllocation();

                $('.js-address-toggle').on('click', function (e) {
                    if (e && e.preventDefault) {
                        e.preventDefault();
                    }
                    else if (e) {
                        e.returnValue = false;
                    }
                    var link = $(this);

                    $('.address-extended').slideToggle(function() {
                        if ($(this).is(':visible')) {
                            link.text('Show Less').prop('title', 'Hide additional addresses');
                        } else {
                            link.text('Show More').prop('title', 'Show additional addresses');
                        }
                        $("#account_entry").height($("#individual_details").height());
                    });
                });

                // sort the amount boxes in the order that they were added
                $('.js-accounts .currency-box').detach().sort(function (a, b) {
                    var sortA = $(a).find("input").data("sort-order");
                    var sortB = $(b).find("input").data("sort-order");
                    if (sortA < sortB)
                        return -1
                    if (sortA > sortB)
                        return 1
                    return 0;

                }).appendTo('.js-accounts');

            })

            // handle onkeypress for the account amount input boxes
            function handleAmountBoxKeyPress(element, keyCode) {
                // if Enter was pressed when in one of the Amount boxes, click the Next button.
                if (keyCode == 13) {
                    $('#<%=btnNext.ClientID%>')[0].click();
                    return false;
                }
                else if (keyCode == 40) {
                    // pressing the down arrow goes to the next input or to the Next button
                    var clientId = element.getAttribute('id');
                    // find the "next" textbox
                    var textbox = $('#' + clientId).parent().parent().next().find('input');
                    if (textbox.length != 0) {
                        textbox.focus();
                    }
                    else {
                        $('#<%=btnNext.ClientID%>').focus();
                    }
                }
                else if (keyCode == 38) {
                    // pressing the up arrow goes to the previous input
                    var clientId = element.getAttribute('id');
                    // find the "previous" textbox
                    var textbox = $('#' + clientId).parent().parent().prev().find('input');
                    if (textbox.length != 0) {
                        textbox.focus();
                    }
                }
            }

            // handle onkeyup for the account amount input boxes
            function handleAmountBoxKeyUp(keyCode) {
                updateRemainingAccountAllocation();
            }

            // handle btnNext so that it warns if the total amount was changed from the original (if there was an amount to start with)
            function verifyUnallocated(e) {
                $unallocatedAmountEl = $('#<%=pnlView.ClientID%>').find('.js-unallocated-amount');
                if ($unallocatedAmountEl.is(':visible')) {
                    if (Number($unallocatedAmountEl.find('input').val()) != 0) {
                        e.preventDefault();

                        var originalTotalAmountCents = Number($('#<%=pnlView.ClientID%>').find('.js-original-total-amount').val());
                        var totalAmountCents = Number($('#<%=pnlView.ClientID%>').find('.js-total-amount :input').val()) * 100;
                        var currencySymbol = $('#<%=pnlView.ClientID%>').find('.js-currencysymbol').val()
                        var warningMsg = 'Note: The original transaction amount was ' + currencySymbol + (originalTotalAmountCents / 100).toFixed(2) + '. This has been changed to ' + currencySymbol + (totalAmountCents / 100).toFixed(2) + '. Are you sure you want to proceed with this change?';
                        Rock.dialogs.confirm(warningMsg, function (result) {
                            if (result) {
                                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                            }
                        });
                    }
                }
                else {
                    return true;
                }
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
