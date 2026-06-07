using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;

namespace HRAssistant.Services
{
    public class AgentService : IAgentService
    {
        private readonly Kernel _kernel;
        public AgentService(Kernel kernel)
        {
            _kernel = kernel;
        }

        private AgentGroupChat BuildGroupChat()
        {
            var executionSettings = new PromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var sqlAgent = new ChatCompletionAgent
            {
                Name = "SqlAgent",
                Instructions = "Handle employee data and leave balance queries only. Return employee/user info and leave details",
                Kernel = _kernel,
                Arguments = new KernelArguments(executionSettings)
            };

            var policyAgent = new ChatCompletionAgent
            {
                Name = "PolicyAgent",
                Instructions = "Handle HR policy questions only",
                Kernel = _kernel,
                Arguments = new KernelArguments(executionSettings)
            };

            var actionAgent = new ChatCompletionAgent
            {
                Name = "ActionAgent",
                Instructions = "Handle leave applications. Always confirm details before applying.",
                Kernel = _kernel,
                Arguments = new KernelArguments(executionSettings)
            };

            KernelFunction selectionFunction = KernelFunctionFactory.CreateFromPrompt("""
                    Given this HR message, which agent should handle it?
                    - SqlAgent: leave balances, history, employee data
                    - PolicyAgent: HR rules, WFH policy, entitlements
                    - ActionAgent: applying leave, submitting requests
                    Message: {{$lastMessage}}
                    Respond with agent name only. No explanation.
                """);

            KernelFunction terminationFunction = KernelFunctionFactory.CreateFromPrompt("""
                    Has the HR request been fully answered?
                    Last response: {{$lastMessage}}
                    Reply yes or no only.
                """);

            return new AgentGroupChat(sqlAgent, policyAgent, actionAgent)
            {
                ExecutionSettings = new AgentGroupChatSettings
                {
                    SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, _kernel),
                    TerminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, _kernel)
                    {
                        MaximumIterations = 3
                    }
                }
            };
        }

        public async IAsyncEnumerable<string> GetChatResponse(string message, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var groupChat = BuildGroupChat();

            groupChat.AddChatMessage(new ChatMessageContent(
            AuthorRole.User, message));

            await foreach (var response in groupChat.InvokeStreamingAsync(cancellationToken))
            {
                if (response.Content is not null)
                    yield return response.Content;
            }
        }
    }
}
