using DomainDrivenDelivery.Domain.Model.Frieght;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// Handling event repository.
    /// </summary>
    public interface HandlingEventRepository
    {
        /// <summary>
        /// Finds the handling event for a sequence number.
        /// </summary>
        /// <param name="eventSequenceNumber">event sequence number</param>
        /// <returns>The handling event with this sequence number, or null if not found</returns>
        HandlingEvent find(EventSequenceNumber eventSequenceNumber);

        /// <summary>
        /// Stores a (new) handling event.
        /// </summary>
        /// <param name="event">event handling event to save</param>
        void store(HandlingEvent @event);

        /// <summary>
        /// Looks up the handling history of a cargo.
        /// </summary>
        /// <param name="cargo">cargo</param>
        /// <returns>The handling history of this cargo</returns>
        HandlingHistory lookupHandlingHistoryOfCargo(Cargo cargo);

        /// <summary>
        /// Gets the most recent handling of the cargo.
        /// </summary>
        /// <param name="cargo">cargo</param>
        /// <returns>The most recent handling of the cargo, or null if it has never been handled.</returns>
        HandlingEvent mostRecentHandling(Cargo cargo);
    }
}