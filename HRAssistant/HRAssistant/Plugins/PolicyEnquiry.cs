using System.ComponentModel;

namespace HRAssistant.Plugins
{
    public class PolicyEnquiry
    {
        [KernelFunction("leave_enquiry"), Description("Get information about leave policy for company")]
        public string LeaveEnquiry()
        {
            return "You can take 5 more leaves this year";
        }
    }
}
