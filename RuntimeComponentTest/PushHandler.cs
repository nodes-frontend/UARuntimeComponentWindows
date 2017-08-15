using System;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
//using PushObject;
using UrbanAirship;
using UrbanAirship.Push;
using Windows.Data.Xml.Dom;
using System.Net;
using Windows.Foundation.Collections;
using System.Diagnostics;

/*namespace PushObject
{
    public sealed class PushPayload
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "type";

        public string Text { get; set; }
    }
}*/

namespace PushComponent
{
    public sealed class PushPayload
    {
        public string Json { get; set; }
        public string Text { get; set; }
    }

    public sealed class PushHandler
    {

        static AirshipConfig uaConfig = new AirshipConfig();
        public static event EventHandler<PushPayload> PushStateChanged;
        public static event EventHandler<RegistrationEventArgs> RegistrationStateChanged;
        public static event EventHandler<string> PushParseError;
        public static event EventHandler<string> DebugEvent;


        public static string SetUAConfig(string key, string secret, bool production)
        {
            try
            {
                if (production)
                {
                    uaConfig.ProductionAppKey = key;
                    uaConfig.ProductionAppSecret = secret;
                }
                else
                {
                    uaConfig.DevelopmentAppKey = key;
                    uaConfig.DevelopmentAppSecret = secret;
                }
                uaConfig.InProduction = production;
                return "Success! (" + key + "/" + secret + ")";
            }
            catch (Exception ex)
            {
                return "Error! (" + key + "/" + secret + "), " + ex.ToString();
            }
        }

        public static string InitUA()
        {
            try
            {
                UAirship.TakeOff(uaConfig);
                uaConfig.DebugLog();
                return "Success!";
            }
            catch (Exception ex)
            {
                return "Error!, " + ex.ToString();
            }
        }

        public static string SetUserNotificationsEnabled(bool enabled)
        {
            if (enabled)
            {
                try
                {
                    UrbanAirship.Push.PushManager.Shared.EnabledNotificationTypes = NotificationType.Toast | NotificationType.Tile;
                    UrbanAirship.Push.PushManager.Shared.InterceptNotifications = true;
                    UrbanAirship.Push.PushManager.Shared.RegistrationEvent += Shared_RegistrationEvent;
                    UrbanAirship.Push.PushManager.Shared.PushReceivedEvent += Shared_PushReceivedEvent;
                    return "Success! (" + enabled + ")";
                }
                catch (Exception ex)
                {
                    return "Error! (" + enabled + "), " + ex.ToString();
                }
            }
            else
            {
                try
                {
                    UrbanAirship.Push.PushManager.Shared.EnabledNotificationTypes = NotificationType.None;
                    return "Success! (" + enabled + ")";
                }
                catch (Exception ex)
                {
                    return "Error! (" + enabled + "), " + ex.ToString();
                }
            }
        }

        public static void Shared_RegistrationEvent(object sender, RegistrationEventArgs e)
        {
            if (DebugEvent != null) DebugEvent(sender, "RegistrationEvent");

            try
            {
                Logger.Debug($"UA is valid: {e.IsValid}. UA APID: {e.Apid}");
                Debug.WriteLine($"UA is valid: {e.IsValid}. UA APID: {e.Apid}");

                var registration = $"UA is valid: {e.IsValid}. UA APID: {e.Apid}";

                EventHandler<RegistrationEventArgs> handler = RegistrationStateChanged;
                if (handler != null)
                {
                    handler(sender, e);
                }

                //RegistrationStateChanged?.Invoke(sender, registration);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                Debug.WriteLine(ex.ToString());
            }
        }

        private static void Shared_PushReceivedEvent(object sender, PushReceivedEventArgs e)
        {
            try
            {
                if (DebugEvent != null) DebugEvent(sender, "e?.Notification?.Type: " + e?.Notification?.Type ?? "null");
                if (DebugEvent != null) DebugEvent(sender, "e?.Notification?.ToastData?.Text: " + e?.Notification?.ToastData?.Text ?? "null");
                if (DebugEvent != null) DebugEvent(sender, "e?.Notification?.ToastData?.Payload?.ToString(): " + e?.Notification?.ToastData?.Payload?.ToString() ?? "null");
                
                if (DebugEvent != null) DebugEvent(sender, "Trying parsing...");
                var xml = e.Notification.ToastData.Payload as XmlDocument;
                var toast = xml.GetElementsByTagName("toast").First();
                if (DebugEvent != null) DebugEvent(sender, "Got toast XML element: " + xml.GetXml());
                var text = e?.Notification?.ToastData?.Text?.First() ?? "null";
                if (DebugEvent != null) DebugEvent(sender, "Got toast XML TEXT: " + text);
                var launchAttribute = toast?.Attributes?.GetNamedItem("launch");
                if (DebugEvent != null) DebugEvent(sender, "Got toast launch attribute: " + launchAttribute?.NodeValue?.ToString() ?? "damn");
                
                var pushPayload = new PushPayload()
                {
                    Text = text,
                    Json = launchAttribute?.NodeValue?.ToString()
                };

                EventHandler<PushPayload> handler = PushStateChanged;
                if (handler != null)
                {
                    handler(sender, pushPayload);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                Debug.WriteLine(ex.ToString());
                EventHandler<string> handler = PushParseError;
                if (handler != null)
                {
                    handler(sender, ex.ToString());
                }
            }
        }

        public static string SetApid(string apid)
        {
            try
            {
                UrbanAirship.Push.PushManager.Shared.Alias = apid;
                UrbanAirship.Push.PushManager.Shared.UpdateRegistration();
                return "Success! (" + apid + ")";
            }
            catch (Exception ex)
            {
                return "Error! (" + apid + "), " + ex.ToString();
            }
        }

        public static string GetAlias()
        {
            try
            {
                return UrbanAirship.Push.PushManager.Shared.Alias.ToString();
            }
            catch (Exception ex)
            {
                return "Error! " + ex.ToString();
            }
        }
    }
}
