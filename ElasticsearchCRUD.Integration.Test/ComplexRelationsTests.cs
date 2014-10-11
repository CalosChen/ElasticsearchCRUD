﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace ElasticsearchCRUD.Integration.Test
{
	[TestFixture]
	public class ComplexRelationsTests
	{
		private readonly IElasticSearchMappingResolver _elasticSearchMappingResolver = new ElasticSearchMappingResolver();

		[Test]
		public void Test1_to_n_1_m_Array()
		{
			using (var context = new ElasticSearchContext("http://localhost:9200/", _elasticSearchMappingResolver))
			{
				context.SaveChanges();
			}
		}

		[Test]
		public void Test1_to_n_1_m_Collection()
		{
			using (var context = new ElasticSearchContext("http://localhost:9200/", _elasticSearchMappingResolver))
			{
				context.SaveChanges();
			}
		}

		[Test]
		public void Test1_to_n_1_m_HashSet()
		{
			var data = new TestNestedDocumentLevelOneHashSet
			{
				DescriptionOne = "D1",
				Id = 1,
				TestNestedDocumentLevelTwoHashSet = new HashSet<TestNestedDocumentLevelTwoHashSet>
					{
						new TestNestedDocumentLevelTwoHashSet
						{
							DescriptionTwo = "D2", 
							Id=1,
							TestNestedDocumentLevelOneHashSet = new TestNestedDocumentLevelOneHashSet{
								Id=1,
								DescriptionOne="D1", 
								TestNestedDocumentLevelTwoHashSet = new HashSet<TestNestedDocumentLevelTwoHashSet>
								{
									new TestNestedDocumentLevelTwoHashSet
									{
										DescriptionTwo="D1", 
										Id=1
									}
								}
							},
							TestNestedDocumentLevelThreeHashSet = new TestNestedDocumentLevelThreeHashSet
							{
								DescriptionThree = "D3", 
								Id=1, 
								TestNestedDocumentLevelFour = new HashSet<TestNestedDocumentLevelFourHashSet>
								{
									new TestNestedDocumentLevelFourHashSet
									{
										DescriptionFour="D4", Id=1, 
										TestNestedDocumentLevelThreeHashSet = new TestNestedDocumentLevelThreeHashSet
										{
											DescriptionThree="D3"
										}
									},
									new TestNestedDocumentLevelFourHashSet
									{
										DescriptionFour="D4", Id=2, 
										TestNestedDocumentLevelThreeHashSet = new TestNestedDocumentLevelThreeHashSet
										{
											DescriptionThree="D3", 
											TestNestedDocumentLevelFour = new HashSet<TestNestedDocumentLevelFourHashSet>
											{
												new TestNestedDocumentLevelFourHashSet{DescriptionFour="D4",
													Id=2
												}
											}
										}
									}
								}
							}
						}
					}
			};

			using (var context = new ElasticSearchContext("http://localhost:9200/", _elasticSearchMappingResolver))
			{
				context.AddUpdateEntity(data,data.Id);
				context.SaveChanges();
			}

			using (var context = new ElasticSearchContext("http://localhost:9200/", _elasticSearchMappingResolver))
			{
				var roundTripData = context.GetEntity<TestNestedDocumentLevelOneHashSet>(data.Id);
				Assert.AreEqual(data.DescriptionOne, roundTripData.DescriptionOne);
				Assert.AreEqual(
					data.TestNestedDocumentLevelTwoHashSet.First().DescriptionTwo, 
					roundTripData.TestNestedDocumentLevelTwoHashSet.First().DescriptionTwo
					);
				Assert.AreEqual(
					data.TestNestedDocumentLevelTwoHashSet.First().TestNestedDocumentLevelThreeHashSet.DescriptionThree,
					roundTripData.TestNestedDocumentLevelTwoHashSet.First().TestNestedDocumentLevelThreeHashSet.DescriptionThree
					);
				Assert.AreEqual(
					data.TestNestedDocumentLevelTwoHashSet.First().TestNestedDocumentLevelThreeHashSet.TestNestedDocumentLevelFour.First().DescriptionFour,
					roundTripData.TestNestedDocumentLevelTwoHashSet.First().TestNestedDocumentLevelThreeHashSet.TestNestedDocumentLevelFour.First().DescriptionFour
					);
			}
		}

		[Test]
		public void TestDefaultContextParentWith2LevelsOfNestedObjects()
		{
			var testSkillParentObject = new SkillDocument
			{
				Id = 7,
				NameSkillParent = "cool",
				SkillNestedDocumentLevelOne = new Collection<SkillNestedDocumentLevelOne>()
				{
					new SkillNestedDocumentLevelOne()
					{
						Id=1,
						NameSkillParent="rr", 
						SkillNestedDocumentLevelTwo= new Collection<SkillNestedDocumentLevelTwo>
						{
							new SkillNestedDocumentLevelTwo
							{
								Id=3, 
								NameSkillParent="eee"
							}
						}
					}
				}
			};

			using (var context = new ElasticSearchContext("http://localhost:9200/", _elasticSearchMappingResolver))
			{

				context.AddUpdateEntity(testSkillParentObject, testSkillParentObject.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.AreEqual(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetEntity<SkillDocument>(testSkillParentObject.Id);
				Assert.AreEqual(roundTripResult.NameSkillParent, testSkillParentObject.NameSkillParent);
				Assert.AreEqual(roundTripResult.SkillNestedDocumentLevelOne.First().NameSkillParent, testSkillParentObject.SkillNestedDocumentLevelOne.First().NameSkillParent);
				Assert.AreEqual(
					roundTripResult.SkillNestedDocumentLevelOne.First().SkillNestedDocumentLevelTwo.First().NameSkillParent,
					testSkillParentObject.SkillNestedDocumentLevelOne.First().SkillNestedDocumentLevelTwo.First().NameSkillParent);
			}
		}

	}

	public class TestNestedDocumentLevelOneHashSet
	{
		public long Id { get; set; }
		public string DescriptionOne { get; set; }

		public virtual HashSet<TestNestedDocumentLevelTwoHashSet> TestNestedDocumentLevelTwoHashSet { get; set; }
	}

	public class TestNestedDocumentLevelTwoHashSet
	{
		public long Id { get; set; }
		public string DescriptionTwo { get; set; }

		public virtual TestNestedDocumentLevelOneHashSet TestNestedDocumentLevelOneHashSet { get; set; }
		public virtual TestNestedDocumentLevelThreeHashSet TestNestedDocumentLevelThreeHashSet { get; set; }
	}

	public class TestNestedDocumentLevelThreeHashSet
	{
		public long Id { get; set; }
		public string DescriptionThree { get; set; }

		public virtual HashSet<TestNestedDocumentLevelTwoHashSet> TestNestedDocumentLevelTwo { get; set; }

		public virtual HashSet<TestNestedDocumentLevelFourHashSet> TestNestedDocumentLevelFour { get; set; }
	}

	public class TestNestedDocumentLevelFourHashSet
	{
		public long Id { get; set; }
		public string DescriptionFour { get; set; }

		public virtual TestNestedDocumentLevelThreeHashSet TestNestedDocumentLevelThreeHashSet { get; set; }
	}
}
