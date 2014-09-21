﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;

namespace ElasticLinq.Response.Model
{
    /// <summary>
    /// A container of hit responses from Elasticsearch.
    /// </summary>
    [DebuggerDisplay("{hits.Count} hits of {total}")]
    public class Hits
    {
        public long total;
        public double? max_score;
        public List<Hit> hits;
    }
}