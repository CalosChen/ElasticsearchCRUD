﻿using System.Collections.Generic;
using ElasticsearchCRUD.Model.GeoModel;
using ElasticsearchCRUD.Model.SearchModel;
using ElasticsearchCRUD.Model.SearchModel.Filters;
using ElasticsearchCRUD.Model.SearchModel.Queries;
using ElasticsearchCRUD.Model.SearchModel.Sorting;
using NUnit.Framework;

namespace ElasticsearchCRUD.Integration.Test.SearchTests
{
	[TestFixture]
	public class SearchQueryFilterFilteredStructureTests : SetupSearch
	{
		[Test]
		public void SearchFilteredQueryFilterMatchAll()
		{
			var search = new Search
			{
				Query = new Query(
					new Filtered(
						new Filter(
							new MatchAllFilter { Boost = 1.1 }
						)
					)
				)
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchTest>());
				var items = context.Search<SearchTest>(search);
				Assert.AreEqual(1, items.PayloadResult.Hits.HitsResult[0].Source.Id);
			}
		}

		[Test]
		public void SearchFilteredQueryFilterMatchAllQueryMatchAll()
		{
			var search = new Search
			{
				Query = new Query(
					new Filtered( 
						new Filter( new MatchAllFilter { Boost = 1.1 } )) { Query = new Query(new MatchAllQuery())}		
				)
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchTest>());
				var items = context.Search<SearchTest>(search);
				Assert.AreEqual(1, items.PayloadResult.Hits.HitsResult[0].Source.Id);
			}
		}

		[Test]
		public void SearchQueryConstantScoreQueryWithFilter()
		{
			var search = new Search { Query = new Query(new ConstantScoreQuery(new RangeFilter("id"){GreaterThan = "1"}) { Boost = 2.0 }) };

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchTest>());
				var items = context.Search<SearchTest>(search);
				Assert.AreEqual(2, items.PayloadResult.Hits.HitsResult[0].Source.Id);
			}
		}

		[Test]
		public void SearchQueryMatchAllSortFieldIdDesc()
		{
			var search = new Search
			{
				Query = new Query(new MatchAllQuery()),
				Sort = new SortHolder(
					new List<ISort>
					{
						new SortStandard("id")
						{
							Order=OrderEnum.desc, Mode=SortMode.avg, Missing = SortMissing._last, UnmappedType="int"					
						}
					}
				) { TrackScores = true }
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchTest>());
				var items = context.Search<SearchTest>(search);
				Assert.AreEqual(3, items.PayloadResult.Hits.HitsResult[0].Source.Id);
			}
		}

		[Test]
		public void SearchQueryMatchAllGeoSortFieldAsc()
		{
			var search = new Search
			{
				Query = new Query(new MatchAllQuery()),
				Sort = new SortHolder(
					new List<ISort>
					{
						new SortGeoDistance("location", DistanceUnitEnum.km, new GeoPoint(46, 46))
						{
							
							Order=OrderEnum.asc, Mode = SortModeGeo.max				
						}
					}
				) { TrackScores = true }
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchTest>());
				var items = context.Search<SearchTest>(search);
				Assert.AreEqual(2, items.PayloadResult.Hits.HitsResult[0].Source.Id);
			}
		}

		[Test]
		public void SearchQueryMatchAllGeoSortMultipleFieldsAsc()
		{
			var search = new Search
			{
				Query = new Query(new MatchAllQuery()),
				Sort = new SortHolder(
					new List<ISort>
					{
						new SortGeoDistance("location", DistanceUnitEnum.km, 
							new List<GeoPoint>
							{
								new GeoPoint(46, 46),
								new GeoPoint(49, 46),
							})
						{
							
							Order=OrderEnum.asc, Mode = SortModeGeo.max				
						}
					}
				)
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchTest>());
				var items = context.Search<SearchTest>(search);
				Assert.AreEqual(2, items.PayloadResult.Hits.HitsResult[0].Source.Id);
			}
		}

		[Test]
		public void SearchQueryMatchAllSortScript()
		{
			var search = new Search
			{
				Query = new Query(new MatchAllQuery()),
				Sort = new SortScript("doc['lift'].value * factor")
				{
					Order = OrderEnum.asc,
					ScriptType= "number",
					Params = new List<ScriptParameter>
					{
						new ScriptParameter("factor", 1.5)
					}
				}
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchTest>());
				var items = context.Search<SearchTest>(search);
				Assert.AreEqual(3, items.PayloadResult.Hits.HitsResult[0].Source.Id);
			}
		}


		[Test]
		public void SearchQueryMatchAllSortFieldIdDescNestedFilter()
		{
			var search = new Search
			{
				Query = new Query(new MatchAllQuery()),
				Sort = new SortHolder(
					new List<ISort>
					{
						new SortStandard("id")
						{
							Order=OrderEnum.desc, Mode=SortMode.avg, Missing = SortMissing._last, UnmappedType="int", NestedFilter= new MatchAllFilter()					
						}
					}
				) { TrackScores = true }
			};

			using (var context = new ElasticsearchContext(ConnectionString, ElasticsearchMappingResolver))
			{
				Assert.IsTrue(context.IndexTypeExists<SearchTest>());
				var items = context.Search<SearchTest>(search);
				Assert.AreEqual(3, items.PayloadResult.Hits.HitsResult[0].Source.Id);
			}
		}


	}
}
