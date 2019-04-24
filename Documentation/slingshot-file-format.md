# Slingshot File Format

A zip file named *.slingshot with the following sub files:

* family-note.csv
* financial-account.csv
* financial-batch.csv
* financial-pledge.csv
* financial-transaction.csv
* financial-transactiondetail.csv
* group.csv
* groupmember.csv
* grouptype.csv
* person-address.csv
* person-attribute.csv
* person-attributevalue.csv
* person-note.csv
* person-phone.csv
* person.csv

# Sub File Formats

## family-note.csv

`FamilyId,Id,NoteType,Caption,IsAlert,IsPrivateNote,Text,DateTime,CreatedByPersonId`

Example:
`8888,1234567890,Family Member Death,,False,False,Joe's wife Samantha (Sam) Smith died 9/1/2003,9/1/2016 12:00:00 AM,`


## financial-account.csv
`Id,Name,IsTaxDeductible,CampusId,ParentAccountId`

Example:
`1111,Staff Parish Misc,False,,`


## financial-batch.csv

`Id,Name,CampusId,StartDate,EndDate,Status,CreatedByPersonId,CreatedDateTime,ModifiedByPersonId,ModifiedDateTime,ControlAmount`

Example:
`9876543210,Imported Batch: 4/21/2018,,4/21/2018 12:00:00 AM,4/21/2018 12:00:00 AM,Closed,,,,,0`


## financial-pledge.csv

`Id,PersonId,AccountId,StartDate,EndDate,PledgeFrequency,TotalAmount,CreatedDateTime,ModifiedDateTime`

Example:
`99,123,888,1/1/2016 12:00:00 AM,12/31/2016 12:00:00 AM,OneTime,50000.00,,`


## financial-transaction.csv

`Id,BatchId,AuthorizedPersonId,TransactionDate,TransactionType,TransactionSource,CurrencyType,Summary,TransactionCode,CreatedByPersonId,CreatedDateTime,ModifiedByPersonId,ModifiedDateTime`

Example:
`333333,1231231230,44444,6/11/2018 12:00:00 AM,Contribution,BankChecks,Check,ACS PaymentType: Check,006060,,6/11/2018 12:00:00 AM,,`


## financial-transactiondetail.csv

`Id,TransactionId,AccountId,Amount,Summary,CreatedByPersonId,CreatedDateTime,ModifiedByPersonId,ModifiedDateTime`

Example:
`1,181818,900,100.00,,,6/21/2012 12:00:00 AM,,`


## group.csv

`Id,Name,Order,ParentGroupId,GroupTypeId,CampusId`

Example:
`3213213210,Activities,0,0,9999,`


## groupmember.csv

`PersonId,GroupId,Role`

Example:
`14,3213213210,Member`


## grouptype.csv

`Id,Name`

Example:
`9999,Imported Group`


## person-address.csv

`PersonId,Street1,Street2,City,State,PostalCode,Country,Latitude,Longitude,AddressType`

Example:
`14,777 Cloud Dr,,Peoria,AZ,85301-1111,USA,,,Home`


## person-attribute.csv

`Key,Name,FieldType,Category`

Example(s):
`OurSpecialAttribKey_,Our Special Thing,Rock.Field.Types.TextFieldType,Imported Attributes`
`ExtBaptism,External Baptism Date,Rock.Field.Types.DateTimeFieldType,Imported Attributes`


## person-attributevalue.csv

`PersonId,AttributeKey,AttributeValue`

Example(s):
`14,OurSpecialAttribKey_,Unknown Pre2015`
`14,ExtBaptism,1978-10-04T00:00:00.0000000`


## person-note.csv

`PersonId,Id,NoteType,Caption,IsAlert,IsPrivateNote,Text,DateTime,CreatedByPersonId`

