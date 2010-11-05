using System;
using System.Runtime.Serialization;

using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Reporting.Api
{
    [DataContract]
    public class Handling
    {
        [DataMember]
        public string type;
        
        [DataMember]
        public string location;
        
        [DataMember]
        public string voyage;
        
        [IgnoreDataMember]
        public DateTime completedOn;

        [DataMember(Name = "completedOn")]
        public String completedOnAsString
        {
            get
            {
                return getCompletedOnAsString();
            }
        }

        public string getType()
        {
            return type;
        }

        public void setType(string type)
        {
            this.type = type;
        }

        public string getLocation()
        {
            return location;
        }

        public void setLocation(string location)
        {
            this.location = location;
        }

        public string getVoyage()
        {
            return voyage;
        }

        public void setVoyage(string voyage)
        {
            this.voyage = voyage;
        }

        public string getCompletedOnAsString()
        {
            return getCompletedOn().ToString(DateFormats.US_FORMAT);            
        }

        public DateTime getCompletedOn()
        {
            return completedOn;
        }

        public void setCompletedOn(DateTime completedOn)
        {
            this.completedOn = completedOn;
        }

        public override bool Equals(object that)
        {
            return EqualsBuilder.reflectionEquals(typeof(Handling), this, that);
        }

        public override int GetHashCode()
        {
            return HashCodeBuilder.reflectionHashCode(this);
        }

        public override string ToString()
        {
            return ToStringBuilder.reflectionToString(this, ToStringStyle.MULTI_LINE_STYLE);
        }
    }
}