using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Services;
using DomainDrivenDelivery.Pathfinder.Api;

using Dotnet.Commons.Logging;

using Spring.Stereotype;

namespace DomainDrivenDelivery.Infrastructure.Routing
{
    /// <summary>
    /// Our end of the routing service. This is basically a data model
    /// translation layer between our domain model and the API put forward
    /// by the routing team, which operates in a different context from us.
    /// </summary>
    [Service]
    public class ExternalRoutingService : RoutingService
    {
        private readonly GraphTraversalService graphTraversalService;
        private readonly LocationRepository locationRepository;
        private readonly VoyageRepository voyageRepository;
        private static readonly ILog log = LogFactory.GetLogger(typeof(ExternalRoutingService));

        public ExternalRoutingService(GraphTraversalService graphTraversalService,
                                      LocationRepository locationRepository,
                                      VoyageRepository voyageRepository)
        {
            this.graphTraversalService = graphTraversalService;
            this.locationRepository = locationRepository;
            this.voyageRepository = voyageRepository;
        }

        public IEnumerable<Itinerary> fetchRoutesForSpecification(RouteSpecification routeSpecification)
        {
            /*
              The RouteSpecification is picked apart and adapted to the external API.
             */
            var origin = routeSpecification.Origin;
            var destination = routeSpecification.Destination;

            var limitations = new Hashtable();
            limitations.Add("DEADLINE", routeSpecification.ArrivalDeadline.ToString());

            IEnumerable<TransitPath> transitPaths;
            try
            {
                transitPaths = graphTraversalService.findShortestPath(
                  origin.UnLocode.Value,
                  destination.UnLocode.Value,
                  limitations
                );
            }
            catch(Exception e)
            {
                log.Error(e, e);
                return new List<Itinerary>();
            }

            /*
             The returned result is then translated back into our domain model.
            */
            var itineraries = new List<Itinerary>();

            foreach(TransitPath transitPath in transitPaths)
            {
                var itinerary = toItinerary(transitPath);
                // Use the specification to safe-guard against invalid itineraries
                if(routeSpecification.IsSatisfiedBy(itinerary))
                {
                    itineraries.Add(itinerary);
                }
                else
                {
                    log.Warn("Received itinerary that did not satisfy the route specification");
                }
            }

            return itineraries;
        }

        private Itinerary toItinerary(TransitPath transitPath)
        {
            var legs = new List<Leg>(transitPath.getTransitEdges().Count());
            foreach(TransitEdge edge in transitPath.getTransitEdges())
            {
                legs.Add(toLeg(edge));
            }
            return new Itinerary(legs);
        }

        private Leg toLeg(TransitEdge edge)
        {
            return Leg.DeriveLeg(
              voyageRepository.find(new VoyageNumber(edge.getVoyageNumber())),
              locationRepository.find(new UnLocode(edge.getFromUnLocode())),
              locationRepository.find(new UnLocode(edge.getToUnLocode()))
            );
        }
    }
}