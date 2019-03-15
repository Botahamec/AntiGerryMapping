using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace AntiGerryMapping {

	//this class contains information about counties and demographics
	public class SaveFile {

		//PROPERTIES
		public county[] Counties { get; set; }
		public string[] Demographics { get; set; }
		public int Districts { get; set; }

		//CONSTRUCTORS -------------------------------------------------------

		//constructor - from arrays
		public SaveFile(county[] counties, string[] demographics, int districts) {
			Counties = counties;
			Demographics = demographics;
			Districts = districts;
		}

		//constructor - from json
		[JsonConstructor] public SaveFile() { }

		//constructor - from file
		public SaveFile(string file) {

			//reads file
			string json;
			using (var streamReader = new StreamReader(file, Encoding.UTF8)) {
				json = streamReader.ReadToEnd();
			}

			//sets properties
			SaveFile save = JsonConvert.DeserializeObject<SaveFile>(json);
			Counties = save.Counties;
			Demographics = save.Demographics;
			Districts = save.Districts;
		}

		//FUNCTIONS --------------------------------------------------------

		//converts object to JSON
		public string ToJson() {
			return JsonConvert.SerializeObject(this);
		}

		public void Save(string file = @"../../counties.json") {
			File.WriteAllText(file, ToJson());
		}

		/* These Methods Have Been Removed Because They Are No Longer Necessary
		//get index of county from name
		public int getIndex(string name) {
			for (int i = 0; i < Counties.Length; ++i) {
				if (Counties[i].name == name) { return i; }
			}
			return 0; //returns 0 if no county of that name exists. Don't let this happen
		}

		//get index of a demographic from it's name
		public int getDemoIndex(string name) {
			for (int i = 0; i < Demographics.Length; ++i) {
				if (Demographics[i] == name) { return i; }
			}
			return 0;
		}
		*/

	}
}
