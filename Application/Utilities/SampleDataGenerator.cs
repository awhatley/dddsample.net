using System;
using System.Data;
using System.Linq;

using Spring.Data.Core;
using Spring.Transaction.Support;

namespace DomainDrivenDelivery.Application.Utilities
{
    /// <summary>
    /// Provides sample data.
    /// </summary>
    public static class SampleDataGenerator // : ServletContextListener
    {
        private static readonly DateTime @base = DateTime.Parse("2008-01-01").AddDays(-100);

        private static void loadHandlingEventData(AdoTemplate jdbcTemplate)
        {
            String activitySql =
                "insert into HandlingActivity (id, handling_event_type, location_id, voyage_id) values ({0}, '{1}', {2}, {3})";
            Object[][] actvityArgs = {
                new object[] {1, "RECEIVE", 1, null}, new object[] {2, "LOAD", 1, 1},
                new object[] {3, "UNLOAD", 5, 1}, new object[] {4, "LOAD", 5, 1}, new object[] {5, "UNLOAD", 6, 1},
                new object[] {6, "LOAD", 6, 1}, new object[] {7, "UNLOAD", 3, 1}, new object[] {8, "LOAD", 3, 1},
                new object[] {9, "UNLOAD", 4, 1}, new object[] {10, "LOAD", 4, 1}, new object[] {11, "UNLOAD", 2, 1},
                new object[] {12, "CLAIM", 2, null}, //ZYX (AUMEL - USCHI - DEHAM -)
                new object[] {13, "RECEIVE", 2, null}, new object[] {14, "LOAD", 2, 2},
                new object[] {15, "UNLOAD", 7, 2}, new object[] {16, "LOAD", 7, 2}, new object[] {17, "UNLOAD", 6, 2},
                new object[] {18, "LOAD", 6, 2}, //ABC
                new object[] {19, "CLAIM", 2, null}, //CBA
                new object[] {20, "RECEIVE", 2, null}, new object[] {21, "LOAD", 2, 2},
                new object[] {22, "UNLOAD", 7, 2}, //FGH
                new object[] {23, "RECEIVE", 3, null}, new object[] {24, "LOAD", 3, 3}, // JKL
                new object[] {25, "RECEIVE", 6, null}, new object[] {26, "LOAD", 6, 3},
                new object[] {27, "UNLOAD", 5, 3} // Unexpected event
            };

            executeUpdate(jdbcTemplate, activitySql, actvityArgs, "HandlingActivity");

            String handlingEventSql =
                "insert into HandlingEvent (id, sequence_number, completionTime, registrationTime, cargo_id, operator_code, activity_id) " +
                    "values ({0}, {0}, '{1}', '{2}', {3}, '{4}', {5})";

            Object[][] handlingEventArgs = {
                //XYZ (SESTO-FIHEL-DEHAM-CNHKG-JPTOK-AUMEL)
                new object[] {1, ts(0), ts((0)), 1, null, 1}, new object[] {2, ts((4)), ts((5)), 1, "AS34F", 2},
                new object[] {3, ts((14)), ts((14)), 1, "AS34F", 3}, new object[] {4, ts((15)), ts((15)), 1, "AS34F", 4}
                , new object[] {5, ts((30)), ts((30)), 1, "AS34F", 5},
                new object[] {6, ts((33)), ts((33)), 1, "AS34F", 6}, new object[] {7, ts((34)), ts((34)), 1, "AS34F", 7}
                , new object[] {8, ts((60)), ts((60)), 1, "AS34F", 8},
                new object[] {9, ts((70)), ts((71)), 1, "AS34F", 9},
                new object[] {10, ts((75)), ts((75)), 1, "AS34F", 10},
                new object[] {11, ts((88)), ts((88)), 1, "AS34F", 11},
                new object[] {12, ts((100)), ts((102)), 1, null, 12}, //ZYX (AUMEL - USCHI - DEHAM -)
                new object[] {13, ts((200)), ts((201)), 3, null, 13},
                new object[] {14, ts((202)), ts((202)), 3, "AS34F", 14},
                new object[] {15, ts((208)), ts((208)), 3, "AS34F", 15},
                new object[] {16, ts((212)), ts((212)), 3, "AS34F", 16},
                new object[] {17, ts((230)), ts((230)), 3, "AS34F", 17},
                new object[] {18, ts((235)), ts((235)), 3, "AS34F", 18}, //ABC
                new object[] {19, ts((20)), ts((21)), 2, null, 19}, //CBA
                new object[] {20, ts((0)), ts((1)), 4, null, 20}, new object[] {21, ts((10)), ts((11)), 4, "AS34F", 21},
                new object[] {22, ts((20)), ts((21)), 4, "AS34F", 22}, //FGH
                new object[] {23, ts(100), ts(160), 5, null, 23}, new object[] {24, ts(150), ts(110), 5, "AS34F", 24},
                // JKL
                new object[] {25, ts(200), ts(220), 6, null, 25}, new object[] {26, ts(300), ts(330), 6, "AS34F", 26},
                new object[] {27, ts(400), ts(440), 6, null, 27} // Unexpected event
            };
            executeUpdate(jdbcTemplate, handlingEventSql, handlingEventArgs, "HandlingEvent");
        }

