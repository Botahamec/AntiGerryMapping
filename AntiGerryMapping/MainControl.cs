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
		SaveFile save;

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
				label = new Label {
					Text = name,
					AutoSize = true,
					Location = new Point(6, yloc)
				};

				//sets up numeric up down
				box = new NumericUpDown {
					Anchor = ((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)),
					Location = new Point(143, yloc),
					Size = new Size(50, 20),
					Maximum = int.MaxValue,
					Value = value,
					Name = name
				};
			}
		}

		//load and save functions
		public void LoadCounties(string file = @"../../counties.json") {save = new SaveFile(file);}
		public void SaveCounties(string file = @"../../counties.json") {File.WriteAllText(file, save.ToJson());}

		//sets up a county tab
		public void InitiateCounty(county county) {

			//adds name label
			TabPage newTab = new TabPage { Text = county.name };
			Label nameLabel = new Label {
				Text = "Name:",
				AutoSize = true,
				Location = new Point(6, 7)
			};
			TextBox nameBox = new TextBox {
				Text = county.name,
				Anchor = ((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)),
				Location = new Point(143, 4),
				Size = new Size(50, 20),
				Name = "nameBox"
			};
			nameBox.TextChanged += new EventHandler(nameBoxTextChange);
			newTab.Controls.Add(nameBox);
			newTab.Controls.Add(nameLabel);

			//adds voter population elements
			NumEntry pop = new NumEntry("Population", 30, county.population);
			pop.box.ValueChanged += new EventHandler(populationChange);
			newTab.Controls.Add(pop.box);
			newTab.Controls.Add(pop.label);

			int yloc = 82; //sets y-position of demographic entries

			//adds form for each demographic
			for (int a = 0; a < save.Demographics.Length; ++a) {
				NumEntry entry = new NumEntry(save.Demographics[a], yloc, county.demo[a]); //creates entry
				entry.box.ValueChanged += new EventHandler(demographicChange);
				newTab.Controls.Add(entry.label); //adds label
				newTab.Controls.Add(entry.box); //adds numericUpDown
				yloc += 24; //increases y value for next entry
			}

			tabControl1.TabPages.Add(newTab); //adds tab to app
		}

		//initializes the app to have all of the counties and demographics required
		public void InitializeApp() {

			//default arrays
			string[] defaultDemographics = {
				"Older Persons (Over 65)",
				"White Persons",
				"African-American Persons",
				"Hispanic/Latino Persons",
				"Asian Persons",
				"Native Persons",
				"Pacific Persons",
				"Female Persons"
			};

			county[] defaultCounties = {
				new county("County 1", new string[] { }, 0, new int[] {0, 0, 0, 0, 0, 0, 0, 0})
			};

			tabControl1.TabPages.Remove(tabPage1); //removes first page

			//default demographics and counties
			if (save.Demographics == null || save.Demographics.Length == 0) 
				{save.Demographics = defaultDemographics;}
			if (save.Counties == null || save.Counties.Length == 0) { save.Counties = defaultCounties; }

			//creates a new tab for each county
			for (int i = 0; i < save.Counties.Length; ++i) {
				InitiateCounty(save.Counties[i]);
			} 

		}

		//deletes all the tabs in the app
		public void DeleteAllTabs() {
			foreach (TabPage tab in tabControl1.TabPages) {
				tabControl1.TabPages.Remove(tab);
			}
		}

		//initializes demographics
		public MainControl() {
			LoadCounties(); //loads from counties.json
			InitializeComponent(); //initializes app
			InitializeApp(); //creates tabs
		}

		//runs whenever the text in the name box changes
		private void nameBoxTextChange(object sender, EventArgs e) {
			TextBox box = (TextBox)sender; //text box
			TabPage tab = (TabPage)box.Parent; //tab
			int index = save.getIndex(tab.Text); //finds county
			tab.Text = box.Text; //changes tab text
			save.Counties[index].name = box.Text; //updates save
		}

		//runs whenever the value in the population box changes
		private void populationChange(object sender, EventArgs e) {
			NumericUpDown box = (NumericUpDown)sender;
			TabPage tab = (TabPage)box.Parent;
			int index = save.getIndex(tab.Text);
			save.Counties[index].population = (int)Math.Round(box.Value);
		}

		//runs whenever the value in a demographic box changes
		private void demographicChange(object sender, EventArgs e) {
			NumericUpDown box = (NumericUpDown)sender;
			TabPage tab = (TabPage)box.Parent;
			int index = save.getIndex(tab.Text);
			int demo = save.getDemoIndex(box.Name);
			save.Counties[index].demo[demo] = (int)Math.Round(box.Value);
		}

		//activates after clicking "New"
		private void newToolStripMenuItem_Click(object sender, EventArgs e) {
			DeleteAllTabs(); //clears out the app
			save = new SaveFile(); //empties out the save file
			InitializeApp(); //restarts the app
		}

		//saves to counties.json
		private void saveToolStripMenuItem_Click(object sender, EventArgs e) {SaveCounties();}

		//shows About box
		private void aboutToolStripMenuItem1_Click(object sender, EventArgs e) {
			About box = new About();
			box.ShowDialog();
		}

		//runs after pressing Finish
		private void finishButton_Click(object sender, EventArgs e) {
			Debug.WriteLine(save.ToJson());
		}

	}
}
