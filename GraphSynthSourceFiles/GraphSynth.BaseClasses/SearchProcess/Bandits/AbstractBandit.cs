using System.Linq;


namespace GraphSynth.Search.Bandits {
    public class AbstractBandit {
        public int numArms;
        public int totalPulls;
        public int[] numPulls; // how many times we've pulled each arm
        public double[] averageReward;

        /// <summary>
        /// Initialize a new AbstractBandit.
        /// </summary>
        /// <param name="numArms_">How many actions are available at the corresponding state.</param>
        public AbstractBandit(int numArms_) {
            numArms = numArms_;
            averageReward = new double [numArms]; // rewards initialized to 0
            numPulls = new int [numArms];
        }

        /// <summary>
        /// Update the given arm's pull count, its average reward, and the total pull count.
        /// </summary>
        /// <param name="arm">The index of the arm to update.</param>
        /// <param name="reward">The observed reward.</param>
        public void Update(int arm, int reward) {
            numPulls[arm]++;
            averageReward[arm] = (averageReward[arm] * (numPulls[arm] - 1) + reward) / numPulls[arm];
            totalPulls++;
        }

        /// <summary>
        /// Get the arm with the best average reward.
        /// </summary>
        public int GetBestArm() {
            var max = double.NegativeInfinity;
            var maxArm = 0;
            for (var i = 0; i < numArms; i++) {
                if (averageReward[i] > max) {
                    max = averageReward[i];
                    maxArm = i;
                }
            }

            return maxArm;
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