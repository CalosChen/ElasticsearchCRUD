﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Response.Model;

namespace ElasticLinq.Response.Materializers
{
    /// <summary>
    /// Interface for all materializers responsible for turning the ElasticResponse into desired
    /// CLR objects.
    /// </summary>
    internal interface IElasticMaterializer
    {
        /// <summary>
        /// Materialize the ElasticResponse into the desired CLR objects.
        /// </summary>
        /// <param name="response">ElasticResponse object received from Elasticsearch.</param>
        /// <returns>List or a single CLR object as requested.</returns>
        object Materialize(ElasticResponse response);
    }
}