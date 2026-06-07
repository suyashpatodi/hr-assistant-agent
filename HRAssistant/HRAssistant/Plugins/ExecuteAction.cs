using System.ComponentModel;

namespace HRAssistant.Plugins
{
    public class ExecuteAction
    {
        [KernelFunction, Description("Send email when user wants to apply for leave. Date range (from dd-mm-yyy to dd-mm-yyy) and reason expected. If any info not provided ask for it")]
        public string SendEmail([Description("Start date for leave range")] DateTime from,
                                [Description("ENd date for leave range")] DateTime to, [Description("Reason for applyting leave")] string reason)
        {
            return $"Leave applied from {from} to {to}: {reason}";
        }
    }
}
