using OpenFeature.Model;

namespace OpenFeature.Providers.Memory
{
    /// <summary>
    /// Context evaluator - use for resolving flag according to evaluation context, for handling targeting. 
    /// </summary>
    /// <typeparam name="T">expected value type</typeparam>
    public interface IContextEvaluator
    {
        ResolutionDetails<T> Evaluate<T>(Flag flag, EvaluationContext evaluationContext);
    }

    // public interface IContextEvaluator<T>
    // {
    //     T Evaluate(Flag<?> flag, EvaluationContext evaluationContext);
    // }
    
    public sealed class DefaultContextEvaluator : IContextEvaluator
    {
        public static DefaultContextEvaluator Instance = new();
        public ResolutionDetails<T> Evaluate<T>(Flag flag, EvaluationContext evaluationContext)
        {
            if (flag.Variants[flag.DefaultVariant] is T defaultVariantValue)
            {
                return new ResolutionDetails<T>(
                    flagKey,
                    defaultVariantValue,
                    // reason: $"flag {flagKey} not found",
                    variant: flag.DefaultVariant
                );
                value = defaultVariantValue;
            }
            return new ResolutionDetails<T>(
                flagKey,
                defaultValue,
                reason: $"flag {flagKey} value is not of type {T.FullName}",
                variant: flag.DefaultVariant
            );
        }
    }
}