﻿using System;
using System.Threading;
using NUnit.Framework;

namespace ElasticsearchCRUD.Integration.Test
{
	[TestFixture]
	public class AliasElasticsearchCrudTests
	{
		private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver = new ElasticsearchMappingResolver();

		[TearDown]
		public void TearDown()
		{
			using (var context = new ElasticsearchContext("http://localhost:9200/", _elasticsearchMappingResolver))
			{
				context.AllowDeleteForIndex = true;
				var entityResult = context.DeleteIndexAsync<IndexAliasDtoTest>();
				entityResult.Wait();

				var secondDelete = context.DeleteIndexAsync<IndexAliasDtoTestTwo>();
				secondDelete.Wait();		
			}
		}

		[Test]
		[ExpectedException(ExpectedException = typeof(ElasticsearchCrudException))]
		public void CreateAliasForNoExistingIndex()
		{
			using (var context = new ElasticsearchContext("http://localhost:9200/", _elasticsearchMappingResolver))
			{
				context.AliasCreateForIndex("test", "doesnotexistindex");
			}
		}

		[Test]
		[ExpectedException(ExpectedException = typeof(ElasticsearchCrudException), ExpectedMessage = "ElasticsearchContextAlias: index is not allowed in Elasticsearch: doeGGGtindex")]
		public void CreateAliasForIndexBadIndex()
		{
			using (var context = new ElasticsearchContext("http://localhost:9200/", _elasticsearchMappingResolver))
			{
				context.AliasCreateForIndex("test", "doeGGGtindex");
			}
		}

		[Test]
		[ExpectedException(ExpectedException = typeof(ElasticsearchCrudException), ExpectedMessage = "ElasticsearchContextAlias: alias is not allowed in Elasticsearch: tesHHHt")]
		public void CreateAliasForIndexBadAlias()
		{
			using (var context = new ElasticsearchContext("http://localhost:9200/", _elasticsearchMappingResolver))
			{
				context.AliasCreateForIndex("tesHHHt", "doendex");
			}
		}

