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
		int SelectedTab;

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
					Size = new Size(200, 20),
					Maximum = int.MaxValue,
					Value = value,
					ThousandsSeparator = true,
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

		//adds checkbox for each county that could be a border
		private void InitiateBorders(string county, string[] borders, int yloc, TabPage tab) {
			int index = 0;
			foreach (county County in save.Counties) {
				string name = County.name;
				if (name == county) {break;}
				index++;
			}
			int num = 0;
			foreach (county County in save.Counties) {
				string name = County.name;
				if (name == county) { num++; continue; }
				CheckState check = new CheckState();
				if (borders.Contains(name)) { check = CheckState.Checked; }
				else {check = CheckState.Unchecked;}
				if (save.Counties[num].borders.Contains(county)) {
					check = CheckState.Checked;
					if (!borders.Contains(save.Counties[num].name)) {
						List<string> list = borders.ToList();
						list.Add(save.Counties[num].name);
						save.Counties[index].borders = list.ToArray();
					}
				}
				CheckBox box = new CheckBox {
					Text = name,
					CheckState = check,
					Location = new Point(6, yloc),
					Name = num.ToString()
				};
				box.CheckStateChanged += new EventHandler(borderChange);
				tab.Controls.Add(box);
				yloc += 24;
				num++;
			}
			GC.Collect();
		}

		//sets up a county tab
		private void InitiateCounty(county County, TabPage newTab) {

			Label nameLabel = new Label {
				Text = "Name",
				AutoSize = true,
				Location = new Point(6, 7),
			};
			TextBox nameBox = new TextBox {
				Text = County.name,
				Anchor = ((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)),
				Location = new Point(143, 4),
				Size = new Size(200, 20),
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

			//sets up borders label
			yloc += 24;
			Label bordersLabel = new Label {
				Text = "Borders:",
				AutoSize = true,
				Location = new Point(6, yloc)
			};
			newTab.Controls.Add(bordersLabel);
			yloc += 24;
			InitiateBorders(County.name, County.borders, yloc, newTab);
		}

		private void EmptyTab(TabPage tab) {
			for (int i = 0; i < tab.Controls.Count; i++) {
				tab.Controls.RemoveAt(0);
			}
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

		//creates tab for a page
		private void InitiateTab(string name) {
			TabPage newTab = new TabPage {
				Text = name,
				AutoScroll = true
			};
			tabControl1.TabPages.Add(newTab);
		}

		//initializes the app to have all of the counties and demographics required
		private void InitializeApp() {

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

			districtsBox.Value = save.Districts; //sets the number of districts

			//creates a new tab for each county
			for (int i = 0; i < save.Counties.Length; ++i) {
				InitiateTab(save.Counties[i].name);
			}
			InitiateCounty(save.Counties[0], tabControl1.TabPages[0]);

			tabControl1.SelectedIndexChanged += new EventHandler(tabChanged);

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

		//runs when the number of districts changes
		private void districtsBox_ValueChanged(object sender, EventArgs e) {
			NumericUpDown box = (NumericUpDown)sender;
			save.Districts = (int)Math.Round(box.Value);
		}

		//runs when a checkbox is toggled in the borders section
		private void borderChange(object sender, EventArgs e) {
			CheckBox box = (CheckBox)sender;
			if (box.Checked) {
				List<string> borders = save.Counties[tabControl1.SelectedIndex].borders.ToList();
				borders.Add(box.Text);
				save.Counties[tabControl1.SelectedIndex].borders = borders.ToArray();
			}
			else {
				List<string> borders = save.Counties[tabControl1.SelectedIndex].borders.ToList();
				borders.Remove(box.Text);
				save.Counties[tabControl1.SelectedIndex].borders = borders.ToArray();
				/*borders = save.Counties[int.Parse(box.Name)].borders.ToList();
				borders.Remove(box.Text);
				save.Counties[int.Parse(box.Name)].borders = borders.ToArray();*/
			}
		}

		//runs when the selected tab changes
		private void tabChanged(object sender, EventArgs e) {
			tabControl1.TabPages[SelectedTab].VerticalScroll.Value = 0;
			EmptyTab(tabControl1.TabPages[SelectedTab]);
			SelectedTab = tabControl1.SelectedIndex;
			InitiateCounty(save.Counties[SelectedTab], tabControl1.TabPages[SelectedTab]);
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
			int newIndex = save.Counties.Length; //index of new county
			List<int> demoList = new List<int>();
			foreach (string dem in save.Demographics) { demoList.Add(0); } //creates list of demographics
			county newCounty = new county( //creates new county
				"County " + (newIndex + 1),
				new string[] { },
				0,
				demoList.ToArray()
			);
			newList.Add(newCounty); //adds county to list
			save.Counties = newList.ToArray(); //converts list to array
			InitiateTab(newCounty.name);
		}

		//deletes the current county (with button)
		private void deleteButton_Click(object sender, EventArgs e) {
			if (tabControl1.TabPages.Count == 0) { return; } //prevents an error if there are no pages
			List<county> newList = save.Counties.ToList(); //new list
			newList.RemoveAt(tabControl1.SelectedIndex); //removes county from list
			save.Counties = newList.ToArray(); //converts list to array
			DeleteAllTabs(); //resets everything so borders will be up to date
			InitializeApp();
		}

		//opens demographic GUI (with button)
		private void demoButton_Click(object sender, EventArgs e) {
			DemographicsWindow demoBox = new DemographicsWindow(this, save);
			demoBox.Show();
		}

		//runs after pressing Finish
		private void finishButton_Click(object sender, EventArgs e) {

			//tells the user the number of districts cannot be zero
			//this is to avoid a divide by zero error
			if (save.Districts == 0) {
				MessageBox.Show("The number of districts cannot be zero");
				return;
			}

			foreach (county County in save.Counties) {

				//tells the user that the population cannot be zero
				if (County.population == 0) {
					MessageBox.Show("The population of " + County.name + "cannot be zero");
					return;
				}

				//tells the user that a demographic cannot be larger than the population
				for (int i = 0; i < save.Demographics.Length; i++) {
					if (County.demo[i] > County.population) {
						MessageBox.Show("Demographic " + save.Demographics[i] + " cannot be greater than the population in " + County.name);
						return;
					}
				}

				//tells the user that every county must border at least one other county
				if (County.borders == null || County.borders.Length == 0) {
					MessageBox.Show(County.name + " must have at least one border");
					return;
				}
			}

			//switches to algorithm window
			Algorithm nextWindow = new Algorithm(save);
			nextWindow.Show();
			Close();
		}

		//runs when closing - only does anything if X button clicked
		private void MainControl_FormClosing(object sender, FormClosingEventArgs e) {
			if (sender == finishButton) { return; } //doesn't do anything unless it's X button

			//prompts user to save
			DialogResult answer = MessageBox.Show("Do you want to save?",
				"Save?",
				MessageBoxButtons.YesNoCancel,
				MessageBoxIcon.Warning);
			switch (answer) {

				//saves if prompted
				case DialogResult.Yes:
					save.Save();
					break;

				//does nothing if prompted
				case DialogResult.No:
					break;

				//cancels closing if prompted
				case DialogResult.Cancel:
					e.Cancel = true;
					break;
			}
			
		}

		//ends the application when X button clicked
		private void MainControl_FormClosed(object sender, FormClosedEventArgs e) {
			try {
				if (JsonConvert.SerializeObject(Application.OpenForms) == "[]") {
					Application.Exit();
				}
			}
			catch(Exception) { return; }
		}
	}
}
