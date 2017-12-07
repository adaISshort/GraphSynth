/*************************************************************************
 *     This Mcts file & class is part of the GraphSynth.
 *     BaseClasses Project which is the foundation of the GraphSynth Ap-
 *     plication. GraphSynth.BaseClasses is protected and copyright under 
 *     the MIT License.
 *     Copyright (c) 2011 Matthew Ira Campbell, PhD.
 *
 *     Permission is hereby granted, free of charge, to any person obtain-
 *     ing a copy of this software and associated documentation files 
 *     (the "Software"), to deal in the Software without restriction, incl-
 *     uding without limitation the rights to use, copy, modify, merge, 
 *     publish, distribute, sublicense, and/or sell copies of the Software, 
 *     and to permit persons to whom the Software is furnished to do so, 
 *     subject to the following conditions:
 *     
 *     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 *     EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
 *     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGE-
 *     MENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
 *     FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 *     CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
 *     WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://www.GraphSynth.com.
 *************************************************************************/

using GraphSynth.Search.Bandits;
using GraphSynth.Representation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSynth.Search {
    /// <summary>
    /// An overload for the RCA class that uses MCTS to choose the next action.
    /// </summary>
    public class Mcts : RecognizeChooseApply {
        private readonly Random _rnd = new Random();

        private int _depth;
        private int _numTrials;
        private int _maxWidth;

        private Delegate _evaluation;
        private readonly Delegate _bandit;
        private object[] _banditParams;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcts"/> class.
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <param name="rulesets">The rulesets.</param>
        /// <param name="numCalls">The num calls.</param>
        /// <param name="display">If set to <c>true</c> [display].</param>
        /// <param name="depth">How many levels to recurse into the constructed tree.</param>
        /// <param name="numTrials">How many times to recurse down the root node (AKA get new information).</param>
        /// <param name="maxWidth">Maximum number of pulls to perform at any given node.</param>
        /// <param name="evaluation">Method for evaluating leaf nodes.</param>
        /// <param name="bandit">Constructor for bandit which dictates how we select arms to pull.</param>
        /// <param name="banditParams">Options for creating the bandit.</param>
        public Mcts(designGraph seed, ruleSet[] rulesets, int[] numCalls, bool display, int depth, int numTrials,
            int maxWidth, Delegate evaluation, Delegate bandit, params object[] banditParams)
            : base(seed, rulesets, numCalls, display) {
            _depth = depth;
            _numTrials = numTrials;
            _maxWidth = maxWidth;
            _evaluation = evaluation;
            _bandit = bandit;
            _banditParams = banditParams;
        }

        #endregion

        /// <summary>
        /// Chooses the specified options. Given the list of options and the candidate,
        /// determine what option to invoke. Return the integer index of this option from the list.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cand">The cand.</param>  TODO clarify
        /// <returns></returns>
        public override int choose(List<option> options, candidate cand) {
            if (_depth == 0 || options.Count == 0) {
                return 0; // there's nothing left to do
            }

            var rootNode = new BanditNode(cand, 0, options, _bandit.DynamicInvoke(_banditParams));

            for (var i = 0; i < _numTrials; i++)
                runTrial(rootNode, _depth);

            return rootNode.bandit.getBestArm();  // return index of best arm we've found
        }


        /// <summary>
        /// Chooses the specified option. Given that the rule has now been chosen, determine
        /// the values needed by the rule to properly apply it to the candidate, cand. The
        /// array of double is to be determined by parametric apply rules written in
        /// complement C# files for the ruleSet being used.
        /// </summary>
        /// <param name="opt">The opt.</param>
        /// <param name="cand">The cand.</param>
        /// <returns></returns>
        public override double[] choose(option opt, candidate cand) {
            return null;
        }
    }
}