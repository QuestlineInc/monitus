using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace Service.Monitus.Core.Notifiers
{
    /// <summary>
    /// Sends a message to a Slack channel.
    /// </summary>
    public class SlackNotifier : INotifier
    {
        public void Notify(string info)
        {
            SendMessage(info);
        }

        private static void SendMessage(string msg)
        {
            try
            {
                // add your own Slack channel URL here
                throw new NotImplementedException();

                using (var client = new HttpClient())
                {
                    var slackChannelUrl = "YOUR_URL_HERE";
                    var content = new StringContent(JsonConvert.SerializeObject(new { text = msg }), Encoding.UTF8, "application/json");
                    client.PostAsync(slackChannelUrl, content).Wait();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
