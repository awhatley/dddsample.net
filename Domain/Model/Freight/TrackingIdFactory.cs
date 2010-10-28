namespace DomainDrivenDelivery.Domain.Model.Freight
{
    /// <summary>
    /// Generates tracking ids for cargo. This is a domain service.
    /// </summary>
    public interface TrackingIdFactory
    {
        TrackingId nextTrackingId();
    }
}