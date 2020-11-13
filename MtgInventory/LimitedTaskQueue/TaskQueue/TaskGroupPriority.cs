namespace TaskQueue
{
    public sealed class TaskGroupPriority
    {
        public string GroupName { get; set; } = "";
        public int Priority { get; set; }

        public override string ToString() => $"{Priority:D5}_{GroupName}";
    }
}