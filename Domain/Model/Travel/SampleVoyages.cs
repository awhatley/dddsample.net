using System;
using System.Collections.Generic;
using System.Reflection;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

namespace DomainDrivenDelivery.Domain.Model.Travel
{
    /// <summary>
    /// Sample carrier movements, for test purposes.
    /// </summary>
    public static class SampleVoyages
    {
        public readonly static Voyage pacific1 = new Voyage.Builder(new VoyageNumber("PAC1"), L.HONGKONG).
          addMovement(L.TOKYO, toDate("2009-03-03"), toDate("2009-03-05")).
          addMovement(L.LONGBEACH, toDate("2009-03-06"), toDate("2009-03-09")).
          addMovement(L.SEATTLE, toDate("2009-03-10"), toDate("2009-03-12")).
          addMovement(L.HONGKONG, toDate("2009-03-15"), toDate("2009-03-19")).
          build();

        public readonly static Voyage pacific2 = new Voyage.Builder(new VoyageNumber("PAC2"), L.SHANGHAI).
          addMovement(L.TOKYO, toDate("2009-03-04"), toDate("2009-03-05")).
          addMovement(L.LONGBEACH, toDate("2009-03-06"), toDate("2009-03-08")).
          addMovement(L.SEATTLE, toDate("2009-03-10"), toDate("2009-03-14")).
          addMovement(L.HANGZOU, toDate("2009-03-14"), toDate("2009-03-16")).
          addMovement(L.SHANGHAI, toDate("2009-03-17"), toDate("2009-03-19")).
          build();

        public readonly static Voyage continental1 = new Voyage.Builder(new VoyageNumber("CNT1"), L.LONGBEACH).
          addMovement(L.DALLAS, toDate("2009-03-06"), toDate("2009-03-08")).
          addMovement(L.CHICAGO, toDate("2009-03-09"), toDate("2009-03-10")).
          addMovement(L.NEWYORK, toDate("2009-03-11"), toDate("2009-03-14")).
          build();

        public readonly static Voyage continental2 = new Voyage.Builder(new VoyageNumber("CNT2"), L.LONGBEACH).
          addMovement(L.DALLAS, toDate("2009-03-06"), toDate("2009-03-08")).
          addMovement(L.NEWYORK, toDate("2009-03-10"), toDate("2009-03-14")).
          build();

        public readonly static Voyage continental3 = new Voyage.Builder(new VoyageNumber("CNT3"), L.SEATTLE).
          addMovement(L.CHICAGO, toDate("2009-03-06"), toDate("2009-03-08")).
          addMovement(L.NEWYORK, toDate("2009-03-10"), toDate("2009-03-13")).
          build();

        public readonly static Voyage atlantic1 = new Voyage.Builder(new VoyageNumber("ATC1"), L.NEWYORK).
            addMovement(L.ROTTERDAM, toDate("2009-03-13"), toDate("2009-03-18")).
            addMovement(L.HAMBURG, toDate("2009-03-19"), toDate("2009-03-20")).
            addMovement(L.HELSINKI, toDate("2009-03-21"), toDate("2009-03-22")).
            addMovement(L.NEWYORK, toDate("2009-03-24"), toDate("2009-03-30")).
            build();

        public readonly static Voyage atlantic2 = new Voyage.Builder(new VoyageNumber("ATC2"), L.NEWYORK).
            addMovement(L.HAMBURG, toDate("2009-03-17"), toDate("2009-03-20")).
            addMovement(L.GOTHENBURG, toDate("2009-03-22"), toDate("2009-03-24")).
            addMovement(L.STOCKHOLM, toDate("2009-03-25"), toDate("2009-03-26")).
            addMovement(L.HELSINKI, toDate("2009-03-27"), toDate("2009-03-28")).
            addMovement(L.NEWYORK, toDate("2009-03-31"), toDate("2009-04-04")).
            build();

        public static readonly Voyage HONGKONG_TO_NEW_YORK =
          new Voyage.Builder(new VoyageNumber("0100S"), L.HONGKONG).
            addMovement(L.HANGZOU, toDate("2008-10-01", "12:00"), toDate("2008-10-03", "14:30")).
            addMovement(L.TOKYO, toDate("2008-10-03", "21:00"), toDate("2008-10-06", "06:15")).
            addMovement(L.MELBOURNE, toDate("2008-10-06", "11:00"), toDate("2008-10-12", "11:30")).
            addMovement(L.NEWYORK, toDate("2008-10-14", "12:00"), toDate("2008-10-23", "23:10")).
            build();

        public static readonly Voyage NEW_YORK_TO_DALLAS =
          new Voyage.Builder(new VoyageNumber("0200T"), L.NEWYORK).
            addMovement(L.CHICAGO, toDate("2008-10-24", "07:00"), toDate("2008-10-24", "17:45")).
            addMovement(L.DALLAS, toDate("2008-10-24", "21:25"), toDate("2008-10-25", "19:30")).
            build();

        public static readonly Voyage DALLAS_TO_HELSINKI =
          new Voyage.Builder(new VoyageNumber("0300A"), L.DALLAS).
            addMovement(L.HAMBURG, toDate("2008-10-29", "03:30"), toDate("2008-10-31", "14:00")).
            addMovement(L.STOCKHOLM, toDate("2008-11-01", "15:20"), toDate("2008-11-01", "18:40")).
            addMovement(L.HELSINKI, toDate("2008-11-02", "09:00"), toDate("2008-11-02", "11:15")).
            build();

        public static readonly Voyage DALLAS_TO_HELSINKI_ALT =
          new Voyage.Builder(new VoyageNumber("0301S"), L.DALLAS).
            addMovement(L.HELSINKI, toDate("2008-10-29", "03:30"), toDate("2008-11-05", "15:45")).
            build();

        public static readonly Voyage HELSINKI_TO_HONGKONG =
          new Voyage.Builder(new VoyageNumber("0400S"), L.HELSINKI).
            addMovement(L.ROTTERDAM, toDate("2008-11-04", "05:50"), toDate("2008-11-06", "14:10")).
            addMovement(L.SHANGHAI, toDate("2008-11-10", "21:45"), toDate("2008-11-22", "16:40")).
            addMovement(L.HONGKONG, toDate("2008-11-24", "07:00"), toDate("2008-11-28", "13:37")).
            build();

        private static DateTime toDate(string date)
        {
            return DateTime.Parse(date);
        }

        private static DateTime toDate(string date, string time)
        {
            return DateTime.Parse(date + " " + time);
        }

        private static readonly IDictionary<VoyageNumber, Voyage> ALL = new Dictionary<VoyageNumber, Voyage>();

        static SampleVoyages()
        {
            foreach(FieldInfo field in typeof(SampleVoyages).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if(field.FieldType.Equals(typeof(Voyage)))
                {
                    Voyage voyage = (Voyage)field.GetValue(null);
                    ALL.Add(voyage.VoyageNumber, voyage);
                }
            }
        }

        public static IEnumerable<Voyage> getAll()
        {
            return ALL.Values;
        }

        public static Voyage lookup(VoyageNumber voyageNumber)
        {
            return ALL.ContainsKey(voyageNumber) ? ALL[voyageNumber] : null;
        }
    }
}