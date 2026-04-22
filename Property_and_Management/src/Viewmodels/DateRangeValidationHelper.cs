using System;

namespace Property_and_Management.Src.Viewmodels
{
    internal static class DateRangeValidationHelper
    {
        public static bool HasValidFutureDateRange(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            if (startDate == null || endDate == null)
            {
                return false;
            }

            if (startDate.Value.Date >= endDate.Value.Date)
            {
                return false;
            }

            return startDate.Value.Date >= DateTimeOffset.Now.Date;
        }
    }
}