Example(s):
`14,1991991991,Emergency Contact,,False,False,Primary Care Doctor:  Emmit Brown  888-8888  Contact:  Amy Adams  w. 771-4400  page 865-1234                 John Jones    cell 903-5555,3/12/2008 12:00:00 AM,`
`14,123456789,Family Information,,False,False,"Married Rebecca Lovell on July 10, 2018.  Possible address: 33 S Main St Quartzsite, AZ 85346-1111",7/10/2018 12:00:00 AM,`


## person-phone.csv

`PersonId,PhoneType,PhoneNumber,IsMessagingEnabled,IsUnlisted`

Example(s):
`14,Home,480-555-1234,,`
`14,Parent's Phone,602-555-1212,,False`


## person.csv

`Id,FamilyId,FamilyName,FamilyImageUrl,FamilyRole,FirstName,NickName,LastName,MiddleName,Salutation,Suffix,Email,Gender,MaritalStatus,Birthdate,AnniversaryDate,RecordStatus,InactiveReason,ConnectionStatus,EmailPreference,CreatedDateTime,ModifiedDateTime,PersonPhotoUrl,CampusId,CampusName,Note,Grade,GiveIndividually,IsDeceased`

Example(s):
`14,10000,Mr Bobby Brown,,Adult,Robert,Bob,Brown,Wayne,Mr,,,Male,Single,,,Active,,Member,EmailAllowed,5/10/1997 12:00:00 AM,8/20/2016 12:00:00 AM,,0,,,,True,False`
`15,10001,Mr Joey Rogers & Ms Rolyn Washington,C:\Temp\Photos\acsThumb999.jpg,Adult,Rolyn,,Washington,Sue,Ms,,rolyn_washington@fakeinbox.com,Female,Single,9/26/1990 12:00:00 AM,,Active,,Member,EmailAllowed,6/9/1997 12:00:00 AM,10/24/2017 12:00:00 AM,C:\Temp\Photos\acsThumb998.jpg,0,,,,True,False`


# Proposed 2/26/2019

The following are new, proposed file format definitions that are being considered for addition to the Slingshot importer.

## connection.csv

`ConnectionType,ConnectionOpportunity,PersonId,Comments,FollowupDate`

## group-locationschedule.csv

`GroupId,Location,ScheduleName,iCalendarContent,CheckInStartOffsetMinutes,CheckInEndOffsetMinutes,EffectiveStartDate,Category`

## person-follow.csv

`PersonId,EntityId,EntityType`

## person-knownrelationship.csv

`FromPersonId,ToPersonId,GroupRole`

## person-userlogin.csv

`PersonId,UserName,SecurityType`

## registrationtemplate.csv

`Id,Name,Category,GroupTypeId,AllowMultipleRegistrants,AllowGroupPlacement,MaxRegistrants,RegistrantsSameFamily,Notify,SetCostOnInstance,Cost,MinimumInitialPayment,AllowExternalRegistrationUpdates,AddPersonNote,LoginRequired,FinancialGateway,BatchNamePrefix,PaymentReminderTimeSpan,RegistrationTerm,RegistrantTerm,DiscountTerm,FeeTerm`

## registrationtemplate-fee.csv

`Id,RegistrationTemplateId,Name,Type,DiscountApplies`

## registrationtemplate-feeitem.csv

`RegistrationTemplateFeeId,Name,Cost`

## registrationtemplate-discount.csv

`Id,RegistrationTemplateId,Code,AutoApplyDiscount,DiscountPercentage,DiscountAmount`

## registrationtemplate-form.csv

`Id,RegistrationTemplateId,Name`

## registrationtemplate-formfield.csv

`RegistrationTemplateFormId,FieldSource,PersonFieldType,PreText,PostText,IsGridField,IsRequired`

## registrationinstance.csv

`Id,Name,RegistrationTemplateId,StartDateTime,EndDateTime,SendReminderDateTime,MaxAttendees,AccountId,ContactPhone,ContactEmail,PersonId,AdditionalReminderDetails,AdditionalConfirmationDetails`







