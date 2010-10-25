using System.ServiceModel;
using System.ServiceModel.Web;

namespace DomainDrivenDelivery.Reporting.Api
{
    [ServiceContract]
    public interface ReportSubmission
    {
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/cargo")]
        void submitCargoDetails(CargoDetails cargoDetails);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/cargo/{trackingId}/handled")]
        void submitHandling(string trackingId, Handling handling);
    }
}