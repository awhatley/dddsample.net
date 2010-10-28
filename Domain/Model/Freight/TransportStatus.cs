using DomainDrivenDelivery.Domain.Model.Shared;

namespace DomainDrivenDelivery.Domain.Model.Freight
{
    public enum TransportStatus
    {
        NOT_RECEIVED,
        IN_PORT,
        ONBOARD_CARRIER,
        CLAIMED,
        UNKNOWN,
    }

    // TODO: java-style enums?
    public static class TransportStatusExtensions
    {
        public static TransportStatus derivedFrom(HandlingActivity handlingActivity)
        {
            if(handlingActivity == null)
            {
                return TransportStatus.NOT_RECEIVED;
            }

            switch(handlingActivity.type())
            {
                case HandlingActivityType.LOAD:
                    return TransportStatus.ONBOARD_CARRIER;

                case HandlingActivityType.UNLOAD:
                case HandlingActivityType.RECEIVE:
                case HandlingActivityType.CUSTOMS:
                    return TransportStatus.IN_PORT;

                case HandlingActivityType.CLAIM:
                    return TransportStatus.CLAIMED;

                default:
                    return TransportStatus.UNKNOWN;
            }
        }
    }
}