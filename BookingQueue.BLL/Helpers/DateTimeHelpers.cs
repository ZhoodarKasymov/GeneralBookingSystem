namespace BookingQueue.BLL.Helpers;

public static class DateTimeHelpers
{
    public static int GetCorrectedTimeBegin(DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Monday:
                return 1;
            case DayOfWeek.Tuesday:
                return 2;
            case DayOfWeek.Wednesday:
                return 3;
            case DayOfWeek.Thursday:
                return 4;
            case DayOfWeek.Friday:
                return 5;
            case DayOfWeek.Saturday:
                return 6;
            case DayOfWeek.Sunday:
                return 7;
            default:
                throw new ArgumentOutOfRangeException(nameof(day), day, @"Не известный день недели!");
        }
    }
}