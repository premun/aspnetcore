// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.Kestrel.Core
{
    internal sealed class LocalhostListenOptions : ListenOptions
    {
        internal LocalhostListenOptions(int port)
            : base(new IPEndPoint(IPAddress.Loopback, port))
        {
            if (port == 0)
            {
                throw new InvalidOperationException(CoreStrings.DynamicPortOnLocalhostNotSupported);
            }
        }

        /// <summary>
        /// Gets the name of this endpoint to display on command-line when the web server starts.
        /// </summary>
        internal override string GetDisplayName()
        {
            return $"{Scheme}://localhost:{IPEndPoint!.Port}";
        }

        internal override async Task BindAsync(AddressBindContext context, CancellationToken cancellationToken)
        {
            var exceptions = new List<Exception>();

            try
            {
                var v4Options = Clone(IPAddress.Loopback);
                await AddressBinder.BindEndpointAsync(v4Options, context, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is IOException or OperationCanceledException))
            {
                context.Logger.LogInformation(0, CoreStrings.NetworkInterfaceBindingFailed, GetDisplayName(), "IPv4 loopback", ex.Message);
                exceptions.Add(ex);
            }

            try
            {
                var v6Options = Clone(IPAddress.IPv6Loopback);
                await AddressBinder.BindEndpointAsync(v6Options, context, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is IOException or OperationCanceledException))
            {
                context.Logger.LogInformation(0, CoreStrings.NetworkInterfaceBindingFailed, GetDisplayName(), "IPv6 loopback", ex.Message);
                exceptions.Add(ex);
            }

            if (exceptions.Count == 2)
            {
                throw new IOException(CoreStrings.FormatAddressBindingFailed(GetDisplayName()), new AggregateException(exceptions));
            }

            // If StartLocalhost doesn't throw, there is at least one listener.
            // The port cannot change for "localhost".
            context.Addresses.Add(GetDisplayName());
        }

        // used for cloning to two IPEndpoints
        internal ListenOptions Clone(IPAddress address)
        {
            var options = new ListenOptions(new IPEndPoint(address, IPEndPoint!.Port))
            {
                KestrelServerOptions = KestrelServerOptions,
                Protocols = Protocols,
                IsTls = IsTls,
                HttpsOptions = HttpsOptions,
                EndpointConfig = EndpointConfig
            };

            options._middleware.AddRange(_middleware);
            options._multiplexedMiddleware.AddRange(_multiplexedMiddleware);
            return options;
        }
    }
}
