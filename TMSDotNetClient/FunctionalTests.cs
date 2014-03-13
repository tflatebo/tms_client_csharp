using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System.Net;
using RestSharp.Serializers;
using System.IO;
using Newtonsoft.Json;
using TMSProxy;
using TMSDotNetClient.Properties;
using TMSResources;

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

namespace TMSDotNetClientTestProject
{
    [TestClass]
    public class SystemTests
    {
        TMSApi api;
        String TestBaseUrl;

        [TestInitialize]
        public void Initialize()
        {
            api = new TMSApi(TMSDotNetClient.Properties.Resources.TMSAPIToken);
            TestBaseUrl = TMSDotNetClient.Properties.Resources.TMSBaseURI;
        }

        /**
         * Hello? Does this thing work? Can we issue a simple GET to the root of the API?
         */
        [TestMethod]
        public void TestSimpleGetToAPI()
        {
            var client = new RestClient();
            client.BaseUrl = TestBaseUrl;

            var request = new RestRequest();
            request.Resource = "/";
            request.AddHeader("X-AUTH-TOKEN", TMSDotNetClient.Properties.Resources.TMSAPIToken);

            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        /**
         * GET a list of services available.
         */
        [TestMethod]
        public void TestGetServices()
        {
            TMSResource services = api.GetServices();

            Assert.IsNotNull(services._links["self"]);
            Assert.AreEqual("/", services._links["self"]);
            // If you are provisioned for email, this will be there
            Assert.IsNotNull(services._links["email_messages"]);
        }
    }

    [TestClass]
    public class EmailTests
    {
        TMSApi api;
        string TestSubject;
        string TestBody;
        string TestFromName;
        string TestEmail;

        [TestInitialize]
        public void Initialize()
        {
            api = new TMSApi(TMSDotNetClient.Properties.Resources.TMSAPIToken);

            TestSubject = "TEST: This is a test subject";
            TestBody = "This is a test body, populate it with interesting information";
            TestFromName = "From the test";
            //TestEmail = "change me to an email address and uncomment";
        }

        /**
         * GET a list of email messages you have sent.
         */
        [TestMethod]
        public void TestGetEmailMessages()
        {
            List<TMSEmailMessage> messages = api.GetEmailMessages();

            Assert.IsNotNull(messages);
            Assert.IsTrue(messages.Count > 0);

            TMSEmailMessage message = messages.First<TMSEmailMessage>();

            Assert.IsNotNull(message);
            Assert.IsNotNull(message._links);
            Assert.IsNotNull(message._links["self"]);
        }

        /**
         * GET details on an email message, starting at the list.
         */
        [TestMethod]
        public void TestGetEmailDetails()
        {
            List<TMSEmailMessage> messages = api.GetEmailMessages();

            Assert.IsNotNull(messages);
            Assert.IsTrue(messages.Count > 0);

            TMSEmailMessage message = messages.First<TMSEmailMessage>();

            Assert.IsNotNull(message);
            Assert.IsNotNull(message._links);
            Assert.IsNotNull(message._links["self"]);

            // body isn't available in the list view
            Assert.IsNull(message.body);

            message = api.Get<TMSEmailMessage>(message._links["self"]);

            // body should now be there
            Assert.IsNotNull(message.body);

            // recipients should not appear yet
            Assert.IsNull(message.recipients);

            // recipient counts are available though
            Assert.IsNotNull(message.recipient_counts);
            Assert.IsNotNull(message.recipient_counts["total"]);
            Assert.IsTrue(Convert.ToDecimal(message.recipient_counts["total"]) > 0);

            // open and link tracking fields are here
            Assert.IsNotNull(message.open_tracking_enabled);
            Assert.IsTrue(message.open_tracking_enabled == true || message.open_tracking_enabled == false);

            Assert.IsNotNull(message.click_tracking_enabled);
            Assert.IsTrue(message.click_tracking_enabled == true || message.click_tracking_enabled == false);

            // created_at should be a positive date
            Assert.IsNotNull(message.created_at);
            Assert.IsTrue(message.created_at > new DateTime(0));
        }

