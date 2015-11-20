﻿using System.Collections.Generic;
using System.Threading;
using ElasticsearchCRUD.ContextSearch.SearchModel.AggModel;
using ElasticsearchCRUD.Model.SearchModel;
using ElasticsearchCRUD.Model.SearchModel.Aggregations;
using ElasticsearchCRUD.Model.SearchModel.Filters;
using ElasticsearchCRUD.Model.SearchModel.Queries;
using ElasticsearchCRUD.Utils;

namespace ElasticsearchCRUD.Integration.Test
{
    using System;

    using Xunit;

    public class GlobalApiTestsSearchCountTests : IDisposable
    {
        private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver = new ElasticsearchMappingResolver();
        private const string ConnectionString = "http://localhost:9200";

        [Fact]
        public void GlobalElasticsearchMappingCountTest()
        {
            // You require this for a global search, it is in the setup fixture
            //_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                var count = context.Count<object>();
                Assert.GreaterOrEqual(count, 3);
            }
        }

        [Fact]
        public void GlobalElasticsearchMappingSearchAllTest()
        {
            // You require this for a global search, it is in the setup fixture
            //_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                var results = context.Search<object>(new Search());
                Assert.GreaterOrEqual(results.PayloadResult.Hits.Total, 3);
            }
        }

        [Fact]
        public void GlobalElasticsearchMappingSearchFilterIndexOneTest()
        {
            // You require this for a global search, it is in the setup fixture
            //_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                var results = context.Search<object>(
                    new Search
                    {
                        Filter= new Filter(
                            new IndicesFilter(
                                new List<string>
                                {
                                    "indexones"
                                }, 
                                new MatchAllFilter()
                            )
                            {
                                NoMatchFilterNone=true
                            }
                        )
                    });
                Assert.Equal(2, results.PayloadResult.Hits.Total);
            }
        }

        [Fact]
        public void GlobalElasticsearchMappingSearchFilterIndexOneIndexTwoTest()
        {
            // You require this for a global search, it is in the setup fixture
            //_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                var results = context.Search<object>(
                    new Search
                    {
                        Filter = new Filter(
                            new IndicesFilter(
                                new List<string>
                                {
                                    "indexones", "indextwos"
                                },
                                new MatchAllFilter()
                            )
                            {
                                NoMatchFilterNone = true
                            }
                        )
                    });
                Assert.Equal(3, results.PayloadResult.Hits.Total);
            }
        }

        [Fact]
        public void GlobalElasticsearchMappingSearchFilterIndexOneGetTypeTest()
        {
            // You require this for a global search, it is in the setup fixture
            //_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                var results = context.Search<object>(
                    new Search
                    {
                        Filter = new Filter(
                            new IndicesFilter(
                                new List<string>
                                {
                                    "indexones"
                                },
                                new MatchAllFilter()
                            )
                            {
                                NoMatchFilterNone = true
                            }
                        )
                    });
                Assert.Equal(2, results.PayloadResult.Hits.Total);
                var indexOneItem = results.PayloadResult.Hits.HitsResult[0].GetSourceFromJToken<IndexOne>();
                Assert.Equal(49.3, indexOneItem.RandomNumber);

            }
        }

        [Fact]
        public void GlobalElasticsearchMappingSearchFilterIndexOneTypeFilterTest()
        {
            // You require this for a global search, it is in the setup fixture
            //_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                var results = context.Search<object>(
                    new Search
                    {
                        Filter = new Filter(
                            new TypeFilter("indexone")
                        )
                    });
                Assert.Equal(2, results.PayloadResult.Hits.Total);
            }
        }

        [Fact]
        public void GlobalElasticsearchMappingSearchFilterIndexTwoTypeFilterTest()
        {
            // You require this for a global search, it is in the setup fixture
            //_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                var results = context.Search<object>(
                    new Search
                    {
                        Filter = new Filter(
                            new TypeFilter("indextwo")
                        )
                    });
                Assert.Equal(1, results.PayloadResult.Hits.Total);
            }
        }

        [Fact]
        public void GlobalElasticsearchMappingSearchQueryIndexOneTest()
        {
            // You require this for a global search, it is in the setup fixture
            //_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                var results = context.Search<object>(
                    new Search
                    {
                        Query = new Query(
                            new IndicesQuery(
                                new List<string>
                                {
                                    "indexones"
                                },
                                new MatchAllQuery()
                            )
                            {
                                NoMatchFilterNone = true
                            }
                        )
                    });
                Assert.Equal(2, results.PayloadResult.Hits.Total);
            }
        }

        [Fact]
        public void GlobalElasticsearchMappingSearchAllAggTest()
        {
            // You require this for a global search, it is in the setup fixture
            //_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                var results = context.Search<object>(
                    new Search
                    {
                        Aggs = new List<IAggs>
                        {
                            new StatsMetricAggregation("stats", "randomnumber")
                        }
                    });
                var stats = results.PayloadResult.Aggregations.GetComplexValue<StatsMetricAggregationsResult>("stats");
                Assert.GreaterOrEqual(2, stats.Count);
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                context.AllowDeleteForIndex = true;
                if (context.IndexExists<IndexOne>())
                {
                    context.DeleteIndex<IndexOne>();
                }
                if (context.IndexExists<IndexTwo>())
                {
                    var entityResult2 = context.DeleteIndexAsync<IndexTwo>();
                    entityResult2.Wait();
                }
            }
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            _elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
            using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
            {
                var doc1 = new IndexOne
                {
                    Id = 1,
                    RandomNumber = 49.3
                };
                var doc2 = new IndexOne
                {
                    Id = 2,
                    RandomNumber = 49.3
                };
                var doc3 = new IndexTwo
                {
                    Id = 1,
                    Description = "nice day"
                };

                context.IndexCreate<IndexOne>();
                context.IndexCreate<IndexTwo>();
                Thread.Sleep(1200);
                context.AddUpdateDocument(doc1, doc1.Id);
                context.AddUpdateDocument(doc2, doc2.Id);
                context.AddUpdateDocument(doc3, doc3.Id);
                context.SaveChanges();
                Thread.Sleep(1200);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class IndexOne
    {
        public long Id { get; set; }
        public double RandomNumber { get; set; }
    }

    public class IndexTwo
    {
        public long Id { get; set; }
        public string Description { get; set; }
    }
}
