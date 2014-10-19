﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ElasticsearchCRUD.Tracing;
using Newtonsoft.Json.Linq;

namespace ElasticsearchCRUD.ContextGet
{
	public class ElasticsearchContextGet
	{
		private readonly ITraceProvider _traceProvider;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ElasticsearchSerializerConfiguration _elasticsearchSerializerConfiguration;
		private readonly HttpClient _client;
		private readonly string _connectionString;

		public ElasticsearchContextGet(ITraceProvider traceProvider, CancellationTokenSource cancellationTokenSource, ElasticsearchSerializerConfiguration elasticsearchSerializerConfiguration, HttpClient client, string connectionString)
		{
			_traceProvider = traceProvider;
			_cancellationTokenSource = cancellationTokenSource;
			_elasticsearchSerializerConfiguration = elasticsearchSerializerConfiguration;
			_client = client;
			_connectionString = connectionString;
		}

		public T GetEntity<T>(object entityId)
		{
			try
			{
				var task = Task.Run(() => GetEntityAsync<T>(entityId));
				task.Wait();
				if (task.Result.Status == HttpStatusCode.NotFound)
				{
					_traceProvider.Trace(TraceEventType.Warning, "ElasticSearchContextGet: HttpStatusCode.NotFound");
					throw new ElasticsearchCrudException("ElasticSearchContextGet: HttpStatusCode.NotFound");
				}
				if (task.Result.Status == HttpStatusCode.BadRequest)
				{
					_traceProvider.Trace(TraceEventType.Warning, "ElasticSearchContextGet: HttpStatusCode.BadRequest");
					throw new ElasticsearchCrudException("ElasticSearchContextGet: HttpStatusCode.BadRequest" + task.Result.Description);
				}
				return task.Result.PayloadResult;
			}
			catch (AggregateException ae)
			{
				ae.Handle(x =>
				{
					_traceProvider.Trace(TraceEventType.Warning, x, "{2} GetEntity {0}, {1}", typeof(T), entityId, "ElasticSearchContextGet");
					if (x is ElasticsearchCrudException || x is HttpRequestException)
					{
						throw x;
					}

					throw new ElasticsearchCrudException(x.Message);
				});
			}

			_traceProvider.Trace(TraceEventType.Error, "{2}: Unknown error for GetEntity {0}, Type {1}", entityId, typeof(T), "ElasticSearchContextGet");
			throw new ElasticsearchCrudException(string.Format("{2}: Unknown error for GetEntity {0}, Type {1}", entityId, typeof(T), "ElasticSearchContextGet"));
		}

		public async Task<ResultDetails<T>> GetEntityAsync<T>(object entityId)
		{
			_traceProvider.Trace(TraceEventType.Verbose, "{2}: Request for select entity with id: {0}, Type: {1}", entityId, typeof(T), "ElasticSearchContextGet");
			var resultDetails = new ResultDetails<T> { Status = HttpStatusCode.InternalServerError };
			try
			{
				var elasticSearchMapping = _elasticsearchSerializerConfiguration.ElasticSearchMappingResolver.GetElasticSearchMapping(typeof(T));
				var elasticsearchUrlForEntityGet = string.Format("{0}/{1}/{2}/", _connectionString, elasticSearchMapping.GetIndexForType(typeof(T)), elasticSearchMapping.GetDocumentType(typeof(T)));
	
				var uri = new Uri(elasticsearchUrlForEntityGet + entityId);
				_traceProvider.Trace(TraceEventType.Verbose, "{1}: Request HTTP GET uri: {0}", uri.AbsoluteUri, "ElasticSearchContextGet");
				var response = await _client.GetAsync(uri, _cancellationTokenSource.Token).ConfigureAwait(false);

				resultDetails.Status = response.StatusCode;
				if (response.StatusCode != HttpStatusCode.OK)
				{
					_traceProvider.Trace(TraceEventType.Warning, "{2}: GetEntityAsync response status code: {0}, {1}", response.StatusCode, response.ReasonPhrase, "ElasticSearchContextGet");
					if (response.StatusCode == HttpStatusCode.BadRequest)
					{
						var errorInfo = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						resultDetails.Description = errorInfo;
						if (errorInfo.Contains("RoutingMissingException"))
						{
							throw new ElasticsearchCrudException("HttpStatusCode.BadRequest: RoutingMissingException, adding the parent Id if this is a child item...");
						}

						return resultDetails;
					}
				}

				var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				_traceProvider.Trace(TraceEventType.Verbose, "{1}: Get Request response: {0}", responseString, "ElasticSearchContextGet");
				var responseObject = JObject.Parse(responseString);

				var source = responseObject["_source"];
				if (source != null)
				{
					var result = _elasticsearchSerializerConfiguration.ElasticSearchMappingResolver.GetElasticSearchMapping(typeof(T)).ParseEntity(source, typeof(T));
					resultDetails.PayloadResult = (T)result;
				}

				return resultDetails;
			}
			catch (OperationCanceledException oex)
			{
				_traceProvider.Trace(TraceEventType.Verbose, oex, "{1}: Get Request OperationCanceledException: {0}", oex.Message, "ElasticSearchContextGet");
				return resultDetails;
			}
		}

	}
}
