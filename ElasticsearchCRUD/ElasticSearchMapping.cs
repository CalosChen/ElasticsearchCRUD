﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElasticsearchCRUD
{
	/// <summary>
	/// Default mapping for your Entity. You can implement this clas to implement your specific mapping if required
	/// Everything is lowercase and the index is pluralized
	/// </summary>
	public class ElasticSearchMapping
	{
		protected HashSet<string> SerializedTypes = new HashSet<string>();

		// default type is lowercase for properties
		public virtual void MapEntityValues(Object entity, ElasticsearchCrudJsonWriter elasticsearchCrudJsonWriter, bool beginMappingTree = false)
		{
			if (beginMappingTree)
			{
				SerializedTypes = new HashSet<string>();
			}

			SerializedTypes.Add(GetDocumentType(entity.GetType()));

			var propertyInfo = entity.GetType().GetProperties();
			foreach (var prop in propertyInfo)
			{				
				if (IsPropertyACollection(prop))
				{
					if (prop.GetValue(entity) != null)
					{
						elasticsearchCrudJsonWriter.JsonWriter.WritePropertyName(prop.Name.ToLower());
						var typeOfEntity = prop.GetValue(entity).GetType().GetGenericArguments();
						if (typeOfEntity.Length > 0)
						{
							if (!SerializedTypes.Contains(GetDocumentType(typeOfEntity[0])))
							{
								MapCollectionOrArray(prop, entity, elasticsearchCrudJsonWriter);
							}
						}
						else
						{
							// Not a generic
							MapCollectionOrArray(prop, entity, elasticsearchCrudJsonWriter);
						}
					}
				}
				else
				{
					if (prop.PropertyType.IsClass && prop.PropertyType.FullName != "System.String" && prop.PropertyType.FullName != "System.Decimal")
					{
						// This is a single object and not a reference to it's parent
						if (prop.GetValue(entity) != null && !SerializedTypes.Contains(GetDocumentType(prop.GetValue(entity).GetType())))
						{
							SerializedTypes.Add(GetDocumentType(prop.GetValue(entity).GetType()));

							elasticsearchCrudJsonWriter.JsonWriter.WritePropertyName(prop.Name.ToLower());
							elasticsearchCrudJsonWriter.JsonWriter.WriteStartObject();
							// Do class mapping for nested type
							MapEntityValues(prop.GetValue(entity), elasticsearchCrudJsonWriter);
							elasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();
						}

						// TODO Add as separate document later inn it's index
					}
					else
					{
						MapValue(prop.Name.ToLower(), prop.GetValue(entity), elasticsearchCrudJsonWriter.JsonWriter);
					}
				}	
			}
		}

		// Nested
		// "tags" : ["elasticsearch", "wow"], (string array or int array)
		// Nested
		//"lists" : [
		//	{
		//		"name" : "prog_list",
		//		"description" : "programming list"
		//	},	
		protected virtual void MapCollectionOrArray(PropertyInfo prop, object entity, ElasticsearchCrudJsonWriter elasticsearchCrudJsonWriter)
		{
			Type type = prop.PropertyType;
			
			if (type.HasElementType)
			{
				// It is a collection
				var ienumerable = (Array)prop.GetValue(entity);
				MapIEnumerableEntities(elasticsearchCrudJsonWriter, ienumerable);				
			}
			else if (prop.PropertyType.IsGenericType)
			{
				// It is a collection
				var ienumerable = (IEnumerable)prop.GetValue(entity);
				MapIEnumerableEntities(elasticsearchCrudJsonWriter, ienumerable);
			}
		}

		private void MapIEnumerableEntities(ElasticsearchCrudJsonWriter elasticsearchCrudJsonWriter, IEnumerable ienumerable)
		{
			string json = null;
			bool isSimpleArrayOrCollection = true;
			bool doProccessingIfTheIEnumerableHasAtLeastOneItem = false;
			if (ienumerable != null)
			{
				var sbCollection = new StringBuilder();
				sbCollection.Append("[");
				foreach (var item in ienumerable)
				{
					doProccessingIfTheIEnumerableHasAtLeastOneItem = true;

					var childElasticsearchCrudJsonWriter = new ElasticsearchCrudJsonWriter(sbCollection);

					var typeofArrayItem = item.GetType();
					if (typeofArrayItem.IsClass && typeofArrayItem.FullName != "System.String" &&
						typeofArrayItem.FullName != "System.Decimal")
					{
						isSimpleArrayOrCollection = false;
						// collection of Objects
						childElasticsearchCrudJsonWriter.JsonWriter.WriteStartObject();
						// Do class mapping for nested type
						MapEntityValues(item, childElasticsearchCrudJsonWriter);
						childElasticsearchCrudJsonWriter.JsonWriter.WriteEndObject();

						// Add as separate document later
					}
					else
					{
						// collection of simple types, serialize all items in one go and break from the loop
						json = JsonConvert.SerializeObject(ienumerable);

						break;
					}
					sbCollection.Append(",");
				}

				if (isSimpleArrayOrCollection && doProccessingIfTheIEnumerableHasAtLeastOneItem)
				{
					elasticsearchCrudJsonWriter.JsonWriter.WriteRawValue(json);
				}
				else
				{
					if (doProccessingIfTheIEnumerableHasAtLeastOneItem)

					{
						sbCollection.Remove(sbCollection.Length - 1, 1);
					}

					sbCollection.Append("]");
					elasticsearchCrudJsonWriter.JsonWriter.WriteRawValue(sbCollection.ToString());
				}
			}
			else
			{
				elasticsearchCrudJsonWriter.JsonWriter.WriteRawValue("");
			}
		}

		protected void MapValue(string key, object valueObj, JsonWriter writer)
		{
			writer.WritePropertyName(key);
			writer.WriteValue(valueObj);
		}

		protected bool IsPropertyACollection(PropertyInfo property)
		{
			if (property.PropertyType.FullName == "System.String" || property.PropertyType.FullName == "System.Decimal")
			{
				return false;
			}
			return property.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null;
		} 

		public virtual object ParseEntity(JToken source, Type type)
		{
			return JsonConvert.DeserializeObject(source.ToString(), type);
		}

		public virtual string GetDocumentType(Type type)
		{
			// Adding support for EF types
			if (type.BaseType != null && type.Namespace == "System.Data.Entity.DynamicProxies")
			{
				type = type.BaseType;
			}
			return type.Name.ToLower();
		}

		// pluralize the default type
		public virtual string GetIndexForType(Type type)
		{
			// Adding support for EF types
			if (type.BaseType != null && type.Namespace == "System.Data.Entity.DynamicProxies")
			{
				type = type.BaseType;
			}
			return type.Name.ToLower() + "s";
		}
	}

}
