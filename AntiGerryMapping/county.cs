using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace AntiGerryMapping {

	class county {

		//defines properties
		public string name { get; set; } ///name of the county
		public string[] borders { get; set; } ///counties that the county borders
		public int population { get; set; }///total population of the county
		public int votingpop { get; set; } ///population that can vote
		public int[] demo { get; set; } ///list of people in specific demographics

		//constructor
		public county(string name0, string[] borders0, int population0, int votingpop0, int[] demo0) {

			//sets certain properties
			name = name0;
			borders = borders0;
			population = population0;
			votingpop = votingpop0;
			demo = demo0;

			double votingpop1 = votingpop0;
			double population1 = population0;
			double ratio = votingpop1 / population1;

			//finds percentages based on population and demo
			for (int i = 0; i < demo.Length; ++i) {
				double start = demo[i];
				demo[i] = (int)Math.Round(start * ratio);
			}

		}

		//I don't know why this needs to be here but JSON.NET requires it to work. Don't touch it
		private county() { }

	}
}
