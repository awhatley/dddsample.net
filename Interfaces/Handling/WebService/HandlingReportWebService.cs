using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

using Aggregator;

using DomainDrivenDelivery.Application.Handling;
using DomainDrivenDelivery.Domain.Model.Handling;

using Dotnet.Commons.Logging;

using Spring.Stereotype;

namespace DomainDrivenDelivery.Interfaces.Handling.WebService
{
    /// <summary>
    /// This web service endpoint implementation performs basic validation and parsing
    /// of an incoming handling report, and attempts to register proper handling events
    /// by calling the application layer.
    /// </summary>
    [Service]
    public class HandlingReportWebService : HandlingReportService
    {
        private HandlingEventService handlingEventService;
        private readonly static ILog logger = LogFactory.GetLogger(typeof(HandlingReportWebService));

        public void submitReport(HandlingReport handlingReport)
        {
            var validationErrors = new List<string>();

            var completionTime = handlingReport.completionTime;
            var voyageNumber = HandlingReportParser.parseVoyageNumber(handlingReport.voyageNumber, validationErrors);
            var type = HandlingReportParser.parseEventType(handlingReport.type, validationErrors);
            var unLocode = HandlingReportParser.parseUnLocode(handlingReport.unLocode, validationErrors);
            var operatorCode = HandlingReportParser.parseOperatorCode();

            var allErrors = new Dictionary<string, string>();
            foreach(string trackingIdStr in handlingReport.trackingIds)
            {
                var trackingId = HandlingReportParser.parseTrackingId(trackingIdStr, validationErrors);

                if(!validationErrors.Any())
                {
                    try
                    {
                        handlingEventService.registerHandlingEvent(completionTime, trackingId, voyageNumber, unLocode, type, operatorCode);
                    }
                    catch(CannotCreateHandlingEventException e)
                    {
                        logger.Error(e, e);
                        allErrors.Add(trackingIdStr, e.Message);
                    }
                }
                else
                {
                    logger.Error("Parse error in handling report: " + validationErrors);
                    allErrors.Add(trackingIdStr, String.Join(", ", validationErrors));
                }
            }

            if(allErrors.Any())
            {
                var faultInfo = new HandlingReportErrors();
                throw new FaultException<HandlingReportErrors>(faultInfo, createErrorMessage(allErrors));
            }
        }

        private String createErrorMessage(Dictionary<string, string> allErrors)
        {
            var sb = new StringBuilder("--- BEGIN HANDLING REPORT ERRORS ---\n");
            foreach(var e in allErrors)
            {
                sb.Append(e.Key).Append(" : ").Append(e.Value).Append("\n");
            }
            sb.Append("--- END HANDLING REPORT ERRORS ---");
            return sb.ToString();
        }

        public void setHandlingEventService(HandlingEventService handlingEventService)
        {
            this.handlingEventService = handlingEventService;
        }
    }
}