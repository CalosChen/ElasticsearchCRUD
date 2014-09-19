﻿using System;
using System.Collections.Generic;
using System.Linq;
using Damienbod.BusinessLayer.Attributes;
using Damienbod.BusinessLayer.DomainModel;
using Damienbod.BusinessLayer.Providers;
using Damienbod.ElasticSearchProvider;

namespace Damienbod.AnimalProvider
{
    [TransientLifetime]
	public class SearchProvider : ISearchProvider
    {
		ElasticSearchContext<Animal> _elasticSearchContext = new ElasticSearchContext<Animal>("http://localhost:9201/", new AnimalElasticSearchSerializerMapping());

        public void CreateAnimal(Animal animal)
        {
			_elasticSearchContext.AddUpdateEntity(animal, animal.Id.ToString(), Animal.SearchIndex);
			var ret = _elasticSearchContext.SaveChangesAsync();    
        }

        public void UpdateAnimal(Animal animal)
        {
			_elasticSearchContext.AddUpdateEntity(animal, animal.Id.ToString(), Animal.SearchIndex);
			var ret = _elasticSearchContext.SaveChangesAsync(); 
        }

		public IQueryable<Animal> GetAnimals()
		{
			return null;
		}

        public void DeleteById(int id)
        {
           // _elasticsearchClient.DeleteById("animals", "animal", id);
        }

        public void DeleteIndex(string index)
        {
            //_elasticsearchClient.DeleteIndex(index);
        }

        public Animal GetAnimal(int id)
        {
	        return null;
        }
    }
}
