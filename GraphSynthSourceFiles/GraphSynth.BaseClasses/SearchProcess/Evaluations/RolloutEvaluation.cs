using GraphSynth.Representation;
using System;
using System.Collections.Generic;

namespace GraphSynth.Search.Evaluations {
    public class RolloutEvaluation : AbstractEvaluation {  
        private readonly Random _rnd = new Random();
        
        /// <summary>
        /// Implements a function for performing random rollout to the given parameters.
        /// </summary>
        /// <param name="width">How many rollouts to run.</param>
        /// <param name="depth">How deep each rollout should go.</param>
        public RolloutEvaluation(int width, int depth) : base(width, depth) {
        }

        public double Evaluate(candidate state) {
            double reward = 0;
            for (var i = 0; i < Width; i++)  // do Width simulations
                reward += RunRollout(state);
            
            return reward / Width;
        }

        /// <summary>
        /// Do one rollout for the state.
        /// </summary>
        public double RunRollout(candidate state) {
            double reward = 0;
            var simState = state.copy();
            for (var i = 0; i < Depth; i++) {
                if (simState.IsTerminal()) break;
                reward += simState.ApplyRule(ChooseRandom(simState));
            }
            
            return reward;
        }

        /// <summary>
        /// Randomly select a valid rule for state.
        /// </summary>
        private option ChooseRandom(candidate state) {
            List<option> options = state.getOptions();
            return options[_rnd.Next(options.Count)];
        }
    }
}