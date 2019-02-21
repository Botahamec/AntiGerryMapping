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
using Newtonsoft.Json.Linq;
using System.IO;

namespace AntiGerryMapping {

	public partial class MainControl : Form
	{

		//defines variables
		string[] demographics;
		county[] counties;
		int yloc = 82;

		//A demographic entry
		class NumEntry {

			//properties
			public string name { get; set; } //the name of the entry
			public Label label { get; set; } //a label for the user
			public NumericUpDown box { get; set; } //the box for user input

			//constructor
			public NumEntry(string Name, int yloc, int value) {

				name = Name;

				//sets up label
				label = new Label();
				label.Text = name;
				label.AutoSize = true;
				label.Location = new Point(6, yloc);

				//sets up numeric up down
				box = new NumericUpDown();
				box.Anchor = ((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right));
				box.Location = new Point(143, yloc);
				box.Size = new Size(50, 20);
				box.Maximum = decimal.MaxValue;
				box.Value = value;
			}

		}

		//returns array of demographics from demographics.json
		public string[] DemographicsFile() {
			JsonSerializer serializer = new JsonSerializer(); //initializes serializer
			string demojson;
			using (var streamReader = new StreamReader(@"../../demographics.json", Encoding.UTF8)) {
				demojson = streamReader.ReadToEnd(); //reads demographics.json
			}
			return JsonConvert.DeserializeObject<List<string>>(demojson).ToArray(); //converts json to array
		}

		public void LoadDemographics() {demographics = DemographicsFile();} //loads list of demographics

		//loads list of counties
		public void LoadCounties() {
			JsonSerializer serializer = new JsonSerializer(); //initializes serializer
			string countiesjson;
			using (var streamReader = new StreamReader(@"../../counties.json", Encoding.UTF8)) {
				countiesjson = streamReader.ReadToEnd(); //reads counties.json
			}
			counties = JsonConvert.DeserializeObject<List<county>>(countiesjson).ToArray();
		}

		//sets up a county tab
		public void InitiateCounty(string name, int[] dems, int voters) {

			//adds name label
			TabPage newTab = new TabPage { Text = name };
			Label nameLabel = new Label {
				Text = "Name:",
				AutoSize = true,
				Location = new Point(6, 7),
			};
			TextBox nameBox = new TextBox {
				Text = name,
				Anchor = ((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)),
				Location = new Point(143, 4),
				Size = new Size(50, 20),
			};
			newTab.Controls.Add(nameBox);
			newTab.Controls.Add(nameLabel);

			//adds voter population elements
			NumEntry pop = new NumEntry("Population", 30, voters);
			newTab.Controls.Add(pop.box);
			newTab.Controls.Add(pop.label);

			//adds form for each demographic
			for (int a = 0; a < demographics.Length; ++a) {
				NumEntry entry = new NumEntry(demographics[a], yloc, dems[a]);
				newTab.Controls.Add(entry.label); //adds label
				newTab.Controls.Add(entry.box); //adds numericUpDown
				yloc += 24; //increases y value for next entry
			}

			tabControl1.TabPages.Add(newTab); //adds tab to app
			yloc = 82;
		}

		//initializes the app to have all of the counties and demographics required
		public void InitializeApp() {

			//creates a new tab for each county
			for(int i = 0; i < counties.Length; ++i) {
				county current = counties[i];
				InitiateCounty(counties[i].name, counties[i].demo, counties[i].population);
			}

			//removes first page if it's unnecessary
			if (counties.Length != 0) { tabControl1.TabPages.Remove(tabPage1); } 

		}

		//initializes demographics
		public MainControl() {
			demographics = DemographicsFile(); //loads the array of demographics
			LoadCounties(); //loads the array of counties
			InitializeComponent(); //initializes app
			InitializeApp();
		}

		private void finishButton_Click(object sender, EventArgs e) {

			Popper popper = new Popper();
			county[] counties = popper.LoadCounties();

		}
	}
}
