# Rock RMS Sample Data Reference

When a SQL script needs to reference people, use these sample data records. They exist in any Rock instance that has loaded the standard sample data. Query by Guid for reliability — Ids vary between installations.

## Table of Contents
1. [Key People (Quick Reference)](#key-people-quick-reference)
2. [Family Details](#family-details)
3. [Staff and Roles](#staff-and-roles)
4. [Business Records](#business-records)
5. [Groups](#groups)
6. [Relationships](#relationships)
7. [Duplicate Records (for testing)](#duplicate-records)

---

## Key People (Quick Reference)

| Name | Guid | Email | Role/Notes |
|---|---|---|---|
| Ted Decker | `8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4` | ted@rocksolidchurchdemo.com | Outreach Pastor, staff |
| Cindy Decker | `B71494DB-D809-451A-A950-28898D0FD92C` | cindy@fakeinbox.com | Ted's wife |
| Noah Decker | `32AAB9E4-970D-4551-A17E-385E66113BD5` | — | Child, grade 5 |
| Alex Decker | `27919690-3CCE-4FA6-95C4-CD21419EB51F` | — | Child, grade 2, peanut allergy |
| Peter Foster | `BE66C5D3-F43E-4F9A-BA62-B7C0103BF54C` | peter@rocksolidchurchdemo.com | Senior Pastor |
| Pamela Foster | `F4450E80-F221-4556-881D-CB92B008C2DA` | pamela@fakeinbox.com | Peter's wife |
| Alisha Marble | `69DC0FDC-B451-4303-BD91-EF17C0015D23` | alisha.marble@rocksolidchurchdemo.com | Rock Administrator |
| Bill Marble | `1EA811BB-3118-42D1-B020-32A82BC8081A` | bill.marble@fakeinbox.com | Born 2/29/1968 (leap day) |
| Ben Jones | `3C402382-3BD2-4337-A996-9E62F1BAB09D` | ben.jones@fakeinbox.com | Divorced, co-parent |
| Brian Jones | `3D7F6605-3666-4AB5-9F4E-D7FEBF93278E` | brian.jones@fakeinbox.com | Child, shared custody |
| Jim Simmons | `FB5D2DC6-5AB4-4CBD-B02F-3FB9ADBEFD33` | jim.simmons@fakeinbox.com | Sarah's husband |
| Sarah Simmons | `FC6B9819-EF2E-44C9-93DB-05571B39E58F` | sarah.simmons@fakeinbox.com | Previously Sarah Jones |
| Mariah Jackson | `9C2A020B-CF34-403E-A948-3E91FDFB958B` | mariah.jackson@fakeinbox.com | Sole custody of Madison |
| Madison Lowe | `C398A8E3-C9BC-4017-A3F6-7C2BFF654056` | madison.lowe@fakeinbox.com | Child, legal note about uncle |
| Craig Lowe | `B701E5D2-ADBD-4C66-91E8-3D6CD298D9A0` | craig.lowe@fakeinbox.com | Email inactive, DoNotEmail |
| Tricia Lowe | `20A2C167-D16E-43B3-A5B0-80CE912589AF` | trish.lowe@fakeinbox.com | Gives weekly |
| Daniel Peak | `C5E4C66A-B836-4A8B-AD02-43F841B94BE9` | daniel.peak@fakeinbox.com | Director of Communications |
| Sam Hanks | `1E6E66C7-A487-48E6-B064-7F9F4DDE6680` | sam.hanks@fakeinbox.com | Developer at Honeywell |
| Jenny Michaels | `82309081-A110-496F-8295-CE6A4720D7A8` | jenny@rocksolidchurchdemo.com | Staff-Like, coordinates car show |
| Frank Dexter | `9C609974-A8E1-4363-8F5E-51F61828E2BA` | frank@fakeinbox.com | Background check failed |
| Tom Miller | `47A42BD0-38A3-49D6-A32A-0FCD5E386CC3` | tom.miller@fakeinbox.com | Duplicate of Thomas Miller |
| Thomas Miller | `955BF3E9-D38A-4DCB-A6F2-EDF5EC2571C5` | tom.miller@fakeinbox.com | Duplicate of Tom Miller |
| Robert Greggs | `28C4F2C0-0691-468F-B40E-550BD2976B9E` | rgreggs@fakeinbox.com | Family lost child Jenny |
| Lorraine Greggs | `1A5A4E5F-E0A4-4EFA-B7D7-6698153ED718` | lgreggs@fakeinbox.com | Sensitive date: Sept 11 |
| Phil Coffee | `3459286F-18C8-4548-9938-53DAC8483D01` | pcoffee@fakeinbox.com | Invited by Allie Song, baptized |
| Allie Song | `D593F194-9DC3-4936-A1C3-FB7E5CB09145` | asong@fakeinbox.com | Bank teller, baptized |
| Paul Smith | `D7CE30DF-58F8-4268-BA19-56FDF17D3890` | paul@fakeinbox.com | ASU Teacher |
| Ty McClintock | `A08C02C7-ED75-4F24-8F75-FB3956521804` | taylor@fakeinbox.com | ASU Graduate Assistant |

---

## Family Details

### Decker Family (Guid: `53A02527-C2A7-4F36-8585-71A85B8E4601`)
- **Campus:** Main Campus
- **Address:** 11624 N 31st Dr, Phoenix, AZ 85029
- **Members:** Ted (adult), Cindy (adult), Noah (child), Alex (child)
- **Attendance:** ~80%, started 12/25/2012
- **Giving:** Monthly, General Fund + Building Fund, ~3% annual growth
- **Notes:** Ted serves as usher. Ted and Cindy in small group with Marbles.

### Marble Family (Guid: `6F2E6B64-3592-4543-8D8E-6BA4040CDC4E`)
- **Campus:** Main Campus
- **Address:** 3002 W Lupine Ave, Phoenix, AZ 85029
- **Members:** Alisha (adult, Rock admin), Bill (adult)
- **Attendance:** ~88%, started 1/1/2012
- **Giving:** Bill gives weekly, General Fund + Mission Fund

### Foster Family (Guid: `FFE1DA77-B596-46B9-AAF8-C7C8E3AC8F2C`)
- **Campus:** Main Campus
- **Address:** 1543 W Joan de Arc Ave, Phoenix, AZ 85029
- **Members:** Peter (adult, Senior Pastor), Pamela (adult)

### Simmons Family (Guid: `FA7316E2-FFA9-4A55-B975-8F762353D0A8`)
- **Campus:** Main Campus
- **Members:** Jim (adult), Sarah (adult, formerly Jones), Brian Jones (child, shared)
- **Attendance:** ~90%, very consistent service attendance

### Jones Family (Guid: `3BE313A1-51B1-4B03-B396-CEB9C3D9B919`)
- **Members:** Ben (adult), Brian (child, shared custody with Simmons)
- **Attendance:** Stopped Aug 2013 after divorce

---

## Staff and Roles

| Person | Position | Employer |
|---|---|---|
| Ted Decker | Outreach Pastor | Rock Solid Church |
| Peter Foster | Senior Pastor | Rock Solid Church |
| Alisha Marble | Assistant (Rock Admin) | Rock Solid Church |
| Daniel Peak | Director of Communications | Rock Solid Church |
| Jenny Michaels | Systems Engineer (Staff-Like) | Honeywell |
| Sam Hanks | Developer (volunteer potential) | Honeywell |

---

## Business Records

| Business Name | Guid | Contact Email | Notes |
|---|---|---|---|
| Ace Hardware | `9B97D1B2-EAF8-4E45-AD6F-D61477FE9E39` | ace.hardware@fakeinbox.com | RecordType = Business |
| Copper State Glass and Screen | `6CE68373-7B38-40DA-B84D-281BEFF41114` | copperstateglass@fakeinbox.com | Owned by Bill Marble |

---

## Groups

### Small Groups
| Group Name | Guid | Leader | Topic |
|---|---|---|---|
| Decker Group | `62DC3753-01D5-48B5-B22D-D2825D92900B` | Ted Decker | Book of Genesis |
| Alisha Marble's Group | `10B60F8D-0F23-4FAA-B35F-9A5F19F5F995` | Alisha Marble | Women of the Bible |
| Marble Group | `90B2CEDA-AE3A-4C10-A2E1-B987020379AD` | Bill Marble | General Bible Study |
| Pete's Group | `0E42C572-3662-4AC5-9DC8-684762439A64` | Peter Foster | Book of Genesis |
| ASU Student Group | `CD65C668-B324-4F41-BBB7-BEC9C62233F1` | — | Students of the Bible |
| Greggs Group | `2C8DD5B2-84DA-4591-8B07-91468001868D` | Robert Greggs | The Story Bible |
| Gilbert Group | `3F61D174-D271-4F96-B5E1-CA98FAAC60BB` | Brian Gilbert | The Story Bible |

### Serving Teams
| Group Name | Guid | Members |
|---|---|---|
| A/V Team | `0BA93D66-21B1-4229-979D-F76CEB57666D` | Ted Decker, Jim Simmons |

---

## Relationships

| Person A | Relationship | Person B |
|---|---|---|
| Ben Jones | Step-child | Brian Jones |
| Brian Jones | Step-parent | Ben Jones |
| Craig Lowe | Can-check-in | Madison Lowe |
| Jenny Greggs (deceased) | Parent | Robert Greggs |
| Jenny Greggs (deceased) | Parent | Lorraine Greggs |
| Bill Marble | Business | Copper State Glass and Screen |

---

## Duplicate Records

These intentional duplicates exist for testing merge functionality:

**Tom Miller / Thomas Miller:**
- Tom Miller: `47A42BD0-38A3-49D6-A32A-0FCD5E386CC3` (moved to new address)
- Thomas Miller: `955BF3E9-D38A-4DCB-A6F2-EDF5EC2571C5` (created from connection card)
- Same email: tom.miller@fakeinbox.com, same phone: 6235550909

**John/Jon Smith variants:**
- Jon Smith: `077F2901-A51E-43F9-A35A-98CC97C4D465`
- John Smith (1): `AAFC133A-F54F-4AFA-A9F3-AD650941B2CE`
- John Smith (2): `C966D4EC-C84A-4F70-BA86-90E1F8480831`
- John Smith (3): `902984C3-BCE1-405A-8A74-416F7D15B288`
- All share email jon.smith@fakeinbox.com at same address

---

## Using Sample Data in SQL

When your script needs people and you want to use sample data:

```sql
-- Look up a well-known sample person by Guid
DECLARE @TedDeckerId INT = (SELECT TOP 1 [Id] FROM [Person] WHERE [Guid] = '8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4')
DECLARE @TedDeckerAliasId INT = (SELECT TOP 1 [Id] FROM [PersonAlias] WHERE [PersonId] = @TedDeckerId AND [AliasPersonId] = @TedDeckerId)

-- If you need "any active adult" rather than a specific person
DECLARE @SomePersonId INT = (SELECT TOP 1 [Id] FROM [Person] WHERE [IsDeceased] = 0 AND [RecordStatusValueId] = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E') ORDER BY [Id])

-- If you need multiple people (e.g., for attendance records), query a set
SELECT TOP 10 [Id] FROM [Person] WHERE [IsDeceased] = 0 ORDER BY [Id]
```

The sample data gives you a realistic set of people with varied statuses, connection levels, attendance patterns, and giving histories — useful for testing scripts against realistic conditions.