        /**
         * GET details on an email message, starting at the list, then
         * following the recipients link relation
         */
        [TestMethod]
        public void TestGetEmailRecipients()
        {
            List<TMSEmailMessage> messages = api.GetEmailMessages();

            Assert.IsNotNull(messages);
            Assert.IsTrue(messages.Count > 0);

            TMSEmailMessage message = messages.First<TMSEmailMessage>();

            Assert.IsNotNull(message);
            Assert.IsNotNull(message._links);
            Assert.IsNotNull(message._links["self"]);

            // recipients aren't available in the list view
            Assert.IsNull(message.recipients);

            List<EmailRecipient> recipients = api.Get<List<EmailRecipient>>(message._links["recipients"]);

            recipients.ForEach(delegate(EmailRecipient r)
            {
                Assert.IsNotNull(r.email);
                Assert.IsNotNull(r.status);
            });
            // recipients should now be there
            Assert.IsNotNull(recipients);
            Assert.IsTrue(recipients.Count > 0);

            // And you should be able to access the data in the recipient
            Assert.IsNotNull(recipients.First().email);
            Assert.IsTrue(recipients.First().email.Length > 1);

            Assert.IsNotNull(recipients.First().created_at);
            Assert.IsTrue(recipients.First().created_at > new DateTime(0));

            Assert.IsNotNull(recipients.First().completed_at);
            if (recipients.First().status == "sent")
            {
                Assert.IsTrue(recipients.First().completed_at > new DateTime(0));
            }
            else
            {
                Assert.AreEqual(new DateTime(0), recipients.First().completed_at);
            }

            Assert.IsNotNull(recipients.First().status);
            Assert.IsTrue(recipients.First().status.Length > 1);
        }

        /**
         * Can we get the list of clicks for a message
         */
        [TestMethod]
        public void TestGetEmailClicks()
        {
            List<TMSEmailMessage> messages = api.GetEmailMessages();

            Assert.IsNotNull(messages);
            Assert.IsTrue(messages.Count > 0);

            TMSEmailMessage message = messages.First<TMSEmailMessage>();

            // We know that this specific message has some clicks
            List<EmailRecipient> recipients_clicked = api.Get<List<EmailRecipient>>(message._links["clicked"]);

            Assert.IsNotNull(recipients_clicked);
            if (recipients_clicked.Count > 0)
            {
                List<Click> clicks = api.Get<List<Click>>(recipients_clicked.First()._links["clicks"]);

                Assert.IsNotNull(clicks);
                Assert.IsTrue(clicks.Count > 0);

                Click click = clicks.First<Click>();

                Assert.IsNotNull(click.url);
                Assert.IsTrue(click.url.IndexOf("http://") == 0);

                Assert.IsNotNull(click.event_at);
                Assert.IsTrue(click.event_at > new DateTime(0));
            }

        }

        /**
         * Send an email via TMS
         * 
         * Uncomment the method to actually send a message.
         * 
         */
        [TestMethod]
        public void TestSendEmail()
        {
            TMSEmailMessage messageIn = new TMSEmailMessage();
            messageIn.subject = TestSubject;
            messageIn.body = TestBody;
            messageIn.from_name = TestFromName;

            messageIn.addRecipient(TestEmail);

            // Uncomment the below to actually send an email
            TMSEmailMessage messageOut = api.SendEmailMessage(messageIn);

            // We should have a valid self relation
            Assert.IsNotNull(messageOut._links["self"]);

            // Are all of the elements the same as what we passed in?
            Assert.AreEqual(messageIn.subject, messageOut.subject);
            Assert.AreEqual(messageIn.body, messageOut.body);
            Assert.AreEqual(messageIn.from_name, messageOut.from_name);

            // Recipient counts and the actual recipients 
            List<EmailRecipient> recipients = api.Get<List<EmailRecipient>>(messageOut._links["recipients"]);
            Assert.AreEqual(messageIn.recipients.First().email, recipients.First().email);
            Assert.AreEqual(1, recipients.Count);
        }
         


        /**
         * What does the message object look like after serialization?
         * 
         * It should not have the links attribute.
         * 
         * Unfortunately we had to bring in a non-stock serializer for 
         * RestSharp just for serializing. It turns out that this is pretty 
         * straightforward
         * 
         */
        [TestMethod]
        public void TestSerializeEmailMessage()
        {
            TMSEmailMessage messageIn = new TMSEmailMessage();
            messageIn.subject = TestSubject;
            messageIn.body = TestBody;
            messageIn.from_name = TestFromName;

            messageIn.addRecipient(TestEmail);

            RestRequest request = new RestRequest();
            request.JsonSerializer = new TMSJsonSerializer();
            String result = request.JsonSerializer.Serialize(messageIn);

            // The links attribute/dictionary shoudl not be in the serialized output
            Assert.IsTrue(result.IndexOf("links") == -1);

            Assert.IsTrue(result.IndexOf("subject") > 0);
            Assert.IsTrue(result.IndexOf(TestSubject) > 0);

            Assert.IsTrue(result.IndexOf("body") > 0);
            Assert.IsTrue(result.IndexOf(TestBody) > 0);

            Assert.IsTrue(result.IndexOf("from_name") > 0);
            Assert.IsTrue(result.IndexOf(TestFromName) > 0);

            Assert.IsTrue(result.IndexOf("email") > 0);
            Assert.IsTrue(result.IndexOf(TestEmail) > 0);
        }
    } // end email tests

