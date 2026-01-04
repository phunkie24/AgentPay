using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 12: Prompt Chaining
/// Chains multiple prompts where output of one feeds into the next
/// </summary>
public class PromptChainingPattern
{
    private readonly ILLMService _llm;

    public PromptChainingPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<ChainResult> ExecuteAsync(List<ChainStep> steps, string initialInput)
    {
        var executedSteps = new List<ExecutedStep>();
        var currentInput = initialInput;

        foreach (var step in steps)
        {
            var prompt = step.PromptTemplate.Replace("{input}", currentInput);

            var response = await _llm.GenerateAsync(prompt, step.Options ?? new LLMOptions
            {
                Temperature = 0.5
            });

            var output = response.Text.Trim();

            executedSteps.Add(new ExecutedStep
            {
                StepName = step.Name,
                Input = currentInput,
                Output = output,
                Prompt = prompt
            });

            currentInput = output; // Output becomes next input
        }

        return new ChainResult
        {
            InitialInput = initialInput,
            ExecutedSteps = executedSteps,
            FinalOutput = currentInput,
            Timestamp = DateTime.UtcNow
        };
    }
}

public class ChainStep
{
    public string Name { get; set; }
    public string PromptTemplate { get; set; } // Use {input} as placeholder
    public LLMOptions? Options { get; set; }
}

public class ExecutedStep
{
    public string StepName { get; set; }
    public string Input { get; set; }
    public string Prompt { get; set; }
    public string Output { get; set; }
}

public class ChainResult
{
    public string InitialInput { get; set; }
    public List<ExecutedStep> ExecutedSteps { get; set; }
    public string FinalOutput { get; set; }
    public DateTime Timestamp { get; set; }
}
