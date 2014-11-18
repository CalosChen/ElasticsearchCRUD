﻿using System;

namespace ElasticsearchCRUD.ContextAddDeleteUpdate.CoreTypeAttributes
{
	/// <summary>
	/// type The type of the number. Can be float, double, integer, long, short, byte. Required.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property , AllowMultiple = false, Inherited = true)]
	public abstract class ElasticsearchNumber : ElasticsearchCoreTypes
	{
		protected string _indexName;
		protected bool _store;
		protected StringIndexNumber _index;
		protected bool _docValues;
		protected double _boost;
		protected object _nullValue;
		protected bool _includeInAll;
		protected PrecisionStep _precisionStep;
		private bool _ignoreMalformed;
		private bool _coerce;

		protected bool _indexNameSet;
		protected bool _storeSet;
		protected bool _indexSet;
		protected bool _docValuesSet;
		protected bool _boostSet;
		protected bool _nullValueSet;
		protected bool _includeInAllSet;
		protected bool _precisionStepSet;
		private bool _ignoreMalformedSet;
		private bool _coerceSet;

		/// <summary>
		/// index_name
		/// The name of the field that will be stored in the index. Defaults to the property/field name.
		/// </summary>
		public virtual string IndexName
		{
			get { return _indexName; }
			set
			{
				_indexName = value;
				_indexNameSet = true;
			}
		}

		/// <summary>
		/// store
		/// Set to true to actually store the field in the index, false to not store it. Defaults to false (note, the JSON document itself is stored, and it can be retrieved from it).
		/// </summary>
		public virtual bool Store
		{
			get { return _store; }
			set
			{
				_store = value;
				_storeSet = true;
			}
		}

		/// <summary>
		/// index
		/// Set to no if the value should not be indexed. Setting to no disables include_in_all. If set to no the field should be either stored in _source, have include_in_all enabled, or store be set to true for this to be useful.
		/// </summary>
		public virtual StringIndexNumber Index
		{
			get { return _index; }
			set
			{
				_index = value;
				_indexSet = true;
			}
		}

		/// <summary>
		/// doc_values
		/// Set to true to store field values in a column-stride fashion. Automatically set to true when the fielddata format is doc_values.
		/// </summary>
		public virtual bool DocValues
		{
			get { return _docValues; }
			set
			{
				_docValues = value;
				_docValuesSet = true;
			}
		}

		/// <summary>
		/// boost
		/// The boost value. Defaults to 1.0.
		/// </summary>
		public virtual double Boost
		{
			get { return _boost; }
			set
			{
				_boost = value;
				_boostSet = true;
			}
		}

		/// <summary>
		/// null_value
		/// When there is a (JSON) null value for the field, use the null_value as the field value. Defaults to not adding the field at all.
		/// </summary>
		public virtual object NullValue
		{
			get { return _nullValue; }
			set
			{
				_nullValue = value;
				_nullValueSet = true;
			}
		}

		/// <summary>
		/// include_in_all
		/// Should the field be included in the _all field (if enabled). If index is set to no this defaults to false, otherwise, defaults to true or to the parent object type setting.
		/// </summary>
		public virtual bool IncludeInAll
		{
			get { return _includeInAll; }
			set
			{
				_includeInAll = value;
				_includeInAllSet = true;
			}
		}

		/// <summary>
		/// precision_step
		/// The precision step (influences the number of terms generated for each number value). Defaults to 16 for long, double, 8 for short, integer, float, and 2147483647 for byte.
		/// </summary>
		public virtual PrecisionStep PrecisionStep
		{
			get { return _precisionStep; }
			set
			{
				_precisionStep = value;
				_precisionStepSet = true;
			}
		}

		/// <summary>
		/// ignore_malformed
		/// Ignored a malformed number. Defaults to false.
		/// </summary>
		public virtual bool IgnoreMalformed
		{
			get { return _ignoreMalformed; }
			set
			{
				_ignoreMalformed = value;
				_ignoreMalformedSet = true;
			}
		}

		/// <summary>
		/// coerce
		/// Try convert strings to numbers and truncate fractions for integers. Defaults to true.
		/// </summary>
		public virtual bool Coerce
		{
			get { return _coerce; }
			set
			{
				_coerce = value;
				_coerceSet = true;
			}
		}
	}

	public enum StringIndexNumber
	{
		no
	}

    //16 for long, double, 8 for short, integer, float, and 2147483647 for byte
	public enum PrecisionStep
	{
		_16 = 16,
		_8 = 8,
		_2147483647 = 2147483647

	}
}
