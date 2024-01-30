using System;
using System.Collections.Generic;
using OpenFeature.Model;

namespace OpenFeature.Providers.Memory
{
    /// <summary>
    /// Flag representation for the in-memory provider.
    /// </summary>
    public class Flag
    {
        public string Key { get; }
        public Dictionary<string, object> Variants { get; }
        public string DefaultVariant { get; }
        public IContextEvaluator ContextEvaluator { get; }
        
        public Flag(string key, object value)
            : this(key, new Dictionary<string, object>{ {"default", value}}, "default", DefaultContextEvaluator.Instance)
        { }
        public Flag(string key, Dictionary<string, object> variants, string defaultVariant)
            : this(key, variants, defaultVariant, DefaultContextEvaluator.Instance)
        { }
        public Flag(string key, Dictionary<string, object> variants, string defaultVariant, IContextEvaluator contextEvaluator)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            if (variants is null)
                throw new ArgumentNullException(nameof(variants));
            if (defaultVariant is null)
                throw new ArgumentNullException(nameof(defaultVariant));
            if (string.IsNullOrWhiteSpace(defaultVariant))
                throw new ArgumentException(nameof(defaultVariant));
            if (contextEvaluator is null)
                throw new ArgumentNullException(nameof(contextEvaluator));
            this.Key = key;
            this.Variants = variants;
            this.DefaultVariant = defaultVariant;
            this.ContextEvaluator = contextEvaluator;
        }
        
        // Optional?
        public virtual ResolutionDetails<T> Evaluate<T>(T defaultValue, EvaluationContext evaluationContext)
            => this.ContextEvaluator.Evaluate<T>(this.Key, defaultValue, this, evaluationContext);
    }

    // public class Flag<T>
    // {
    //     public Dictionary<string, object> Variants {get; init;}
    //     public string DefaultVariant {get; init;}
    //     public IContextEvaluator<T>? ContextEvaluator {get; init;} = null;
    // }
}