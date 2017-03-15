using System;
using System.Collections.Generic;
using Microsoft.Azure.IoT.Gateway;
using Proficy.Historian.ClientAccess.API;
using Newtonsoft.Json;

// Module for collecting data from GE (Proficy) Historian database
namespace Proficy.Historian.Module
{
    /// <summary>
    /// Gateway module - Connects to GE (Proficy) Historian on specified intervals and get specified values for further processing
    /// </summary>
    public class GatewayModule : IGatewayModule, IGatewayModuleStart
    {
        // Variables regarding gateway
        private Broker _broker;
        private Dictionary<string, string> _messageProperties = new Dictionary<string, string>();

        // Variables regarding Proficy Historian
        private ServerConnection _historian;
        private ProficyHistorianConfiguration _config;

        /// <summary>
        /// This function initiates the module and create all the connections
        /// </summary>
        /// <param name="broker">The broker of the Gateway this Module should run on</param>
        /// <param name="configuration">The json configuration for the module.</param>
        public void Create(Broker broker, byte[] configuration)
        {
            Console.WriteLine("Proficy.Historian.Module initializing...");

            // Initialize module
            this._broker = broker;

            // Get configuration
            try
            {
                string configurationString = System.Text.Encoding.UTF8.GetString(configuration);
                _config = JsonConvert.DeserializeObject<ProficyHistorianConfiguration>(configurationString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Proficy.Historian.Module not initialized - error: " + ex.Message);
                Destroy();
            }

            // Header for messages
            _messageProperties.Add("source", "Proficy.Historian.Module");
            _messageProperties.Add("name", "data");

            Console.WriteLine("Proficy.Historian.Module initialized.");
        }

        /// <summary>
        /// Start the data collection
        /// </summary>
        public void Start()
        {
            Console.WriteLine("Proficy.Historian.Module starting up...");

            try
            {
                // Define connection and establish it
                _historian = new ServerConnection(new ConnectionProperties { ServerHostName = _config.ServerName, Username = _config.UserName, Password = _config.Password, ServerCertificateValidationMode = CertificateValidationMode.None });
                _historian.Connect();

                // establish event handler
                _historian.DataChangedEvent += new DataChangedHandler(Historian_DataChangedEvent);

                // Setup data subscriptions
                foreach (ProficyHistorianTag tag in _config.TagsToSubscribe)
                {
                    _historian.IData.Subscribe(new DataSubscriptionInfo { Tagname = tag.TagName, MinimumElapsedMilliSeconds = tag.MinimumElapsedMilliSeconds });
                    Console.WriteLine("Proficy.Historian.Module - subscribing to " + tag.TagName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Proficy.Historian.Module - Error while initializing: " + ex.Message);
                Destroy();
            }

            Console.WriteLine("Proficy.Historian.Module started...");
        }

        /// <summary>
        /// Handle data change events from the Historian
        /// </summary>
        /// <param name="values">The list of changed elements</param>
        private void Historian_DataChangedEvent(List<CurrentValue> values)
        {
            // Run thru all received data changes
            foreach (CurrentValue cv in values)
            {
                // Publish to broker
                PublishTag(cv.Tagname, cv.Value.ToString(), cv.Time, cv.Quality.ToString());
            }
        }

        /// <summary>
        /// Wraps data nicely in json and publishes to the gateway broker
        /// </summary>
        /// <param name="TagName">Name of the tag</param>
        /// <param name="TagValue">Value of the tag</param>
        /// <param name="TagDateTime">Datetime of the observed value change</param>
        /// <param name="TagQuality">The reported quality of the value</param>
        public void PublishTag(string TagName, string TagValue, DateTime TagDateTime, string TagQuality)
        {
            // Wrap data nicely before releasing them to the broker
            messageWrap mw = new messageWrap();
            mw.content[0] = new sensorData(TagName, TagValue, TagDateTime.ToString("yyyy-MM-dd HH:mm:ss"), TagQuality);
            string json = "[" + JsonConvert.SerializeObject(mw, Formatting.None) + "]";
            Microsoft.Azure.IoT.Gateway.Message messageToPublish = new Microsoft.Azure.IoT.Gateway.Message(json, _messageProperties);

            // Publish to broker
            this._broker.Publish(messageToPublish);

            // If applicable - print to console window the message sent
            if (_config.PrintToConsole) Console.WriteLine("Proficy.Historian.Module - sent: " + json);
        }

        /// <summary>
        /// This function will dispose this Module
        /// </summary>
        public void Destroy()
        {
            Console.WriteLine("Proficy.Historian.Module closing.");

            try
            {
                // Drop subscriptions and remove event handler
                _historian.IData.DropSubscriptions();
                _historian.DataChangedEvent -= Historian_DataChangedEvent;

                // Close connection
                _historian.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Proficy.Historian.Module - error while disposing: " + ex.Message);
            }

            Console.WriteLine("Proficy.Historian.Module closed.");
        }

        /// <summary>
        /// This function is for processing messages from the broker (Not implemented as this module is strictly a data source)
        /// </summary>
        /// <param name="received_message">The message to be processed</param>
        public void Receive(Microsoft.Azure.IoT.Gateway.Message received_message)
        {
            // Function must be implemented, but this module shouldn't process data - it is only a data source.
            // In future scenario, should be able to receive new list of tags to subscribe, and possibly also requests for batch exports.
        }
    }

    /// <summary>
    /// Class for handling efficient message wrapping on broker, reducing payload size
    /// </summary>
    public class messageWrap
    {
        public string name = "Historian";
        public sensorData[] content = new sensorData[1];
    }

    /// <summary>
    /// Call for handling individual tag readings
    /// </summary>
    public class sensorData
    {
        public string t;
        public string v;
        public string dt;
        public string q;

        // Initialize the class while populating the variables
        public sensorData(string TagName, string TagValue, string TagDateTime, string TagQuality)
        {
            this.t = TagName;
            this.v = TagValue;
            this.dt = TagDateTime;
            this.q = TagQuality;
        }
    }

    /// <summary>
    /// Wrapper for handling the configuration of the module
    /// </summary>
    public class ProficyHistorianConfiguration
    {
        public string ServerName;
        public string UserName;
        public string Password;
        public bool PrintToConsole = false;
        public IList<ProficyHistorianTag> TagsToSubscribe;
    }

    /// <summary>
    /// Wrapper for handling the tags to subscribe to
    /// </summary>
    public class ProficyHistorianTag
    {
        public string TagName;
        public int MinimumElapsedMilliSeconds = 1000;
    }
}
