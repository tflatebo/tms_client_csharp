using System.Collections.Generic;
using System;
using Newtonsoft.Json;

/**
 * GovDelivery TMS example client for DotNet
 * 
 * This test file is intended to help you build your own
 * API client. You are free to base your client from this code.
 * 
 * Uses RestSharp library for HTTP interactions with the API and
 * object serialization.
 * 
 * copyright 2014 GovDelivery
 * 
 */
namespace TMSResources
{
    public abstract class TMSMessage : TMSResource
    {
        public string body { get; set; }
        public string subject { get; set; }

        public Dictionary<string, string> recipient_counts { get; set; }

        public abstract void addRecipient(string email);
    }

    public class TMSEmailMessage : TMSMessage
    {
        public string from_name { get; set; }
        public bool open_tracking_enabled { get; set; }
        public bool click_tracking_enabled { get; set; }
        public List<EmailRecipient> recipients { get; set; }

        public TMSEmailMessage()
        {
            open_tracking_enabled = true;
            click_tracking_enabled = true;
        }

        public override void addRecipient(string email)
        {
            if (recipients == null)
            {
                recipients = new List<EmailRecipient>();
            }

            recipients.Add(new EmailRecipient(email));
        }
    }

    public class TMSSMSMessage : TMSMessage
    {
        public List<SMSRecipient> recipients { get; set; }

        public override void addRecipient(string email)
        {
            if (recipients == null)
            {
                recipients = new List<SMSRecipient>();
            }

            recipients.Add(new SMSRecipient(email));
        }

    }

    public class TMSRecipient : TMSResource
    {
        public string status { get; set; }
        public DateTime completed_at { get; set; }
    }

    public class EmailRecipient : TMSRecipient
    {
        public string email { get; set; }

        public EmailRecipient() { }

        public EmailRecipient(string emailIn)
        {
            email = emailIn;
        }
    }

    public class SMSRecipient : TMSRecipient
    {
        public string phone { get; set; }
        public string formatted_phone { get; set; }

        public SMSRecipient() { }

        public SMSRecipient(string phoneIn)
        {
            phone = phoneIn;
        }
    }

    public class Click : TMSResource
    {
        public string url { get; set; }
        public DateTime event_at { get; set; }
    }

    public class TMSResource
    {
        [JsonIgnoreAttribute]
        public Dictionary<String, String> _links { get; set; }

        public DateTime created_at { get; set; }

        public TMSResource()
        {
            _links = new Dictionary<String, String>();
        }
    }
}
