﻿using ElasticsearchCRUD.Utils;

namespace ElasticsearchCRUD.Model.SearchModel.Filters
{
	public class RangeFilter : IFilter
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
		private bool _cache;
		private bool _cacheSet;
		private ExecutionOption _execution;
		private bool _executionSet;

		public RangeFilter(string field)
		{
			_field = field;
		}

		public bool Cache
		{
			get { return _cache; }
			set
			{
				_cache = value;
				_cacheSet = true;
			}
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

		/// <summary>
		/// execution
		/// The execution option controls how the range filter internally executes. The execution option accepts the index and fielddata values.
		/// 
		/// In general for small ranges the index execution is faster and for longer ranges the fielddata execution is faster.
		/// 
		/// The fielddata execution, as the name suggests, uses field data and therefore requires more memory, 
		/// so make sure you have sufficient memory on your nodes in order to use this execution mode. It usually makes sense to use it on fields you’re already aggregating or sorting by.
		/// </summary>
		public ExecutionOption Execution
		{
			get { return _execution; }
			set
			{
				_execution = value;
				_executionSet = true;
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
			JsonHelper.WriteValue("_cache", _cache, elasticsearchCrudJsonWriter, _cacheSet);
			JsonHelper.WriteValue("execution", _execution.ToString(), elasticsearchCrudJsonWriter, _executionSet);

			elasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();
			elasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();
		}
	}

	public enum ExecutionOption
	{
		/// <summary>
		/// Uses the field’s inverted index in order to determine whether documents fall within the specified range.
		/// </summary>
		index,
		
		/// <summary>
		/// Uses fielddata in order to determine whether documents fall within the specified range. 
		/// </summary>
		fielddata
	}
}
