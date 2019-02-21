using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace AntiGerryMapping {

	public class county {

		//defines properties
		public string name { get; set; } ///name of the county
		public string[] borders { get; set; } ///counties that the county borders
		public int population { get; set; }///total population of the county
		public int[] demo { get; set; } ///list of people in specific demographics

		//constructor
		public county(string name0, string[] borders0, int votingpop0, int[] demo0) {
			name = name0;
			borders = borders0;
			population = votingpop0;
			demo = demo0;
		}

		//I don't know why this needs to be here but JSON.NET requires it to work. Don't touch it
		[JsonConstructor] public county() { }

	}
}
