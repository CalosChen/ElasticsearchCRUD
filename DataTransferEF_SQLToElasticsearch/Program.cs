﻿using System;

namespace DataTransferSQLToEl
{
	class Program
	{
		static void Main(string[] args)
		{
			Repo repo = new Repo();
			//repo.SaveToElasticsearchPerson();
			
			repo.SaveToElasticsearchAddress();
			var addressX = repo.GetAddressFromElasticsearch("22");
			Console.WriteLine(addressX);
		}
	}
}
