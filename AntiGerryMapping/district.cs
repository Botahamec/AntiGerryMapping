using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AntiGerryMapping {

	public class district {

		//properties
		public int Number { get; set; }
		public county[] Counties { get; set; }

		//constructor
		public district(int number, county[] counties) {
			Number = number;
			Counties = counties;
		}

		//gets the population of the district
		public int Population() {
			if (Counties == null || Counties.Length == 0) { return 0; } //no counties, no population
			int population = 0;
			foreach (county County in Counties) { //loops for each county
				population += County.population; //adds population of county into the population of the district
			}
			return population;
		}

		//gets the demographic number
		public int GetDemo(int i) {
			if (Counties == null || Counties.Length == 0) { return 0; }
			int population = 0;
			foreach (county County in Counties) {
				population += County.demo[i];
			}
			return population;
		}

	}

}