    [TestClass]
    public class SMSTests
    {

        TMSApi api;
        string TestBody;
        string TestPhone;

        [TestInitialize]
        public void Initialize()
        {
            api = new TMSApi(TMSDotNetClient.Properties.Resources.TMSAPIToken);

            TestBody = "This is a test body, populate it with interesting information";
            //TestPhone = "change me to a phone number and uncomment";
        }

        /**
         * GET a list of email messages you have sent.
         */
        [TestMethod]
        public void TestGetSMSMessages()
        {
            List<TMSSMSMessage> messages = api.GetSMSMessages();

            Assert.IsNotNull(messages);
            Assert.IsTrue(messages.Count > 0);

            TMSSMSMessage message = messages.First<TMSSMSMessage>();

            Assert.IsNotNull(message);
            Assert.IsNotNull(message._links);
            Assert.IsNotNull(message._links["self"]);
        }

        /**
         * GET details on an SMS message, starting at the list.
         */
        [TestMethod]
        public void TestGetSMSDetails()
        {
            List<TMSSMSMessage> messages = api.GetSMSMessages();

            Assert.IsNotNull(messages);
            Assert.IsTrue(messages.Count > 0);

            // Look at the last message in the list in case
            // you are running all of the tests and you just
            // inserted one, the recipient list may not be built
            TMSSMSMessage message = messages.Last<TMSSMSMessage>();

            Assert.IsNotNull(message);
            Assert.IsNotNull(message._links);
            Assert.IsNotNull(message._links["self"]);

            // body is available in the list view
            Assert.IsNotNull(message.body);

            message = api.Get<TMSSMSMessage>(message._links["self"]);

            // body is here too!
            Assert.IsNotNull(message.body);

            // recipients should not appear yet
            Assert.IsNull(message.recipients);

            // recipient counts are available though
            Assert.IsNotNull(message.recipient_counts);
            Assert.IsNotNull(message.recipient_counts["total"]);
            Assert.IsTrue(Convert.ToDecimal(message.recipient_counts["total"]) > 0);

            // created_at should be a positive date
            Assert.IsNotNull(message.created_at);
            Assert.IsTrue(message.created_at > new DateTime(0));
        }

        /**
         * GET details on an email message, starting at the list, then
         * following the recipients link relation
         */
        [TestMethod]
        public void TestGetSMSRecipients()
        {
            List<TMSSMSMessage> messages = api.GetSMSMessages();

            Assert.IsNotNull(messages);
            Assert.IsTrue(messages.Count > 0);

            // Look at the last message in the list in case
            // you are running all of the tests and you just
            // inserted one, the recipient list may not be built
            TMSSMSMessage message = messages.Last<TMSSMSMessage>();

            Assert.IsNotNull(message);
            Assert.IsNotNull(message._links);
            Assert.IsNotNull(message._links["self"]);

            // recipients aren't available in the list view
            Assert.IsNull(message.recipients);

            List<SMSRecipient> recipients = api.Get<List<SMSRecipient>>(message._links["recipients"]);

            // recipients should now be there
            Assert.IsNotNull(recipients);
            Assert.IsTrue(recipients.Count > 0);

            // And you should be able to access the data in the recipient
            Assert.IsNotNull(recipients.First().phone);
            Assert.IsTrue(recipients.First().phone.Length > 1);

            Assert.IsNotNull(recipients.First().created_at);
            Assert.IsTrue(recipients.First().created_at > new DateTime(0));

            Assert.IsNotNull(recipients.First().completed_at);
            Assert.IsTrue(recipients.First().completed_at > new DateTime(0));

            Assert.IsNotNull(recipients.First().status);
            Assert.IsTrue(recipients.First().status.Length > 1);
        }

        /**
         * Send an email via TMS
         * 
         * Uncomment the method to actually send a message.
         * 
         */
        [TestMethod]
        public void TestSendSMS()
        {
            TMSSMSMessage messageIn = new TMSSMSMessage();
            messageIn.body = TestBody;
            
            messageIn.addRecipient(TestPhone);

            // Uncomment the below to actually send an email
            TMSSMSMessage messageOut = api.SendSMSMessage(messageIn);

            // We should have a valid self relation
            Assert.IsNotNull(messageOut._links["self"]);

            // Are all of the elements the same as what we passed in?
            Assert.AreEqual(messageIn.body, messageOut.body);

            // Recipient counts and the actual recipients 
            List<SMSRecipient> recipients = api.Get<List<SMSRecipient>>(messageOut._links["recipients"]);
            Assert.AreEqual(messageIn.recipients.First().phone, recipients.First().phone);
            Assert.AreEqual(1, recipients.Count);
        } 
    } // end SMS tests
}
