using System.ComponentModel;

namespace HRAssistant.Plugins
{
    public class SqlEnquiry
    {
        [KernelFunction("get_info"), Description("Get user info")]
        public string GetInfo()
        {
            return "Suyash Patodi";
        }
    }
}
