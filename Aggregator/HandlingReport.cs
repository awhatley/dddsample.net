using System;
using System.Runtime.Serialization;

namespace Aggregator
{
    [DataContract]
    public class HandlingReport
    {
        [DataMember(IsRequired = true)]
        public DateTime completionTime { get; set; }
        
        [DataMember(IsRequired = true)]
        public string[] trackingIds { get; set; }
        
        [DataMember(IsRequired = true)]
        public string type { get; set; }
        
        [DataMember(IsRequired = true)]
        public string unLocode { get; set; }
        
        public string voyageNumber { get; set; }
    }
}