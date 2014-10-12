﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ElasticsearchCRUD.Tracing;
using Newtonsoft.Json;

namespace ElasticsearchCRUD
{
	public class ElasticsearchSerializer  : IDisposable
	{
		private readonly IElasticSearchMappingResolver _elasticSearchMappingResolver;
		private readonly ITraceProvider _traceProvider;

		public ElasticsearchSerializer( ITraceProvider traceProvider, IElasticSearchMappingResolver elasticSearchMappingResolver)
		{
			_elasticSearchMappingResolver = elasticSearchMappingResolver;
			_traceProvider = traceProvider;
		}

		private ElasticsearchCrudJsonWriter _elasticsearchCrudJsonWriter;

		public string Serialize(IEnumerable<Tuple<EntityContextInfo, object>> entities)
		{
			if (entities == null)
			{
				return null;
			}

			_elasticsearchCrudJsonWriter = new ElasticsearchCrudJsonWriter();

			foreach (var entity in entities)
			{
				string index = _elasticSearchMappingResolver.GetElasticSearchMapping(entity.GetType()).GetIndexForType(entity.GetType());
				if (Regex.IsMatch(index, "[\\\\/*?\",<>|\\sA-Z]"))
				{
					_traceProvider.Trace(TraceEventType.Error, "index is not allowed in Elasticsearch: {0}", index);
					throw new ElasticsearchCrudException(string.Format("index is not allowed in Elasticsearch: {0}", index));
				}
				if (entity.Item1.DeleteEntity)
				{
					DeleteEntity(entity.Item1);
				}
				else
				{
					AddUpdateEntity(entity.Item2, entity.Item1);
				}
			}

			_elasticsearchCrudJsonWriter.Dispose();

			return _elasticsearchCrudJsonWriter.Stringbuilder.ToString();
		}

		private void DeleteEntity(EntityContextInfo entityInfo)
		{
			var elasticSearchMapping = _elasticSearchMappingResolver.GetElasticSearchMapping(entityInfo.EntityType);
			_elasticsearchCrudJsonWriter.JsonWriter.WriteStartObject();

			_elasticsearchCrudJsonWriter.JsonWriter.WritePropertyName("delete");

			// Write the batch "index" operation header
			_elasticsearchCrudJsonWriter.JsonWriter.WriteStartObject();
			WriteValue("_index", elasticSearchMapping.GetIndexForType(entityInfo.EntityType));
			WriteValue("_type", elasticSearchMapping.GetDocumentType(entityInfo.EntityType));
			WriteValue("_id", entityInfo.Id);
			_elasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();
			_elasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();
		
			_elasticsearchCrudJsonWriter.JsonWriter.WriteRaw("\n");
		}

		private void AddUpdateEntity(object entity, EntityContextInfo entityInfo)
		{
			var elasticSearchMapping = _elasticSearchMappingResolver.GetElasticSearchMapping(entityInfo.EntityType);
			_elasticsearchCrudJsonWriter.JsonWriter.WriteStartObject();

			_elasticsearchCrudJsonWriter.JsonWriter.WritePropertyName("index");

			// Write the batch "index" operation header
			_elasticsearchCrudJsonWriter.JsonWriter.WriteStartObject();
			WriteValue("_index", elasticSearchMapping.GetIndexForType(entityInfo.EntityType));
			WriteValue("_type", elasticSearchMapping.GetDocumentType(entityInfo.EntityType));
			WriteValue("_id", entityInfo.Id);
			_elasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();
			_elasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();
			_elasticsearchCrudJsonWriter.JsonWriter.WriteRaw("\n");  //ES requires this \n separator

			_elasticsearchCrudJsonWriter.JsonWriter.WriteStartObject();

			elasticSearchMapping.MapEntityValues(entity, _elasticsearchCrudJsonWriter, true);

			_elasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();
			_elasticsearchCrudJsonWriter.JsonWriter.WriteRaw("\n");
		}

		private void WriteValue(string key, object valueObj)
		{
			_elasticsearchCrudJsonWriter.JsonWriter.WritePropertyName(key);
			_elasticsearchCrudJsonWriter.JsonWriter.WriteValue(valueObj);
		}

		public void Dispose()
		{
			_elasticsearchCrudJsonWriter.Dispose();
		}
	}
}
