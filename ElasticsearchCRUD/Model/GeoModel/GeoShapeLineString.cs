﻿using System.Collections.Generic;
using ElasticsearchCRUD.Utils;

namespace ElasticsearchCRUD.Model.GeoModel
{
	public class GeoShapeLineString : GeoType
	{
		// TODO validate that at least two items exist
		public List<Coordinate> Coordinates { get; set; }

		public override void WriteJson(ElasticsearchCrudJsonWriter elasticsearchCrudJsonWriter)
		{
			JsonHelper.WriteValue("type", DefaultGeoShapes.LineString, elasticsearchCrudJsonWriter);
			elasticsearchCrudJsonWriter.JsonWriter.WritePropertyName("coordinates");
			elasticsearchCrudJsonWriter.JsonWriter.WriteStartArray();
			foreach (var item in Coordinates)
			{
				item.WriteJson(elasticsearchCrudJsonWriter);
			}
			elasticsearchCrudJsonWriter.JsonWriter.WriteEndArray();	
		}
	}
}