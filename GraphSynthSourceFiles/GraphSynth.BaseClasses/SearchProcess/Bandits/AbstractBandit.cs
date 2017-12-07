using System;
using System.Linq;


namespace GraphSynth.Search.Bandits {
    public class AbstractBandit {
        protected readonly int NumArms;
        protected int TotalPulls;
        public int[] numPulls; // how many times we've pulled each arm
        public double[] averageReward;

        /// <summary>
        /// Initialize a new AbstractBandit.
        /// </summary>
        /// <param name="umArms_">How many actions are available at the corresponding state.</param>
        public AbstractBandit(int numArms_) {
            NumArms = numArms_;
            averageReward = new double [NumArms]; // rewards initialized to 0
            numPulls = new int [NumArms];
        }

        /// <summary>
        /// Update the given arm's pull count, its average reward, and the total pull count.
        /// </summary>
        /// <param name="arm">The index of the arm to update.</param>
        /// <param name="reward">The observed reward.</param>
        public void Update(int arm, int reward) {
            numPulls[arm]++;
            averageReward[arm] = (averageReward[arm] * (numPulls[arm] - 1) + reward) / numPulls[arm];
            TotalPulls++;
        }

        /// <summary>
        /// Get the arm with the best average reward.
        /// </summary>
        public int GetBestArm() {
            return Array.IndexOf(averageReward, GetBestReward());  // TODO makes two passes
        }

        /// <summary>
        /// Return the best average reward.
        /// </summary>
        public double GetBestReward() {
            return averageReward.Max();
        }

        /// <summary>
        /// Selects and returns a pull arm pursuant to the details of the bandit algorithm.
        /// </summary>
        public int SelectPullArm() {
            throw new NotImplementedException();
        }
    }
}