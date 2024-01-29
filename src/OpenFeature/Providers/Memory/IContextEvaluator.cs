using System;
using OpenFeature.Model;

namespace OpenFeature.Providers.Memory
{
    /// <summary>
    /// Context evaluator - use for resolving flag according to evaluation context, for handling targeting. 
    /// </summary>
    /// <typeparam name="T">expected value type</typeparam>
    public interface IContextEvaluator
    {
        ResolutionDetails<T> Evaluate<T>(string flagKey, T defaultValue, Flag flag, EvaluationContext evaluationContext);
    }

    // public interface IContextEvaluator<T>
    // {
    //     T Evaluate(Flag<?> flag, EvaluationContext evaluationContext);
    // }
    
    public sealed class DefaultContextEvaluator : IContextEvaluator
    {
        public readonly static DefaultContextEvaluator Instance = new DefaultContextEvaluator();

        public ResolutionDetails<T> Evaluate<T>(string flagKey, T defaultValue, Flag flag, EvaluationContext evaluationContext)
        {
            if (flag.Variants[flag.DefaultVariant] is T defaultVariantValue)
            {
                return new ResolutionDetails<T>(
                    flagKey,
                    defaultVariantValue,
                    // reason: $"flag {flagKey} not found",
                    variant: flag.DefaultVariant
                );
            }
            return new ResolutionDetails<T>(
                flagKey,
                defaultValue,
                reason: $"flag {flagKey} value is not of type {typeof(T).FullName}",
                variant: flag.DefaultVariant
            );
        }
    }
}