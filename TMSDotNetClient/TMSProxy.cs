using TMSResources;
using RestSharp;
using System.Collections.Generic;
using RestSharp.Serializers;
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
namespace TMSProxy
{
    public class TMSApi
    {
        const string BaseUrl = "replace with TMS API URL";

        readonly string _accountKey;

        public TMSApi(string secretKey)
        {
            _accountKey = secretKey;
        }

        public T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient();
            client.BaseUrl = BaseUrl;

            request.AddHeader("X-AUTH-TOKEN", _accountKey);

            request.RequestFormat = DataFormat.Json;

            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            return response.Data;
        }

        /**
         * Discover the list of services your account is provisioned for. 
         * 
         * Use this as an entry point to use the services so that you
         * don't have to code in the URIs for each service.
         * 
         */
        public TMSResource GetServices()
        {
            TMSResource resource = new TMSResource();
            resource._links["self"] = "/";

            return Get<TMSResource>(resource._links["self"]);
        }

        /**
         * Generic Get method to retrieve more details on an object.
         * 
         * This is used to fill an object up if you received a lightweight
         * version of it from a list context.
         * 
         * e.g. You get the list of email messages, and select one of them. 
         * Not all of the parameters are filled in from the list view, so you execute
         * a Get(message) and you get the remaining details.
         */
        public T Get<T>(string resource) where T : new()
        {
            var request = new RestRequest();
            request.Resource = resource;

            return Execute<T>(request);
        }

        /**
         * Give me the list of messages I have sent from my account.
         * 
         * Use the GetServices() entry point to get the email messages
         * relation.
         *  
         */
        public List<TMSEmailMessage> GetEmailMessages()
        {
            TMSResource services = GetServices();

            var request = new RestRequest();
            request.Resource = services._links["email_messages"];
            request.Method = Method.GET;

            return Execute<List<TMSEmailMessage>>(request);
        }

        /**
         * Give me the list of SMS message I have sent from my account
         */
        public List<TMSSMSMessage> GetSMSMessages()
        {
            TMSResource services = GetServices();

            var request = new RestRequest();
            request.Resource = services._links["sms_messages"];
            request.Method = Method.GET;

            return Execute<List<TMSSMSMessage>>(request);
        }

        /**
         * Send an email message
         */
        public TMSEmailMessage SendEmailMessage(TMSEmailMessage messageIn)
        {
            TMSResource services = GetServices();

            var request = new RestRequest();
            request.Resource = services._links["email_messages"];

            request.Method = Method.POST;

            request.JsonSerializer = new TMSJsonSerializer();
            // anytime you want to send a POST you need to make sure 
            // you tell the request object you want to send JSON
            // so that it can serialize correctly
            request.RequestFormat = DataFormat.Json;

            request.AddBody(messageIn);

            return Execute<TMSEmailMessage>(request);
        }

        /**
         * Send an SMS message
         */
        public TMSSMSMessage SendSMSMessage(TMSSMSMessage messageIn)
        {
            TMSResource services = GetServices();

            var request = new RestRequest();
            request.Resource = services._links["sms_messages"];

            request.Method = Method.POST;

            request.JsonSerializer = new TMSJsonSerializer();
            // anytime you want to send a POST you need to make sure 
            // you tell the request object you want to send JSON
            // so that it can serialize correctly
            request.RequestFormat = DataFormat.Json;

            request.AddBody(messageIn);

            return Execute<TMSSMSMessage>(request);
        }
    }

    // Custom serializer to ignore _links attribute when serializing
    public class TMSJsonSerializer : ISerializer
    {
        public TMSJsonSerializer()
        {
            ContentType = "application/json";
        }

        public string Serialize(object obj)
        {
            String json = JsonConvert.SerializeObject(obj,
                Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return json;
        }

        public string DateFormat { get; set; }
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string ContentType { get; set; }
    }
}
