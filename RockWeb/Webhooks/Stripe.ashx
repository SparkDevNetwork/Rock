<%@ WebHandler Language="C#" Class="StripeHook" %>
using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.IO;
using Stripe;
using io.lanio.stripe.jobs;
using System.Web.Hosting;
using io.lanio.stripe.util;

public class StripeHook : IHttpHandler
{
    ImportLogger log = ImportLogger.Instance;
    public bool IsReusable
    {
        get { return true; }
    }

    public void ProcessRequest(HttpContext context)
    {
        log.SetPath(string.Format("{0}/{1}", HostingEnvironment.MapPath("~/App_Data/Logs/"), "StripeLog.txt"), false);
        var json = new StreamReader(context.Request.InputStream).ReadToEnd();
        var stripeFKId =  AttributeCache.Read(io.lanio.stripe.SystemGuid.Attribute.STRIPE_CUSTOMER_TOKEN.AsGuid()).Id;

        var stripeEvent = StripeEventUtility.ParseEvent(json);
        var accId = -1;
        using (var rc = new Rock.Data.RockContext())
        {   
            var accGuid = Guid.Parse(io.lanio.stripe.SystemGuid.Accounts.STRIPE_WEBHOOK_FUND);
            accId = new FinancialAccountService(rc).Queryable().Where(fa => fa.Guid == accGuid).Select(fa => fa.Id).FirstOrDefault();
        }
            switch (stripeEvent.Type)
            {

            case StripeEvents.CustomerCreated:

                StripeCustomer stripeCus = Stripe.Mapper<StripeCustomer>.MapFromJson(stripeEvent.Data.Object.ToString());
                using (var rc = new Rock.Data.RockContext())
                {

                    var ps = new PersonService(rc);
                    var avs = new AttributeValueService(rc);

                    var person = Util.GetStripePerson(stripeCus.Id);
                    if(person == null)
                        person = ps.Queryable().Where(p => p.Email == stripeCus.Email).FirstOrDefault();

                    if (person == null)
                    {
                        person = new Rock.Model.Person();
                        person.IsEmailActive = true;
                        person.RecordTypeValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;

                        person.Aliases.Add(new PersonAlias { AliasPerson = person, AliasPersonGuid = person.Guid, Guid = Guid.NewGuid() });

                        ps.Add(person);
                    }

                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.Email = stripeCus.Email;

                    Util.SetStripeFK(person, stripeCus.Id, rc);
                    
                    person.FirstName = stripeCus.Email;
                    person.LastName = stripeCus.Email;

                    rc.SaveChanges();
                }
                break;


            case StripeEvents.ChargeSucceeded:

                StripeCharge stripeCharge = Stripe.Mapper<StripeCharge>.MapFromJson(stripeEvent.Data.Object.ToString());
                var stripePersonFK = stripeCharge.CustomerId;
                
                if (stripeCharge.Paid)
                {
                    log.Write(string.Format("recived stripe charge\n{0}", stripeEvent.Data.Object.ToString()));
                    using (var rc = new Rock.Data.RockContext())
                    {

                        var ps = new PersonService(rc);
                        var fbs = new FinancialBatchService(rc);
                        var fts = new FinancialTransactionService(rc);
                        var ftds = new FinancialTransactionDetailService(rc);
                        var fpds = new FinancialPaymentDetailService(rc);

                        //see if the charge already exsists
                        var transCode = fts.Queryable().Any(f => f.TransactionCode == stripeCharge.Id);
                        if (transCode)
                        {
                            break;
                        }

                        string email = stripeCharge.ReceiptEmail;
                        var person = Util.GetStripePerson(stripePersonFK);
                        if(person == null)
                            person = new PersonService(rc).Queryable().Where(p => p.Email == email).FirstOrDefault();

                        if (person == null)
                        {
                            person = new Rock.Model.Person();
                            person.IsEmailActive = true;
                            person.EmailPreference = EmailPreference.EmailAllowed;
                            person.RecordTypeValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;

                            string name = stripeCharge.Source.Card.Name;
                            if (name.Split(' ').Count() < 2)
                            {
                                person.FirstName = stripeCharge.Source.Card.Name;
                                person.LastName = stripeCharge.Source.Card.Name;
                            }
                            else if (name.Split(' ').Count() == 2)
                            {
                                person.FirstName = stripeCharge.Source.Card.Name.Split(' ')[0];
                                person.LastName = stripeCharge.Source.Card.Name.Split(' ')[1];
                            }
                            else if (name.Split(' ').Count() == 3)
                            {
                                person.FirstName = stripeCharge.Source.Card.Name.Split(' ')[0];
                                person.MiddleName = stripeCharge.Source.Card.Name.Split(' ')[1];
                                person.LastName = stripeCharge.Source.Card.Name.Split(' ')[2];
                            }
                            else if (name.Split(' ').Count() > 3)
                            {
                                person.FirstName = stripeCharge.Source.Card.Name.Split(' ')[0];
                                person.LastName = stripeCharge.Source.Card.Name.Substring(stripeCharge.Source.Card.Name.IndexOf(" "));
                            }

                            person.Email = stripeCharge.ReceiptEmail;
                            person.Guid = Guid.NewGuid();
                            Util.SetStripeFK(person, stripePersonFK, rc);
                            person.Aliases.Add(new PersonAlias { AliasPerson = person, AliasPersonGuid = person.Guid, Guid = Guid.NewGuid() });

                            ps.Add(person);
                        }


                        //add batch
                        var batch = fbs.Queryable().Where(b => b.Status == BatchStatus.Open).FirstOrDefault();
                        if (batch == null || batch.BatchEndDateTime < DateTime.Now)
                        {
                            if (batch.BatchEndDateTime < DateTime.Now)
                            {
                                batch.Status = BatchStatus.Closed;
                                rc.SaveChanges();
                            }

                            batch = new Rock.Model.FinancialBatch();
                            batch.Name = DateTime.Now.ToMonthDayString();
                            batch.BatchStartDateTime = DateTime.Now;
                            batch.BatchEndDateTime = DateTime.Now.AddDays(1);
                            batch.Status = BatchStatus.Open;
                            batch.Guid = Guid.NewGuid();
                            batch.ControlAmount = 0;
                            fbs.Add(batch);
                        }
                        batch.ControlAmount += ((decimal)stripeCharge.Amount) * (decimal)0.01;
                        //add transaction
                        var trans = new Rock.Model.FinancialTransaction();
                        trans.Batch = batch;
                        trans.TransactionTypeValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION).Id;
                        trans.SourceTypeValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE).Id;
                        trans.CreatedDateTime = DateTime.Now;
                        trans.TransactionDateTime = stripeCharge.Created;
                        trans.ForeignKey = stripeCharge.Id;
                        trans.AuthorizedPersonAlias = person.Aliases.FirstOrDefault();
                        trans.TransactionCode = stripeCharge.Id;
                        trans.Guid = Guid.NewGuid();
                        //add transaction detail
                        var td = new Rock.Model.FinancialTransactionDetail();
                        td.Transaction = trans;
                        td.AccountId = accId;
                        td.Amount = ((decimal)stripeCharge.Amount) * (decimal)0.01;
                        td.EntityId = person.Id;
                        td.EntityTypeId = EntityTypeCache.Read(Rock.SystemGuid.EntityType.PERSON.AsGuid()).Id;
                        td.CreatedDateTime = DateTime.Now;
                        td.ModifiedDateTime = DateTime.Now;
                        td.CreatedByPersonAlias = person.Aliases.FirstOrDefault();
                        td.ForeignKey = stripeCharge.Id;
                        td.Guid = Guid.NewGuid();
                        //add payment detail
                        var fpd = new FinancialPaymentDetail();
                        var curTypeId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid()).Id;
                        fpd.CurrencyTypeValueId = curTypeId;

                        trans.FinancialPaymentDetailId = fpd.Id;

                        fpds.Add(fpd);
                        fts.Add(trans);
                        ftds.Add(td);

                        log.Write(string.Format("Now is {0}", DateTime.Now));
                        rc.SaveChanges();

                    }
                }
                break;


            case StripeEvents.CustomerSubscriptionCreated:

                StripeSubscription stripeSub = Stripe.Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());
                
                using (var rc = new Rock.Data.RockContext())
                {
                    var fsts = new FinancialScheduledTransactionService(rc);
                    var fstds = new FinancialScheduledTransactionDetailService(rc);
                    var fpds = new FinancialPaymentDetailService(rc);
                    var ps = new PersonService(rc);

                    var person = Util.GetStripePerson(stripeSub.CustomerId);
                    if(person == null)
                    {
                        log.Write(string.Format("Faild to create Scheduled Transactions because  could not fine customer with ID : {0}", stripeSub.CustomerId));
                        break;
                    }

                    //see if schedule already exsists
                    var subId = fsts.Queryable().Any(s => s.GatewayScheduleId == stripeSub.Id);
                    if (subId)
                    {
                        break;
                    }
                    
                    // make the payment details
                    var fpd = new FinancialPaymentDetail();
                    var curTypeId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid()).Id;
                    fpd.CurrencyTypeValueId = curTypeId;

                    //make the scheduled transaction
                    var st = new FinancialScheduledTransaction();

                    st.CreatedDateTime = stripeSub.Created;
                    st.ModifiedDateTime = stripeSub.Created;
                    if (stripeSub.Start.HasValue) st.StartDate = stripeSub.Start.Value;
                    if (stripeSub.EndedAt.HasValue) st.EndDate = stripeSub.EndedAt.Value;
                    st.IsActive = true;
                    st.AuthorizedPersonAlias = person.PrimaryAlias;
                    st.FinancialPaymentDetail = fpd;
                    st.GatewayScheduleId = stripeSub.Id;
                    st.TransactionCode = stripeSub.Id;

                    if (stripeSub.StripePlan.Interval == "month") st.TransactionFrequencyValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY).Id;
                    else if (stripeSub.StripePlan.Interval == "week") st.TransactionFrequencyValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY).Id;
                    else if (stripeSub.StripePlan.Interval == "year") st.TransactionFrequencyValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_YEARLY).Id;
                    else
                    {
                        log.Write(string.Format("Faild to create Scheduled Transactions because the stripe plan has a frequency that Rock does not support" ));
                        break;
                    }

                    //make the scheduled transaction detail
                    var std = new FinancialScheduledTransactionDetail();
                    std.ScheduledTransaction = st;
                    std.AccountId = 1;
                    std.Amount = ((decimal)stripeSub.StripePlan.Amount) * (decimal)0.01;
                    std.CreatedDateTime = stripeSub.StripePlan.Created;
                    std.ModifiedDateTime = stripeSub.Created;

                    fpds.Add(fpd);
                    fsts.Add(st);
                    fstds.Add(std);

                    rc.SaveChanges();

                }


                break;

            case StripeEvents.InvoicePaymentSucceeded:

                StripeInvoice stripeInvoice = Stripe.Mapper<StripeInvoice>.MapFromJson(stripeEvent.Data.Object.ToString());

                using (var rc = new Rock.Data.RockContext())
                {

                    var ps = new PersonService(rc);
                    var fbs = new FinancialBatchService(rc);
                    var fts = new FinancialTransactionService(rc);
                    var ftds = new FinancialTransactionDetailService(rc);
                    var fpds = new FinancialPaymentDetailService(rc);
                    var fsts = new FinancialScheduledTransactionService(rc);
                    
                    var person = Util.GetStripePerson(stripeInvoice.CustomerId);
                    if(person == null)
                    {
                        log.Write(string.Format("Failed to add invoice payment because there is no customer with the id: {0}", stripeInvoice.CustomerId));
                        break;
                    }

                    //see if transaction already happened
                    var transCode = fts.Queryable().Any(f => f.TransactionCode == stripeInvoice.ChargeId);
                    if(transCode){
                        break;
                    }

                    var batch = fbs.Queryable().Where(b => b.Status == BatchStatus.Open).FirstOrDefault();
                    if (batch == null || batch.BatchEndDateTime < DateTime.Now)
                    {
                        if (batch != null)
                        {
                            batch.Status = BatchStatus.Closed;

                            rc.SaveChanges();
                        }

                        batch = null;
                        batch = new Rock.Model.FinancialBatch();
                        batch.Name = DateTime.Now.ToMonthDayString();
                        batch.BatchStartDateTime = DateTime.Now;
                        batch.BatchEndDateTime = DateTime.Now.AddDays(1);
                        batch.Status = BatchStatus.Open;
                        batch.Guid = Guid.NewGuid();
                        batch.ControlAmount = 0;
                        fbs.Add(batch);
                    }
                    batch.ControlAmount += ((decimal)stripeInvoice.Total) * (decimal)0.01;

                    var st = fsts.Queryable().Where(t => t.GatewayScheduleId == stripeInvoice.SubscriptionId).FirstOrDefault();
                    if(st == null)
                    {
                        log.Write(string.Format("Failed to add invoice payment because there is no scheduled transaction with an id: {0}", stripeInvoice.SubscriptionId));
                        break;
                    }
                    //add transaction
                    var trans = new Rock.Model.FinancialTransaction();
                    trans.Batch = batch;
                    trans.TransactionTypeValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION).Id;
                    trans.SourceTypeValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE).Id;
                    trans.CreatedDateTime = DateTime.Now;
                    trans.TransactionDateTime = stripeInvoice.Date.Value;
                    trans.ForeignKey = stripeInvoice.ChargeId;
                    trans.AuthorizedPersonAlias = person.Aliases.FirstOrDefault();
                    trans.TransactionCode = stripeInvoice.ChargeId;
                    trans.Guid = Guid.NewGuid();
                    trans.ScheduledTransaction = st;
                    //add transaction detail
                    var td = new Rock.Model.FinancialTransactionDetail();
                    td.Transaction = trans;
                    td.AccountId = accId;
                    td.Amount = ((decimal)stripeInvoice.Total) * (decimal)0.01;
                    td.EntityId = person.Id;
                    td.EntityTypeId = EntityTypeCache.Read(Rock.SystemGuid.EntityType.PERSON.AsGuid()).Id;
                    td.CreatedDateTime = DateTime.Now;
                    td.ModifiedDateTime = DateTime.Now;
                    td.CreatedByPersonAlias = person.Aliases.FirstOrDefault();
                    td.ForeignKey = stripeInvoice.ChargeId;
                    td.Guid = Guid.NewGuid();
                    //add payment detail
                    var fpd = new FinancialPaymentDetail();
                    var curTypeId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid()).Id;
                    fpd.CurrencyTypeValueId = curTypeId;

                    trans.FinancialPaymentDetailId = fpd.Id;

                    fpds.Add(fpd);
                    fts.Add(trans);
                    ftds.Add(td);
                        
                    
                    rc.SaveChanges();

                }


                break;
        }
        
        context.Response.StatusCode = 200;
        
    }
}


