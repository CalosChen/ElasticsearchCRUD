﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using ElasticsearchCRUD.Tracing;
using Xunit;

namespace ElasticsearchCRUD.Integration.Test.OneToN
{
	public class OneToNNestedElasticsearchCrudTests : IDisposable
	{
	    public OneToNNestedElasticsearchCrudTests()
	    {
	        SetUp();
	    }

        public void Dispose()
        {
            TearDown();
        }
        private List<SkillChild> _entitiesForSkillChild;
		private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver = new ElasticsearchMappingResolver();
		private const string ConnectionString = "http://localhost:9200";

		public void SetUp()
		{
			_entitiesForSkillChild = new List<SkillChild>();

			for (int i = 0; i < 3; i++)
			{
				var entityTwo = new SkillChild
				{
					CreatedSkillChild = DateTime.UtcNow,
					UpdatedSkillChild = DateTime.UtcNow,
					DescriptionSkillChild = "A test SkillChild description",
					Id = i,
					NameSkillChild = "cool"
				};

				_entitiesForSkillChild.Add(entityTwo);
			}
		}

		public void TearDown()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.AllowDeleteForIndex = true;
				var entityResult1 = context.DeleteIndexAsync<SkillTestEntityTwo>();entityResult1.Wait();
				var entityResult2 = context.DeleteIndexAsync<SkillParentCollection>(); entityResult2.Wait();
				var entityResult3 = context.DeleteIndexAsync<SkillParentArray>(); entityResult3.Wait();
				var entityResult4 = context.DeleteIndexAsync<SkillChild>(); entityResult4.Wait();
				var entityResult5 = context.DeleteIndexAsync<SkillTestEntity>(); entityResult5.Wait();
				var entityResult6 = context.DeleteIndexAsync<SkillWithIntArray>(); entityResult6.Wait();
				var entityResult7 = context.DeleteIndexAsync<SkillWithIntCollection>(); entityResult7.Wait();
				var entityResult8 = context.DeleteIndexAsync<SkillWithSingleChild>(); entityResult8.Wait();
				var entityResult9 = context.DeleteIndexAsync<SkillSingleChildElement>(); entityResult9.Wait();
				var entityResult10 = context.DeleteIndexAsync<SkillWithIntCollection>(); entityResult10.Wait();
				var entityResult11 = context.DeleteIndexAsync<SkillWithStringLongAndDoubleArray>(); entityResult11.Wait();
				var entityResult12 = context.DeleteIndexAsync<SkillWithStringLongAndDoubleCollection>(); entityResult12.Wait();
				var entityResult13 = context.DeleteIndexAsync<SkillDocumentHastSet>(); entityResult13.Wait();		
				
			}
		}

		#region NESTED COLLECTION 1 to N

		[Fact]
		public void TestDefaultContextParentWithACollectionOfOneChildObjectNested()
		{
			var testSkillParentObject = new SkillParentCollection
			{
				CreatedSkillParent = DateTime.UtcNow,
				UpdatedSkillParent = DateTime.UtcNow,
				DescriptionSkillParent = "A test entity description",
				Id = 7,
				NameSkillParent = "cool",
				SkillChildren = new Collection<SkillChild> {_entitiesForSkillChild[0]}
			};

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(testSkillParentObject, testSkillParentObject.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillParentCollection>(testSkillParentObject.Id);
				Assert.Equal(roundTripResult.DescriptionSkillParent, testSkillParentObject.DescriptionSkillParent);
				Assert.Equal(roundTripResult.SkillChildren.First().DescriptionSkillChild, testSkillParentObject.SkillChildren.First().DescriptionSkillChild);
			}
		}

		[Fact]
		public void TestDefaultContextParentWithACollectionOfOneChildObjectNestedIgnoreChildren()
		{
			var testSkillParentObject = new SkillParentCollection
			{
				CreatedSkillParent = DateTime.UtcNow,
				UpdatedSkillParent = DateTime.UtcNow,
				DescriptionSkillParent = "A test entity description",
				Id = 7,
				NameSkillParent = "cool",
				SkillChildren = new Collection<SkillChild> { _entitiesForSkillChild[0] }
			};

			const bool includeChildObjects = false;
			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver, includeChildObjects)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(testSkillParentObject, testSkillParentObject.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillParentCollection>(testSkillParentObject.Id);
				Assert.Equal(roundTripResult.DescriptionSkillParent, testSkillParentObject.DescriptionSkillParent);
				Assert.Equal(roundTripResult.SkillChildren, null);
			}
		}

		[Fact]
		public void TestDefaultContextParentWithACollectionOfThreeChildObjectsNested()
		{
			var testSkillParentObject = new SkillParentCollection
			{
				CreatedSkillParent = DateTime.UtcNow,
				UpdatedSkillParent = DateTime.UtcNow,
				DescriptionSkillParent = "A test entity description",
				Id = 8,
				NameSkillParent = "cool",
				SkillChildren = new Collection<SkillChild> { _entitiesForSkillChild[0], _entitiesForSkillChild[1], _entitiesForSkillChild[2] }
			};

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(testSkillParentObject, testSkillParentObject.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillParentCollection>(testSkillParentObject.Id);
				Assert.Equal(roundTripResult.DescriptionSkillParent, testSkillParentObject.DescriptionSkillParent);
				Assert.Equal(roundTripResult.SkillChildren.First().DescriptionSkillChild, testSkillParentObject.SkillChildren.First().DescriptionSkillChild);
				Assert.Equal(roundTripResult.SkillChildren.ToList()[1].DescriptionSkillChild, testSkillParentObject.SkillChildren.ToList()[1].DescriptionSkillChild);
				Assert.Equal(roundTripResult.SkillChildren.ToList()[1].DescriptionSkillChild, testSkillParentObject.SkillChildren.ToList()[1].DescriptionSkillChild);
			}
		}

		[Fact]
		public void TestDefaultContextParentWithACollectionOfNoChildObjectNested()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skill = new SkillParentCollection
				{
					CreatedSkillParent = DateTime.UtcNow,
					UpdatedSkillParent = DateTime.UtcNow,
					Id = 34,
					NameSkillParent = "rr",
					DescriptionSkillParent = "ee",
					SkillChildren = new Collection<SkillChild>()
				};
				context.AddUpdateDocument(skill, skill.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillParentCollection>(skill.Id);
				Assert.Equal(roundTripResult.DescriptionSkillParent, skill.DescriptionSkillParent);
			}
		}

		[Fact]
		public void TestDefaultContextParentWithNullCollectionNested()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skill = new SkillParentCollection
				{
					CreatedSkillParent = DateTime.UtcNow,
					UpdatedSkillParent = DateTime.UtcNow,
					Id = 34,
					NameSkillParent = "rr",
					DescriptionSkillParent = "ee"
				};
				context.AddUpdateDocument(skill, skill.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillParentCollection>(skill.Id);
				Assert.Equal(roundTripResult.DescriptionSkillParent, skill.DescriptionSkillParent);
			}
		}

		
		#endregion //NESTED COLLECTION 1 to N

		#region NESTED HashSet 1 to N
		//SkillDocumentHastSet

		[Fact]
		public void TestDefaultContextParentWithAHashSetOfOneChildObjectNested()
		{
			var testSkillParentObject = new SkillDocumentHastSet
			{
				Id = 1,
				NameSkillParent = "cool",
				SkillNestedDocumentLevelTwoHashSet = new HashSet<SkillNestedDocumentLevelTwo>{ new SkillNestedDocumentLevelTwo{ Id = 1, NameSkillParent = "TestHashSet"} }
			};

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(testSkillParentObject, testSkillParentObject.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillDocumentHastSet>(testSkillParentObject.Id);
				Assert.Equal(roundTripResult.NameSkillParent, testSkillParentObject.NameSkillParent);
				Assert.Equal(roundTripResult.SkillNestedDocumentLevelTwoHashSet.First().NameSkillParent, testSkillParentObject.SkillNestedDocumentLevelTwoHashSet.First().NameSkillParent);
			}
		}

		[Fact]
		public void TestDefaultContextParentWithAHashSetOfTwoChildObjectNested()
		{
			var testSkillParentObject = new SkillDocumentHastSet
			{
				Id = 1,
				NameSkillParent = "cool",
				SkillNestedDocumentLevelTwoHashSet = new HashSet<SkillNestedDocumentLevelTwo> { new SkillNestedDocumentLevelTwo { Id = 1, NameSkillParent = "TestHashSet" }, new SkillNestedDocumentLevelTwo { Id = 2, NameSkillParent = "TestHashSet2" } }
			};

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(testSkillParentObject, testSkillParentObject.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillDocumentHastSet>(testSkillParentObject.Id);
				Assert.Equal(roundTripResult.NameSkillParent, testSkillParentObject.NameSkillParent);
				Assert.Equal(roundTripResult.SkillNestedDocumentLevelTwoHashSet.First().NameSkillParent, testSkillParentObject.SkillNestedDocumentLevelTwoHashSet.First().NameSkillParent);
				Assert.Equal(roundTripResult.SkillNestedDocumentLevelTwoHashSet.Single(t => t.Id == 2).NameSkillParent, testSkillParentObject.SkillNestedDocumentLevelTwoHashSet.Single(t => t.Id == 2).NameSkillParent);
			}
		}

		[Fact]
		public void TestDefaultContextParentWithAHashSetOfNoChildObjectNested()
		{
			var testSkillParentObject = new SkillDocumentHastSet
			{
				Id = 1,
				NameSkillParent = "cool",
				SkillNestedDocumentLevelTwoHashSet = new HashSet<SkillNestedDocumentLevelTwo>()
			};

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(testSkillParentObject, testSkillParentObject.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillDocumentHastSet>(testSkillParentObject.Id);
				Assert.Equal(roundTripResult.NameSkillParent, testSkillParentObject.NameSkillParent);
			}
		}

		[Fact]
		public void TestDefaultContextParentWithANullHashSetOfChildObjectNested()
		{
			var testSkillParentObject = new SkillDocumentHastSet
			{
				Id = 1,
				NameSkillParent = "cool",
			};

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(testSkillParentObject, testSkillParentObject.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillDocumentHastSet>(testSkillParentObject.Id);
				Assert.Equal(roundTripResult.NameSkillParent, testSkillParentObject.NameSkillParent);
			}
		}

		#endregion NESTED HashSet 1 to N

		#region NESTED Array 1 to N

		[Fact]
		public void TestDefaultContextParentWithAnArrayOfOneChildObjectNested()
		{
			var testSkillParentObject = new SkillParentArray
			{

				CreatedSkillParent = DateTime.UtcNow,
				UpdatedSkillParent = DateTime.UtcNow,
				DescriptionSkillParent = "A test entity description",
				Id = 7,
				NameSkillParent = "cool",
				SkillChildren = new[] { _entitiesForSkillChild[0] }
			};

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(testSkillParentObject, testSkillParentObject.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillParentArray>(testSkillParentObject.Id);
				Assert.Equal(roundTripResult.DescriptionSkillParent, testSkillParentObject.DescriptionSkillParent);
				Assert.Equal(roundTripResult.SkillChildren[0].DescriptionSkillChild, testSkillParentObject.SkillChildren[0].DescriptionSkillChild);
			}
		}

		[Fact]
		public void TestDefaultContextParentWithAnArrayOfThreeChildObjectsNested()
		{
			var testSkillParentObject = new SkillParentArray
			{

				CreatedSkillParent = DateTime.UtcNow,
				UpdatedSkillParent = DateTime.UtcNow,
				DescriptionSkillParent = "A test entity description",
				Id = 7,
				NameSkillParent = "cool",
				SkillChildren = new[] { _entitiesForSkillChild[0], _entitiesForSkillChild[1], _entitiesForSkillChild[2] }
			};

			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(testSkillParentObject, testSkillParentObject.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillParentArray>(testSkillParentObject.Id);
				Assert.Equal(roundTripResult.DescriptionSkillParent, testSkillParentObject.DescriptionSkillParent);
				Assert.Equal(roundTripResult.SkillChildren[0].DescriptionSkillChild, testSkillParentObject.SkillChildren[0].DescriptionSkillChild);
				Assert.Equal(roundTripResult.SkillChildren[1].DescriptionSkillChild, testSkillParentObject.SkillChildren[1].DescriptionSkillChild);
				Assert.Equal(roundTripResult.SkillChildren[2].DescriptionSkillChild, testSkillParentObject.SkillChildren[2].DescriptionSkillChild);
			}
		}

		// Note, Object arrays with null values are not supported
		[Fact]
		public void TestDefaultContextParentWithNullArrayNested()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skill = new SkillParentArray
				{
					CreatedSkillParent = DateTime.UtcNow,
					UpdatedSkillParent = DateTime.UtcNow,
					Id = 34,
					NameSkillParent = "rr",
					DescriptionSkillParent = "ee"
				};
				context.AddUpdateDocument(skill, skill.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillParentArray>(skill.Id);
				Assert.Equal(roundTripResult.DescriptionSkillParent, skill.DescriptionSkillParent);
			}
		}

		#endregion //NESTED Array 1 to N

		#region NESTED Simple collections

		[Fact]
		public void TestDefaultContextParentNestedIntCollection()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skillWithIntArray = new SkillWithIntCollection
				{
					MyIntArray = new List<int> {2, 4, 6, 99, 7},
					BlahBlah = "test3 with int array",
					Id = 2
				};
				context.AddUpdateDocument(skillWithIntArray, skillWithIntArray.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var returned = context.GetDocument<SkillWithIntCollection>(2);
				Assert.Equal(skillWithIntArray.MyIntArray[2], returned.MyIntArray[2]);
			}
		}

		[Fact]
		public void TestDefaultContextParentNestedIntCollectionEqualsNull()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skillWithIntArray = new SkillWithIntCollection { BlahBlah = "test with no int array", Id = 3 };

				context.AddUpdateDocument(skillWithIntArray, skillWithIntArray.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);


				var returned = context.GetDocument<SkillWithIntCollection>(3);
				Assert.Equal(skillWithIntArray.BlahBlah, returned.BlahBlah);
				Assert.Null(returned.MyIntArray);
			}
		}

		[Fact]
		public void TestDefaultContextParentNestedSkillWithStringAndLongCollection()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skillWithIntArray = new SkillWithStringLongAndDoubleCollection
				{
					MyStringArray = new List<String>
					{
						"one", "two","three"
					},
					BlahBlah = "test3 with int array",
					Id = 2,
					MyDoubleArray = new List<double> { 2.4, 5.7, 67.345 },
					MyLongArray = new List<long> { 34444445, 65432, 7889999 }
				};
				context.AddUpdateDocument(skillWithIntArray, skillWithIntArray.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var returned = context.GetDocument<SkillWithStringLongAndDoubleCollection>(2);
				Assert.Equal(skillWithIntArray.MyStringArray[2], returned.MyStringArray[2]);
				Assert.Equal(skillWithIntArray.MyDoubleArray[1], returned.MyDoubleArray[1]);
				Assert.Equal(skillWithIntArray.MyLongArray[1], returned.MyLongArray[1]);
			}
		}


		#endregion NESTED Simple collections

		#region NESTED Simple Arrays

		[Fact]
		public void TestDefaultContextParentNestedIntArray()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skillWithIntArray = new SkillWithIntArray
				{
					MyIntArray = new[] { 2, 4, 6, 99, 7 },
					BlahBlah = "test3 with int array",
					Id = 2
				};
				context.AddUpdateDocument(skillWithIntArray, skillWithIntArray.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var returned = context.GetDocument<SkillWithIntArray>(2);
				Assert.Equal(skillWithIntArray.MyIntArray[2], returned.MyIntArray[2]);
			}
		}
	
		[Fact]
		public void TestDefaultContextParentNestedIntArrayEqualsNull()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skillWithIntArray = new SkillWithIntArray { BlahBlah = "test with no int array", Id = 3 };
				context.AddUpdateDocument(skillWithIntArray, skillWithIntArray.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var returned = context.GetDocument<SkillWithIntArray>(3);
				Assert.Equal(skillWithIntArray.BlahBlah, returned.BlahBlah);
				Assert.Null(returned.MyIntArray);
			}
		}

		[Fact]
		public void TestDefaultContextParentNestedSkillWithStringAndLongArray()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skillWithIntArray = new SkillWithStringLongAndDoubleArray
				{
					MyStringArray = new[]
					{
						"one", "two","three"
					},
					BlahBlah = "test3 with int array",
					Id = 2,
					MyDoubleArray = new[] { 2.4, 5.7, 67.345 },
					MyLongArray = new long[] { 34444445, 65432, 7889999 }
				};
				context.AddUpdateDocument(skillWithIntArray, skillWithIntArray.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var returned = context.GetDocument<SkillWithStringLongAndDoubleArray>(2);
				Assert.Equal(skillWithIntArray.MyStringArray[2], returned.MyStringArray[2]);
				Assert.Equal(skillWithIntArray.MyDoubleArray[1], returned.MyDoubleArray[1]);
				Assert.Equal(skillWithIntArray.MyLongArray[1], returned.MyLongArray[1]);
			}
		}

		#endregion NESTED Simple arrays

		#region NESTED Object

		[Fact]
		public void TestDefaultContextParentWithSingleChild()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skillWithSingleChild = new SkillWithSingleChild
				{
					Id = 5,
					BlahBlah = "TEST",
					MySkillSingleChildElement = new SkillSingleChildElement{ Id= 5, Details = "tteesstt"}
				};
				context.AddUpdateDocument(skillWithSingleChild, skillWithSingleChild.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillWithSingleChild>(skillWithSingleChild.Id);
				Assert.Equal(roundTripResult.MySkillSingleChildElement.Details, skillWithSingleChild.MySkillSingleChildElement.Details);
			}
		}

		[Fact]
		public void TestDefaultContextParentWithSingleChildEqualsNull()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var skillWithSingleChild = new SkillWithSingleChild {MySkillSingleChildElement = null};
				context.AddUpdateDocument(skillWithSingleChild, skillWithSingleChild.Id);

				// Save to Elasticsearch
				var ret = context.SaveChanges();
				Assert.Equal(ret.Status, HttpStatusCode.OK);

				var roundTripResult = context.GetDocument<SkillWithSingleChild>(skillWithSingleChild.Id);
				Assert.Equal(roundTripResult.BlahBlah, skillWithSingleChild.BlahBlah);
			}
		}

        #endregion NESTED Object

    }
}
