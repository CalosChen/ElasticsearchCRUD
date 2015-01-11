﻿using ElasticsearchCRUD.Utils;

namespace ElasticsearchCRUD.Model.SearchModel.Queries
{
	/// <summary>
	/// Matches documents with fields that have terms within a certain range. The type of the Lucene query depends on the field type, 
	/// for string fields, the TermRangeQuery, while for number/date fields, the query is a NumericRangeQuery. The 
	/// </summary>
	public class RangeQuery : IQuery
	{
		private readonly string _field;
		private double _boost;
		private bool _boostSet;
		private string _greaterThanOrEqualTo;
		private bool _greaterThanOrEqualToSet;
		private string _greaterThan;
		private bool _greaterThanSet;
		private string _lessThanOrEqualTo;
		private bool _lessThanOrEqualToSet;
		private string _lessThan;
		private bool _lessThanSet;
		private string _timeZone;
		private bool _timeZoneSet;

		public RangeQuery(string field)
		{
			_field = field;
		}

		public double Boost
		{
			get { return _boost; }
			set
			{
				_boost = value;
				_boostSet = true;
			}
		}

		/// <summary>
		/// gte
		/// Greater-than or equal to
		/// </summary>
		public string GreaterThanOrEqualTo
		{
			get { return _greaterThanOrEqualTo; }
			set
			{
				_greaterThanOrEqualTo = value;
				_greaterThanOrEqualToSet = true;
			}
		}

		/// <summary>
		/// gt
		/// Greater-than
		/// </summary>
		public string GreaterThan
		{
			get { return _greaterThan; }
			set
			{
				_greaterThan = value;
				_greaterThanSet = true;
			}
		}

		/// <summary>
		/// lte
		/// Less-than or equal to
		/// </summary>
		public string LessThanOrEqualTo
		{
			get { return _lessThanOrEqualTo; }
			set
			{
				_lessThanOrEqualTo = value;
				_lessThanOrEqualToSet = true;
			}
		}

		/// <summary>
		/// lt
		/// Less-than
		/// </summary>
		public string LessThan
		{
			get { return _lessThan; }
			set
			{
				_lessThan = value;
				_lessThanSet = true;
			}
		}

		/// <summary>
		/// time_zone
		/// When applied on date fields the range filter accepts also a time_zone parameter. 
		/// The time_zone parameter will be applied to your input lower and upper bounds and will move them to UTC time based date
		/// 
		/// if you give a date with a timezone explicitly defined and use the time_zone parameter, time_zone will be ignored. For example,
		/// setting gte to 2012-01-01T00:00:00+01:00 with "time_zone":"+10:00" will still use +01:00 time zone.
		/// </summary>
		public string TimeZone
		{
			get { return _timeZone; }
			set
			{
				_timeZone = value;
				_timeZoneSet = true;
			}
		}

		public void WriteJson(ElasticsearchCrudJsonWriter elasticsearchCrudJsonWriter)
		{
			elasticsearchCrudJsonWriter.JsonWriter.WritePropertyName("range");
			elasticsearchCrudJsonWriter.JsonWriter.WriteStartObject();

			elasticsearchCrudJsonWriter.JsonWriter.WritePropertyName(_field);
			elasticsearchCrudJsonWriter.JsonWriter.WriteStartObject();

			JsonHelper.WriteValue("gte", _greaterThanOrEqualTo, elasticsearchCrudJsonWriter, _greaterThanOrEqualToSet);
			JsonHelper.WriteValue("gt", _greaterThan, elasticsearchCrudJsonWriter, _greaterThanSet);
			JsonHelper.WriteValue("lte", _lessThanOrEqualTo, elasticsearchCrudJsonWriter, _boostSet);
			JsonHelper.WriteValue("lt", _lessThan, elasticsearchCrudJsonWriter, _lessThanOrEqualToSet);
			JsonHelper.WriteValue("boost", _boost, elasticsearchCrudJsonWriter, _lessThanSet);
			JsonHelper.WriteValue("time_zone", _timeZone, elasticsearchCrudJsonWriter, _timeZoneSet);

			elasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();
			elasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();
		}
	}
}
