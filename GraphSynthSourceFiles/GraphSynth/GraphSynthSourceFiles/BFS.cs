﻿using System;
using System.Collections.Generic;
﻿using System.Data;
﻿using System.Linq;
using System.Data.Linq;
using System.Text;
﻿using System.Threading;
﻿using System.Windows.Navigation;
﻿using GraphSynth;
using GraphSynth.Representation;
using GraphSynth.Search;
using System.Threading.Tasks;
using PMKS;

namespace GraphSynth.Search
{
    public class BFS : SearchProcess
    {
        public BFS()
        {
            AutoPlay = true;
        }
        public override string text
        {
            get { return ("BFS"); }
        }

        protected override void Run()
        {
            int num_of_links = 10;
            int num_of_pjoints = 0;
            int num_of_rpjoints = 0;
            int num_of_groundjoints =5;
            bool contain_indep_4bar = false;

            var candidates = new Queue<candidate>();
            candidate current = null;
            Boolean found = false;
            candidates.Enqueue(seedCandidate);
            double k = 1;

            // no idea what this does 
            var results = new List<candidate>();
            var setting = new GlobalSettings();
            var sa = new BasicFiler(setting.InputDir, setting.OutputDir, setting.RulesDir);
            sa.outputDirectory = outputDirectory;
//start
            //While not found and candidates remain 
            while (!found && !SearchIO.terminateRequest && candidates.Count != 0)
            {
                //Remove current candidate 
                current = candidates.Dequeue();
                //Count candidates remaining 
                SearchIO.miscObject = candidates.Count;
                //Count applicable rules 
                SearchIO.iteration = current.recipe.Count;


                #region find all results
                //Put in rules of evaluation here, and should run 
                if (isCurrentTheGoal(current, num_of_links, num_of_pjoints, num_of_rpjoints, num_of_groundjoints))
                {
                    //
                    if (current.recipe.Exists(a => a.rule.name.Equals("Detect_independ_4bar")))
                        continue;
                    //Active rule set selects current rule set 
                    var testrsIndex = current.activeRuleSetIndex;
                    var testruleChoices = rulesets[testrsIndex].recognize(current.graph);
                    if (testruleChoices.Exists(a => a.rule.name.Equals("Detect_independ_4bar")) &&
                        current.graph.nodes.Count == num_of_links)
                        continue;
                    if (!results.Exists(c => SameIsomorphism(c, current)))
                    {
                        results.Add(current);
                    }
                }
                #endregion

                else if (current.graph.nodes.Count > num_of_links)
                    continue;
                int rsIndex = current.activeRuleSetIndex;
                //Recognize any rule on the current graph 
                var ruleChoices = rulesets[rsIndex].recognize(current.graph);
                if (ruleChoices.Exists(a => a.rule.name.Equals("Detect_independ_4bar")) &&
                    current.graph.nodes.Count < num_of_links)
                    ruleChoices.RemoveAll(a => a.rule.name.Equals("Detect_independ_4bar"));
                if (current.graph.nodes.Count == num_of_links)
                {
                    ruleChoices.RemoveAll(a => a.rule.name.Equals("RRR"));
                    ruleChoices.RemoveAll(a => a.rule.name.Equals("Triad"));
                }
                PruneDuplicateOptions(ruleChoices);

                // for each option in the active rule in the ruleset
                // option is the active rule 
                foreach (var opt in ruleChoices)

                    //Applies the rules to the child
                {
                    candidate child = current.copy();
                    // TransferLmapping to seed to genereate children, as seen her for child 
                    transferLmappingToChild(child.graph, current.graph, opt);
                    opt.apply(child.graph, null);
                    child.addToRecipe(opt);
                    if (child.graph.nodes.Count > num_of_links)
                        continue;
                    var testrsIndex = child.activeRuleSetIndex;
                    var testruleChoices = rulesets[testrsIndex].recognize(child.graph);

                    if (testruleChoices.Exists(a => a.rule.name.Equals("Detect_independ_4bar")) && current.graph.nodes.Count == num_of_links)
                        continue;
                    
                    if (candidates.Any(c => SameIsomorphism(c, child)))
                    {
                        SearchIO.output("copy found", 4);
                    }
                    else
                        candidates.Enqueue(child);
                }
            }
            SearchIO.output("Completed! Found " + results.Count + " isomorphisms.");
            SearchIO.output("now saving...");
            foreach (var cand in results)
            {   
           
                //cand.graph.nodes.Find(n => n.name == "n0").localLabels.Add("ground");
                //cand.graph.arcs.Find(n => n.name == "a1").localLabels.Add("input");
                var converter1 = new GraphToPMKSDataConverter2(cand.graph);
                converter1.savePMKSDataFile(outputDirectory + k + ".txt");
                sa.Save(outputDirectory + k + ".gxml", cand.graph, false);
                k++;
            }
        }

        private bool SameIsomorphism(candidate a, candidate b)
        {
            designGraph c, d;
            //remove lables
            var n1 = a.graph.nodes.FindIndex(n => n.localLabels.Contains("ground"));
            var a1 = a.graph.arcs.FindIndex(n => n.localLabels.Contains("input"));
            if (n1 == -1 && a1 == -1)
                c = a.graph;
            else
            {
                c = a.graph.copy(true);
                c.nodes[n1].localLabels.Remove("ground");
                c.arcs[a1].localLabels.Remove("input");
            }
            n1 = b.graph.nodes.FindIndex(n => n.localLabels.Contains("ground"));
            a1 = b.graph.arcs.FindIndex(n => n.localLabels.Contains("input"));
            if (n1 == -1 && a1 == -1)
                d = b.graph;
            else
            {
                d = b.graph.copy(true);
                d.nodes[n1].localLabels.Remove("ground");
                d.arcs[a1].localLabels.Remove("input");
            }

            return (c.Equals(d, true));
        }


        private void PruneDuplicateOptions(List<option> options)
        {
            for (int i = options.Count - 1; i > 0; i--)
            {
                var duplicateFound = false;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (DuplicateOptions(options[i], options[j]))
                    {
                        duplicateFound = true;
                        break;
                    }
                }
                if (duplicateFound) options.RemoveAt(i);
            }
        }

        private bool DuplicateOptions(option o1, option o2)
        {
            if (o1.ruleSetIndex != o2.ruleSetIndex) return false;
            if (o1.ruleNumber != o2.ruleNumber) return false;
            if (o1.nodes.Union(o2.nodes).Count() != o2.nodes.Count) return false;
            return true;

        }

        private bool isCurrentTheGoal(candidate current, int a, int b, int c, int d)
        {
            var g = current.graph.nodes.Where(j => j.localLabels.Contains("ground"));
            var numlinks = current.graph.nodes.Count(j => j.localLabels.Contains("link"));
            var numP = current.graph.arcs.Count(j => j.localLabels.Contains("p"));
            var numRP = current.graph.arcs.Count(j => j.localLabels.Contains("rp"));
            var numj = current.graph.arcs.Count;
            var numG = 0;

            foreach (var VARIABLE in g)
            {
                numG = VARIABLE.degree;
            }

            if (numlinks == a && numP <= b && numRP <= c && numG <= d)
            {
                return true;
            }

            return false;
        }
    }
}
