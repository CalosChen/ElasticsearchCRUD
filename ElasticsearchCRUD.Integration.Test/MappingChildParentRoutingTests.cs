﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using ElasticsearchCRUD.ContextAddDeleteUpdate;
using ElasticsearchCRUD.Tracing;
using ElasticsearchCRUD.Utils;
using NUnit.Framework;

namespace ElasticsearchCRUD.Integration.Test
{
	[TestFixture]
	public class MappingChildParentRoutingTests
	{
		private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver = new ElasticsearchMappingResolver();
		private const string ConnectionString = "http://localhost.fiddler:9200";

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				if (context.IndexExists<MappingChildParentRoutingTestsLevel1>())
				{
					context.AllowDeleteForIndex = true;
					context.DeleteIndex<MappingChildParentRoutingTestsLevel1>();
				}
			}
		}

		[Test]
		public void DeleteNonExistingChildTypeFromExistingIndex()
		{
			CreateIndex();

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				var doc1 = context.GetDocument<MappingChildParentRoutingTestsLevel1>(1);
				Assert.IsNotNull(doc1);

				var doc2 = context.GetDocument<MappingChildParentRoutingTestsLevel2>(2, new RoutingDefinition { ParentId = 1, RoutingId = 1 });
				Assert.IsNotNull(doc2);

				var doc3 = context.GetDocument<MappingChildParentRoutingTestsLevel3>(3, new RoutingDefinition { ParentId = 2, RoutingId = 1 });
				Assert.IsNotNull(doc3);

				context.AllowDeleteForIndex = true;
				var result = context.DeleteIndexType<MappingChildParentRoutingTestsLevel3>();
				Assert.IsTrue(result);

				Thread.Sleep(1000);
				result = context.DeleteIndexType<MappingChildParentRoutingTestsLevel3>();
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void DeleteNonExistingChildTypeFromExistingIndexWithList()
		{
			CreateIndexList();

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				var doc1 = context.GetDocument<ListMappingChildParentRoutingTestsLevel1>(1);
				Assert.IsNotNull(doc1);

				var doc2 = context.GetDocument<ListMappingChildParentRoutingTestsLevel2>(2, new RoutingDefinition { ParentId = 1, RoutingId = 1 });
				Assert.IsNotNull(doc2);

				var doc3 = context.GetDocument<ListMappingChildParentRoutingTestsLevel3>(3, new RoutingDefinition { ParentId = 2, RoutingId = 1 });
				Assert.IsNotNull(doc3);

				context.AllowDeleteForIndex = true;
				var result = context.DeleteIndexType<ListMappingChildParentRoutingTestsLevel3>();
				Assert.IsTrue(result);

				Thread.Sleep(1000);
				result = context.DeleteIndexType<ListMappingChildParentRoutingTestsLevel3>();
				Assert.IsFalse(result);
			}
		}

		private void CreateIndex()
		{
			var doc = new MappingChildParentRoutingTestsLevel1
			{
				MappingChildParentRoutingTestsLevel1Id = 1,
				Level2 = new MappingChildParentRoutingTestsLevel2()
				{
					MappingChildParentRoutingTestsLevel2Id = 2,
					Level3 = new MappingChildParentRoutingTestsLevel3()
					{
						MappingChildParentRoutingTestsLevel3Id = 3
					}
				}
			};

			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(MappingChildParentRoutingTestsLevel1),
			MappingUtils.GetElasticsearchMapping(new IndexTypeDescription("masterindex", "level1")));
			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(MappingChildParentRoutingTestsLevel2),
				MappingUtils.GetElasticsearchMapping(new IndexTypeDescription("masterindex", "level2")));
			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(MappingChildParentRoutingTestsLevel3),
				MappingUtils.GetElasticsearchMapping(new IndexTypeDescription("masterindex", "level3")));

			using ( var context = new ElasticsearchContext(ConnectionString,
					new ElasticsearchSerializerConfiguration( _elasticsearchMappingResolver,true,true,true)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(doc, doc.MappingChildParentRoutingTestsLevel1Id);

				var ret = context.SaveChangesAndInitMappings();
				// Save to Elasticsearch
				Assert.AreEqual(ret.Status, HttpStatusCode.OK);
			}
		}

		private void CreateIndexList()
		{
			var doc = new ListMappingChildParentRoutingTestsLevel1
			{
				MappingChildParentRoutingTestsLevel1Id = 1,
				Level2 = new List<ListMappingChildParentRoutingTestsLevel2>
				{
					new ListMappingChildParentRoutingTestsLevel2
					{
						MappingChildParentRoutingTestsLevel2Id = 2,
						Level3 = new List<ListMappingChildParentRoutingTestsLevel3>
					    {
							new ListMappingChildParentRoutingTestsLevel3
							{
								MappingChildParentRoutingTestsLevel3Id = 3
							}
						}
					}
				}
			};

			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(ListMappingChildParentRoutingTestsLevel1),
			MappingUtils.GetElasticsearchMapping("masterindexlist"));
			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(ListMappingChildParentRoutingTestsLevel2),
				MappingUtils.GetElasticsearchMapping("masterindexlist"));
			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(ListMappingChildParentRoutingTestsLevel3),
				MappingUtils.GetElasticsearchMapping("masterindexlist"));

			using (var context = new ElasticsearchContext(ConnectionString,
					new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver, true, true, true)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(doc, doc.MappingChildParentRoutingTestsLevel1Id);

				var ret = context.SaveChangesAndInitMappings();
				// Save to Elasticsearch
				Assert.AreEqual(ret.Status, HttpStatusCode.OK);
			}
		}
	}

	public class MappingChildParentRoutingTestsLevel1
	{
		public short MappingChildParentRoutingTestsLevel1Id { get; set; }

		public MappingChildParentRoutingTestsLevel2 Level2 { get; set; }
	}

	public class MappingChildParentRoutingTestsLevel2
	{
		[Key]
		public short MappingChildParentRoutingTestsLevel2Id { get; set; }

		public MappingChildParentRoutingTestsLevel3 Level3 { get; set; }
	}

	public class MappingChildParentRoutingTestsLevel3
	{
		[Key]
		public short MappingChildParentRoutingTestsLevel3Id { get; set; }
	}

	public class ListMappingChildParentRoutingTestsLevel1
	{
		public short MappingChildParentRoutingTestsLevel1Id { get; set; }

		public List<ListMappingChildParentRoutingTestsLevel2> Level2 { get; set; }
	}

	public class ListMappingChildParentRoutingTestsLevel2
	{
		[Key]
		public short MappingChildParentRoutingTestsLevel2Id { get; set; }

		public List<ListMappingChildParentRoutingTestsLevel3> Level3 { get; set; }
	}

	public class ListMappingChildParentRoutingTestsLevel3
	{
		[Key]
		public short MappingChildParentRoutingTestsLevel3Id { get; set; }
	}
}
