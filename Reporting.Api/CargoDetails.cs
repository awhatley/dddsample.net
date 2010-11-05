using System;
using System.Runtime.Serialization;

using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Reporting.Api
{
    [DataContract]
    public class CargoDetails
    {
        [DataMember]
        public string trackingId;

        [DataMember]
        public string receivedIn;

        [DataMember]
        public string finalDestination;

        [IgnoreDataMember]
        public DateTime arrivalDeadline;

        [DataMember(Name = "arrivalDeadline")]
        public string arrivalDeadlineAsString
        {
            get { return getArrivalDeadlineAsString(); }
        }

        [IgnoreDataMember]
        public DateTime eta;

        [DataMember(Name = "eta")]
        public string etaAsString
        {
            get { return getEtaAsString(); }
        }

        [DataMember]
        public string currentStatus;

        [DataMember]
        public string currentVoyage;

        [DataMember]
        public string currentLocation;

        [IgnoreDataMember]
        public DateTime lastUpdatedOn;

        [DataMember(Name = "lastUpdatedOn")]
        public string lastUpdatedOnAsString
        {
            get { return getLastUpdatedOnAsString(); }
        }

        public string getTrackingId()
        {
            return trackingId;
        }

        public void setTrackingId(string trackingId)
        {
            this.trackingId = trackingId;
        }

        public string getReceivedIn()
        {
            return receivedIn;
        }

        public void setReceivedIn(string receivedIn)
        {
            this.receivedIn = receivedIn;
        }

        public string getFinalDestination()
        {
            return finalDestination;
        }

        public void setFinalDestination(string finalDestination)
        {
            this.finalDestination = finalDestination;
        }

        public string getArrivalDeadlineAsString()
        {
            return getArrivalDeadline().ToString(DateFormats.US_FORMAT);
        }

        public DateTime getArrivalDeadline()
        {
            return arrivalDeadline;
        }

        public void setArrivalDeadline(DateTime arrivalDeadline)
        {
            this.arrivalDeadline = arrivalDeadline;
        }

        public string getEtaAsString()
        {
            return getEta().ToString(DateFormats.US_FORMAT);
        }

        public DateTime getEta()
        {
            return eta;
        }

        public void setEta(DateTime eta)
        {
            this.eta = eta;
        }

        public string getCurrentStatus()
        {
            return currentStatus;
        }

        public void setCurrentStatus(string currentStatus)
        {
            this.currentStatus = currentStatus;
        }

        public string getCurrentVoyage()
        {
            return currentVoyage;
        }

        public void setCurrentVoyage(string currentVoyage)
        {
            this.currentVoyage = currentVoyage;
        }

        public string getCurrentLocation()
        {
            return currentLocation;
        }

        public void setCurrentLocation(string currentLocation)
        {
            this.currentLocation = currentLocation;
        }

        public string getLastUpdatedOnAsString()
        {
            return getLastUpdatedOn().ToString(DateFormats.US_FORMAT);
        }

        public DateTime getLastUpdatedOn()
        {
            return lastUpdatedOn;
        }

        public void setLastUpdatedOn(DateTime lastUpdatedOn)
        {
            this.lastUpdatedOn = lastUpdatedOn;
        }

        public override bool Equals(object that)
        {
            return EqualsBuilder.reflectionEquals(typeof(CargoDetails), this, that);
        }

        public override int GetHashCode()
        {
            return HashCodeBuilder.reflectionHashCode(this);
        }

        public override String ToString()
        {
            return ToStringBuilder.reflectionToString(this, ToStringStyle.MULTI_LINE_STYLE);
        }
    }
}