﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GA.Lib;
using GA.Lib.Chromosome;
using GA.Lib.Gene;
using GA.Lib.Population;
using GA.SC;

namespace GA.SC.UI.Sublayouts {
	public partial class Dev : System.Web.UI.UserControl {

		/* 
		 * populations can be used just like levels of navigation. create one at a top level and each 
		 * could have it's own population as subcategories. visually stored in a tree or bucket but just in a sense of 
		 * giving depth to the category itself. for example you like shoes (top level chromosome) but you prefer sandals 
		 * and sneakers (second level)
		 * 
		 * may want to change how the genotypelist works by collecting "Type" objects and having a default constructor that takes in an object
		 * to be able to create new objects and set the datasource.
		 * not sure which is more advantageous; to set the dominance automatically or manually
		 * 
		 * */

		private StringBuilder sbOut = new StringBuilder();

		protected List<Literal> Placeholders;
		protected List<Button> Buttons;
		protected List<Panel> Panels;

		protected List<KeyValuePair<int, string>> Chromosomes = new List<KeyValuePair<int, string>>();

		protected IKaryotype CurrentKaryotype;

		protected DefaultPopulationManager apo = new DefaultPopulationManager();

		protected void Page_Load(object sender, EventArgs e) {

			//setup accessors
			Placeholders = new List<Literal>() { ltlOne, ltlTwo, ltlThree, ltlFour };
			Panels = new List<Panel>() { pnlOne, pnlTwo, pnlThree, pnlFour };
			Buttons = new List<Button>() { btnA, btnB, btnC, btnD, btnE, btnF, btnG };
			
			//setup chromosomes
			Chromosomes.Add(new KeyValuePair<int, string>(Placeholders.Count, "pageContent")); 
			
			//setup population options
			apo.PopulationType = Type.GetType("GA.SC.SCPopulation,GA.SC");
			apo.KaryotypeType = Type.GetType("GA.SC.PageKaryotype,GA.SC");
			foreach (KeyValuePair<int, string> c in Chromosomes) {
				Genotype g = new Genotype();
				//number of genes corresponds to the number of placeholders to fill with display content
				g.GeneLimit = c.Key;
				for (int z = 0; z < Buttons.Count; z++) { //add all the tags to the genotype
					TagGene t = new TagGene(Buttons[z].Text, true);
					g.Add(t);
				}
				apo.Genotype.Add(c.Value, g);
			}
			
			//run pop... or let events run it
			if (!IsPostBack) {

				//default options
				txtCrossover.Text = apo.CrossoverRatio.ToString();
				txtElitism.Text = apo.ElitismRatio.ToString();
				txtFitness.Text = apo.FitnessRatio.ToString();
				txtMutation.Text = apo.MutationRatio.ToString();
				txtTourney.Text = apo.TourneySize.ToString();
				txtPopSize.Text = apo.PopSize.ToString();

				RunAlgo();
			} else {
				apo.CrossoverRatio = float.Parse(txtCrossover.Text);
				apo.ElitismRatio = float.Parse(txtElitism.Text);
				apo.FitnessRatio = float.Parse(txtFitness.Text);
				apo.MutationRatio = float.Parse(txtMutation.Text);
				apo.TourneySize = int.Parse(txtTourney.Text);
				apo.PopSize = int.Parse(txtPopSize.Text);
			}

			ltlOut.Text = sbOut.ToString();
		}

		protected void RunAlgo() {

			//get or create the population
			SCPopulation p = SCPopulation.GetPop(apo);

			//list the chromosomes
			List<IKaryotype> u = p.GetUniqueKaryotypes();
			rptDNAList.DataSource = u;
			rptDNAList.DataBind(); 

			//choose best
			CurrentKaryotype = p.ChooseFitKaryotype();
			ltlKaryotype.Text = string.Format("{0}-{1}", CurrentKaryotype.ExpressedHaploid.DNASequence(), CurrentKaryotype.Fitness);

			//wire up renderings with results
			for (int z = 0; z < Placeholders.Count; z++) {
				string gid = CurrentKaryotype.ExpressedHaploid[Chromosomes[0].Value][z].GeneID;
				Placeholders[z].Text = gid;
				Panels[z].Attributes["style"] = string.Format("background-color:{0};", gid);
			}

			//list karyotype data
			ltlKaryos.Text = p.Karyotypes.Count.ToString();
			ltlUKaryos.Text = u.Count.ToString();

			//list all engagement values stored
			rptEV.DataSource = EngagementValue.KnownValues;
			rptEV.DataBind();

			//evolve
			p.Evolve();
		}

		#region Log

		protected void Log() {
			Log(string.Empty);
		}

		protected void Log(string str) {
			Log(str, new string[0]);
		}

		protected void Log(string str, params object[] args) {
			sbOut.AppendLine("<br/>").AppendFormat(str, args);
		}

		#endregion Log

		protected string OddEven(int i) {
			return (i % 2 == 0) ? "odd" : "even";
		}

		/// <summary>
		/// This adds to engagement value by the tag that was clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btn_Click(object sender, EventArgs e) {

			//update clicks
			Button b = (Button)sender;
			string key = b.Text; // string.Format("ltl{0}-{1}", b.CssClass, b.Text);
			if (!EngagementValue.KnownValues.ContainsKey(key))
				EngagementValue.KnownValues.Add(key, new List<EngagementValue>());
			EngagementValue.KnownValues[key].Add(new EngagementValue(1));
			
			//run algo
			RunAlgo();
		}

		protected void btnClearEvents_Click(object sender, EventArgs e) {
			EngagementValue.KnownValues.Clear();
			RunAlgo();
		}

		protected void btnNextGen_Click(object sender, EventArgs e) {
			RunAlgo();
		}

		protected void btnRestart_Click(object sender, EventArgs e) {
			SCPopulation.RestartPop(apo);
			RunAlgo();
		}
	}
}