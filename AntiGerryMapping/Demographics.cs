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

namespace AntiGerryMapping {

	public partial class DemographicsWindow : Form {

		int yloc;
		bool AddingBox;
		MainControl main;
		SaveFile saveFile;

		private string[] LoadDemographics() {
			SaveFile save = new SaveFile(@"../../counties.json");
			return save.Demographics;
		}

		//creates text boxes
		private void InitializeApp() {

			string[] demographics = saveFile.Demographics;

			//SETS UP EXISTING DEMOGRAPHIC BOXES 
			foreach (string demographic in demographics) {

				TextBox boxy = new TextBox { //creates text box
					Text = demographic,
					Location = new Point(7, yloc),
					Size = new Size(300, 20),
					Anchor = ((AnchorStyles.Top | AnchorStyles.Right))
				};
				boxy.TextChanged += new EventHandler(TextChange); //adds method for text change
				groupBox1.Controls.Add(boxy); //adds text box to the component
				yloc += 27; //increases y-value of next box
			}

			TextBox box = new TextBox { //creates extra text box
				Text = "",
				Location = new Point(7, yloc),
				Size = new Size(300, 20),
				Anchor = ((AnchorStyles.Top | AnchorStyles.Right))
			};
			box.TextChanged += new EventHandler(TextChange);
			groupBox1.Controls.Add(box);
			yloc += 27;
			button1.Location = new Point(8, yloc); //puts "Finish" button at bottom of the screen

		}

		//runs on opening
		public DemographicsWindow(MainControl mainWindow, SaveFile file) {

			//definitions
			yloc = 7; //starting point of textboxes
			AddingBox = false; //allows for adding new demographics
			main = mainWindow; //allows to refer back to the original

			//set up save
			List<county> counties = new List<county>();
			List<string> demographics = new List<string>();
			int Districts = file.Districts;
			foreach (county County in file.Counties) { counties.Add(new county(County.name, County.borders, County.population, County.demo)); }
			foreach (string Dem in file.Demographics) { demographics.Add(Dem); }
			saveFile = new SaveFile(counties.ToArray(), demographics.ToArray(), Districts); //allows for editing save

			//initiation
			InitializeComponent();
			InitializeApp();
		}

		//called when text in a box changes
		private void TextChange(object sender, EventArgs e) {

			if (!AddingBox) {

				AddingBox = true; //prevents extra boxes from being concieved

				//checks to see if all boxes are filled
				bool boxesFilled = true;
				foreach (TextBox boxy in groupBox1.Controls) {
					if (string.IsNullOrEmpty(boxy.Text)) { boxesFilled = false; }
				}

				//adds new box if necessary
				if (boxesFilled) {
					TextBox boxy = new TextBox { //creates text box
						Text = "",
						Location = new Point(7, yloc),
						Size = new Size(293, 20),
						Anchor = ((AnchorStyles.Top | AnchorStyles.Left))
					};
					boxy.TextChanged += new EventHandler(TextChange);
					groupBox1.Controls.Add(boxy);
					yloc += 27;
					button1.Location = new Point(8, yloc);
				}

				AddingBox = false;
			}
		}

		//runs when "Finish" button clicked
		private void button1_Click(object sender, EventArgs e) {

			//gets old lists
			List<county> counties = new List<county>(); 
			List<string> oldinfo = new List<string>();
			foreach(string dem in saveFile.Demographics) { oldinfo.Add(dem); }
			foreach (county County in saveFile.Counties) { counties.Add(new county(County.name, County.borders, County.population, County.demo)); }

			//creates list of demographics
			List<string> demographics = new List<string>(); //list of demographics
			foreach (TextBox boxy in groupBox1.Controls) {
				if (!string.IsNullOrEmpty(boxy.Text)) {
					demographics.Add(boxy.Text); //adds to list, if the box isn't empty
				}
			}
			saveFile.Demographics = demographics.ToArray(); //puts new demographics into save

			//initializes demo arrays in save (so there's no overloading)
			List<int> initialdemo = new List<int>();
			foreach (string dem in demographics) { initialdemo.Add(0); } //creates lists
			for (int i = 0; i < counties.Count; ++i) {
				saveFile.Counties[i].demo = initialdemo.ToArray(); //sets array to list
			}

			//resolves deltas
			for (int i = 0; i < demographics.Count; ++i) {

				//places old numbers in new indexes if a demographic moved
				string dem = demographics[i];
				if (oldinfo.Contains(dem)) {
					
					//replaces information in save for each county
					int index = oldinfo.IndexOf(dem);
					for (int a = 0; a < counties.Count; ++a) {
						saveFile.Counties[a].demo[i] = counties[a].demo[index];
					}
				}
			}

			//commits suicide
			main.UpdateDemographics(saveFile);
			Close();
		}
	}
}
