﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ElasticsearchCRUD.ContextSearch;
using ElasticsearchCRUD.Model;
using ElasticsearchCRUD.Model.Units;
using ElasticsearchCRUD.Tracing;

namespace ElasticsearchCRUD.Integration.Test
{
    using Xunit;

    public class SearchCreateScanAndScrollTests : IDisposable
    {
        public SearchCreateScanAndScrollTests()
        {
            SetUp();
        }

        public void Dispose()
        {
            TearDown();
        }

        private List<ScanScrollTypeV1> _entitiesForTests;
		private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver = new ElasticsearchMappingResolver();
		private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
		private const string ConnectionString = "http://localhost:9200";

		private void WaitForDataOrFail()
		{
			if (!_resetEvent.WaitOne(7000))
			{
				Assert.Fail("No data received within specified time");
			}
		}

		public void SetUp()
		{
			_entitiesForTests = new List<ScanScrollTypeV1>();
			// Create a 100 entities
			for (int i = 0; i < 10000; i++)
			{
				var entity = new ScanScrollTypeV1
				{
					Created = DateTime.UtcNow,
					Updated = DateTime.UtcNow,
					Description = "A test entity description",
					Id = i,
					Name = "cool"
				};

				_entitiesForTests.Add(entity);
			}
		}

		public void TearDown()
		{
			_entitiesForTests = null;

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.AllowDeleteForIndex = true;
				var entityResult = context.DeleteIndexAsync<ScanScrollTypeV1>();
				entityResult.Wait();

				var entityResultV2 = context.DeleteIndexAsync<ScanScrollTypeV2>();
				entityResultV2.Wait();
			}
		}

		 [Fact]
		public void TestScanAndScollReindexFor1000Entities()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				//context.TraceProvider = new ConsoleTraceProvider();
				for (int i = 0; i < 1000; i++)
				{
					context.AddUpdateDocument(_entitiesForTests[i], i);
				}

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);
				context.IndexClearCache<ScanScrollTypeV1>();
			}

			Task.Run(() =>
			{
				using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
				{
					while (true)
					{
						Thread.Sleep(1300);
						var itemOk = context.SearchById<ScanScrollTypeV1>(2);
						if (itemOk != null)
						{
							_resetEvent.Set();
						}
					}
				}
				// ReSharper disable once FunctionNeverReturns
			});

			WaitForDataOrFail();

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				//context.TraceProvider = new ConsoleTraceProvider();
				var scanScrollConfig = new ScanAndScrollConfiguration(new TimeUnitSecond(1), 100);
				var result = context.SearchCreateScanAndScroll<ScanScrollTypeV1>(BuildSearchMatchAll(), scanScrollConfig);

				var scrollId = result.PayloadResult.ScrollId;

				int processedResults = 0;
				while (result.PayloadResult.Hits.Total > processedResults)
				{
					var resultCollection = context.SearchScanAndScroll<ScanScrollTypeV1>(scrollId, scanScrollConfig);
					scrollId = resultCollection.PayloadResult.ScrollId;

					foreach (var item in resultCollection.PayloadResult.Hits.HitsResult)
					{
						processedResults++;
						context.AddUpdateDocument(ConvertScanScrollTypeV1(item.Source), item.Id);
					}
					context.SaveChanges();				
				}
			}

			Task.Run(() =>
			{
				using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
				{
					while (true)
					{
						Thread.Sleep(300);
						if (10000 == context.Count<ScanScrollTypeV2>())
						{
							_resetEvent.Set();
						}
					}
				}
// ReSharper disable once FunctionNeverReturns
			});

			WaitForDataOrFail();
		}

		private ScanScrollTypeV2 ConvertScanScrollTypeV1(ScanScrollTypeV1 item)
		{
			return new ScanScrollTypeV2
			{
				Created = item.Created,
				Updated = item.Updated,
				Description = item.Description,
				Id = item.Id,
				Name = item.Name,
				Vorname = "Yes we are reindexing"
			};
		}

		//{
		//	"query" : {
		//		"match_all" : {}
		//	}
		//}
		private string BuildSearchMatchAll()
		{
			var buildJson = new StringBuilder();
			buildJson.AppendLine("{");
			buildJson.AppendLine("\"query\": {");
			buildJson.AppendLine("\"match_all\" : {}");
			buildJson.AppendLine("}");
			buildJson.AppendLine("}");

			return buildJson.ToString();
		}
    }

	public class ScanScrollTypeV1
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public DateTimeOffset Created { get; set; }
		public DateTimeOffset Updated { get; set; }
	}

	public class ScanScrollTypeV2
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Vorname { get; set; }
		public string Description { get; set; }
		public DateTimeOffset Created { get; set; }
		public DateTimeOffset Updated { get; set; }
	}
}
