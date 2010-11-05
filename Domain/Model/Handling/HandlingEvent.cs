using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Patterns.DomainEvent;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// A HandlingEvent is used to register the event when, for instance,
    /// a cargo is unloaded from a carrier at a some location at a given time.
    /// </summary>
    /// <remarks>
    /// The HandlingEvent's are sent from different Incident Logging Applications
    /// some time after the event occured and contain information about the
    /// <see cref="TrackingId"/>, <see cref="Locations.Location"/>, timestamp of the completion of the event,
    /// and possibly, if applicable a <see cref="Travel.Voyage"/>.
    /// <p/>
    /// This class is the only member, and consequently the root, of the HandlingEvent aggregate.
    /// <p/>
    /// HandlingEvent's could contain information about a <see cref="Travel.Voyage"/> and if so,
    /// the event type must be either <see cref="HandlingActivityType.LOAD"/> or <see cref="HandlingActivityType.UNLOAD"/>.
    /// <p/>
    /// All other events must be of <see cref="HandlingActivityType.RECEIVE"/>, <see cref="HandlingActivityType.CLAIM"/> or <see cref="HandlingActivityType.CUSTOMS"/>.
    /// </remarks>
    public class HandlingEvent : DomainEvent<HandlingEvent>
    {
        public virtual EventSequenceNumber SequenceNumber { get; private set; }
        public virtual Cargo Cargo { get; private set; }
        public virtual DateTime CompletionTime { get; private set; }
        public virtual DateTime RegistrationTime { get; private set; }
        public virtual HandlingActivity Activity { get; private set; }
        public virtual OperatorCode OperatorCode { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cargo">cargo</param>
        /// <param name="completionTime">completion time, the reported time that the event actually happened (e.g. the receive took place).</param>
        /// <param name="registrationTime">registration time, the time the message is received</param>
        /// <param name="type">type of event</param>
        /// <param name="location">where the event took place</param>
        /// <param name="voyage">the voyage</param>
        /// <param name="operatorCode">operator code for port operator</param>
        internal HandlingEvent(Cargo cargo,
                      DateTime completionTime,
                      DateTime registrationTime,
                      HandlingActivityType type,
                      Location location,
                      Voyage voyage,
                      OperatorCode operatorCode)
        {
            Validate.notNull(cargo, "Cargo is required");
            Validate.notNull(location, "Location is required");
            Validate.notNull(voyage, "Voyage is required");
            Validate.notNull(operatorCode, "Operator code is required");

            if(!type.isVoyageRelated())
            {
                throw new ArgumentException("Voyage is not allowed with event type " + type);
            }

            SequenceNumber = EventSequenceNumber.Next();
            Cargo = cargo;
            CompletionTime = completionTime;
            RegistrationTime = registrationTime;
            Activity = new HandlingActivity(type, location, voyage);
            OperatorCode = operatorCode;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cargo">cargo</param>
        /// <param name="completionTime">completion time, the reported time that the event actually happened (e.g. the receive took place).</param>
        /// <param name="registrationTime">registration time, the time the message is received</param>
        /// <param name="type">type of event</param>
        /// <param name="location">where the event took place</param>
        public HandlingEvent(Cargo cargo,
                       DateTime completionTime,
                       DateTime registrationTime,
                       HandlingActivityType type,
                       Location location)
        {
            // TODO: make internal
            Validate.notNull(cargo, "Cargo is required");
            Validate.notNull(location, "Location is required");

            if(type.isVoyageRelated())
            {
                throw new ArgumentException("Voyage is required for event type " + type);
            }

            SequenceNumber = EventSequenceNumber.Next();
            CompletionTime = completionTime;
            RegistrationTime = registrationTime;
            Cargo = cargo;
            Activity = new HandlingActivity(type, location);
        }

        public virtual HandlingActivityType Type
        {
            get { return Activity.Type; }
        }

        public virtual Voyage Voyage
        {
            get { return Activity.Voyage != null ? Activity.Voyage : Voyage.None; }
        }

        public virtual Location Location
        {
            get { return Activity.Location; }
        }

        public override bool Equals(Object o)
        {
            if(this == o) return true;
            if(o == null || GetType() != o.GetType()) return false;

            var @event = (HandlingEvent)o;

            return sameEventAs(@event);
        }

        public virtual bool sameEventAs(HandlingEvent other)
        {
            var equal = other != null &&
                Cargo.Equals(other.Cargo) &&
                CompletionTime.Equals(other.CompletionTime) &&
                Activity.Equals(other.Activity);

            return equal;
        }

        public override int GetHashCode()
        {
            var hash = new HashCodeBuilder().
              append(Cargo).
              append(CompletionTime).
              append(Activity).
              toHashCode();

            return hash;
        }

        public override string ToString()
        {
            return "Cargo: " + Cargo +
              "\nActivity: " + Activity +
              "\nCompleted on: " + CompletionTime +
              "\nRegistered on: " + RegistrationTime;
        }

        protected internal HandlingEvent()
        {
        }

        // Auto-generated surrogate key
        private long _id;
    }
}