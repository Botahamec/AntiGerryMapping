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
					Name = ((yloc - 82) / 24).ToString()
				};
			}
		}

		//load and save functions
		private void LoadCounties(string file = @"../../counties.json") {save = new SaveFile(file);}

		private void InitiateDemographic(string name, int yloc, int number, TabPage tab) {
			NumEntry entry = new NumEntry(name, yloc, number); //creates entry
			entry.box.ValueChanged += new EventHandler(demographicChange);
			tab.Controls.Add(entry.label); //adds label
			tab.Controls.Add(entry.box); //adds numericUpDown
		}

		//sets up a county tab
		private void InitiateCounty(county County) {

			//adds name label
			TabPage newTab = new TabPage {
				Text = County.name,
				AutoScroll = true
			};
			Label nameLabel = new Label {
				Text = "Name",
				AutoSize = true,
				Location = new Point(6, 7),
			};
			TextBox nameBox = new TextBox {
				Text = County.name,
				Anchor = ((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)),
				Location = new Point(143, 4),
				Size = new Size(50, 20),
				Name = "nameBox"
			};
			nameBox.TextChanged += new EventHandler(nameBoxTextChange);
			newTab.Controls.Add(nameBox);
			newTab.Controls.Add(nameLabel);

			//adds voter population elements
			NumEntry pop = new NumEntry("Population", 30, County.population);
			pop.box.ValueChanged += new EventHandler(populationChange);
			newTab.Controls.Add(pop.box);
			newTab.Controls.Add(pop.label);

			int yloc = 82; //sets y-position of demographic entries

			//adds form for each demographic
			for (int a = 0; a < save.Demographics.Length; ++a) {
				InitiateDemographic(save.Demographics[a], yloc, County.demo[a], newTab);
				yloc += 24; //increases y value for next entry
			}

			tabControl1.TabPages.Add(newTab); //adds tab to app
		}

		//resets demographic information
		private void ResetDemographics() {
			foreach (string demo in save.Demographics) {
				foreach (TabPage tab in tabControl1.TabPages) {
					tab.Controls.RemoveAt(tab.Controls.Count - 1);
					tab.Controls.RemoveAt(tab.Controls.Count - 1);
				}
			}
		}

		//deletes all the tabs in the app
		private void DeleteAllTabs() {
			foreach (TabPage tab in tabControl1.TabPages) {
				tabControl1.TabPages.Remove(tab);
			}
		}

		//initializes the app to have all of the counties and demographics required
		private void InitializeApp() {

			tabControl1.TabPages.Remove(tabPage1); //removes first page

			//default demographics and counties
			if (save.Demographics == null || save.Demographics.Length == 0) {
				save.Demographics = new string[] {
					"Older Persons (Over 65)",
					"White Persons",
					"African-American Persons",
					"Hispanic/Latino Persons",
					"Asian Persons",
					"Native Persons",
					"Pacific Persons",
					"Female Persons"
				};
			}
			if (save.Counties == null || save.Counties.Length == 0) {
				List<int> demolist = new List<int>();
				foreach (string dem in save.Demographics) {demolist.Add(0);} //makes list of demographics
				save.Counties = new county[] { new county(
					"County 1",
					new string[] {},
					0,
					demolist.ToArray()
				)};
			}

			//creates a new tab for each county
			for (int i = 0; i < save.Counties.Length; ++i) {
				InitiateCounty(save.Counties[i]);
			} 

		}

		//initializes MainControl
		public MainControl() {
			LoadCounties(); //loads from counties.json
			InitializeComponent(); //initializes app
			InitializeApp(); //creates tabs
		}

		//updates demographics
		public void UpdateDemographics(SaveFile file) {
			DeleteAllTabs();
			save = file;
			InitializeApp();
		}

		//runs whenever the text in the name box changes
		private void nameBoxTextChange(object sender, EventArgs e) {
			TextBox box = (TextBox)sender; //text box
			tabControl1.TabPages[tabControl1.SelectedIndex].Text = box.Text; //changes tab text
			save.Counties[tabControl1.SelectedIndex].name = box.Text; //updates save
		}

		//runs whenever the value in the population box changes
		private void populationChange(object sender, EventArgs e) {
			NumericUpDown box = (NumericUpDown)sender;
			save.Counties[tabControl1.SelectedIndex].population = (int)Math.Round(box.Value);
		}

		//runs whenever the value in a demographic box changes
		private void demographicChange(object sender, EventArgs e) {
			NumericUpDown box = (NumericUpDown)sender;
			save.Counties[tabControl1.SelectedIndex].demo[Int32.Parse(box.Name)] = (int)Math.Round(box.Value);
		}

		//activates after clicking "New"
		private void newToolStripMenuItem_Click(object sender, EventArgs e) {
			DeleteAllTabs(); //clears out the app
			save = new SaveFile(); //empties out the save file
			InitializeApp(); //restarts the app
		}

		//saves to counties.json
		private void saveToolStripMenuItem_Click(object sender, EventArgs e) { save.Save(); }

		//shows About box
		private void aboutToolStripMenuItem1_Click(object sender, EventArgs e) {
			About box = new About();
			box.ShowDialog();
		}
		
		//adds a new county to the project (with button)
		private void newButton_Click(object sender, EventArgs e) {
			List<county> newList = save.Counties.ToList(); //new list
			int newIndex = save.Counties.Length + 1; //index of new county
			List<int> demoList = new List<int>();
			foreach (string dem in save.Demographics) { demoList.Add(0); } //creates list of demographics
			county newCounty = new county( //creates new county
				"County " + newIndex,
				new string[] { },
				0,
				demoList.ToArray()
			);
			newList.Add(newCounty); //adds county to list
			save.Counties = newList.ToArray(); //converts list to array
			InitiateCounty(save.Counties[newIndex]);
		}

		//deletes the current county (with button)
		private void deleteButton_Click(object sender, EventArgs e) {
			List<county> newList = save.Counties.ToList(); //new list
			newList.RemoveAt(tabControl1.SelectedIndex); //removes county from list
			save.Counties = newList.ToArray(); //converts list to array
			tabControl1.TabPages.RemoveAt(tabControl1.SelectedIndex); //removes tab
		}

		//opens demographic GUI (with button)
		private void demoButton_Click(object sender, EventArgs e) {
			save.Save();
			DemographicsWindow demoBox = new DemographicsWindow(this, save);
			demoBox.Show();
		}

		//runs after pressing Finish
		private void finishButton_Click(object sender, EventArgs e) {
			MessageBox.Show(save.ToJson());
		}
	}
}
