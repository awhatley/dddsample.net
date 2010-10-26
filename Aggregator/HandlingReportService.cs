using System.ServiceModel;

namespace Aggregator
{
    [ServiceContract(Namespace = "http://ws.handling.interfaces.dddsample.net/")]
    public interface HandlingReportService
    {
        [OperationContract]
        [FaultContract(typeof(HandlingReportErrors))]
        void submitReport(HandlingReport arg0);
    }
}