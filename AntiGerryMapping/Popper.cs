using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Diagnostics;

/*
	** Popper is an algorithm that narrows down the number of scenarios before GerryMapping executes **
	* It works by first testing the scenario where the districts have the closest populations
	* It can find this scenario by putting counties into the district with the lowest population
	* It starts with the most populous county first and moves on
	* It records the steps as it goes
	* It then finds the scenarios here the districts have really populations
	* It does this by checking the steps where the least difference would be made by putting the
		county into a different district.
	* It continues to do this until it finds a scenario where the extreme districts are apart by a factor of 1.5
	* This number can be changed easily if Popper runs too slow or doesn't find very good districts

*/

namespace AntiGerryMapping {

	//contains methods for Popper
	class Popper {

		//loads list of counties from counties.json
		public county[] LoadCounties() {

			//converts counties.json into a string
			string json;
			using (var streamReader = new StreamReader(@"../../counties.json", Encoding.UTF8)) {
				json = streamReader.ReadToEnd();
			}

			JsonSerializer serializer = new JsonSerializer();
			county[] counties = JsonConvert.DeserializeObject<List<county>>(json, new JsonSerializerSettings {
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
			}).ToArray();
			Debug.WriteLine(JsonConvert.SerializeObject(counties));
			return counties;

		}

	}

}