namespace DCI_TASK_API.Interfaces
{
    public class ParamUpdateTaskStatus
    {
        public int taskId { get; set; } = 0;
        public bool taskAction { get; set; } = false;
        public string taskUpdateBy { get; set; } = "";
    }
}
