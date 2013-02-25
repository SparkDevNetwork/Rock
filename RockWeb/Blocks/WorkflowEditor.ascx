<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowEditor.ascx.cs" Inherits="RockWeb.Blocks.WorkflowEditor" %>

<fieldset>
    <legend>Activities
        <span class="pull-right">
            <a class="btn btn-mini"><i class="icon-plus"></i>Add Activity</a>
        </span>
    </legend>

    <div class="workflow-activity-list">

        <section class="widget widget-dark workflow-activity" data-key="exampledatakey01">
            <header class="clearfix clickable">
                <div class="filter-toogle pull-left">
                    <h3>Inititalize Workflow</h3>
                    Used to initialize the workflow and setup attributes with their default values.
                </div>
                <div class="pull-right">
                    <a class="btn btn-mini workflow-activity-reorder"><i class="icon-reorder"></i></a>
                    <a class="btn btn-mini"><i class="workflow-activity-state icon-chevron-down"></i></a>
                    <a class="btn btn-mini btn-danger"><i class="icon-remove"></i></a>
                </div>
            </header>

            <div class="widget-content" style="display: none;">
                # Put Activity Edit Fields Here #
            <p></p>
                <fieldset>
                    <legend>Actions
                    <span class="pull-right">
                        <a class="btn btn-mini"><i class="icon-plus"></i>Add Action</a>
                    </span>
                    </legend>

                    <div class="workflow-action-list">

                        <article class="widget workflow-action">
                            <header class="clearfix clickable">
                                <div class="pull-left">
                                    Set Neighborhood Pastor
                                </div>
                                <div class="pull-right">
                                    <a class="btn btn-mini workflow-action-reorder"><i class="icon-reorder"></i></a>
                                    <a class="btn btn-mini"><i class="workflow-action-state icon-chevron-down"></i></a>
                                    <a class="btn btn-mini btn-danger"><i class="icon-remove"></i></a>
                                </div>
                            </header>
                            <div class="widget-content" style="display: none;">
                                Put Action field controls here
                        <p>
                            Got it?
                        </p>
                            </div>
                        </article>

                        <article class="widget workflow-action">
                            <header class="clearfix clickable">
                                <div class="pull-left">
                                    Set Requestor Details
                                </div>
                                <div class="pull-right">
                                    <a class="btn btn-mini workflow-action-reorder"><i class="icon-reorder"></i></a>
                                    <a class="btn btn-mini"><i class="workflow-action-state icon-chevron-down"></i></a>
                                    <a class="btn btn-mini btn-danger"><i class="icon-remove"></i></a>
                                </div>
                            </header>
                            <div class="widget-content" style="display: none;">
                                Put Action field controls here
                        <p>
                            Got it?
                        </p>
                            </div>
                        </article>

                    </div>

                </fieldset>
            </div>
        </section>

        <section class="widget widget-dark workflow-activity" data-key="exampledatakey02">
            <header class="clearfix clickable">
                <div class="filter-toogle pull-left">
                    <h3>Send Emails</h3>
                    Sends emails to the approval chain defined by the HR org chart.
                </div>
                <div class="pull-right">
                    <a class="btn btn-mini workflow-activity-reorder"><i class="icon-reorder"></i></a>
                    <a class="btn btn-mini"><i class="workflow-activity-state icon-chevron-down"></i></a>
                    <a class="btn btn-mini btn-danger"><i class="icon-remove"></i></a>
                </div>
            </header>

            <div class="widget-content" style="display: none;">
                # Put Activity Edit Fields Here #
            <p></p>
                <fieldset class="workflow-action-list">
                    <legend>Actions
                    <span class="pull-right">
                        <a class="btn btn-mini"><i class="icon-plus"></i>Add Action</a>
                    </span>
                    </legend>

                    <article class="widget workflow-action">
                        <header class="clearfix clickable">
                            <div class="pull-left">
                                Send Email to Manager
                            </div>
                            <div class="pull-right">
                                <a class="btn btn-mini workflow-action-reorder"><i class="icon-reorder"></i></a>
                                <a class="btn btn-mini"><i class="workflow-action-state icon-chevron-down"></i></a>
                                <a class="btn btn-mini btn-danger"><i class="icon-remove"></i></a>
                            </div>
                        </header>
                        <div class="widget-content" style="display: none;">
                            Put Action field controls here
                        <p>
                            Got it?
                        </p>
                        </div>
                    </article>

                    <article class="widget workflow-action">
                        <header class="clearfix clickable">
                            <div class="pull-left">
                                Send Email to Area Head
                            </div>
                            <div class="pull-right">
                                <a class="btn btn-mini workflow-action-reorder"><i class="icon-reorder"></i></a>
                                <a class="btn btn-mini"><i class="workflow-action-state icon-chevron-down"></i></a>
                                <a class="btn btn-mini btn-danger"><i class="icon-remove"></i></a>
                            </div>
                        </header>
                        <div class="widget-content" style="display: none;">
                            Put Action field controls here
                        <p>
                            Got it?
                        </p>
                        </div>
                    </article>

                </fieldset>
            </div>
        </section>

    </div>
</fieldset>


<script>
    var fixHelper = function (e, ui) {
        ui.children().each(function () {
            $(this).width($(this).width());
        });
        return ui;
    };

    $('.workflow-activity-list').sortable({
        helper: fixHelper,
        handle: '.workflow-activity-reorder',
        containment: 'parent',
        start: function (event, ui) {
            {
                var start_pos = ui.item.index();
                ui.item.data('start_pos', start_pos);
            }
        },
        update: function (event, ui) {
            {
                __doPostBack('{1}', 're-order-activity:' + ui.item.attr('data-key') + ';' + ui.item.data('start_pos') + ';' + ui.item.index());
            }
        }
    }).disableSelection();

    $('.workflow-action-list').sortable({
        helper: fixHelper,
        handle: '.workflow-action-reorder',
        containment: 'parent',
        start: function (event, ui) {
            {
                var start_pos = ui.item.index();

                ui.item.data('start_pos', start_pos);
            }
        },
        update: function (event, ui) {
            {
                __doPostBack('{1}', 're-order-action:' + ui.item.attr('data-key') + ';' + ui.item.data('start_pos') + ';' + ui.item.index());
            }
        }
    }).disableSelection();


    // activity animation
    $('.workflow-activity > header').click(function () {
        $(this).siblings('.widget-content').slideToggle();

        $('i.workflow-activity-state', this).toggleClass('icon-chevron-down');
        $('i.workflow-activity-state', this).toggleClass('icon-chevron-up');
    });

    // fix so that the Remove button will fire its event, but not the parent event 
    $('.workflow-activity .icon-remove').click(function (event) {
        event.stopImmediatePropagation();
    });

    // fix so that the Reorder button will fire its event, but not the parent event 
    $('.workflow-activity .icon-reorder').click(function (event) {
        event.stopImmediatePropagation();
    });

    // action animation
    $('.workflow-action > header').click(function () {
        $(this).siblings('.widget-content').slideToggle();

        $('i.workflow-action-state', this).toggleClass('icon-chevron-down');
        $('i.workflow-action-state', this).toggleClass('icon-chevron-up');
    });

    // fix so that the Remove button will fire its event, but not the parent event 
    $('.workflow-action .icon-remove').click(function (event) {
        event.stopImmediatePropagation();
    });

    // fix so that the Reorder button will fire its event, but not the parent event 
    $('.workflow-action .icon-reorder').click(function (event) {
        event.stopImmediatePropagation();
    });
</script>
