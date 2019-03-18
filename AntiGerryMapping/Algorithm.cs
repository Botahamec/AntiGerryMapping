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

		//----------------------------------------------------------------------------
		//
		//                         VARIABLES AND CLASSES
		//
		//------------------------------------------------------------------------

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

		class state {

			//properties
			public int Pop { get; set; }
			public int[] Demo { get; set; }

			//constructor
			public state(SaveFile Save) {

				//initializes variables
				int pop = 0;
				List<int> demo = new List<int>();				
				foreach (string dem in Save.Demographics) {demo.Add(0);} //initializes demo list

				//calculates values
				foreach (county County in Save.Counties) {
					pop += County.population; //adds population
					for (int i = 0; i < County.demo.Length; i++) { //adds demographics
						demo[i] += County.demo[i];
					}
				}

				//sets properties
				Pop = pop;
				Demo = demo.ToArray();
			}
		}

		//----------------------------------------------------------------------------
		//
		//                                 ALGORITHMS
		//
		//----------------------------------------------------------------------------

		//POPPER ---------------------------------------------------------------------------------------------
		//Popper is an algorithm which creates scenarios where the populations of districts are kind of similar
		//it does this by putting the counties into the least populous districts that have borders
		//if it's impossible for a certain county, then that county will be saved for later
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

				//new scenario
				scenario Scenario = CreateScenario(new step[] { stepstaken[s] });
				//prevents duplicate scenarios and 
				if (!scenarios.Contains(Scenario) && Scenario != new scenario(new district[] { })) {
					scenarios.Add(Scenario);
				}

				s++;//increments
			}

			return scenarios.ToArray(); //final return value
		}

		//ASCA -----------------------------------------------------------------------
		//this algorithm finds the most proportional scenario compared to the state
		//it calculates first what the demographic percentage is for the state
		//compares it to the percentage based on the districts of the scenario
		//repeats for each demographic
		//the percent delta between the state and the scenario is its score
		//whichever scenario has the highest score is the best scenario
		private scenario Asca(scenario[] scenarios) {
			
			//VARIABLES ---------------------------------------

			state State = new state(save); //state information
			int P = State.Pop; //total population of the state
			int t = save.Demographics.Length; //total number of districts
			List<int> Scores = new List<int>();//list of scores

			//ALGORITHM -------------------------------------------

			label1.Text = "Finding Best Scenario..."; //tells the user what's going on
			Refresh();

			//calculates score of each 
			foreach (scenario Scenario in scenarios) {

				int score = 0; //score of the scenario

				//calculates an error for each demographic
				for (int i = 0; i < save.Demographics.Length; i++) {

					//variables
					int D = State.Demo[i]; //population of a demographic
					int d = 0; //number of districts where a demogrpahic is the majority

					//calculate number of districts where a demogrpahic is the majority
					foreach (district District in Scenario.Districts) {
						int pop = 0; //number of people in the demographic
						foreach (county County in District.Counties) {
							pop += County.demo[i]; //adds demographic from each county to pop
						}
						if (pop >= District.Population() / 2) //checks if demo is the majority
							{ d++; } //adds one if it is
					}

					//calculates error
					int error = Math.Abs((D / P) - (d / t));
					score += error; //adds error to score
				}

				Scores.Add(score); //adds the score the the list
			}

			//finds lowest score
			int index = 0; //index of the lowest score, default scenario is first one generated
			int lowScore = int.MaxValue; //lowest score
			for (int i = 0; i < scenarios.Length; i++) {
				//sets new index if it finds a lower score
				if (Scores[i] < lowScore) {
					lowScore = Scores[i];
					index = i;
				}
			}
			return scenarios[index]; //DONE! :))))
		}

		//------------------------------------------------------------------------
		//
		//                              INITIATION
		//
		//-----------------------------------------------------------------------

		//initializes algorithm
		public Algorithm(SaveFile Save) {
			InitializeComponent(); //initializes window
			save = Save;
		}

		//runs the algorithm
		private void Algorithm_Shown(object sender, EventArgs e) {

			//start
			label1.Text = "Initializing..."; //tells user the stuff is setting up
			Refresh();

			//algorithms
			scenario[] scenarios = Popper(save); //runs Popper algorithm
			scenario results = Asca(scenarios); //runs ASCA algorithm
			results.Districts = results.Districts.OrderBy(o => o.Number).ToArray(); //orders the districts

			//results
			Results ResultsScreen = new Results(results, save); //sets up results screen
			ResultsScreen.Show();
			Close(); //closes current window
		}
	}
}
