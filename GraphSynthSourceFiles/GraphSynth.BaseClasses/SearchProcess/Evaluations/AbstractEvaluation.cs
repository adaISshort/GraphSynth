using System;

namespace GraphSynth.Search.Evaluations {
    /// <summary>
    /// The main class for implementing state evaluation functions.
    /// </summary>
    public class AbstractEvaluation {
        protected int Width;
        protected int Depth;
        
        public AbstractEvaluation(int width, int depth) {
            Width = width;
            Depth = depth;
        }
        
        /// <summary>
        /// Using the predefined policy/policies, width, and depth, evaluates the state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public double evaluate(object state) {
            throw new NotImplementedException();
        }
    }
}