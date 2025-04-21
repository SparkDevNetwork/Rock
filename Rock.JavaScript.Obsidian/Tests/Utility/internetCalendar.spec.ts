import { RecurrenceRule, Event, Calendar } from "../../Framework/Utility/internetCalendar";
import { DayOfWeek, RockDateTime } from "../../Framework/Utility/rockDateTime";

describe("RecurrenceRule", () => {
    it("Builds every day schedule", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "DAILY";

        expect(rrule.build()).toBe("FREQ=DAILY");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 3);
        const endDateTime = RockDateTime.fromParts(2022, 4, 7);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(4);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 3, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 4, 14, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 5, 14, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 6, 14, 0, 0).toASPString("r"));
    });

    it("Builds every other day schedule", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "DAILY";
        rrule.interval = 2;

        expect(rrule.build()).toBe("FREQ=DAILY;INTERVAL=2");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 3);
        const endDateTime = RockDateTime.fromParts(2022, 4, 7);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(2);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 3, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 5, 14, 0, 0).toASPString("r"));
    });

    it("Builds every schedule ending on specific date", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "DAILY";
        rrule.endDate = RockDateTime.fromParts(2022, 5, 14, 0, 0, 0);

        expect(rrule.build()).toBe("FREQ=DAILY;UNTIL=20220514T000000");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 5, 12);
        const endDateTime = RockDateTime.fromParts(2022, 5, 15);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(2);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 5, 12, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 5, 13, 14, 0, 0).toASPString("r"));
    });

    it("Builds every day schedule of limited count", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "DAILY";
        rrule.count = 12;

        expect(rrule.build()).toBe("FREQ=DAILY;COUNT=12");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 11);
        const endDateTime = RockDateTime.fromParts(2022, 4, 15);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(2);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 11, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 12, 14, 0, 0).toASPString("r"));
    });

    it("Builds weekly schedule on monday", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "WEEKLY";
        rrule.byDay.push({ day: DayOfWeek.Monday, value: 1 });

        expect(rrule.build()).toBe("FREQ=WEEKLY;BYDAY=MO");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 10);
        const endDateTime = RockDateTime.fromParts(2022, 4, 30);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(3);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 11, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 18, 14, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 25, 14, 0, 0).toASPString("r"));
    });

    it("Builds weekly schedule on monday and tuesday", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "WEEKLY";
        rrule.byDay.push({ day: DayOfWeek.Monday, value: 1 });
        rrule.byDay.push({ day: DayOfWeek.Tuesday, value: 1 });

        expect(rrule.build()).toBe("FREQ=WEEKLY;BYDAY=MO,TU");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 10);
        const endDateTime = RockDateTime.fromParts(2022, 4, 30);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(6);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 11, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 12, 14, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 18, 14, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 19, 14, 0, 0).toASPString("r"));
        expect(dates[4].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 25, 14, 0, 0).toASPString("r"));
        expect(dates[5].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 26, 14, 0, 0).toASPString("r"));
    });

    it("Builds weekly schedule every other monday and tuesday", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "WEEKLY";
        rrule.interval = 2;
        rrule.byDay.push({ day: DayOfWeek.Monday, value: 1 });
        rrule.byDay.push({ day: DayOfWeek.Tuesday, value: 1 });

        expect(rrule.build()).toBe("FREQ=WEEKLY;INTERVAL=2;BYDAY=MO,TU");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 10);
        const endDateTime = RockDateTime.fromParts(2022, 4, 30);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(4);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 11, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 12, 14, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 25, 14, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 26, 14, 0, 0).toASPString("r"));
    });

    it("Builds monthly schedule on 5th of every month", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "MONTHLY";
        rrule.byMonthDay.push(5);

        expect(rrule.build()).toBe("FREQ=MONTHLY;BYMONTHDAY=5");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 10);
        const endDateTime = RockDateTime.fromParts(2022, 6, 30);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(2);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 5, 5, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 6, 5, 14, 0, 0).toASPString("r"));
    });

    it("Builds monthly schedule on 5th of every other month", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "MONTHLY";
        rrule.interval = 2;
        rrule.byMonthDay.push(5);

        expect(rrule.build()).toBe("FREQ=MONTHLY;INTERVAL=2;BYMONTHDAY=5");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 10);
        const endDateTime = RockDateTime.fromParts(2022, 8, 30);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(2);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 6, 5, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 8, 5, 14, 0, 0).toASPString("r"));
    });

    it("Builds monthly schedule on the first monday of every month", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "MONTHLY";
        rrule.byDay.push({ day: DayOfWeek.Monday, value: 1 });

        expect(rrule.build()).toBe("FREQ=MONTHLY;BYDAY=1MO");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 10);
        const endDateTime = RockDateTime.fromParts(2022, 8, 30);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(4);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 5, 2, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 6, 6, 14, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 7, 4, 14, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 8, 1, 14, 0, 0).toASPString("r"));
    });

    it("Builds monthly schedule on the first and third monday of every month", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "MONTHLY";
        rrule.byDay.push({ day: DayOfWeek.Monday, value: 1 });
        rrule.byDay.push({ day: DayOfWeek.Monday, value: 3 });

        expect(rrule.build()).toBe("FREQ=MONTHLY;BYDAY=1MO,3MO");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 10);
        const endDateTime = RockDateTime.fromParts(2022, 8, 30);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(9);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 18, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 5, 2, 14, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 5, 16, 14, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 6, 6, 14, 0, 0).toASPString("r"));
        expect(dates[4].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 6, 20, 14, 0, 0).toASPString("r"));
        expect(dates[5].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 7, 4, 14, 0, 0).toASPString("r"));
        expect(dates[6].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 7, 18, 14, 0, 0).toASPString("r"));
        expect(dates[7].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 8, 1, 14, 0, 0).toASPString("r"));
        expect(dates[8].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 8, 15, 14, 0, 0).toASPString("r"));
    });

    it("Builds monthly schedule on the last monday of every month", () => {
        const rrule = new RecurrenceRule();

        rrule.frequency = "MONTHLY";
        rrule.byDay.push({ day: DayOfWeek.Monday, value: -1 });

        expect(rrule.build()).toBe("FREQ=MONTHLY;BYDAY=-1MO");

        const eventStartDateTime = RockDateTime.fromParts(2022, 4, 1, 14, 0, 0);
        const startDateTime = RockDateTime.fromParts(2022, 4, 10);
        const endDateTime = RockDateTime.fromParts(2022, 8, 30);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(5);
        expect(dates[0].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 4, 25, 14, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 5, 30, 14, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 6, 27, 14, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 7, 25, 14, 0, 0).toASPString("r"));
        expect(dates[4].toASPString("r")).toEqual(RockDateTime.fromParts(2022, 8, 29, 14, 0, 0).toASPString("r"));
    });

    it("Enforces maximum of 100,000 dates", () => {
        const rrule = new RecurrenceRule("FREQ=DAILY");
        const eventStartDateTime = RockDateTime.fromParts(2000, 1, 1);
        const startDateTime = RockDateTime.fromParts(2000, 1, 1);
        const endDateTime = RockDateTime.fromParts(2400, 1, 1);

        const dates = rrule.getDates(eventStartDateTime, startDateTime, endDateTime);

        expect(dates.length).toBe(100000);
    });
});

