using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft.Json;

namespace AntiGerryMapping {

	public partial class Algorithm : Form {

		//a step for Popper to change
		class step {

			//properties
			public county County { get; set; }
			public district District { get; set; }
			public int Difference { get; set; }

			//constructor
			public step(county county, district district, int difference) {
				County = county;
				District = district;
			}
		}

		//POPPER ---------------------------------------------------------------------------------------------
		//Popper is an algorithm which creates scenarios where the populations of districts are kind of similar
		//it does this by putting the counties into the least populous districts
		//it records the steps along the way
		//it will change the least effective steps until there's a scenario 
		//where the extreme districts are apart by a factor of 1.5
		private scenario[] Popper(SaveFile save) {

			//VARIABLES --------------------------------------------------------------------------------

			const float FACTOR = 1.5f; //the factor by which the extreme districts can vary
			int DISTRICTS = save.Districts; //number of districts
			List<scenario> scenarios = new List<scenario>(); //the list of scenarios
			List<step> stepstaken = new List<step>(); //the list of steps taken
			county[] counties = InitializeCountyArray(); //array of counties

			//FUNCTIONS ----------------------------------------------------------------------------

			//creates a list of counties from save
			county[] InitializeCountyArray() {
				List<county> countyl = new List<county>();
				foreach (county curent in save.Counties) { countyl.Add(new county(curent.name, curent.borders, curent.population, curent.demo)); }
				return countyl.OrderBy(o=>o.population).ToArray();
			}

			//creates a new scenario
			scenario CreateScenario(step[] changes) {


				List<district> Districts = new List<district>(); //list of districts
				for (int i = 1; i <= DISTRICTS; i++) { //sets up list
					Districts.Add(new district(i, new county[] { }));
				}
				Debug.Write(JsonConvert.SerializeObject(Districts));

				//loops for each county
				for (int i = counties.Length - 1; i >= 0; i--) {

					county County = counties[i]; //county to  be placed
					Districts = Districts.OrderBy(o => o.Population()).ToList(); //orders the list
					district finalDistrict = new district(0, new county[] { });

					//looks for smallest district that borders the county
					for (int a = 0; (a < DISTRICTS - 1) && (finalDistrict.Number == 0); a++) {

						//automatically accepts empty districts
						if (Districts[a].Counties == null) {
							finalDistrict = Districts[a];
							break;
						}

						//checks if the district borders the district
						foreach (county check in Districts[a].Counties) {
							if (County.borders.Contains(check.name)) {
								finalDistrict = Districts[a];
								break;
							}
						}
					}

					//puts the county into the district
					Districts.Remove(finalDistrict); //removes that district from the list
					List<county> countyl = new List<county>();
					if(finalDistrict.Counties != null) { //checks to make sure a null error won't occur
						countyl = finalDistrict.Counties.ToList(); //puts the list of counties in the countyl list
					} 
					countyl.Add(County); //places the county in the list
					finalDistrict.Counties = countyl.ToArray(); //sets the new list of districts
					Districts.Add(finalDistrict); //puts the new district in the list of districts
				}

				return new scenario(Districts.OrderBy(o=>o.Counties.Length).ToArray());

			}

			//makes sure the difference between extreme districts isn't too large
			bool CheckScenario(scenario Scenario) {
				int maxPop = Scenario.Districts.Last().Population();
				int minPop = Scenario.Districts[0].Population();
				if (maxPop > FACTOR * minPop) { //checks if the districts are too far apart
					return false; //is false if they are
				}
				progressBar1.Value = (int)Math.Round(maxPop / minPop / FACTOR * 100); //sets the progress bar
				return true; //returns true if they aren't
			}

			//ALGORITHM ----------------------------------------------------------------------------

			label1.Text = "Generating Scenarios..."; //tells the user that Popper is running

			scenarios.Add(CreateScenario(new step[] { })); //first scenario

			//generates scenarios
			int s = 0;
			int e = stepstaken.Count();
			stepstaken = stepstaken.OrderBy(o => o.Difference).ToList();
			//do {
				scenarios.Add(CreateScenario(new step[] {  }));
				s++;
				foreach (district District in scenarios.Last().Districts) {
					Debug.WriteLine(District.Population());
					Debug.WriteLine(District.Counties.Length);
				}
				Debug.WriteLine("-----------------------------------------");
			//} while (CheckScenario(scenarios.Last()) && s < e);


			return scenarios.ToArray(); //final return value
		}

		//runs the algorithms
		private void Run(SaveFile save) {
			label1.Text = "Initializing..."; //tells user the stuff is setting up
			scenario[] scenarios = Popper(save); //runs Popper algorithm
			string json = JsonConvert.SerializeObject(scenarios);
			Debug.WriteLine(json);
		}

		//initializes algorithm
		public Algorithm(SaveFile save) {
			InitializeComponent(); //initializes window
			Run(save);
		}
	}
}
