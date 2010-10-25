using DomainDrivenDelivery.Domain.Model.Frieght;
using DomainDrivenDelivery.Domain.Model.Handling;

namespace DomainDrivenDelivery.Application.Event
{
    /// <summary>
    /// This interface provides a way to let other parts
    /// of the system know about events that have occurred.
    /// </summary>
    /// <remarks>
    /// It may be implemented synchronously or asynchronously, using
    /// for example MSMQ.
    /// </remarks>
    public interface SystemEvents
    {
        /// <summary>
        /// A cargo has been handled.
        /// </summary>
        /// <param name="event">handling event</param>
        void notifyOfHandlingEvent(HandlingEvent @event);

        /// <summary>
        /// Cargo delivery has been updated.
        /// </summary>
        /// <param name="cargo">cargo</param>
        void notifyOfCargoUpdate(Cargo cargo);

        // TODO
        //void notifyOfScheduleUpdate(Voyage voyage);
    }
}