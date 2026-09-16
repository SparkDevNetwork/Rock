# Figma "Jon's Scratch Pad" — InteractionComponentDailyCount Overview

Transcript of the Figma frame at https://www.figma.com/design/Sedg93yqucAcBJfr6Ux9xX/Jon-s-Scratch-Pad?node-id=10761-3844&t=ZyYwClpKmuQcen2Q-0

The Figma file required authentication and could not be fetched programmatically by the spec skill at write time (2026-06-15). This transcript was reproduced verbatim from screenshots supplied by the spec's author (Nick Airdo) and is treated as the canonical design source by [the spec](../../260615-interaction-component-daily-count.md).

---

## Overview

We would like to be able to speed up the ability to get metrics on interactions summarized at the component level. To do this we'll create a new model to store daily counts.

## Specifics

1. This table will not have values for the current day. The newest value will be for the day before.
2. Loggedin will be considered having a personal alias that is not an nameless person.
3. The [InteractionChannel] table will get a new property to determine if these daily counts should be enabled.
4. We'll also add a new attribute to the "Interaction Mediums" defined type to determine if new Channels of this medium type should default to have their [EnableComponentDailyCounts] set to true on creation. The help text of this attribute should be:
   a. When enabled, newly created interaction channels will automatically have Enable Component Daily Counts turned on.
5. Update the InteractionChannel's model's pre-save method so that on Add (and Add only) it looks at it's Interaction Medium type to see if it should default the EnableComponentDailyCounts to true.
6. The [Operation] column on Interactions is nullable. When it is null it will be translated as a empty string on the new table. This is because the [Operation] column on this new table is a part of the primary key it can't be null.
7. The Rock Clean-up job will be responsible for updating this table. The job should create all records from the last recorded date through yesterday. This ensures that enabling EnableComponentDailyCounts on the next run will automatically backfill any missing historical records.
   a. Be sure to optimize the query to make this performant. This might be best done in SQL.
   b. Set a timeout to ensure that the first run doesn't timeout.
   c. We don't need to go back in time after the first run. If an interaction is written with a date of 5 days ago it won't be in the calculation that that date has already been written.
8. Write a migration to go through and update the "Default Component Daily Counts" on the mediums to match the values to the left.
9. Write a migration to set the current Interaction Channels to set their [EnableComponentDailyCounts] bits based on the value from their Interaction Medium.
