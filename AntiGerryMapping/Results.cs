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
using System.Globalization;

namespace AntiGerryMapping {
	public partial class Results : Form {

		//inserts thousand seperators into a number
		private string TS(int number) {
			NumberFormatInfo nfi = new NumberFormatInfo { NumberGroupSeparator = ",", NumberDecimalDigits = 0 };
			return number.ToString("n", nfi);
		}

		//constructor
		public Results(scenario Scenario, SaveFile Save) {
			InitializeComponent();
			
			//create tabs for each district
			foreach (district District in Scenario.Districts) {

				TabPage newTab = new TabPage { // new tab
					Text = "District " + District.Number,
					AutoScroll = true
				};

				Label nextLabel = new Label { //Counties label
					Text = "Counties:",
					AutoSize = true,
					Location = new Point(3, 9)
				};
				newTab.Controls.Add(nextLabel);

				//county labels
				int yloc = 33; //location of next label
				foreach (county County in District.Counties) { //label for each county

					nextLabel = new Label { //new label
						Text = "· " + County.name,
						AutoSize = true,
						Location = new Point(3, yloc)
					};

					newTab.Controls.Add(nextLabel); //adds label to tab

					yloc += 24; //increases location of next label
				}

				nextLabel = new Label { //Population label
					Text = "Population: " + TS(District.Population()),
					AutoSize = true,
					Location = new Point(3, yloc)
				};
				newTab.Controls.Add(nextLabel);

				//demographic labels
				yloc += 24;
				for (int i = 0; i < Save.Demographics.Length; i++) {

					nextLabel = new Label {
						Text = Save.Demographics[i] + ": " + TS(District.GetDemo(i)),
						AutoSize = true,
						Location = new Point(3, yloc)
					};

					//indicates if a demographic is a majority in the region
					if (District.GetDemo(i) > District.Population() / 2) {
						nextLabel.Text = nextLabel.Text + " ✔";
					}

					yloc += 24;
					newTab.Controls.Add(nextLabel);
				}

				tabControl1.TabPages.Add(newTab); //adds new tab to tabControl
			}
		}

		//closes application when form is closed
		private void Results_FormClosed(object sender, FormClosedEventArgs e) {Application.Exit();}
	}
}
