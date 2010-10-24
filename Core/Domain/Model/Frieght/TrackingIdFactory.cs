namespace DomainDrivenDelivery.Domain.Model.Frieght
{
    /// <summary>
    /// Generates tracking ids for cargo. This is a domain service.
    /// </summary>
    public interface TrackingIdFactory
    {
        TrackingId nextTrackingId();
    }
}