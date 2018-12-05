using System;

namespace Feladat5 {
    class RandomAvgTask : Task {

        private int n;

        public RandomAvgTask(int color, int n) : base(color) {
            this.n = n;
        }

        public override void Run() {
            var rand = new Random(42);
            long sum = 0;
            for (int i = 0; i < n; i++)
                sum += rand.Next();
            long avg = sum / n;
        }

        public override string ToString() {
            return $"RandomAvgTask{{ID={ID},Color={Color},n={n}}}";
        }
    }
}