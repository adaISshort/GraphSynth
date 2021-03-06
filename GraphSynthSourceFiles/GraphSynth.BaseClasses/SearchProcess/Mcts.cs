﻿/*************************************************************************
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
        /// <param name="maxWidth">Maximum number of pulls to perform at any given node for an action.</param>
        /// <param name="evaluation">Method for evaluating leaf nodes.</param>
        public Mcts(designGraph seed, ruleSet[] rulesets, int[] numCalls, bool display, int depth, int numTrials,
            int maxWidth, Delegate evaluation)
            : base(seed, rulesets, numCalls, display) {
            // TODO parameterize default values
            _depth = 5;
            _numTrials = 1000;
            _maxWidth = 1;
            _evaluation = evaluation;
        }

        #endregion

        /// <summary>
        /// Chooses the specified options. Given the list of options and the candidate,
        /// determine what option to invoke. Return the integer index of this option from the list.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cand">The cand.</param>  TODO clarify
        /// <returns></returns>
        public override int[] choose(List<option> options, candidate cand) {
            if (_depth == 0 || options.Count == 0)
                return new[] { 0 }; // there's nothing left to do

            var rootNode = new BanditNode(cand, 0, options, new EGreedyBandit(options.Count, .5)); // default to .5 for epsilon

            for (var i = 0; i < _numTrials; i++)
                RunTrial(rootNode, _depth);

            return new[] { rootNode.Bandit.GetBestArm() }; // return index of best arm we've found
        }

        private double RunTrial(BanditNode node, int depth) {
            if (depth == 0) // leaf node
                return (double) _evaluation.DynamicInvoke(node.State);

            var optionIndex = node.Bandit.SelectPullArm();
            double totalReward;
            // If we reach max child nodes, then select randomly among children according to how much we've visited
            if (node.Children[optionIndex].Count >= _maxWidth) {
                var keys = new List<candidate> (node.Children[optionIndex].Keys);
                var successorIndex = node.Multinomial(optionIndex);
                var successorNode = node.Children[optionIndex][keys[successorIndex]].Node;
                totalReward = successorNode.TransitionReward + RunTrial(successorNode, depth - 1);
            } else {
                // generate a new successor node (assume reward is 0)
                var successorState = node.State.copy();
                // TODO get immediate reward, even if it's 0 here
                node.Options[optionIndex].apply(successorState.graph, null); // TODO possible error

                // If the successor state is already in node.Children
                if (node.Children[optionIndex].ContainsKey(successorState)) {
                    var successorNode = node.Children[optionIndex][successorState].Node;
                    node.Children[optionIndex][successorState].Visits += 1; // mark that we've sampled
                    
                    totalReward =  RunTrial(successorNode, depth - 1);
                } else {
                    // TODO trying to find options applicable for given candidate - only using first ruleset
                    var successorOptions = Rulesets[0].recognize(successorState.graph, InParallel);
                    var successorNode = new BanditNode(successorState, 0, successorOptions, 
                        new EGreedyBandit(successorOptions.Count, .5));
                    node.Children[optionIndex][successorState] = new BanditNode.NodeCountTuple(successorNode);
                    totalReward = (double) _evaluation.DynamicInvoke(successorState);  // TODO add immediate reward back in
                }
            }
            
            node.Bandit.Update(optionIndex, totalReward);
            return totalReward;
        }
    }

    /// <summary>
    /// Stores information on a state, the reward for reaching the state, the options available, and the bandit used.
    /// </summary>
    public class BanditNode {
        private readonly Random _rnd = new Random();

        public candidate State;
        public List<option> Options;
        public AbstractBandit Bandit;
        public double TransitionReward;

        /// <summary>
        /// Each action is associated with a dictionary that stores successor bandits/states.
        /// The key for each successor is the state. 
        /// The value is a tuple [n,c], where n is a BanditNode and c is the number of times that n has been sampled.
        /// </summary>
        public List<Dictionary<candidate, NodeCountTuple>> Children;

        public class NodeCountTuple {
            public BanditNode Node;
            public int Visits = 1; // should be initialized on the first visit

            public NodeCountTuple(BanditNode node) {
                Node = node;
            }
        }

        public BanditNode(candidate state, double transitionReward, List<option> options, AbstractBandit bandit) {
            State = state;
            TransitionReward = transitionReward;
            Options = options;
            Bandit = bandit;
        }

        /// <summary>
        /// Samples the multinomial for the weighted number of visits to each child of the specified option index.
        /// </summary>
        public int Multinomial(int index) {
            var keys = new List<candidate> (Children[index].Keys); 
            var counts = keys.Select(k => Children[index][k].Visits).ToList();
            var countSum = counts.Sum();
            // List of counts proportional to number of times each child was sampled
            var averagedCounts = counts.Select(c => (double) c / countSum).ToList();

            var randVal = _rnd.NextDouble();
            double current = 0;
            for (var i = 0; i < averagedCounts.Count; i++) {
                current += averagedCounts[i];
                if (randVal <= current)
                    return i;
            }
            return -1; // should never reach this (included for compilation purposes)
        }
    }
}