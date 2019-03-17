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

		SaveFile save;

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
				foreach (county curent in save.Counties) {
					countyl.Add(new county(curent.name, curent.borders, curent.population, curent.demo));
				}
				return countyl.OrderBy(o=>o.population).ToArray();
			}

			//creates a new scenario
			scenario CreateScenario(step[] changes) {

				//set up list of districts
				List<district> Districts = new List<district>();
				for (int i = 1; i <= DISTRICTS; i++) {
					Districts.Add(new district(i, new county[] { }));
				}

				//sets up new independent list of counties
				List<county> origCounties = new List<county>();
				foreach (county County in counties) {
					origCounties.Add(new county(County.name, County.borders, County.population, County.demo));
				}

				//loops for each county
				for (int z = 1; z <= origCounties.Count; z++) {

					Debug.WriteLine("z=" + z);

					int i = origCounties.Count - z;
					county County = origCounties[i]; //county to  be placed
					Districts = Districts.OrderBy(o => o.Population()).ToList(); //orders the list
					district finalDistrict = new district(0, new county[] { });
					int a; //incrementor

					//looks for smallest district that borders the county
					for (a = 0; (a < DISTRICTS - 1) && (finalDistrict.Number == 0); a++) {


						//automatically accepts empty districts
						if (Districts[a].Counties == null || Districts[a].Counties.Length == 0) {
							finalDistrict = Districts[a];

							//puts step into stepstaken
							int secPop = 0; //second least populated viable district
							for (a++; a < DISTRICTS - 1; a++) { //finds second closest district

								//automatically accepts empty districts
								if (Districts[a].Counties == null) {
									secPop = Districts[a].Population();
									break;
								}

								//checks if the district borders the district
								foreach (county check in Districts[a].Counties) {
									if (County.borders.Contains(check.name)) {
										secPop = Districts[a].Population();
										break;
									}
								}
							}
							int difference = Math.Abs(secPop - finalDistrict.Population()); //calculates difference
							step thisStep = new step(County, finalDistrict, difference);
							if (changes.Contains(thisStep)) { continue; } //skips if this step is the one to avoid
							stepstaken.Add(thisStep); //adds step to list

							break;
						}

						//checks if the district borders the district
						foreach (county check in Districts[a].Counties) {
							if (County.borders.Contains(check.name)) {
								finalDistrict = Districts[a];

								//puts step into stepstaken
								int secPop = 0; //second least populated viable district
								for (a++; a < DISTRICTS - 1; a++) { //finds second closest district

									//automatically accepts empty districts
									if (Districts[a].Counties == null) {
										secPop = Districts[a].Population();
										break;
									}

									//checks if the district borders the district
									foreach (county test in Districts[a].Counties) {
										if (County.borders.Contains(test.name)) {
											secPop = Districts[a].Population();
											break;
										}
									}
								}
								int difference = Math.Abs(secPop - finalDistrict.Population()); //calculates difference
								step thisStep = new step(County, finalDistrict, difference);
								if (changes.Contains(thisStep)) { continue; }
								stepstaken.Add(thisStep); //adds step to list

								break;
							}
						}
					}

					//prevent loop from taking too long
					if (z > counties.Length * 12) {
						Debug.Write("didn't work");
						return new scenario(new district[] { });						
					}

					//runs if there is no viable district
					if (finalDistrict.Number == 0) {
						origCounties.Insert(0, County); //saves county for later
						continue; //tries next county
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

				return new scenario(Districts.OrderBy(o=>o.Population()).ToArray());

			}

			//makes sure the difference between extreme districts isn't too large
			bool CheckScenario(scenario Scenario) {
				int maxPop = Scenario.Districts.Last().Population();
				int minPop = Scenario.Districts[0].Population();
				if (maxPop > FACTOR * minPop) { //checks if the districts are too far apart
					return false; //is false if they are
				}
				progressBar1.Value = (int)Math.Round(maxPop / minPop / FACTOR * 100); //sets the progress bar;
				return true; //returns true if they aren't
			}

			//ALGORITHM ----------------------------------------------------------------------------

			label1.Text = "Generating Scenarios..."; //tells the user that Popper is running
			Refresh();

			scenarios.Add(CreateScenario(new step[] { })); //first scenario

			//second round
			int s = 0; //incrementer
			int e = stepstaken.Count();
			stepstaken = stepstaken.OrderBy(o => o.Difference).ToList();
			while (CheckScenario(scenarios.Last()) && s < e) {

				//new scenariio
				scenario Scenario = CreateScenario(new step[] { stepstaken[s] });
				//prevents duplicate scenarios and 
				if (!scenarios.Contains(Scenario) && Scenario != new scenario(new district[] { })) {
					scenarios.Add(Scenario);
				}

				s++;//increments
			}

			return scenarios.ToArray(); //final return value
		}

		//initializes algorithm
		public Algorithm(SaveFile Save) {
			InitializeComponent(); //initializes window
			save = Save;
		}

		private void Algorithm_Shown(object sender, EventArgs e) {
			label1.Text = "Initializing..."; //tells user the stuff is setting up
			Refresh();
			scenario[] scenarios = Popper(save); //runs Popper algorithm
			string json = JsonConvert.SerializeObject(scenarios);
			Debug.WriteLine(json);
		}
	}
}
