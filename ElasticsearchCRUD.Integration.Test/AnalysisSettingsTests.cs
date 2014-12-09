﻿using System.Collections.Generic;
using System.Threading;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel.SettingsModel;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel.SettingsModel.Analyzers;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel.SettingsModel.Filters;
using ElasticsearchCRUD.Model;
using ElasticsearchCRUD.Tracing;
using NUnit.Framework;

namespace ElasticsearchCRUD.Integration.Test
{
	[TestFixture]
	public class AnalysisSettingsTests
	{
		private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver = new ElasticsearchMappingResolver();
		private const string ConnectionString = "http://localhost:9200";

		public void TearDown()
		{
			using (var context = new ElasticsearchContext(ConnectionString, _elasticsearchMappingResolver))
			{
				if (context.IndexExists<TestSettingsIndex>())
				{
					context.AllowDeleteForIndex = true;
					context.DeleteIndex<TestSettingsIndex>();
				}
			}
		}
		
		//"settings": {
		//	"analysis": {
		//		"analyzer": {
		//			"alfeanalyzer": {
		//				"type": "pattern",
		//				"pattern": "\\s+"
		//			}
		//		}
		//	}
		//},
		[Test]
		public void CreateNewIndexDefinitionForPatternAnalyzer()
		{
			var testSettingsIndexDefinition = new IndexDefinition
			{
				IndexSettings =
				{
					Analysis = new Analysis
					{
						Analyzer =
						{
							Analyzers = new List<AnalyzerBase>
							{
								new PatternAnalyzer("alfeanalyzer")
								{
									Pattern = "\\s+"
								}
							}
						}
					},
					NumberOfShards = 3,
					NumberOfReplicas = 1
				},
			};

			using ( var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<TestSettingsIndex>(testSettingsIndexDefinition);
				Thread.Sleep(1500);
				Assert.IsTrue(context.IndexExists<TestSettingsIndex>());
			}

			Thread.Sleep(1500);
			TearDown();
		}

		//{
		//	"index" : {
		//		"analysis" : {
		//			"analyzer" : {
		//				"my_analyzer" : {
		//					"type" : "snowball",
		//					"language" : "English"
		//				}
		//			}
		//		}
		//	}
		//}
		[Test]
		public void CreateNewIndexDefinitionForSnowballAnalyzer()
		{
			var testSettingsIndexDefinition = new IndexDefinition
			{
				IndexSettings =
				{
					Analysis = new Analysis
					{
						Analyzer =
						{
							Analyzers = new List<AnalyzerBase>
							{
								new SnowballAnalyzer("my_analyzer")
								{
									Language = SnowballLanguage.English
								}
							}
						}
					},
					NumberOfShards = 3,
					NumberOfReplicas = 1
				},
			};

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<TestSettingsIndex>(testSettingsIndexDefinition);

				Thread.Sleep(1500);
				Assert.IsTrue(context.IndexExists<TestSettingsIndex>());
			}

			Thread.Sleep(1500);
			TearDown();
		}

		//{
		//	"index" : {
		//		"analysis" : {
		//			"analyzer" : {
		//				"my_analyzer" : {
		//					"tokenizer" : "standard",
		//					"filter" : ["standard", "lowercase", "my_snow"]
		//				}
		//			},
		//			"filter" : {
		//				"my_snow" : {
		//					"type" : "snowball",
		//					"language" : "Lovins"
		//				}
		//			}
		//		}
		//	}
		//}
		[Test]
		public void CreateNewIndexDefinitionForSnowballFilter()
		{
			var testSettingsIndexDefinition = new IndexDefinition
			{
				IndexSettings =
				{
					Analysis = new Analysis
					{
						Analyzer =
						{
							Analyzers = new List<AnalyzerBase>
							{
								new CustomAnalyzer("my_analyzer")
								{
									Tokenizer= DefaultTokenizers.Standard,
									Filter = new List<string>{ DefaultTokenFilters.Standard, DefaultTokenFilters.Lowercase, "my_snow"}
								}
							}
						},
						Filters =
						{
							CustomFilters = new List<AnalysisFilterBase>
							{
								new SnowballTokenFilter("my_snow")
								{
									Language = SnowballLanguage.Lovins
								}
							}
						}
					},
					NumberOfShards = 3,
					NumberOfReplicas = 1
				},
			};

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<TestSettingsIndex>(testSettingsIndexDefinition);

				Thread.Sleep(1500);
				Assert.IsTrue(context.IndexExists<TestSettingsIndex>());
			}

