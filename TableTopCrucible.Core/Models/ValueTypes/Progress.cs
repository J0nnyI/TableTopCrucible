namespace TableTopCrucible.Core.ValueTypes
{
    public struct Progress
    {
        public Progress(int min, int value, int max)
        {
            this.Min = min;
            this.Value = value;
            this.Max = max;
        }
        public Progress(int progress, int taskCount)
        {
            this.Min = 0;
            this.Value = progress;
            this.Max = taskCount;
        }

        public int Min { get; }
        public int Value { get; }
        public int Max { get; }

        public Progress OnNextStep()
            => new Progress(Min, Value + 1, Max);
    }
}