		[Test]
		public void CreateAliasForIndex()
		{
			var indexAliasDtoTest = new IndexAliasDtoTest {Id = 1, Description = "Test index for aliases"};

			using (var context = new ElasticsearchContext("http://localhost:9200/", _elasticsearchMappingResolver))
			{
				context.AddUpdateDocument(indexAliasDtoTest, indexAliasDtoTest.Id);
				context.SaveChanges();

				var result = context.AliasCreateForIndex("test", "indexaliasdtotests");
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void RemoveAliasForIndex()
		{
			var indexAliasDtoTest = new IndexAliasDtoTest { Id = 1, Description = "Test index for aliases" };

			using (var context = new ElasticsearchContext("http://localhost:9200/", _elasticsearchMappingResolver))
			{
				context.AddUpdateDocument(indexAliasDtoTest, indexAliasDtoTest.Id);
				context.SaveChanges();

				var resultCreate = context.AliasCreateForIndex("test", "indexaliasdtotests");
				Assert.IsTrue(resultCreate);

				var resultRemove = context.AliasRemoveForIndex("test", "indexaliasdtotests");
				Assert.IsTrue(resultRemove);
			}
		}

		[Test]
		[ExpectedException(ExpectedException = typeof(ElasticsearchCrudException))]
		public void RemoveAliasthatDoesNotExistForIndex()
		{
			var indexAliasDtoTest = new IndexAliasDtoTest { Id = 1, Description = "Test index for aliases" };

			using (var context = new ElasticsearchContext("http://localhost:9200/", _elasticsearchMappingResolver))
			{
				context.AddUpdateDocument(indexAliasDtoTest, indexAliasDtoTest.Id);
				context.SaveChanges();

				var result = context.AliasRemoveForIndex("tefdfdfdsfst", "indexaliasdtotests");
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ReplaceIndexForAlias()
		{
			var indexAliasDtoTest = new IndexAliasDtoTest { Id = 1, Description = "Test index for aliases" };
			var indexAliasDtoTestTwo = new IndexAliasDtoTestTwo { Id = 1, Description = "Test Doc Type Two index for aliases" };

			using (var context = new ElasticsearchContext("http://localhost:9200/", _elasticsearchMappingResolver))
			{
				context.AddUpdateDocument(indexAliasDtoTest, indexAliasDtoTest.Id);
				context.AddUpdateDocument(indexAliasDtoTestTwo, indexAliasDtoTestTwo.Id);
				context.SaveChanges();

				var resultCreate = context.AliasCreateForIndex("test", "indexaliasdtotests");
				Assert.IsTrue(resultCreate);

				var result = context.AliasReplaceIndex("test", "indexaliasdtotests", "indexaliasdtotesttwos");
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ReindexTest()
		{
			var indexAliasDtoTestV1 = new IndexAliasDtoTestThree { Id = 1, Description = "V1" };
			var indexAliasDtoTestV2 = new IndexAliasDtoTestThree { Id = 2, Description = "V2" };

			IElasticsearchMappingResolver elasticsearchMappingResolverDirectIndex = new ElasticsearchMappingResolver();
			IElasticsearchMappingResolver elasticsearchMappingResolverDirectIndexV1 = new ElasticsearchMappingResolver();
			var mappingV1 = new IndexAliasDtoTestThreeMappingV1();

			IElasticsearchMappingResolver elasticsearchMappingResolverDirectIndexV2 = new ElasticsearchMappingResolver();
			var mappingV2 = new IndexAliasDtoTestThreeMappingV2();

			elasticsearchMappingResolverDirectIndexV1.AddElasticSearchMappingForEntityType(typeof(IndexAliasDtoTestThree), mappingV1);
			elasticsearchMappingResolverDirectIndexV2.AddElasticSearchMappingForEntityType(typeof(IndexAliasDtoTestThree), mappingV2);

			// Step 1 create index V1 and add alias
			using (var context = new ElasticsearchContext("http://localhost:9200/", elasticsearchMappingResolverDirectIndexV1))
			{
				// create the index
				context.AddUpdateDocument(indexAliasDtoTestV1, indexAliasDtoTestV1.Id);
				context.SaveChanges();

				var resultCreate = context.AliasCreateForIndex("indexaliasdtotestthrees", "indexaliasdtotestthree_v1");
				Assert.IsTrue(resultCreate);
			}

			// Step 2 create index V2 and replace alias
			using (var context = new ElasticsearchContext("http://localhost:9200/", elasticsearchMappingResolverDirectIndexV2))
			{
				// create the index
				context.AddUpdateDocument(indexAliasDtoTestV2, indexAliasDtoTestV2.Id);
				context.SaveChanges();

				var result = context.AliasReplaceIndex("indexaliasdtotestthrees", "indexaliasdtotestthree_v1", "indexaliasdtotestthree_v2");
				Assert.IsTrue(result);
			}

			// wait till el updates
			Thread.Sleep(1000);

			// Step 3 CUD using alias
			using (var context = new ElasticsearchContext("http://localhost:9200/", elasticsearchMappingResolverDirectIndex))
			{
				// create the index
				//var itemNull = context.SearchById<IndexAliasDtoTest>(1);
				var itemOk = context.SearchById<IndexAliasDtoTestThree>(2);
				//Assert.IsNull(itemNull);
				Assert.IsNotNull(itemOk);
			}

			// delete index v1
			using (var context = new ElasticsearchContext("http://localhost:9200/", elasticsearchMappingResolverDirectIndexV1))
			{
				context.AllowDeleteForIndex = true;
				var thirdDelete = context.DeleteIndexAsync<IndexAliasDtoTestThree>();
				thirdDelete.Wait();
			}

			// delete index v2
			using (var context = new ElasticsearchContext("http://localhost:9200/", elasticsearchMappingResolverDirectIndexV2))
			{
				context.AllowDeleteForIndex = true;
				var thirdDelete = context.DeleteIndexAsync<IndexAliasDtoTestThree>();
				thirdDelete.Wait();
			}

		}
	}

	
				

	public class IndexAliasDtoTest
	{
		public long Id { get; set; }
		public string Description { get; set; }
	}

	public class IndexAliasDtoTestTwo
	{
		public long Id { get; set; }
		public string Description { get; set; }
	}

	public class IndexAliasDtoTestThree
	{
		public long Id { get; set; }
		public string Description { get; set; }
	}

	public class IndexAliasDtoTestThreeMappingV1 : ElasticsearchMapping
	{
		public override string GetIndexForType(Type type)
		{
			return "indexaliasdtotestthree_v1";
		}
	}

	public class IndexAliasDtoTestThreeMappingV2 : ElasticsearchMapping
	{
		public override string GetIndexForType(Type type)
		{
			return "indexaliasdtotestthree_v2";
		}
	}
}
