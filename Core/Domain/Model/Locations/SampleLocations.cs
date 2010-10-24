using System;
using System.Collections.Generic;
using System.Reflection;

namespace DomainDrivenDelivery.Domain.Model.Locations
{
    /// <summary>
    /// Sample locations, for test purposes.
    /// </summary>
    public static class SampleLocations
    {
        private static readonly TimeZoneInfo CHINA = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
        private static readonly TimeZoneInfo CENTRAL_EUROPE = TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");
        private static readonly TimeZoneInfo JAPAN = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
        private static readonly TimeZoneInfo EASTERN = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        private static readonly TimeZoneInfo CENTRAL = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
        private static readonly TimeZoneInfo PACIFIC = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
        private static readonly TimeZoneInfo EASTERN_AUSTRALIA = TimeZoneInfo.FindSystemTimeZoneById("Australia/Melbourne");

        public static readonly CustomsZone US = new CustomsZone("US", "United States");
        public static readonly CustomsZone EU = new CustomsZone("EU", "European Union");
        public static readonly CustomsZone CN = new CustomsZone("CN", "United States");
        public static readonly CustomsZone AU = new CustomsZone("AU", "Australia");
        public static readonly CustomsZone JN = new CustomsZone("JN", "Japan");

        public static readonly Location HONGKONG = new Location(new UnLocode("CNHKG"), "Hongkong", CHINA, CN);
        public static readonly Location MELBOURNE = new Location(new UnLocode("AUMEL"), "Melbourne", EASTERN_AUSTRALIA, AU);
        public static readonly Location STOCKHOLM = new Location(new UnLocode("SESTO"), "Stockholm", CENTRAL_EUROPE, EU);
        public static readonly Location HELSINKI = new Location(new UnLocode("FIHEL"), "Helsinki", CENTRAL_EUROPE, EU);
        public static readonly Location CHICAGO = new Location(new UnLocode("USCHI"), "Chicago", CENTRAL, US);
        public static readonly Location LONGBEACH = new Location(new UnLocode("USLBG"), "Long Beach", PACIFIC, US);
        public static readonly Location OAKLAND = new Location(new UnLocode("USOAK"), "Oakland", PACIFIC, US);
        public static readonly Location SEATTLE = new Location(new UnLocode("USSEA"), "Seattle", PACIFIC, US);
        public static readonly Location TOKYO = new Location(new UnLocode("JNTKO"), "Tokyo", JAPAN, JN);
        public static readonly Location HAMBURG = new Location(new UnLocode("DEHAM"), "Hamburg", CENTRAL_EUROPE, EU);
        public static readonly Location SHANGHAI = new Location(new UnLocode("CNSHA"), "Shanghai", CHINA, CN);
        public static readonly Location ROTTERDAM = new Location(new UnLocode("NLRTM"), "Rotterdam", CENTRAL_EUROPE, EU);
        public static readonly Location GOTHENBURG = new Location(new UnLocode("SEGOT"), "Göteborg", CENTRAL_EUROPE, EU);
        public static readonly Location HANGZOU = new Location(new UnLocode("CNHGH"), "Hangzhou", CHINA, CN);
        public static readonly Location NEWYORK = new Location(new UnLocode("USNYC"), "New York", EASTERN, US);
        public static readonly Location DALLAS = new Location(new UnLocode("USDAL"), "Dallas", CENTRAL, US);

        private static readonly IDictionary<UnLocode, Location> ALL = new Dictionary<UnLocode, Location>();

        static SampleLocations()
        {
            foreach(FieldInfo field in typeof(SampleLocations).GetFields())
            {
                if(field.FieldType.Equals(typeof(Location)))
                {
                    Location location = (Location)field.GetValue(null);
                    ALL.Add(location.unLocode(), location);
                }
            }
        }

        public static IEnumerable<Location> getAll()
        {
            return ALL.Values;
        }

        public static Location lookup(UnLocode unLocode)
        {
            return ALL[unLocode];
        }
    }
}