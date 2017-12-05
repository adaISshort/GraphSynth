using System;

namespace GraphSynth.Search.Bandits {
    public class EGreedyBandit : AbstractBandit {
        private double _epsilon;
        private readonly Random _rand = new Random();

        public EGreedyBandit(int numArms_, double epsilon_) : base(numArms_) {
            _epsilon = epsilon_;
        }

        /// <summary>
        /// Pulls the most rewarding arm with (1 - epsilon) probability; otherwise, a non-optimal arm is pulled at random.
        /// </summary>
        public int SelectPullArm() {
            if (numArms <= 1) {
                return 0; // return the only arm we can
            }

            if (totalPulls < numArms) {
                return totalPulls; // haven't tried each arm at least once yet
            }

            var bestArm = GetBestArm();
            if (_rand.NextDouble() < _epsilon) {
                var nonBest = _rand.Next(numArms);
                while (nonBest == bestArm) {
                    nonBest = _rand.Next(numArms);
                }
                return nonBest; // pull a random non-best arm
            } 
            
            return bestArm;
        }
    }
}