        private static void loadCarrierMovementData(AdoTemplate jdbcTemplate)
        {
            String voyageSql = "insert into Voyage (id, voyage_number) values ({0}, '{1}')";
            Object[][] voyageArgs = {new object[] {1, "0101"}, new object[] {2, "0202"}, new object[] {3, "0303"}};
            executeUpdate(jdbcTemplate, voyageSql, voyageArgs, "Voyage");

            String carrierMovementSql =
                "insert into CarrierMovement (id, voyage_id, departure_location_id, arrival_location_id, departure_time, arrival_time, cm_index) " +
                    "values ({0},{1},{2},{3},'{4}','{5}',{6})";

            Object[][] carrierMovementArgs = {
                // SESTO - FIHEL - DEHAM - CNHKG - JPTOK - AUMEL (voyage 0101)
                new object[] {1, 1, 1, 5, ts(1), ts(2), 0}, new object[] {2, 1, 5, 6, ts(1), ts(2), 1},
                new object[] {3, 1, 6, 3, ts(1), ts(2), 2}, new object[] {4, 1, 3, 4, ts(1), ts(2), 3},
                new object[] {5, 1, 4, 2, ts(1), ts(2), 4}, // AUMEL - USCHI - DEHAM - SESTO - FIHEL (voyage 0202)
                new object[] {7, 2, 2, 7, ts(1), ts(2), 0}, new object[] {8, 2, 7, 6, ts(1), ts(2), 1},
                new object[] {9, 2, 6, 1, ts(1), ts(2), 2}, new object[] {6, 2, 1, 5, ts(1), ts(2), 3},
                // CNHKG - AUMEL - FIHEL - DEHAM - SESTO - USCHI - JPTKO (voyage 0303)
                new object[] {10, 3, 3, 2, ts(1), ts(2), 0}, new object[] {11, 3, 2, 5, ts(1), ts(2), 1},
                new object[] {12, 3, 6, 1, ts(1), ts(2), 2}, new object[] {13, 3, 1, 7, ts(1), ts(2), 3},
                new object[] {14, 3, 7, 4, ts(1), ts(2), 4}
            };
            executeUpdate(jdbcTemplate, carrierMovementSql, carrierMovementArgs, "CarrierMovement");
        }

        private static void loadCargoData(AdoTemplate jdbcTemplate)
        {
            String cargoSql =
                "insert into Cargo (id, tracking_id, spec_origin_id, spec_destination_id, spec_arrival_deadline, last_update) " +
                    "values ({0}, '{1}', {2}, {3}, '{4}', '{5}')";

            Object[][] cargoArgs = {
                new object[] {1, "XYZ", 1, 2, ts(10), ts(100)},
                new object[] {2, "ABC", 1, 5, ts(20), ts(100)}, new object[] {3, "ZYX", 2, 1, ts(30), ts(100)},
                new object[] {4, "CBA", 5, 1, ts(40), ts(100)}, new object[] {5, "FGH", 3, 5, ts(50), ts(100)},
                new object[] {6, "JKL", 6, 4, ts(60), ts(100)}
            };
            executeUpdate(jdbcTemplate, cargoSql, cargoArgs, "Cargo");
        }

        private static void loadLocationData(AdoTemplate jdbcTemplate)
        {
            String locationSql = "insert into Location (id, unlocode, name) " + "values ({0}, '{1}', '{2}')";

            Object[][] locationArgs = {
                new object[] {1, "SESTO", "Stockholm"}, new object[] {2, "AUMEL", "Melbourne"},
                new object[] {3, "CNHKG", "Hongkong"}, new object[] {4, "JPTOK", "Tokyo"},
                new object[] {5, "FIHEL", "Helsinki"}, new object[] {6, "DEHAM", "Hamburg"},
                new object[] {7, "USCHI", "Chicago"}
            };
            executeUpdate(jdbcTemplate, locationSql, locationArgs, "Location");
        }

        private static void loadItineraryData(AdoTemplate jdbcTemplate)
        {
            String legSql =
                "insert into Leg (id, cargo_id, voyage_id, load_location_id, unload_location_id, load_time, unload_time, leg_index) " +
                    "values ({0},{1},{2},{3},{4},'{5}','{6}',{7})";

            Object[][] legArgs = {
                // Cargo 5: Hongkong - Melbourne - Stockholm - Helsinki
                new object[] {1, 5, 1, 3, 2, ts(1), ts(2), 0}, new object[] {2, 5, 1, 2, 1, ts(3), ts(4), 1},
                new object[] {3, 5, 1, 1, 5, ts(4), ts(5), 2}, // Cargo 6: Hamburg - Stockholm - Chicago - Tokyo
                new object[] {4, 6, 2, 6, 1, ts(1), ts(2), 0}, new object[] {5, 6, 2, 1, 7, ts(3), ts(4), 1},
                new object[] {6, 6, 2, 7, 4, ts(5), ts(6), 2}
            };

            executeUpdate(jdbcTemplate, legSql, legArgs, "Leg");
        }

        public static void loadSampleData(AdoTemplate jdbcTemplate, TransactionTemplate transactionTemplate)
        {
            transactionTemplate.Execute(status => {
                loadLocationData(jdbcTemplate);
                loadCarrierMovementData(jdbcTemplate);
                loadCargoData(jdbcTemplate);
                loadItineraryData(jdbcTemplate);
                loadHandlingEventData(jdbcTemplate);
                return null;
            });
        }

        private static void executeUpdate(AdoTemplate jdbcTemplate, String sql, Object[][] args, string tableName)
        {
            jdbcTemplate.ExecuteNonQuery(CommandType.Text, String.Format("SET IDENTITY_INSERT {0} ON", tableName));

            foreach(Object[] arg in args)
                jdbcTemplate.ExecuteNonQuery(CommandType.Text, String.Format(sql, arg.Select(a => a ?? "NULL").ToArray()).Replace("'NULL'", "NULL"));

            jdbcTemplate.ExecuteNonQuery(CommandType.Text, String.Format("SET IDENTITY_INSERT {0} OFF", tableName));
        }

        private static DateTime ts(int hours)
        {
            return @base.AddHours(hours);
        }
 
        public static DateTime offset(int hours)
        {
            return ts(hours);
        }
    }
}