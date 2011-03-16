DDD Sample .NET
===============

A port of the [DDD Sample Application](http://dddsample.sourceforge.net/) to C#.

The conversion strives to maintain as much of the original design as possible, while
taking advantage of .NET features like WCF and ASP.NET MVC3.

The projects in the solution are:

* Aggregator: Service and Data contracts for the upstream aggregation service. 
* Application: The application layer for the main Booking and Tracking system.
* Booking.Api: Service and Data contracts for the remote Booking service.
* Booking.Service: A WCF host for the Booking service.
* Booking.Web: An MVC3 UI for Booking and Tracking.
* Domain: The domain model, services, and repository interfaces.
* Infrastructure: Implementations of the various abstractions used throughout.
* Interfaces: Implementations for the remote booking and handling services.
* Pathfinder.Api: Service and Data contracts for the remote Routing service.
* Reporting.Api: Service and Data contracts for the remote handling submission service.
* Resources: Spring, Hibernate configuration files, etc.
* Utilities: Miscellaneous helper classes to bridge the gap from Java.

Todo List
---------

* Perhaps move over to fluent NHibernate / code-based Spring.NET
* Convert held-over java syntax conventions to .NET style
* Continue to wire everything up
* Port over the incident logging application