describe("Event", () => {
    it("Parses specific dates", () => {
        const ical = `BEGIN:VEVENT
DTEND:20220401T170000
DTSTAMP:20221007T093105
DTSTART:20220401T160000
RDATE:20221003T160000,20221004T160000,20221007T160000
SEQUENCE:0
UID:123af714-192f-4539-9f96-7c1d899cdc5a
END:VEVENT`;

        const event = new Event(ical);

        expect(event.toFriendlyText()).toBe("Multiple dates between 4/1/2022 4:00 PM and 10/7/2022 4:00 PM");

        const startDateTime = RockDateTime.fromParts(2022, 1, 1);
        const endDateTime = RockDateTime.fromParts(2022, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(4);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 1, 16, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2022, 10, 3, 16, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toBe(RockDateTime.fromParts(2022, 10, 4, 16, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toBe(RockDateTime.fromParts(2022, 10, 7, 16, 0, 0).toASPString("r"));
    });

    it("Parses Daily at 4:00 PM", () => {
        const ical = `BEGIN:VEVENT
DTEND:20220401T170000
DTSTAMP:20221007T093105
DTSTART:20220401T160000
RRULE:FREQ=DAILY;UNTIL=20220405T000000
SEQUENCE:0
UID:123af714-192f-4539-9f96-7c1d899cdc5a
END:VEVENT`;

        const event = new Event(ical);

        expect(event.toFriendlyText()).toBe("Daily at 4:00 PM");

        const startDateTime = RockDateTime.fromParts(2022, 1, 1);
        const endDateTime = RockDateTime.fromParts(2022, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(4);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 1, 16, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 2, 16, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 3, 16, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 4, 16, 0, 0).toASPString("r"));
    });

    it("Parses Daily every 2 days at 4:00 PM", () => {
        const ical = `BEGIN:VEVENT
DTEND:20220401T170000
DTSTAMP:20221007T093105
DTSTART:20220401T160000
RRULE:FREQ=DAILY;INTERVAL=2;UNTIL=20220410T000000
SEQUENCE:0
UID:123af714-192f-4539-9f96-7c1d899cdc5a
END:VEVENT`;

        const event = new Event(ical);

        expect(event.toFriendlyText()).toBe("Daily every 2 days at 4:00 PM");

        const startDateTime = RockDateTime.fromParts(2022, 1, 1);
        const endDateTime = RockDateTime.fromParts(2022, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(5);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 1, 16, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 3, 16, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 5, 16, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 7, 16, 0, 0).toASPString("r"));
        expect(dates[4].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 9, 16, 0, 0).toASPString("r"));
    });

    it("Parses Weekly: Sundays,Saturdays at 4:00 PM", () => {
        const ical = `BEGIN:VEVENT
DTEND:20220401T170000
DTSTAMP:20221007T093105
DTSTART:20220401T160000
RRULE:FREQ=WEEKLY;UNTIL=20220410T000000;BYDAY=SU,SA
SEQUENCE:0
UID:123af714-192f-4539-9f96-7c1d899cdc5a
END:VEVENT`;

        const event = new Event(ical);

        expect(event.toFriendlyText()).toBe("Weekly: Sundays,Saturdays at 4:00 PM");

        const startDateTime = RockDateTime.fromParts(2022, 1, 1);
        const endDateTime = RockDateTime.fromParts(2022, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(4);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 1, 16, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 2, 16, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 3, 16, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 9, 16, 0, 0).toASPString("r"));
    });

    it("Parses Every 2 weeks: Mondays,Tuesdays at 4:00 PM", () => {
        const ical = `BEGIN:VEVENT
DTEND:20220401T170000
DTSTAMP:20221007T093105
DTSTART:20220401T160000
RRULE:FREQ=WEEKLY;INTERVAL=2;UNTIL=20220430T000000;BYDAY=MO,TU
SEQUENCE:0
UID:123af714-192f-4539-9f96-7c1d899cdc5a
END:VEVENT`;

        const event = new Event(ical);

        expect(event.toFriendlyText()).toBe("Every 2 weeks: Mondays,Tuesdays at 4:00 PM");

        const startDateTime = RockDateTime.fromParts(2022, 1, 1);
        const endDateTime = RockDateTime.fromParts(2022, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(5);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 1, 16, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 11, 16, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 12, 16, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 25, 16, 0, 0).toASPString("r"));
        expect(dates[4].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 26, 16, 0, 0).toASPString("r"));
    });

    it("Parses Day 5 of every 2 months at 4:00 PM", () => {
        const ical = `BEGIN:VEVENT
DTEND:20220401T170000
DTSTAMP:20221007T093105
DTSTART:20220401T160000
RRULE:FREQ=MONTHLY;INTERVAL=2;UNTIL=20221231T000000;BYMONTHDAY=5
SEQUENCE:0
UID:123af714-192f-4539-9f96-7c1d899cdc5a
END:VEVENT`;

        const event = new Event(ical);

        expect(event.toFriendlyText()).toBe("Day 5 of every 2 months at 4:00 PM");

        const startDateTime = RockDateTime.fromParts(2022, 1, 1);
        const endDateTime = RockDateTime.fromParts(2022, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(6);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 1, 16, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 5, 16, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toBe(RockDateTime.fromParts(2022, 6, 5, 16, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toBe(RockDateTime.fromParts(2022, 8, 5, 16, 0, 0).toASPString("r"));
        expect(dates[4].toASPString("r")).toBe(RockDateTime.fromParts(2022, 10, 5, 16, 0, 0).toASPString("r"));
        expect(dates[5].toASPString("r")).toBe(RockDateTime.fromParts(2022, 12, 5, 16, 0, 0).toASPString("r"));
    });

    it("Parses The 1st and last Monday of every month at 4:00 PM", () => {
        const ical = `BEGIN:VEVENT
DTEND:20220401T170000
DTSTAMP:20221007T093105
DTSTART:20220401T160000
RRULE:FREQ=MONTHLY;UNTIL=20220731T000000;BYDAY=1MO,-1MO
SEQUENCE:0
UID:123af714-192f-4539-9f96-7c1d899cdc5a
END:VEVENT`;

        const event = new Event(ical);

        expect(event.toFriendlyText()).toBe("The 1st and last Monday of every month at 4:00 PM");

        const startDateTime = RockDateTime.fromParts(2022, 1, 1);
        const endDateTime = RockDateTime.fromParts(2022, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(9);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 1, 16, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 4, 16, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toBe(RockDateTime.fromParts(2022, 4, 25, 16, 0, 0).toASPString("r"));
        expect(dates[3].toASPString("r")).toBe(RockDateTime.fromParts(2022, 5, 2, 16, 0, 0).toASPString("r"));
        expect(dates[4].toASPString("r")).toBe(RockDateTime.fromParts(2022, 5, 30, 16, 0, 0).toASPString("r"));
        expect(dates[5].toASPString("r")).toBe(RockDateTime.fromParts(2022, 6, 6, 16, 0, 0).toASPString("r"));
        expect(dates[6].toASPString("r")).toBe(RockDateTime.fromParts(2022, 6, 27, 16, 0, 0).toASPString("r"));
        expect(dates[7].toASPString("r")).toBe(RockDateTime.fromParts(2022, 7, 4, 16, 0, 0).toASPString("r"));
        expect(dates[8].toASPString("r")).toBe(RockDateTime.fromParts(2022, 7, 25, 16, 0, 0).toASPString("r"));
    });

    it("Builds Daily at 4:00 PM", () => {
        const event = new Event();

        event.uid = "123af714-192f-4539-9f96-7c1d899cdc5a";
        event.startDateTime = RockDateTime.fromParts(2022, 4, 1, 16, 0, 0);
        event.endDateTime = RockDateTime.fromParts(2022, 4, 1, 17, 0, 0);

        const rrule = new RecurrenceRule();
        rrule.frequency = "DAILY";
        rrule.endDate = RockDateTime.fromParts(2022, 4, 5, 0, 0, 0);
        event.recurrenceRules.push(rrule);

        const lines = event.buildLines();

        expect(lines.length).toBe(8);
        expect(lines[0]).toBe("BEGIN:VEVENT");
        expect(lines[1]).toBe("DTEND:20220401T170000");
        expect(lines[2].substring(0, 8)).toBe("DTSTAMP:");
        expect(lines[3]).toBe("DTSTART:20220401T160000");
        expect(lines[4]).toBe("RRULE:FREQ=DAILY;UNTIL=20220405T000000");
        expect(lines[5]).toBe("SEQUENCE:0");
        expect(lines[6]).toBe("UID:123af714-192f-4539-9f96-7c1d899cdc5a");
        expect(lines[7]).toBe("END:VEVENT");
    });

    it("Parses specific dates in period format when value type not specified", () => {
        const ical = `BEGIN:VEVENT
DTEND:20240108T000001
DTSTAMP:20240110T160949
DTSTART:20240108T000000
RDATE:20240117/P1D,20240118/P1D
SEQUENCE:0
UID:b07576b4-9766-4bc8-ade0-cab82c20c88e
END:VEVENT`;

        const event = new Event(ical);

        expect(event.toFriendlyText()).toBe("Multiple dates between 1/8/2024 12:00 AM and 1/18/2024 12:00 AM");

        const startDateTime = RockDateTime.fromParts(2024, 1, 1);
        const endDateTime = RockDateTime.fromParts(2024, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(3);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2024, 1, 8, 0, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2024, 1, 17, 0, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toBe(RockDateTime.fromParts(2024, 1, 18, 0, 0, 0).toASPString("r"));
    });

    it("Parses specific dates in date format when value type not specified", () => {
        const ical = `BEGIN:VEVENT
DTEND:20240108T000001
DTSTAMP:20240110T160949
DTSTART:20240108T000000
RDATE:20240117
SEQUENCE:0
UID:b07576b4-9766-4bc8-ade0-cab82c20c88e
END:VEVENT`;

        const event = new Event(ical);

        expect(event.toFriendlyText()).toBe("Multiple dates between 1/8/2024 12:00 AM and 1/17/2024 12:00 AM");

        const startDateTime = RockDateTime.fromParts(2024, 1, 1);
        const endDateTime = RockDateTime.fromParts(2024, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(2);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2024, 1, 8, 0, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2024, 1, 17, 0, 0, 0).toASPString("r"));
    });

    it("Parses specific dates in date format when value type not specified", () => {
        const ical = `BEGIN:VEVENT
DTEND:20240108T000001
DTSTAMP:20240110T160949
DTSTART:20240108T000000
RDATE:20240201/20240202,
SEQUENCE:0
UID:b07576b4-9766-4bc8-ade0-cab82c20c88e
END:VEVENT`;

        const event = new Event(ical);
        const startDateTime = RockDateTime.fromParts(2024, 1, 1);
        const endDateTime = RockDateTime.fromParts(2024, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(3);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2024, 1, 8, 0, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2024, 2, 1, 0, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toBe(RockDateTime.fromParts(2024, 2, 2, 0, 0, 0).toASPString("r"));
    });

    it("Parses specific dates in date time format when value type is not specified", () => {
        const ical = `BEGIN:VEVENT
DTEND:20240108T000001
DTSTAMP:20240110T160949
DTSTART:20240108T000000
RDATE:20240110T160949,
SEQUENCE:0
UID:b07576b4-9766-4bc8-ade0-cab82c20c88e
END:VEVENT`;

        const event = new Event(ical);
        const startDateTime = RockDateTime.fromParts(2024, 1, 1);
        const endDateTime = RockDateTime.fromParts(2024, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(2);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2024, 1, 8, 0, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2024, 1, 10, 16, 9, 49).toASPString("r"));
    });

    it("Parses specific dates in date time in UTC format when value type is not specified", () => {
        const ical = `BEGIN:VEVENT
DTEND:20240108T000001
DTSTAMP:20240110T160949
DTSTART:20240108T000000
RDATE:20240310T160949Z,
SEQUENCE:0
UID:b07576b4-9766-4bc8-ade0-cab82c20c88e
END:VEVENT`;

        const event = new Event(ical);
        const startDateTime = RockDateTime.fromParts(2024, 1, 1);
        const endDateTime = RockDateTime.fromParts(2024, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(2);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2024, 1, 8, 0, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2024, 3, 10, 16, 9, 49).toASPString("r"));
    });

    it("Parses specific dates in date format when value type not specified", () => {
        const ical = `BEGIN:VEVENT
DTEND:20240108T000001
DTSTAMP:20240110T160949
DTSTART:20240108T000000
RDATE:20240201/P2D,
SEQUENCE:0
UID:b07576b4-9766-4bc8-ade0-cab82c20c88e
END:VEVENT`;

        const event = new Event(ical);
        const startDateTime = RockDateTime.fromParts(2024, 1, 1);
        const endDateTime = RockDateTime.fromParts(2024, 12, 31);

        const dates: RockDateTime[] = event.getDates(startDateTime, endDateTime);

        expect(dates.length).toBe(3);

        expect(dates[0].toASPString("r")).toBe(RockDateTime.fromParts(2024, 1, 8, 0, 0, 0).toASPString("r"));
        expect(dates[1].toASPString("r")).toBe(RockDateTime.fromParts(2024, 2, 1, 0, 0, 0).toASPString("r"));
        expect(dates[2].toASPString("r")).toBe(RockDateTime.fromParts(2024, 2, 2, 0, 0, 0).toASPString("r"));
    });
});

describe("Calendar", () => {
    it("Parses single event calendar", () => {
        const ical = `BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20130501T235900
DTSTAMP:20221005T161039
DTSTART;TZID=US-Eastern:20130501T000200
EXDATE:20221101/P1D,20221102/P1D,20221103/P1D,20221104/P1D,20221105/P1D,20
 221106/P1D,20221107/P1D,20221108/P1D,20221109/P1D,20221110/P1D,20221111/P
 1D,20221112/P1D,20221113/P1D,20221114/P1D,20221115/P1D,20221116/P1D,20221
 117/P1D,20221118/P1D,20221119/P1D,20221120/P1D,20221121/P1D,20221122/P1D,
 20221123/P1D,20221124/P1D,20221125/P1D,20221126/P1D,20221127/P1D,20221128
 /P1D,20221129/P1D,20221130/P1D
RRULE:FREQ=WEEKLY;UNTIL=20240410T000000;BYDAY=MO,TU,WE
SEQUENCE:0
UID:91343141-238a-4b7b-a3d4-ce93b08d2712
END:VEVENT
END:VCALENDAR`;

        const calendar = new Calendar(ical);

        expect(calendar.events.length).toBe(1);
    });
});
