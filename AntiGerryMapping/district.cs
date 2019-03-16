using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiGerryMapping {

	class district {

		//properties
		public int Number { get; set; }
		public county[] Counties { get; set; }

		//constructor
		public district(int number, county[] counties) {
			int Number = number;
			county[] Counties = counties;
		}

		//gets the population of the district
		public int Population() {
			if (Counties == null) { return 0; } //if there are no counties, the population is zero
			int population = 0;
			foreach (county County in Counties) { //loops for each county
				population += County.population; //adds population of county into the population of the district
			}
			return population;
		}

	}

}
