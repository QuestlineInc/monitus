using Service.Monitus.Core.SLOs;

namespace Service.Monitus.Core.Notifiers
{
    /// <summary>
    /// Notifiers are used to send alerts to the appropriate channels when an SLO is violated.
    /// 
    /// Slack is one example, though any preferred form of communication (email, SMS, Teams, etc.) could be used.
    /// 
    /// Different notifiers can be used for different SLOs.
    /// 
    /// Specifying the Notifier in the SLO configuration will determine which Notifier is used.
    /// </summary>
    public interface INotifier
    {
        void Notify(string info);
    }
}