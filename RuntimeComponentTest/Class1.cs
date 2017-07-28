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

namespace RuntimeComponentTest
{
    // event testing
    public sealed class Eventful
    {
        public event EventHandler<TestEventArgs> TestTest;
        public void OnTest(string msg, long number)
        {
            EventHandler<TestEventArgs> temp = TestTest;
            if (temp != null)
            {
                temp(this, new TestEventArgs()
                {
                    Value1 = msg,
                    Value2 = number
                });
            }
        }
    }
    public sealed class TestEventArgs
    {
        public string Value1 { get; set; }
        public long Value2 { get; set; }
    }
    // end of testing

    public sealed class PushPayload
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "type";

        public string Text { get; set; }
    }

    public sealed class Class1
    {

        static AirshipConfig uaConfig = new AirshipConfig();
        public static event EventHandler<PushPayload> PushStateChanged;
        public static event EventHandler<RegistrationEventArgs> RegistrationStateChanged;
        public static event EventHandler<string> PushParseError;
        public static event EventHandler<XmlDocument> TosseEvent;
        public static event EventHandler<string> TossetAttempt;

        public static string SetUAConfig(string key, string secret, bool production)
        {
            try
            {

                uaConfig.DevelopmentAppKey = key;
                uaConfig.DevelopmentAppSecret = secret;
                uaConfig.InProduction = production;
                return "Success! (" + key + "/" + secret + ")";
            }
            catch(Exception ex)
            {
                return "Error! (" + key + "/" + secret + "), "+ex.ToString();
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
                    PushManager.Shared.EnabledNotificationTypes = NotificationType.Toast | NotificationType.Tile;
                    PushManager.Shared.InterceptNotifications = true;
                    PushManager.Shared.RegistrationEvent += Shared_RegistrationEvent;
                    PushManager.Shared.PushReceivedEvent += Shared_PushReceivedEvent;
                    return "Success! (" + enabled + ")";
                }
                catch(Exception ex)
                {
                    return "Error! (" + enabled + "), " + ex.ToString();
                }
            }
            else
            {
                try
                {
                    PushManager.Shared.EnabledNotificationTypes = NotificationType.None;
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
            try
            {
                Logger.Debug($"UA is valid: {e.IsValid}. UA APID: {e.Apid}");
                Debug.WriteLine($"UA is valid: {e.IsValid}. UA APID: {e.Apid}");

                var registration = $"UA is valid: {e.IsValid}. UA APID: {e.Apid}";

                EventHandler<RegistrationEventArgs> handler = RegistrationStateChanged;
                if(handler != null)
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
            //EventHandler<string> tossetAttempt = TossetAttempt;
            //tossetAttempt(sender, "Push oncoming!");
            try
            {
                //EventHandler<XmlDocument> tosset = TosseEvent;
                //tosset(sender, e.Notification.ToastData.Payload as XmlDocument);
                Logger.Debug($"UA received push: {e.Notification}");
                Logger.Debug($"Push type: {e.Notification.Type}");
                Logger.Debug("Push text: " + e.Notification.ToastData.Text);
                Debug.WriteLine($"UA received push: {e.Notification}");
                Debug.WriteLine($"Push type: {e.Notification.Type}");
                Debug.WriteLine("Push text: " + e.Notification.ToastData.Text);

                 var xml = e.Notification.ToastData.Payload as XmlDocument;
                 var toast = xml.GetElementsByTagName("toast").First();
                 var text = e.Notification.ToastData.Text.First();
                 var launchAttribute = toast?.Attributes?.GetNamedItem("launch");
                 var jsonPayload = WebUtility.HtmlDecode(launchAttribute?.NodeValue?.ToString());
                 var pushPayload = JsonConvert.DeserializeObject<PushPayload>(jsonPayload);

                 if (pushPayload == null) return;

                pushPayload.Text = text;
                EventHandler<PushPayload> handler = PushStateChanged;
                if(handler != null)
                {
                    handler(sender, pushPayload);
                }
                //handler?.Invoke(sender, pushPayload);

                //PushStateChanged?.Invoke(sender,pushPayload);

                
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                Debug.WriteLine(ex.ToString());
                EventHandler<string> handler = PushParseError;
                if(handler != null)
                {
                    handler(sender, ex.ToString());
                }
            }
        }

        public static string SetApid(string apid)
        {
            try
            {
                PushManager.Shared.Alias = apid;
                PushManager.Shared.UpdateRegistration();
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
                return PushManager.Shared.Alias.ToString();
            }
            catch (Exception ex)
            {
                return "Error! " + ex.ToString();
            }
        }
    }
}
