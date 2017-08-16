using System;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UrbanAirship;
using UrbanAirship.Push;
using Windows.Data.Xml.Dom;
using System.Net;
using Windows.Foundation.Collections;
using System.Diagnostics;

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
        public static event EventHandler<PushPayload> PushActivated;
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

        public static string SetUserNotificationsEnabled(bool enabled, bool interceptNotifications)
        {
            if (enabled)
            {
                try
                {
                    UrbanAirship.Push.PushManager.Shared.EnabledNotificationTypes = NotificationType.Toast | NotificationType.Tile;
                    UrbanAirship.Push.PushManager.Shared.InterceptNotifications = interceptNotifications;
                    UrbanAirship.Push.PushManager.Shared.RegistrationEvent += Shared_RegistrationEvent;
                    UrbanAirship.Push.PushManager.Shared.PushReceivedEvent += Shared_PushReceivedEvent;
                    UrbanAirship.Push.PushManager.Shared.PushActivatedEvent += Shared_PushActivatedEvent;
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

        private static void Shared_PushActivatedEvent(object sender, PushActivatedEventArgs e)
        {
            if (DebugEvent != null) DebugEvent(sender, "PushActivatedEvent");

            try
            {
                var pushPayload = ParsePushPayload(sender, e?.Notification);

                EventHandler<PushPayload> handler = PushActivated;
                if (handler != null)
                {
                    handler(sender, pushPayload);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                Debug.WriteLine(ex.ToString());
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

        private static PushPayload ParsePushPayload(object sender, PushNotification notification)
        {
            try
            {
                if (DebugEvent != null) DebugEvent(sender, "Notification?.Type: " + notification?.Type ?? "null");
                if (DebugEvent != null) DebugEvent(sender, "Notification?.ToastData?.Text: " + notification?.ToastData?.Text ?? "null");
                if (DebugEvent != null) DebugEvent(sender, "Notification?.ToastData?.Payload?.ToString(): " + notification?.ToastData?.Payload?.ToString() ?? "null");

                if (DebugEvent != null) DebugEvent(sender, "Trying parsing...");
                var xml = notification.ToastData.Payload as XmlDocument;
                var toast = xml.GetElementsByTagName("toast").First();
                if (DebugEvent != null) DebugEvent(sender, "Got toast XML element: " + xml.GetXml());
                var text = notification?.ToastData?.Text?.First() ?? "null";
                if (DebugEvent != null) DebugEvent(sender, "Got toast XML TEXT: " + text);
                var launchAttribute = toast?.Attributes?.GetNamedItem("launch");
                if (DebugEvent != null) DebugEvent(sender, "Got toast launch attribute: " + launchAttribute?.NodeValue?.ToString() ?? "damn");

                var pushPayload = new PushPayload()
                {
                    Text = text,
                    Json = launchAttribute?.NodeValue?.ToString()
                };

                return pushPayload;
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

                return null;
            }
        }

        private static void Shared_PushReceivedEvent(object sender, PushReceivedEventArgs e)
        {
            try
            {
                var pushPayload = ParsePushPayload(sender, e?.Notification);

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

        public static string SetAlias(string alias)
        {
            try
            {
                UrbanAirship.Push.PushManager.Shared.Alias = alias;
                UrbanAirship.Push.PushManager.Shared.UpdateRegistration();
                return "Success! (" + alias + ")";
            }
            catch (Exception ex)
            {
                return "Error! (" + alias + "), " + ex.ToString();
            }
        }

        public static void SetTags(IList<string> tags)
        {
            try
            {
                PushManager.Shared.Tags = tags;
                PushManager.Shared.UpdateRegistration();
            }
            catch (Exception e)
            {
                if (DebugEvent != null)
                {
                    DebugEvent(null, "SetTags() threw an error: " + e.ToString());
                }
            }
        }

        public static IList<string> GetTags()
        {
            try
            {
                return PushManager.Shared.Tags;
            }
            catch(Exception e)
            {
                if(DebugEvent != null)
                {
                    DebugEvent(null, "GetTags() threw an error: " + e.ToString());
                }

                return new List<string>();
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
                if (DebugEvent != null)
                {
                    DebugEvent(null, "GetAlias() threw an error: " + ex.ToString());
                }

                return "";
            }
        }
    }
}
