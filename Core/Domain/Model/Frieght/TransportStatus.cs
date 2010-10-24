using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Frieght
{
    public class TransportStatus : ValueObject<TransportStatus>
    {
        public static readonly TransportStatus
            NOT_RECEIVED = new TransportStatus(),
            IN_PORT = new TransportStatus(),
            ONBOARD_CARRIER = new TransportStatus(),
            CLAIMED = new TransportStatus(),
            UNKNOWN = new TransportStatus();

        public static TransportStatus derivedFrom(HandlingActivity handlingActivity)
        {
            if(handlingActivity == null)
            {
                return NOT_RECEIVED;
            }

            else if(handlingActivity.type() == HandlingActivityType.LOAD)
            {
                return ONBOARD_CARRIER;
            }
            
            else if(handlingActivity.type() == HandlingActivityType.UNLOAD ||
                    handlingActivity.type() == HandlingActivityType.RECEIVE ||
                    handlingActivity.type() == HandlingActivityType.CUSTOMS)
            {
                return IN_PORT;
            }
            
            else if(handlingActivity.type() == HandlingActivityType.CLAIM)
            {
                return CLAIMED;
            }
            
            else
            {
                return UNKNOWN;
            }
        }

        public bool sameValueAs(TransportStatus other)
        {
            return this.Equals(other);
        }
    }
}