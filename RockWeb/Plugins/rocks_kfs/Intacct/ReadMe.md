![Kingdom First Solutions](https://user-images.githubusercontent.com/81330042/113314137-9628ab80-92d1-11eb-9541-5f95c2ba1d92.png)



# Intacct Export to Journal 
_Tested/Supported in Rock Version:  8.0-13.0_    
_Released:  10/9/2018_   
_Updated:  2/2/2022_   



## Summary 

This plug in will allow you to create journal entries in Intacct for batches with the click of a button.

Quick Links:

- [What's New](#whats-new)
- [Configuration](#configuration)
- [Advanced Configuration](#advanced-configuration)

## What's New

The following new goodness will be added to your Rock install with this plugin:

- **New Block**: Batch to Journal (added to the Batch Detail Page on install)  
- **New Account Attributes**: There are a number of new Account attributes that control where transactions are posted in Intacct  
- **New Page**: Intacct Projects (Finance > Administration > Intacct Projects)  
- **New Defined Type**: Financial Projects stores the Defined Values that designate what Project a Transaction should be associated with  
- **New Batch Attribute**: Date Exported  

## Configuration

There is configuration needed in Intacct. You may need to contact the Intacct Administrator for your organization for help with these steps.

### **Intacct Configuration** 

**Create a new Role**

In Intacct, go to Company > Roles

![IntacctCreateRoleName](https://user-images.githubusercontent.com/81330042/113312495-f880ac80-92cf-11eb-9876-f5ac28f632ad.png)

```
    Name: API Journal
    Description: Used to make Journals via API
```

After you save the Role, the next screen will allow you to assign subscriptions to the Role

![IntacctCreateRoleApplication](https://user-images.githubusercontent.com/81330042/113313001-73e25e00-92d0-11eb-8a91-f203408eb11b.png)


```
    Application Module: General Ledger
    Click on Permissions
```

In the permissions window, grant All permissions for the General Ledger to the Role

![IntacctCreateRolePermissions](https://user-images.githubusercontent.com/81330042/113313610-05ea6680-92d1-11eb-9606-09606b4c73f4.png)



```
    Select the All radio button
```

Save to close the window

Then save your changes on the Role Subscriptions page

**Create a new User**

Note: Creating users can cost extra in Intacct. Only create a new user if there are not any existing API or generic users that you can use.

Go to Company > Admin > Users to add a user

![IntacctCreateUser](https://user-images.githubusercontent.com/81330042/113313372-ca4f9c80-92d0-11eb-8748-e1ee226acb4f.png)


```
    User Id: RockAPI
    Last name: API
    First name: Rock
    Email address: A valid email where you can receive the password email
    Contact name: Leave this blank to create a new contact automatically
    User name: Rock API
    User Type: Business Account
    Admin Privileges: Full
```

**Assign User Role**

Find your new or existing API user in the Users page

![Images/IntacctAssignRoleEdit](https://user-images.githubusercontent.com/81330042/113315231-af7e2780-92d2-11eb-88a8-1b0988be8cb8.png)


```
    Click the edit link next to your API user then go to the Role Information tab
```

![IntacctAssignRoleSelect](https://user-images.githubusercontent.com/81330042/113315334-c9b80580-92d2-11eb-9452-068999de7324.png)


```
    In the blank drop down, select your API Journal Role
```

Save your changes

**Create a new Employee**

Go to Company > Setup > Employees

Add a new Employee

![IntacctCreateEmployee](https://user-images.githubusercontent.com/81330042/113315437-e8b69780-92d2-11eb-864d-a9a720d19647.png)


```
    Primary contact name: RockAPI (or existing API user)
```


### **Rock Configuration** 

**Batch to Journal Block**

After install, the Batch to Journal block was added to your Batch Details page. The export button will only show up if the batch Transaction and Control amounts match.

![BatchToJournalBlock](https://user-images.githubusercontent.com/81330042/113315547-03890c00-92d3-11eb-9ee3-fe08d4ed142a.png)


You will need to configure the Batch to Journal block settings.

![BatchToJournalBlockSettings](https://user-images.githubusercontent.com/81330042/113315597-126fbe80-92d3-11eb-8a4c-4681e322a735.png)


```
    Name: Block name
    
    Enable Debug: Turns on/off the Lava debug panel

    Journal Id: The Intacct Symbol of the Journal that the Entry should be posted to (example: GJ)
    
    Journal Memo Lava: Allows you to use Lava to control what is saved in the memo column of the export. Default: {{ Batch.Id }}: {{ Batch.Name }}

    Button Text: Customize the text for the export button

    Close Batch: Flag indicating if the Financial Batch should be closed in Rock when successfully posted to Intacct.
    
    Log Response: Flag indicting if the Intacct Response should be logged to the Batch Audit Log

    Sender Id: The permanent Web Services sender Id
    
    Sender Password: The permanent Web Services sender password
    
    Company Id: The Intacct company Id. This is the same information you use when you log into the Sage Intacct UI.
    
    User Id: The Intacct API user Id. This is the same information you use when you log into the Sage Intacct UI.
    
    User Password: The Intacct API password. This is the same information you use when you log into the Sage UI.
    
    Location Id: The optional Intacct Location Id. Add a location ID to log into a multi-entity shared company. Entities are typically different locations of a single company.
```

**Financial Projects Defined Type**

You will need to define the values for the Financial Projects defined type so the export knows what GL Project to associate accounts or transactions to in Intacct. We have added a new Intacct Projects page under Finance > Administration. This page allows you to manage Intacct Projects defined values without needing the RSR-Rock Admin security role.

On the Intacct Projects page, add a value for each of your organization's Projects. The Value must be the Intacct Journal Id. Description will be a friendly name for the Project.

![FinancialProjectsDefinedValues](https://user-images.githubusercontent.com/81330042/113315670-24516180-92d3-11eb-9d69-544ac2ac463b.png)



**Account Attributes**

The export will always create (at a minimum) two lines for a Journal - a debit and a credit line. The Credit and Debit Account attributes are how this is defined.

In addition to the Intacct Dimensions included, custom Dimensions can also be added.

Most organizations will mark the GL Project designation by setting a default Project on an account in Rock. If more specific Project marking is needed, the export utility also created a Financial Transaction Detail Attribute that allows for designation at the gift level.

![AccountAttributes](https://user-images.githubusercontent.com/81330042/113315734-359a6e00-92d3-11eb-89ff-2ac2e021d851.png)


```
    Default Project: Designates the project at the financial account level.

    Credit Account: Account number to be used for the credit column. Required by Intacct.

    Debit Account: Account number to be used for the debit column. Required by Intacct.

    Class: The Intacct dimension for Class Id.

    Department: The Intacct dimension for Department Id.

    Location: The Intacct dimension for Location Id. Required if multi-entity enabled.

    Restriction: A custom Intacct dimension included for example purposes. See the Advanced Configuration section to learn how to add custom dimensions to a Rock Account. 
```

## Advanced Configuration

### **Adding Custom Dimension Example**

- Go to Admin Tools > System Settings > Entity Attributes
- In the filter options, set the Entity Type to Financial Account
- Add a new Attribute

![CustomDimension](https://user-images.githubusercontent.com/81330042/113315802-46e37a80-92d3-11eb-9c9c-c6c3b7fb2d3a.png)


```
    Name: Restriction
    
    Categories: Intacct Export
    
    Key: GLDIMRESTRICTION
    
    Field Type: Text
```

**Important Information about Your Custom Dimension**

- The Name is for internal (Rock) purposes only
- The Category has to be set to the Intacct Export category in order to be included in the API post
- The Key is the actual Intacct specific name of the Custom Dimension in all caps, beginning with "GLDIM". Also, you'll notice that the core KFS Dimensions use the format `rocks.kfs.Intacct.CLASSID`. The custom key can be anything you'd like, so long as there is a period before the Dimension name. For example, `org.mychurch.Intacct.GLDIMMYCUSTOMDIM` is a valid Attribute Key.
- Text attributes are recommended for the Custom Dimensions. However, the value you enter in the attribute on the Financial Account must be the System Info > ID, not the text value. (i.e. Value: MT-Event-P1234, ID: 10005. You must enter 10005 in Rock.)

**Examples:**
- *Custom Dimension 1:*
  - Record Name: Restriction
  - Integration Name: restriction
  - Rock Key: GLDIMRESTRICTION
- *Custom Dimension 2:*
  - Record Name: Custom Project
  - Integration Name: custom_project
  - Rock Key: GLDIMCUSTOM_PROJECT
- *Custom Dimension 2 Value:*
  - Custom Project: MT - Event - P1234
  - ID: 10005
  - Rock Value: 10005

![IntacctCustomDimensionSystemInfoScreenshot](https://user-images.githubusercontent.com/2990519/174348414-42ead26f-0dd5-4c3c-b985-d1fb48141508.jpeg)





