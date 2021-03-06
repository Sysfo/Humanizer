using System;
using System.Globalization;
using Humanizer.Configuration;
using Humanizer.Localisation;

namespace Humanizer.DateTimeHumanizeStrategy
{
    /// <summary>
    /// Algorithms used to convert distance between two dates into words.
    /// </summary>
    internal static class DateTimeHumanizeAlgorithms
    {
        /// <summary>
        /// Returns localized &amp; humanized distance of time between two dates; given a specific precision.
        /// </summary>
        public static string PrecisionHumanize(DateTime input, DateTime comparisonBase, double precision, CultureInfo culture,  ShowQuantityAs showQuantityAs)
        {
            var ts = new TimeSpan(Math.Abs(comparisonBase.Ticks - input.Ticks));
            var tense = input > comparisonBase ? Tense.Future : Tense.Past;

            int seconds = ts.Seconds, minutes = ts.Minutes, hours = ts.Hours, days = ts.Days;
            int years = 0, months = 0;

            // start approximate from smaller units towards bigger ones
            if (ts.Milliseconds >= 999 * precision) seconds += 1;
            if (seconds >= 59 * precision) minutes += 1;
            if (minutes >= 59 * precision) hours += 1;
            if (hours >= 23 * precision) days += 1;

            // month calculation 
            if (days >= 30 * precision & days <= 31) months = 1;
            if (days > 31 && days < 365 * precision)
            {
                int factor = Convert.ToInt32(Math.Floor((double)days / 30));
                int maxMonths = Convert.ToInt32(Math.Ceiling((double)days / 30));
                months = (days >= 30 * (factor + precision)) ? maxMonths : maxMonths - 1;
            }

            // year calculation
            if (days >= 365 * precision && days <= 366) years = 1;
            if (days > 365)
            {
                int factor = Convert.ToInt32(Math.Floor((double)days / 365));
                int maxMonths = Convert.ToInt32(Math.Ceiling((double)days / 365));
                years = (days >= 365 * (factor + precision)) ? maxMonths : maxMonths - 1;
            }

            // start computing result from larger units to smaller ones
            var formatter = Configurator.GetFormatter(culture);
            if (years > 0) return formatter.DateHumanize(TimeUnit.Year, tense, years, showQuantityAs);
            if (months > 0) return formatter.DateHumanize(TimeUnit.Month, tense, months, showQuantityAs);
            if (days > 0) return formatter.DateHumanize(TimeUnit.Day, tense, days, showQuantityAs);
            if (hours > 0) return formatter.DateHumanize(TimeUnit.Hour, tense, hours, showQuantityAs);
            if (minutes > 0) return formatter.DateHumanize(TimeUnit.Minute, tense, minutes, showQuantityAs);
            if (seconds > 0) return formatter.DateHumanize(TimeUnit.Second, tense, seconds, showQuantityAs);
            return formatter.DateHumanize(TimeUnit.Millisecond, tense, 0, showQuantityAs);
        }

        // http://stackoverflow.com/questions/11/how-do-i-calculate-relative-time
        /// <summary>
        /// Calculates the distance of time in words between two provided dates
        /// </summary>
        public static string DefaultHumanize(DateTime input, DateTime comparisonBase, CultureInfo culture,  ShowQuantityAs showQuantityAs)
        {
            var tense = input > comparisonBase ? Tense.Future : Tense.Past;
            var ts = new TimeSpan(Math.Abs(comparisonBase.Ticks - input.Ticks));

            var formatter = Configurator.GetFormatter(culture);

            if (ts.TotalMilliseconds < 500)
                return formatter.DateHumanize(TimeUnit.Millisecond, tense, 0, showQuantityAs);

            if (ts.TotalSeconds < 60)
                return formatter.DateHumanize(TimeUnit.Second, tense, ts.Seconds, showQuantityAs);

            if (ts.TotalSeconds < 120)
                return formatter.DateHumanize(TimeUnit.Minute, tense, 1, showQuantityAs);

            if (ts.TotalMinutes < 60)
                return formatter.DateHumanize(TimeUnit.Minute, tense, ts.Minutes, showQuantityAs);

            if (ts.TotalMinutes < 90)
                return formatter.DateHumanize(TimeUnit.Hour, tense, 1, showQuantityAs);

            if (ts.TotalHours < 24)
                return formatter.DateHumanize(TimeUnit.Hour, tense, ts.Hours, showQuantityAs);

            if (ts.TotalHours < 48)
            {
                var days = Math.Abs((input.Date - comparisonBase.Date).Days);
                return formatter.DateHumanize(TimeUnit.Day, tense, days, showQuantityAs);
            }

            if (ts.TotalDays < 28)
                return formatter.DateHumanize(TimeUnit.Day, tense, ts.Days, showQuantityAs);

            if (ts.TotalDays >= 28 && ts.TotalDays < 30)
            {
                if (comparisonBase.Date.AddMonths(tense == Tense.Future ? 1 : -1) == input.Date)
                    return formatter.DateHumanize(TimeUnit.Month, tense, 1, showQuantityAs);
                return formatter.DateHumanize(TimeUnit.Day, tense, ts.Days, showQuantityAs);
            }

            if (ts.TotalDays < 345)
            {
                int months = Convert.ToInt32(Math.Floor(ts.TotalDays / 29.5));
                return formatter.DateHumanize(TimeUnit.Month, tense, months, showQuantityAs);
            }

            int years = Convert.ToInt32(Math.Floor(ts.TotalDays / 365));
            if (years == 0) years = 1;

            return formatter.DateHumanize(TimeUnit.Year, tense, years, showQuantityAs);
        }
    }
}