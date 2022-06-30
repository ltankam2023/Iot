﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using Microsoft.Azure.Devices.Authentication;

namespace Microsoft.Azure.Devices.Provisioning.Client
{
    internal class HttpAuthStrategySymmetricKey : HttpAuthStrategy
    {
        private readonly AuthenticationProviderSymmetricKey _authentication;

        public HttpAuthStrategySymmetricKey(AuthenticationProviderSymmetricKey authentication)
        {
            _authentication = authentication;
        }

        public override DeviceProvisioningServiceRuntimeClient CreateClient(Uri uri, HttpClientHandler httpClientHandler)
        {
            var serviceCredentials = new SymmetricKeyCredentials(_authentication.GetPrimaryKey());
            var dpsClient = new DeviceProvisioningServiceRuntimeClient(
                uri,
                serviceCredentials,
                httpClientHandler,
                new ApiVersionDelegatingHandler());

            dpsClient.HttpClient.Timeout = TimeoutConstant;

            return dpsClient;
        }

        public override DeviceRegistrationHttp CreateDeviceRegistration()
        {
            return new DeviceRegistrationHttp(registrationId: _authentication.GetRegistrationID());
        }

        public override void SaveCredentials(RegistrationOperationStatus status)
        {
            // no-op
        }
    }
}