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

namespace AntiGerryMapping
{
	public partial class MainControl : Form
	{
		public MainControl()
		{
			InitializeComponent();
		}

		private void finishButton_Click(object sender, EventArgs e) {

			Popper popper = new Popper();
			county[] counties = popper.LoadCounties();

		}
	}
}
