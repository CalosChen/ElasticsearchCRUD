﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Utility;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace ElasticLinq
{
    /// <summary>
    /// Specifies connection parameters for Elasticsearch.
    /// </summary>
    [DebuggerDisplay("{Endpoint.ToString(),nq}{Index,nq}")]
    public class ElasticConnection : IDisposable
    {
        private static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(10);

        private readonly Uri endpoint;
        private readonly string index;
        private readonly TimeSpan timeout = defaultTimeout;
        private readonly HttpClient httpClient;

        private bool disposed;

        /// <summary>
        /// Create a new ElasticConnection with the given parameters defining its properties.
        /// </summary>
        /// <param name="endpoint">The URL endpoint of the Elasticsearch server.</param>
        /// <param name="userName">UserName to use to connect to the server (optional).</param>
        /// <param name="password">Password to use to connect to the server (optional).</param>
        /// <param name="timeout">TimeSpan to wait for network responses before failing (optional, defaults to 10 seconds).</param>
        /// <param name="index">Name of the index to use on the server (optional).</param>
        public ElasticConnection(Uri endpoint, string userName = null, string password = null, TimeSpan? timeout = null, string index = null)
            : this(new HttpClientHandler(), endpoint, userName, password, index, timeout) { }


        /// <summary>
        /// Create a new ElasticConnection with the given parameters for internal testing.
        /// </summary>
        /// <param name="innerMessageHandler">The HttpMessageHandler used to intercept network requests for testing.</param>
        /// <param name="endpoint">The URL endpoint of the Elasticsearch server.</param>
        /// <param name="userName">UserName to use to connect to the server (optional).</param>
        /// <param name="password">Password to use to connect to the server (optional).</param>
        /// <param name="timeout">TimeSpan to wait for network responses before failing (optional, defaults to 10 seconds).</param>
        /// <param name="index">Name of the index to use on the server (optional).</param>
        internal ElasticConnection(HttpMessageHandler innerMessageHandler, Uri endpoint, string userName = null, string password = null, string index = null, TimeSpan? timeout = null)
        {
            Argument.EnsureNotNull("endpoint", endpoint);
            if (timeout.HasValue)
                Argument.EnsurePositive("value", timeout.Value);
            if (index != null)
                Argument.EnsureNotBlank("index", index);

            this.endpoint = endpoint;
            this.index = index;
            this.timeout = timeout ?? defaultTimeout;

            var httpClientHandler = innerMessageHandler as HttpClientHandler;
            if (httpClientHandler != null && httpClientHandler.SupportsAutomaticDecompression)
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip;

            httpClient = new HttpClient(new ForcedAuthHandler(userName, password, innerMessageHandler), true);
        }

        /// <summary>
        /// The HttpClient used for issuing HTTP network requests.
        /// </summary>
        internal HttpClient HttpClient
        {
            get { return httpClient; }
        }

        /// <summary>
        /// The Uri that specifies the public endpoint for the server.
        /// </summary>
        /// <example>http://myserver.example.com:9200</example>
        public Uri Endpoint
        {
            get { return endpoint; }
        }

        /// <summary>
        /// The name of the index on the Elasticsearch server.
        /// </summary>
        /// <example>northwind</example>
        public string Index
        {
            get { return index; }
        }

        /// <summary>
        /// How long to wait for a response to a network request before
        /// giving up.
        /// </summary>
        public TimeSpan Timeout
        {
            get { return timeout; }
        }

        /// <summary>
        /// Dispose of this ElasticConnection and any associated resources.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            httpClient.Dispose();
        }
    }
}