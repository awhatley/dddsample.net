using System;
using System.Runtime.Serialization;

namespace DomainDrivenDelivery.Reporting.Api
{
    [DataContract]
    public class VoyageDetails
    {
        [DataMember] public string voyageNumber;
        [DataMember] public string nextStop;
        [DataMember] public DateTime etaNextStop;
        [DataMember] public string currentStatus;
        [DataMember] public int delayedByMinutes;
        [DataMember] public DateTime lastUpdatedOn;
    }
}