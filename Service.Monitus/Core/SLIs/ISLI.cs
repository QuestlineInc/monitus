
namespace Service.Monitus.Core.SLIs
{
    /// <summary>
    /// The interface for a Service Level Indicator (SLI).
    /// This is where the logic for determining the status of a component is defined.
    /// </summary>
    public interface ISLI
    {
        /// <summary>
        /// Returns a value representing the status of the component.
        /// This value is then interpreted by the parent SLO.
        /// </summary>
        double GetIndicator();
        /// <summary>
        /// Retrieves a message describing the status of the component.
        /// </summary>
        string GetIndicatorMessage();
        /// <summary>
        /// Used in the case of an exception to retrieve the exception message.
        /// </summary>
        string GetMessage();
    }
}
