using System;
using System.Collections.Generic;
using System.Globalization;
using ElasticsearchCRUD.ContextSearch.SearchModel;
using ElasticsearchCRUD.Model.SearchModel;
using ElasticsearchCRUD.Model.SearchModel.Aggregations;
using NUnit.Framework;

namespace ElasticsearchCRUD.Integration.Test.AggregationTests
{
	[TestFixture]
	public class SearchAggStructureTests : SetupSearchAgg
	{
		[Test]
		public void SearchAggMinAggregationWithHits()
		{
			var search = new Search { Aggs= new MinAggregation("test_min", "lift")};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search);
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_min");
				Assert.AreEqual("2.1", Math.Round(aggResult, 2).ToString(CultureInfo.InvariantCulture));
			}
		}

		[Test]
		public void SearchAggMinAggregationNoHits()
		{
			var search = new Search { Aggs = new MinAggregation("test_min", "lift") };

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search, new SearchUrlParameters{SeachType= SeachType.count});
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_min");
				Assert.AreEqual(2.1, aggResult);
			}
		}

		[Test]
		public void SearchAggMinAggregationScriptNoHits()
		{
			var search = new Search
			{
				Aggs = new MinAggregation("test_min", "lift")
				{
					Script = "_value * times * constant",
					Params = new List<ScriptParameter>
					{
						new ScriptParameter("times", 1.4),
						new ScriptParameter("constant", 10.2)
					}
				}
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search, new SearchUrlParameters { SeachType = SeachType.count });
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_min");
				Assert.AreEqual(29, (int)aggResult);
			}
		}

		[Test]
		public void SearchAggMaxAggregationNoHits()
		{
			var search = new Search { Aggs = new MaxAggregation("test_max", "lift") };

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search, new SearchUrlParameters { SeachType = SeachType.count });
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_max");
				Assert.AreEqual("2.9", Math.Round(aggResult, 2).ToString(CultureInfo.InvariantCulture));
			}
		}

		[Test]
		public void SearchAggMaxAggregationScriptNoHits()
		{
			var search = new Search
			{
				Aggs = new MaxAggregation("test_max", "lift")
				{
					Script = "_value * times * constant",
					Params = new List<ScriptParameter>
					{
						new ScriptParameter("times", 1.4),
						new ScriptParameter("constant", 10.2)
					}
				}
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search, new SearchUrlParameters { SeachType = SeachType.count });
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_max");
				Assert.AreEqual(41, (int)aggResult);
			}
		}

		[Test]
		public void SearchAggSumAggregationNoHits()
		{
			var search = new Search { Aggs = new SumAggregation("test_sum", "lift") };

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search, new SearchUrlParameters { SeachType = SeachType.count });
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_sum");
				Assert.AreEqual("7.5", Math.Round(aggResult, 2).ToString(CultureInfo.InvariantCulture));
			}
		}

		[Test]
		public void SearchAggSumAggregationScriptNoHits()
		{
			var search = new Search
			{
				Aggs = new SumAggregation("test_sum", "lift")
				{
					Script = "_value * times * constant",
					Params = new List<ScriptParameter>
					{
						new ScriptParameter("times", 1.4),
						new ScriptParameter("constant", 10.2)
					}
				}
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search, new SearchUrlParameters { SeachType = SeachType.count });
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_sum");
				Assert.AreEqual(107, (int)aggResult);
			}
		}

		[Test]
		public void SearchAggAvgAggregationNoHits()
		{
			var search = new Search { Aggs = new AvgAggregation("test_avg", "lift") };

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search, new SearchUrlParameters { SeachType = SeachType.count });
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_avg");
				Assert.AreEqual("2.5", Math.Round(aggResult, 2).ToString(CultureInfo.InvariantCulture));
			}
		}

		[Test]
		public void SearchAggAvgAggregationScriptNoHits()
		{
			var search = new Search
			{
				Aggs = new AvgAggregation("test_avg", "lift")
				{
					Script = "_value * times * constant",
					Params = new List<ScriptParameter>
					{
						new ScriptParameter("times", 1.4),
						new ScriptParameter("constant", 10.2)
					}
				}
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search, new SearchUrlParameters { SeachType = SeachType.count });
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_avg");
				Assert.AreEqual(35, (int)aggResult);
			}
		}

		[Test]
		public void SearchAggStatsAggregationNoHits()
		{
			var search = new Search { Aggs = new StatsAggregation("test_StatsAggregation", "lift") };

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search, new SearchUrlParameters { SeachType = SeachType.count });
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_StatsAggregation");
				Assert.AreEqual("2.5", Math.Round(aggResult, 2).ToString(CultureInfo.InvariantCulture));
			}
		}

		[Test]
		public void SearchAggStatsAggregationScriptNoHits()
		{
			var search = new Search
			{
				Aggs = new StatsAggregation("test_StatsAggregation", "lift")
				{
					Script = "_value * times * constant",
					Params = new List<ScriptParameter>
					{
						new ScriptParameter("times", 1.4),
						new ScriptParameter("constant", 10.2)
					}
				}
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchAggTest>());
				var items = context.Search<SearchAggTest>(search, new SearchUrlParameters { SeachType = SeachType.count });
				var aggResult = items.PayloadResult.Aggregations.GetSingleValueMetric<double>("test_StatsAggregation");
				Assert.AreEqual(35, (int)aggResult);
			}
		}
	}
}