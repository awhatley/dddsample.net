namespace DomainDrivenDelivery.Domain.Model.Travel
{
    /// <summary>
    /// Voyage repository.
    /// </summary>
    public interface VoyageRepository
    {
        /// <summary>
        /// Finds a voyage using voyage number.
        /// </summary>
        /// <param name="voyageNumber">voyage number</param>
        /// <returns>The voyage, or null if not found.</returns>
        Voyage find(VoyageNumber voyageNumber);
    }
}