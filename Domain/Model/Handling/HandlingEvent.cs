using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Patterns;
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
        private EventSequenceNumber _sequenceNumber;
        private HandlingActivity _activity;
        private DateTime _completionTime;
        private DateTime _registrationTime;
        private Cargo _cargo;
        private OperatorCode _operatorCode;

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

            this._sequenceNumber = EventSequenceNumber.next();
            this._cargo = cargo;
            this._completionTime = completionTime;
            this._registrationTime = registrationTime;
            this._activity = new HandlingActivity(type, location, voyage);
            this._operatorCode = operatorCode;
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

            this._sequenceNumber = EventSequenceNumber.next();
            this._completionTime = completionTime;
            this._registrationTime = registrationTime;
            this._cargo = cargo;
            this._activity = new HandlingActivity(type, location);
        }

        public virtual EventSequenceNumber SequenceNumber
        {
            get { return _sequenceNumber; }
        }

        public virtual HandlingActivity Activity
        {
            get { return _activity; }
        }

        public virtual HandlingActivityType Type
        {
            get { return _activity.Type; }
        }

        public virtual Voyage Voyage
        {
            get { return _activity.Voyage != null ? _activity.Voyage : Voyage.NONE; }
        }

        public virtual OperatorCode OperatorCode
        {
            get { return _operatorCode; }
        }

        public virtual DateTime CompletionTime
        {
            get { return _completionTime; }
        }

        public virtual DateTime RegistrationTime
        {
            get { return _registrationTime; }
        }

        public virtual Location Location
        {
            get { return _activity.Location; }
        }

        public virtual Cargo Cargo
        {
            get { return this._cargo; }
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
                this.Cargo.Equals(other.Cargo) &&
                this.CompletionTime.Equals(other.CompletionTime) &&
                this.Activity.Equals(other.Activity);

            return equal;
        }

        public override int GetHashCode()
        {
            var hash = new HashCodeBuilder().
              append(_cargo).
              append(_completionTime).
              append(_activity).
              toHashCode();

            return hash;
        }

        public override string ToString()
        {
            return "Cargo: " + _cargo +
              "\nActivity: " + _activity +
              "\nCompleted on: " + _completionTime +
              "\nRegistered on: " + _registrationTime;
        }

        internal HandlingEvent()
        {
            // Needed by Hibernate
        }

        // Auto-generated surrogate key
        private long _id;
    }
}