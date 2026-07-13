export function formatTo12Hour(timeStr) {
  if (!timeStr) return "";
  
  // timeStr usually comes in "HH:mm:ss" or "HH:mm"
  const [hours, minutes] = timeStr.split(":");
  let hour = parseInt(hours, 10);
  const ampm = hour >= 12 ? "PM" : "AM";
  
  hour = hour % 12;
  hour = hour ? hour : 12; // the hour '0' should be '12'
  
  return `${hour}:${minutes} ${ampm}`;
}
