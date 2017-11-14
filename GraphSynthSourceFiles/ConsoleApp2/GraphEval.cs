using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GraphSynth.Representation;
using GraphSynth.Search;
using GraphSynth;

namespace ConsoleApp2
{
    class GraphEval:SearchProcess
    {
        //Trivial changes 

        // Variable that tracks score of a graph
        // Should be local to this class 
        // should be a double
        private double ColorScore;

        //Number of rules called, 
        public static int[] numOfCalls;
  
        //Displays RandomChoose.cs something? 
        public Boolean display;

        // Random number for assigning values 
        // Max and Min values in range 
        private int Min = -5;
        private int Max = 5;
        // A random number 
        public static Random random = new Random();
        // The number of "colors" or node labels 
        private double rnbw=6;
        // Goal Properties 
        public static double[] GOAL = new double[] { 5, -2, 11 };
        // The three properties are generated as 3 6 by 6 arrays 
        public static double[,,] AGP = new double[3, 6,6] { {{ -2, 3, 2, 0, 1, -2 },
                { 3, -2, 2, -2, 3, 2 }, 
                { 2, 2, -1, 1, 3, 0 }, 
                { 0, -2, 1, 2, 3, -3 }, 
                { 1, 3, 3, 3, -1, -1 }, 
                { -2, 2, 0, -2, -1, -2 } },
            { { -2, 0, 1, -3, 2, -2 },
                { -2, 1, -2, 0, -2, -3 },
                { 2, -2, 0, 1, -1, 2 },
                { 2, 2, -1, 2, 2, -1 },
                { 1, 3, -2, -2, -2, 0 },
                { -1, -1, 3, -1, 2, -3 } },
            { { -2, 0, -2, 3, -1, 1 }, 
                { 0, 2,-1, 1, 0, 2 }, 
                { -1, -1, 1, 3, 1, -1}, 
                { 0, -2, -2, 1, 3, 2 }, 
                { 0, -2, 2, 1, 2, 2 }, 
                { -3, 2, -2, -1, -1, 2 } }};

        // An array to keep track of design choices and evaluate them,
        // super temporary to get crappy GA working 
        public static int genSZ = 6;
        public static double[,] smirkString = new double[6, genSZ];
        public static string[] nodeColors = new string[6] { "red", "orange", "yellow", "green", "blue", "violet" };
        // Score as closeness in 3D 
        public static double[] Objtv3d = new double[genSZ];
        //public static double[,] beta = new double[6, 6];
        //public static double[,] gamma = new double[6, 6];

        public override string text
        {
            get
            {
                return "Ada's plugin!";
            }
        }

        /// <summary>
        /// This should open a graph in graphsynth, 
        /// Evaluate the graph, 
        /// Sum a score based on properties (probably assigned in a different class, 
        /// and Report back the score
        /// </summary>
        static void Main(string[] args)
        {
            ////Generate random properties alpha, beta, and gamma
            //for (int i = 0; i < 6; i++)
            //{
            //    for (int j = 0; j < 6; j++)
            //    {
            //        alpha[i, j] = random.NextDouble();
            //        beta[i, j] = random.NextDouble();
            //        gamma[i, j] = random.NextDouble();
            //    }
            //}
            var filer = new GraphSynth.BasicFiler("", "", "");
            var graph = (designGraph)(filer.Open("test.gxml")[0]);

            double[] objfun = EvaluateGraph(graph,genSZ);
            Console.WriteLine(objfun);
            //Console.WriteLine(alpha);
        }

        /// <summary>
        /// This is the evaluate graph function 
        /// in theory it should count graphs with a node label
        /// and return a summed value
        /// will later be expanded to include more complex operations
        /// </summary>
        private static double[] EvaluateGraph(designGraph graph, int offspring)
        {
            //Set ColorScore to 0 when Evaluate Graph is called 
            double[] ColorScore = { 0, 0, 0 };

            // Go through every node in the graph
            foreach (var node in graph.nodes)
            {
                // For every color listed in the string array nodeColors 
                for ( int g = 0;g < nodeColors.Length;g++ )
                {
                    // take action if the node has the label that match current color of interest 
                    if(node.localLabels.Contains(nodeColors[g]))
                    {
                        // Record color score 
                        ////this section is unfinished and uses place holder properties 
                        for (int i = 0; i < 3; i++)
                        {
                            ColorScore[i] = ColorScore[i] + AGP[i, g, g];
                        }
                        // Count the number of nodes of this color 
                        ////this is not a great way to keep track of this, but is maybe a thing for now 
                        smirkString[g, offspring] = smirkString[g, offspring] + 1;
                    }
                }
            }

            return ColorScore;
        }

        //Generate arbitrary properties
        //public static double ArbitraryPropertyGeneration(double[,,] ArbGenProp)
        //{
        //    for (int i = 0; i < 6; i++)
        //    {
        //        for (int j = 0; j < 6; j++)
        //        {
        //            for (int k = 0; k < 3; k++)
        //                ArbGenProp[i, j, k] = random.NextDouble();
        //        }
        //    }
        //    return ArbGenProp;
        //}

        protected override void Run()
        {
            //Dialog (stolen from RandomChooser.cs
            //var setupWin = new RandomStartDialog(this);

            //I need to change this to run multiple iterations of a design search
            //Should add a drop down menu for Graph Tree Searches 
            var filer = new GraphSynth.BasicFiler("", "", "");
            var graph = this.seedGraph;
            //Set iteration properties, replace later with a message prompt 
            int[] numOfCalls= { 10 };

            //RandomChooser.cs 
            for (int j =0;j<genSZ;j++)
            {
                var userChoose = new GAChooseRCA(seedGraph, rulesets, numOfCalls, display);
                var cand = userChoose.GenerateOneCandidate();
                SearchIO.addAndShowGraphWindow(cand.graph, "After Rule Application");
                //SaveResultDialog.Show(settings.filer, cand); was not added, find in RandomChooser.cs
                double[] objfun = EvaluateGraph(cand.graph,j);
                //Calculate proximity 
                Objtv3d[j] = Math.Sqrt(Math.Pow(GOAL[0] - objfun[0], 2) + Math.Pow(GOAL[1] - objfun[1], 2) + Math.Pow(GOAL[2] - objfun[2], 2));
                //Report Results
                Console.WriteLine(objfun[0]);
                //State offspring 
                SearchIO.output("Offspring " + (j + 1));
                //Display Score
                SearchIO.output(Objtv3d[j]);
                //Display smirkString 
                for (int k=0; k<6;k++)
                {
                    SearchIO.output(smirkString[k, j]);

                }
                
            }

            // Find and display closest result 
            double lowestScore = Objtv3d.Min();
            int closestGraph = Objtv3d.ToList().IndexOf(lowestScore);
            SearchIO.output("Best");
            SearchIO.output(closestGraph+1);

            //double[] objfun = EvaluateGraph(cand.graph);

            ////Calculate proximity 
            //double Objtv3d = Math.Sqrt(Math.Pow(GOAL[0] - objfun[0], 2) + Math.Pow(GOAL[1] - objfun[1], 2) + Math.Pow(GOAL[2] - objfun[2], 2));
            ////Report Results
            //Console.WriteLine(objfun[0]);
            //SearchIO.output(Objtv3d);
            ////SearchIO.output(objfun[0],objfun[1],objfun[2]);
            //SearchIO.output(objfun[0]);
            //SearchIO.output(objfun[1]);
            //SearchIO.output(objfun[2]);

        }
    }
}
