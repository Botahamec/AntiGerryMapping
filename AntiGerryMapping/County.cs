using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiGerryMapping {

	class county {

		//constructor
		public county(string name, county[] borders, int population, int votingpop, int[] demo) {

			//sets certain properties
			string Name = name;
			county[] Borders = borders;
			int Population = votingpop;
			int[] Demo = demo;
			
			//finds percentages based on population and demo
			for(int i = 0; i < Demo.Length; ++i) {
				Demo[i] = (Demo[i] * votingpop / population);
			}

		}

		//properties
		public string Name {get => Name; set => Name = value;}
		public county[] Borders {get => Borders; set => Borders = value;}
		public int Population {get => Population; set => Population = value;}
		public int[] Demo {get => Demo; set => Demo = value;}

	}
}
