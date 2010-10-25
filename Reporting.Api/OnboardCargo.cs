using System.Runtime.Serialization;

namespace DomainDrivenDelivery.Reporting.Api
{
    [DataContract]
    public class OnboardCargo
    {
        [DataMember] public string trackingId;
        [DataMember] public string finalDestination;
    }
}