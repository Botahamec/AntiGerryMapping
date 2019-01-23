using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiGerryMapping
{
	class county {

		//defining the properties
		string name;
		county[] borders;
		int population;
		Dictionary<string, int> demo;

		//constructor
		county(string name, county[] borders, int population, int votingpop, Dictionary<string, int> demo) {

			this.name = name;
			this.borders = borders;
			this.population = votingpop;
			
			//finds percentages based on population and demo
			foreach (KeyValuePair<string, int> dem in demo) {
				this.demo.Add(dem.Key, dem.Value * votingpop / population);
			}

		}

		//properties
		public string Name {
			get {return name;}
			set {name = value;}
		}

		public county[] Borders {
			get {return borders;}
			set {borders = value;}
		}

		public int Pop {
			get {return population;}
			set {population = value;}
		}

		public Dictionary<string, int> Demo {
			get {return demo;}
			set {demo = value;}
		}

	}
}
