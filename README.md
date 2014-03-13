C# .Net TMS Client
===========
This is an example API client you can use in C# .Net to interact with GovDelivery's TMS API. Take a look at the FunctionalTests.cs file 
for examples of how to use the classes and proxy. The TMSProxy.cs has the API interactions, while TMSResources.cs has the object mappings 
for the API resources.

Compatibility & Requirements
-----------
RestSharp 103.0.0
Json.NET 5.0.5
.Net 4.0

Example Usage
-----------

### Sending an Email Message
```csharp
TMSEmailMessage message = new TMSEmailMessage();
message.subject = "Test Subject";
message.body = "Test Body";
message.from_name = "Test From Name";
message.addRecipient("user@example.com");

TMSApi api = new TMSApi(Properties.Resources.TMSUserName, Properties.Resources.TMSPassword);
TMSEmailMessage messageResult = api.SendEmailMessage(message);
```

### Sending an SMS Message
```csharp
TMSSMSMessage messageIn = new TMSSMSMessage();
messageIn.body = "Test Body";   
messageIn.addRecipient("6515551212");

TMSApi api = new TMSApi(Properties.Resources.TMSUserName, Properties.Resources.TMSPassword);
TMSSMSMessage messageOut = api.SendSMSMessage(messageIn);
```

### Retrieve the list of recipients on a sent email message
```csharp
List<TMSEmailMessage> messages = api.GetEmailMessages();
TMSEmailMessage message = messages.First<TMSEmailMessage>();

List<EmailRecipient> recipients = api.Get<List<EmailRecipient>>(message._links["recipients"]);
recipients.ForEach(delegate(EmailRecipient r)
            {
                YourMethod(r.email);
                YourMethod(r.status);

				// Some other meaningful stuff
            });

```

### Installing dependencies

Install NuGet console/powershell, then install the below packages

```
Package Manager Console Host Version 2.0.30717.9005

Type 'get-help NuGet' to see all available NuGet commands.

PM> Install-Package RestSharp
Successfully installed 'RestSharp 104.1'.
Successfully added 'RestSharp 104.1' to TMSDotNetClient.

PM> Install-Package Newtonsoft.Json
Successfully installed 'Newtonsoft.Json 5.0.5'.
Successfully added 'Newtonsoft.Json 5.0.5' to TMSDotNetClient.
```

License
-------
Copyright (c) 2014, GovDelivery, Inc.

All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
* Neither the name of GovDelivery nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
