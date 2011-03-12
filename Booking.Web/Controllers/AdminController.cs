using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using DomainDrivenDelivery.Booking.Api;
using DomainDrivenDelivery.Booking.Web.Models;

namespace DomainDrivenDelivery.Booking.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly BookingServiceFacade _bookingServiceFacade;

        public AdminController(BookingServiceFacade bookingServiceFacade)
        {
            _bookingServiceFacade = bookingServiceFacade;
        }

        [HttpGet]
        public ActionResult CargoBookingForm()
        {
            var dtoList = new List<LocationDTO>().AsEnumerable(); // _bookingServiceFacade.listShippingLocations();
            dtoList = dtoList.OrderBy(l => l.getUnLocode());
            
            return View(dtoList);
        }

        [HttpPost]
        public ActionResult BookCargo(CargoBookingCommand command)
        {
            var arrivalDeadline = DateTime.Parse(command.arrivalDeadline);
            var trackingId = _bookingServiceFacade.bookNewCargo(
                command.originUnlocode, 
                command.destinationUnlocode, 
                arrivalDeadline);

            return RedirectToAction("Show", new { trackingId });
        }

        public ActionResult List()
        {
            var cargoList = _bookingServiceFacade.listAllCargos();
            return View(cargoList);
        }

        public ActionResult Show(string trackingId)
        {
            var dto = _bookingServiceFacade.loadCargoForRouting(trackingId);
            return View(dto);
        }

        public ActionResult SelectItinerary(string trackingId)
        {
            var model = new SelectItineraryModel {
                Cargo = _bookingServiceFacade.loadCargoForRouting(trackingId),
                RouteCandidates = _bookingServiceFacade.requestPossibleRoutesForCargo(trackingId),
            };

            return View(model);
        }

        public ActionResult AssignItinerary(RouteAssignmentCommand command)
        {
            var legs = command.legs
                .Select(leg => new LegDTO(
                    leg.voyageNumber, 
                    leg.fromUnLocode, 
                    leg.toUnLocode, 
                    leg.fromDate, 
                    leg.toDate));

            var selectedRoute = new RouteCandidateDTO(legs);
            _bookingServiceFacade.assignCargoToRoute(command.trackingId, selectedRoute);

            return RedirectToAction("Show", new { command.trackingId });
        }

        public ActionResult PickNewDestination(string trackingId)
        {
            var model = new PickNewDestinationModel {
                Cargo = _bookingServiceFacade.loadCargoForRouting(trackingId),
                Locations = _bookingServiceFacade.listShippingLocations(),
            };

            return View(model);
        }

        public ActionResult ChangeDestination(string trackingId, string unLocode)
        {
            _bookingServiceFacade.changeDestination(trackingId, unLocode);
            return RedirectToAction("Show", new { trackingId });
        }

        public JsonResult VoyageDelayedForm()
        {
            var departures = new Dictionary<string, IEnumerable<string>>();
            var arrivals = new Dictionary<string, IEnumerable<string>>();
            var voyages = _bookingServiceFacade.listAllVoyages();

            foreach(VoyageDTO voyage in voyages)
            {
                var departureLocations = new List<string>();
                var arrivalLocations = new List<string>();

                foreach(var dto in voyage.getMovements())
                {
                    departureLocations.Add(dto.getDepartureLocation().getUnLocode());
                    arrivalLocations.Add(dto.getArrivalLocation().getUnLocode());
                }

                departures.Add(voyage.getVoyageNumber(), departureLocations);
                arrivals.Add(voyage.getVoyageNumber(), arrivalLocations);
            }

            var model = new VoyageDelayedFormModel {
                Departures = departures,
                Arrivals = arrivals,
                Voyages = voyages,
            };

            return Json(model);
        }

        public ActionResult VoyageDelayed(VoyageDelayCommand command)
        {
            if(command.type == VoyageDelayCommand.DelayType.DEPT)
            {
                _bookingServiceFacade.departureDelayed(new VoyageDelayDTO(command.voyageNumber,
                    command.hours * 60));
            }

            else if(command.type == VoyageDelayCommand.DelayType.ARR)
            {
                _bookingServiceFacade.arrivalDelayed(new VoyageDelayDTO(command.voyageNumber,
                    command.hours * 60));
            }

            return RedirectToAction("List");
        }
    }
}