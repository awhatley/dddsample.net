using System;
using System.Globalization;

namespace DomainDrivenDelivery.Reporting.Api
{
    internal class DateFormats
    {
        internal static readonly IFormatProvider US_FORMAT = CultureInfo.CreateSpecificCulture("en-US");
    }
}