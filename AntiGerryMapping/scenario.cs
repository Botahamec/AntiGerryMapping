using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiGerryMapping {

	public class scenario {

		public district[] Districts { get; set; }

		//constructor
		public scenario(district[] districts) {
			Districts = districts;
		}

	}

}
