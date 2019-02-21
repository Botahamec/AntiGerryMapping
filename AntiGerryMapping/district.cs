using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiGerryMapping {

	class district {

		//constructor
		public district(int number, county[] counties) {
			int Number = number;
			county[] Counties = counties;
		}

		//properties
		int Number {get; set;}
		county[] Counties { get; set;}

	}

}
