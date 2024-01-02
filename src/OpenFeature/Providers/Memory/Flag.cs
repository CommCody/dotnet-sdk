
using System.Collections.Generic;
using OpenFeature.Model;

namespace OpenFeature.Providers.Memory
{
    /// <summary>
    /// Flag representation for the in-memory provider.
    /// </summary>
    public class Flag
    {
        public Dictionary<string, object> Variants {get;}
        public string DefaultVariant {get;}
        public IContextEvaluator ContextEvaluator {get;}
        
        public Flag(Dictionary<string, object> variants, string defaultVariant)
            : this(variants, defaultVariant, DefaultContextEvaluator.Instance)
        {
        }
        public Flag(Dictionary<string, object> variants, string defaultVariant, IContextEvaluator contextEvaluator)
        {
            ArgumentNullException.ThrowIfNull(variants, nameof(variants));
            ArgumentException.ThrowIfNullOrWhiteSpace(defaultVariant, nameof(defaultVariant));
            ArgumentNullException.ThrowIfNull(contextEvaluator, nameof(contextEvaluator));
            this.Variants = variants;
            this.DefaultVariant = defaultVariant;
            this.ContextEvaluator = contextEvaluator;
        }
        
        // Optional?
        public virtual ResolutionDetails<T> Evaluate<T>(EvaluationContext evaluationContext)
            => this.ContextEvaluator.Evaluate<T>(this, evaluationContext);
    }

    // public class Flag<T>
    // {
    //     public Dictionary<string, object> Variants {get; init;}
    //     public string DefaultVariant {get; init;}
    //     public IContextEvaluator<T>? ContextEvaluator {get; init;} = null;
    // }
}