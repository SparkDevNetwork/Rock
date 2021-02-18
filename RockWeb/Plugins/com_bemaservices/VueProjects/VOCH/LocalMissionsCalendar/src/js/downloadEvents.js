let baseUrl = 'https://voxchurch.org'

export async function  getEvents(calendarId, startDate, endDate){

  const response = await fetch(`${baseUrl}/api/com_bemaservices/EventLink/GetCalendarItems?CalendarIds=${calendarId}&startDateTime=${startDate}&endDateTime=${endDate}`, {
    credentials: 'include',
  });

  const events = await response.json();
  return events
}
