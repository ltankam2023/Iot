﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Azure.Devices.Client.Test
{
    [TestClass]
    [TestCategory("Unit")]
    public class IotHubModuleClientTests
    {
        private const string DeviceId = "module-twin-test";
        private const string ModuleId = "mongo-server";
        private const string ConnectionStringWithModuleId = "GatewayHostName=edge.iot.microsoft.com;HostName=acme.azure-devices.net;DeviceId=module-twin-test;ModuleId=mongo-server;SharedAccessKey=dGVzdFN0cmluZzQ=";
        private const string ConnectionStringWithoutModuleId = "GatewayHostName=edge.iot.microsoft.com;HostName=acme.azure-devices.net;DeviceId=module-twin-test;SharedAccessKey=dGVzdFN0cmluZzQ=";
        private const string FakeConnectionString = "HostName=acme.azure-devices.net;SharedAccessKeyName=AllAccessKey;DeviceId=dumpy;ModuleId=dummyModuleId;SharedAccessKey=dGVzdFN0cmluZzE=";

        public const string NoModuleTwinJson = "{ \"maxConnections\": 10 }";

        public readonly string ValidDeviceTwinJson = string.Format(
            @"
{{
    ""{1}"": {{
        ""properties"": {{
            ""desired"": {{
                ""maxConnections"": 10,
                ""$metadata"": {{
                    ""$lastUpdated"": ""2017-05-30T22:37:31.1441889Z"",
                    ""$lastUpdatedVersion"": 2
                }}
            }}
        }}
    }},
    ""nginx-server"": {{
        ""properties"": {{
            ""desired"": {{
                ""forwardUrl"": ""http://example.com""
            }}
        }}
    }}
}}
",
            DeviceId,
            ModuleId);

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ModuleClient_CreateFromConnectionString_NullConnectionStringThrows()
        {
            using var mc = new IotHubModuleClient(null);
        }

        [TestMethod]
        public void ModuleClient_CreateFromConnectionString_WithModuleId()
        {
            using var moduleClient = new IotHubModuleClient(ConnectionStringWithModuleId);
            Assert.IsNotNull(moduleClient);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModuleClient_CreateFromConnectionString_WithNoModuleIdThrows()
        {
            using var mc = new IotHubModuleClient(ConnectionStringWithoutModuleId);
        }

        [TestMethod]
        public void ModuleClient_CreateFromConnectionString_NoTransportSettings()
        {
            using var moduleClient = new IotHubModuleClient(FakeConnectionString);
            Assert.IsNotNull(moduleClient);
        }

        [TestMethod]
        public void ModuleClient_CreateFromConnectionStringWithClientOptions_DoesNotThrow()
        {
            // setup
            var clientOptions = new IotHubClientOptions(new IotHubClientMqttSettings())
            {
                ModelId = "tempModuleId"
            };

            // act
            using var moduleClient = new IotHubModuleClient(FakeConnectionString, clientOptions);
        }

        [TestMethod]
        public async Task ModuleClient_SetReceiveCallbackAsync_SetCallback_Mqtt()
        {
            var options = new IotHubClientOptions(new IotHubClientMqttSettings());
            using var moduleClient = new IotHubModuleClient(FakeConnectionString, options);
            var innerHandler = new Mock<IDelegatingHandler>();
            moduleClient.InnerHandler = innerHandler.Object;

            await moduleClient.SetIncomingMessageCallbackAsync((message) => Task.FromResult(MessageAcknowledgement.Complete)).ConfigureAwait(false);

            innerHandler.Verify(
                x => x.EnableReceiveMessageAsync(It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
            innerHandler.Verify(x => x.DisableReceiveMessageAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task ModuleClient_OnReceiveEventMessageCalled_DefaultCallbackCalled()
        {
            using var moduleClient = new IotHubModuleClient(FakeConnectionString);
            var innerHandler = new Mock<IDelegatingHandler>();
            moduleClient.InnerHandler = innerHandler.Object;

            bool isDefaultCallbackCalled = false;
            await moduleClient
                .SetIncomingMessageCallbackAsync(
                    (message) =>
                    {
                        isDefaultCallbackCalled = true;
                        return Task.FromResult(MessageAcknowledgement.Complete);
                    })
                .ConfigureAwait(false);

            var testMessage = new IncomingMessage(Encoding.UTF8.GetBytes("test message"))
            {
                InputName = "endpoint1",
            };

            await moduleClient.OnMessageReceivedAsync(testMessage).ConfigureAwait(false);
            Assert.IsTrue(isDefaultCallbackCalled);
        }
    }
}