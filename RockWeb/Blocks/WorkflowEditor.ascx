<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowEditor.ascx.cs" Inherits="RockWeb.Blocks.WorkflowEditor" %>

<fieldset>
    <legend>Activities
        <span class="pull-right">
            <a class="btn btn-mini"><i class="icon-plus"></i>Add Activity</a>
        </span>
    </legend>

    <section class="widget widget-dark workflow-activity">
        <header class="clearfix clickable">
            <div class="filter-toogle pull-left">
                <h3>Inititalize Workflow</h3>
                Used to initialize the workflow and setup attributes with their default values.
            </div>
            <div class="pull-right">
                <a class="btn btn-mini"><i class="icon-reorder"></i></a>
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

                <article class="widget workflow-action">
                    <header class="clearfix clickable">
                        <div class="pull-left">
                            Set Neighborhood Pastor
                        </div>
                        <div class="pull-right">
                            <a class="btn btn-mini"><i class="icon-reorder"></i></a>
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
                            <a class="btn btn-mini"><i class="icon-reorder"></i></a>
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

    <section class="widget widget-dark workflow-activity">
        <header class="clearfix clickable">
            <div class="filter-toogle pull-left">
                <h3>Send Emails</h3>
                Sends emails to the approval chain defined by the HR org chart.
            </div>
            <div class="pull-right">
                <a class="btn btn-mini"><i class="icon-reorder"></i></a>
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

                <article class="widget workflow-action">
                    <header class="clearfix clickable">
                        <div class="pull-left">
                            Send Email to Manager
                        </div>
                        <div class="pull-right">
                            <a class="btn btn-mini"><i class="icon-reorder"></i></a>
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
                            <a class="btn btn-mini"><i class="icon-reorder"></i></a>
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
</fieldset>

<script>
    /*
    // activity annimation
    $('.workflow-activity > header').click(function () {
        $(this).siblings('.widget-content').slideToggle();

        $('i.workflow-activity-state', this).toggleClass('icon-chevron-down');
        $('i.workflow-activity-state', this).toggleClass('icon-chevron-up');

    });

    // action annimation
    $('.workflow-action > header').click(function () {
        $(this).siblings('.widget-content').slideToggle();

        $('i.workflow-action-state', this).toggleClass('icon-chevron-down');
        $('i.workflow-action-state', this).toggleClass('icon-chevron-up');

    });
    */
</script>