			Thread.Sleep(1500);
			TearDown();
		}

		// "analysis": {
		//	"filter": {
		//		"stop_filter": {
		//			"type": "stop",
		//			"stopwords": ["_english_"]
		//		},
		//		"stemmer_filter": {
		//			"type": "stemmer",
		//			"name": "minimal_english"
		//		}
		//	},
		//	"analyzer": {
		//		"wp_analyzer": {
		//			"type": "custom",
		//			"tokenizer": "uax_url_email",
		//			"filter": ["lowercase", "stop_filter", "stemmer_filter"],
		//			"char_filter": ["html_strip"]
		//		},
		//		"wp_raw_lowercase_analyzer": {
		//			"type": "custom",
		//			"tokenizer": "keyword",
		//			"filter": ["lowercase"]
		//		}
		//	}
		//}
		[Test]
		public void CreateNewIndexDefinitionForStopAndStemmerFilter()
		{
			var testSettingsIndexDefinition = new IndexDefinition
			{
				IndexSettings =
				{
					Analysis = new Analysis
					{
						Analyzer =
						{
							Analyzers = new List<AnalyzerBase>
							{
								new CustomAnalyzer("wp_analyzer")
								{
									Tokenizer= DefaultTokenizers.UaxUrlEmail,
									Filter = new List<string>{DefaultTokenFilters.Lowercase, "stop_filter", "stemmer_filter"},
									CharFilter = new List<string>{"html_strip"}
								},
								new CustomAnalyzer("wp_raw_lowercase_analyzer")
								{
									Tokenizer = DefaultTokenizers.Keyword,
									Filter = new List<string>{DefaultTokenFilters.Lowercase}
								}
							}
						},
						Filters =
						{
							CustomFilters = new List<AnalysisFilterBase>
							{
								new  StopTokenFilter("stop_filter")
								{
									StopwordsList = new List<string>{"_english_"}
								},
								new StemmerTokenFilter("stemmer_filter")
								{
									StemmerName = Stemmer.minimal_english
								}
							}
						}
					},
					NumberOfShards = 3,
					NumberOfReplicas = 1
				},
			};

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<TestSettingsIndex>(testSettingsIndexDefinition);

				Thread.Sleep(1500);
				Assert.IsTrue(context.IndexExists<TestSettingsIndex>());
			}

			Thread.Sleep(1500);
			TearDown();
		}

		//		"settings" : {
		//   "analysis" : {
		//	   "filter" : {
		//		   "blocks_filter" : {
		//			   "type" : "word_delimiter",
		//			   "preserve_original": "true"
		//		   },
		//		  "shingle":{
		//			  "type":"shingle",
		//			  "max_shingle_size":5,
		//			  "min_shingle_size":2,
		//			  "output_unigrams":"true"
		//		   },
		//		   "filter_stop":{
		//			  "type":"stop",
		//			  "ignore_case":"true"
		//		   }
		//	   },
		//	   "analyzer" : {
		//		   "blocks_analyzer" : {
		//			   "type" : "custom",
		//			   "tokenizer" : "whitespace",
		//			   "filter" : ["lowercase", "blocks_filter", "shingle"]
		//		   }
		//	   }
		//   }
		//},
		[Test]
		public void CreateNewIndexDefinitionShingle()
		{
			var testSettingsIndexDefinition = new IndexDefinition
			{
				IndexSettings =
				{
					Analysis = new Analysis
					{
						Analyzer =
						{
							Analyzers = new List<AnalyzerBase>
							{
								new CustomAnalyzer("blocks_analyzer")
								{
									Tokenizer= DefaultTokenizers.Whitespace,
									Filter = new List<string>{DefaultTokenFilters.Lowercase, "blocks_filter", "shingle"}
								}
							}
						},
						Filters =
						{
							CustomFilters = new List<AnalysisFilterBase>
							{
								new WordDelimiterTokenFilter("blocks_filter")
								{
									PreserveOriginal = true
								},
								new ShingleTokenFilter("shingle")
								{
									MaxShingleSize = 5,
									MinShingleSize = 2,
									OutputUnigrams = true
								},
								new StopTokenFilter("filter_stop")
								{
									IgnoreCase = true
								}
							}
						}
					},
					NumberOfShards = 2,
					NumberOfReplicas = 1
				},
			};

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<TestSettingsIndex>(testSettingsIndexDefinition);

				Thread.Sleep(1500);
				Assert.IsTrue(context.IndexExists<TestSettingsIndex>());
			}

			Thread.Sleep(1500);
			TearDown();
		}

		//		{
		//	"settings": {
		//		"analysis": {
		//			"analyzer": {
		//				"es_std": {
		//					"type":      "standard",
		//					"stopwords": "_spanish_"
		//				}
		//			}
		//		}
		//	}
		//}
		[Test]
		public void CreateNewIndexDefinitionStandardAnalyzer()
		{
			var testSettingsIndexDefinition = new IndexDefinition
			{
				IndexSettings =
				{
					Analysis = new Analysis
					{
						Analyzer =
						{
							Analyzers = new List<AnalyzerBase>
							{
								new StandardAnaylzer("es_std")
								{
									Stopwords = "_spanish_"
								}
							}
						}
					},
					NumberOfShards = 2,
					NumberOfReplicas = 1
				},
			};

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<TestSettingsIndex>(testSettingsIndexDefinition);

				Thread.Sleep(1500);
				Assert.IsTrue(context.IndexExists<TestSettingsIndex>());
			}

			Thread.Sleep(1500);
			TearDown();
		}

		[Test]
		public void CreateNewIndexDefinitionForSynonymFilter()
		{
			var testSettingsIndexDefinition = new IndexDefinition
			{
				IndexSettings =
				{
					Analysis = new Analysis
					{
						Analyzer =
						{
							Analyzers = new List<AnalyzerBase>
							{
								new CustomAnalyzer("my_analyzer")
								{
									Tokenizer= DefaultTokenizers.Standard,
									Filter = new List<string>{ DefaultTokenFilters.Standard, DefaultTokenFilters.Lowercase, "my_synonym"}
								}
							}
						},
						Filters =
						{
							CustomFilters = new List<AnalysisFilterBase>
							{
								new SynonymTokenFilter("my_synonym")
								{
									Synonyms = new List<string>
									{
										"william  => bob",
										"sean, johny => john"
									}
								}
							}
						}
					},
					NumberOfShards = 3,
					NumberOfReplicas = 1
				},
			};

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<TestSettingsIndex>(testSettingsIndexDefinition);

				Thread.Sleep(1500);
				Assert.IsTrue(context.IndexExists<TestSettingsIndex>());
			}

			Thread.Sleep(1500);
			TearDown();
		}

		//		{
		//	"analysis" : {
		//		"analyzer" : {
		//			"en" : {
		//				"tokenizer" : "standard",
		//				"filter" : [ "lowercase", "en_US" ]
		//			}
		//		},
		//		"filter" : {
		//			"en_US" : {
		//				"type" : "hunspell",
		//				"locale" : "en_US",
		//				"dedup" : true
		//			}
		//		}
		//	}
		//}
		// THIS TEST WILL ONLY WORK IF YOU SETUP THE *.aff AND ONE OR MORE *.dic FILES
		//[Test]
		public void CreateNewIndexDefinitionForHunspellFilter()
		{
			var testSettingsIndexDefinition = new IndexDefinition
			{
				IndexSettings =
				{
					Analysis = new Analysis
					{
						Analyzer =
						{
							Analyzers = new List<AnalyzerBase>
							{
								new CustomAnalyzer("en")
								{
									Tokenizer= DefaultTokenizers.Standard,
									Filter = new List<string>{  DefaultTokenFilters.Lowercase, "en_US"}
								}
							}
						},
						Filters =
						{
							CustomFilters = new List<AnalysisFilterBase>
							{
								new HunspellTokenFilter("en_US")
								{
									Locale = "en_US",
									Dedup = true
								}
							}
						}
					},
					NumberOfShards = 3,
					NumberOfReplicas = 1
				},
			};

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<TestSettingsIndex>(testSettingsIndexDefinition);

				Thread.Sleep(1500);
				Assert.IsTrue(context.IndexExists<TestSettingsIndex>());
			}

			Thread.Sleep(1500);
			TearDown();
		}

		//	"settings": {
		//	"analysis": {
		//		"analyzer": {
		//			"autocomplete": {
		//				"type": "custom",
		//				"tokenizer": "standard",
		//				"filter": ["standard", "lowercase", "kstem", "edgeNGram"]
		//			}
		//		},
		//		"filter" : {
		//			"ngram" : {
		//			   "type": "edgeNGram",
		//			   "min_gram": 2,
		//			   "max_gram": 15
		//			}
		//		}
		//	}
		//},
		[Test]
		public void CreateNewIndexDefinitionForEdgeNGramFilter()
		{
			var testSettingsIndexDefinition = new IndexDefinition
			{
				IndexSettings =
				{
					Analysis = new Analysis
					{
						Analyzer =
						{
							Analyzers = new List<AnalyzerBase>
							{
								new CustomAnalyzer("autocomplete")
								{
									Tokenizer= DefaultTokenizers.Standard,
									Filter = new List<string>{ DefaultTokenFilters.Standard, DefaultTokenFilters.Lowercase, DefaultTokenFilters.Kstem, DefaultTokenFilters.EdgeNGram}
								}
							}
						},
						Filters =
						{
							CustomFilters = new List<AnalysisFilterBase>
							{
								new EdgeNGramTokenFilter("ngram")
								{
									MinGram = 2,
									MaxGram = 15
								}
							}
						}
					},
					NumberOfShards = 3,
					NumberOfReplicas = 1
				},
			};

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<TestSettingsIndex>(testSettingsIndexDefinition);

				Thread.Sleep(1500);
				Assert.IsTrue(context.IndexExists<TestSettingsIndex>());
			}

			Thread.Sleep(1500);
			TearDown();
		}

		//		{
		//	"index" : {
		//		"analysis" : {
		//			"analyzer" : {
		//				"folding" : {
		//					"tokenizer" : "standard",
		//					"filter" : ["my_icu_folding", "lowercase"]
		//				}
		//			}
		//			"filter" : {
		//				"my_icu_folding" : {
		//					"type" : "icu_folding"
		//					"unicodeSetFilter" : "[^åäöÅÄÖ]"
		//				}
		//			}
		//		}
		//	}
		//}
		// THIS TEST WILL ONLY WORK IF YOU INSTALL THE PLUGIN
		//[Test]
		public void CreateNewIndexDefinitionForIcuFoldingTokenFilter()
		{
			var testSettingsIndexDefinition = new IndexDefinition
			{
				IndexSettings =
				{
					Analysis = new Analysis
					{
						Analyzer =
						{
							Analyzers = new List<AnalyzerBase>
							{
								new CustomAnalyzer("folding")
								{
									Tokenizer= DefaultTokenizers.Standard,
									Filter = new List<string>{ DefaultTokenFilters.Standard, DefaultTokenFilters.Lowercase, "my_icu_folding"}
								}
							}
						},
						Filters =
						{
							CustomFilters = new List<AnalysisFilterBase>
							{
								new IcuFoldingTokenFilter("my_icu_folding")
								{
									UnicodeSetFilter = "[^åäöÅÄÖ]"
								}
							}
						}
					},
					NumberOfShards = 3,
					NumberOfReplicas = 1
				},
			};

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<TestSettingsIndex>(testSettingsIndexDefinition);

				Thread.Sleep(1500);
				Assert.IsTrue(context.IndexExists<TestSettingsIndex>());
			}

			Thread.Sleep(1500);
			TearDown();
		}
	}

	public class TestSettingsIndex
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}

}
