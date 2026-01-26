/**
 * Parses a date string (YYYY-MM-DD) as local time instead of UTC.
 *
 * JavaScript's Date constructor interprets date-only strings like "2026-03-03"
 * as UTC midnight. For users in timezones west of UTC, this causes the displayed
 * date to shift to the previous day when formatted.
 *
 * This function appends T00:00:00 to treat the date as local midnight.
 */
export function parseLocalDate(dateString: string): Date {
  // If the string is just a date (YYYY-MM-DD), append time to interpret as local
  if (/^\d{4}-\d{2}-\d{2}$/.test(dateString)) {
    return new Date(dateString + 'T00:00:00');
  }
  // Otherwise, parse as-is (for full ISO datetime strings)
  return new Date(dateString);
}
