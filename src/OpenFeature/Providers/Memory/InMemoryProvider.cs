using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Providers.Memory
{
    /// <summary>
    /// The in memory provider
    /// </summary>
    /// <seealso href="https://openfeature.dev/specification/appendix-a#in-memory-provider">In Memory Provider specification</seealso>
    public class InMemoryFeatureProvider : FeatureProvider {

        private readonly Metadata _metadata = new Metadata(InMemoryProvider.InMemoryProviderName);

        private Dictionary<string, Flag> _flags;

        private ProviderStatus _status = ProviderStatus.NotReady;

        /// <inheritdoc/>
        // public override IImmutableList<Hook> GetProviderHooks()
        // {
        //     return base.GetProviderHooks();
        // }

        /// <inheritdoc/>
        public override ProviderStatus GetStatus() => this._status;

        /// <inheritdoc/>
        public override Metadata GetMetadata() {
            return this._metadata;
        }

        public InMemoryFeatureProvider()
        { }

        public InMemoryFeatureProvider(string name)
        {
            _metadata = new Metadata(name);
        }

        public InMemoryFeatureProvider(IEnumerable<Flag> flags)
        {
            if (flags is null)
                throw new ArgumentNullException(nameof(flags));
            this._flags = flags.ToDictionary(flag => flag.Key);
        }

        /// <summary>
        /// Initialize the provider.
        /// </summary>
        /// <param name="evaluationContext">the evaluationContext</param>
        /// <exception cref="Exception">on error</exception>
        public override async Task Initialize(EvaluationContext evaluationContext)
        {
            _status = ProviderStatus.Ready;
            var @event = new ProviderEventPayload
            {
                Type = ProviderEventTypes.ProviderReady,
                ProviderName = _metadata.Name,
                FlagsChanged = new List<string>(),
                // Message = ???,
                // EventMetadata = ???
            };
            await this.EventChannel.Writer.WriteAsync(@event).ConfigureAwait(false);
            // TODO log
            // log.debug("finished initializing provider, state: {}", state);
        }

        /// <summary>
        /// Updating provider flags configuration, replacing existing flags.
        /// </summary>
        /// <param name="flags">the flags to use instead of the previous flags.</param>
        public async ValueTask UpdateFlags(IEnumerable<Flag> flags)
        {
            if (flags is null)
                throw new ArgumentNullException(nameof(flags));
            var newFlags = flags.ToDictionary(flag => flag.Key);
            var flagsChanged = new HashSet<string>(this._flags.Keys);
            flagsChanged.UnionWith(newFlags.Keys);
            this._flags = newFlags;
            var @event = new ProviderEventPayload
            {
                Type = ProviderEventTypes.ProviderConfigurationChanged,
                ProviderName = _metadata.Name,
                FlagsChanged = flagsChanged.ToList(),
                Message = "flags changed",
                // EventMetadata = ???
            };
            await this.EventChannel.Writer.WriteAsync(@event).ConfigureAwait(false);
        }

        /// <summary>
        /// Updating provider flags configuration with adding or updating a flag.
        /// </summary>
        /// <param name="flag">
        /// the flag to update. If a flag with this key already exists, new flag replaces it.
        /// </param>
        public async ValueTask UpdateFlag(Flag flag)
        {
            if (flag is null)
                throw new ArgumentNullException(nameof(flag));
            this._flags[flag.Key] = flag;
            var @event = new ProviderEventPayload
            {
                Type = ProviderEventTypes.ProviderConfigurationChanged,
                ProviderName = _metadata.Name,
                FlagsChanged = new List<string>{flag.Key},
                Message = "flag added/updated",
                // EventMetadata = ???
            };
            await this.EventChannel.Writer.WriteAsync(@event).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<bool>> ResolveBooleanValue(
            string flagKey,
            bool defaultValue,
            EvaluationContext context = null)
        {
            // ArgumentException.ThrowIfNullOrWhiteSpace(flagKey, nameof(flagKey));
            return Task.FromResult(GetEvaluation(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<string>> ResolveStringValue(
            string flagKey,
            string defaultValue,
            EvaluationContext context = null)
        {
            // ArgumentException.ThrowIfNullOrWhiteSpace(flagKey, nameof(flagKey));
            return Task.FromResult(GetEvaluation(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<int>> ResolveIntegerValue(
            string flagKey,
            int defaultValue,
            EvaluationContext context = null)
        {
            // ArgumentException.ThrowIfNullOrWhiteSpace(flagKey, nameof(flagKey));
            return Task.FromResult(GetEvaluation(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<double>> ResolveDoubleValue(
            string flagKey,
            double defaultValue,
            EvaluationContext context = null)
        {
            // ArgumentException.ThrowIfNullOrWhiteSpace(flagKey, nameof(flagKey));
            return Task.FromResult(GetEvaluation(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<Value>> ResolveStructureValue(
            string flagKey,
            Value defaultValue,
            EvaluationContext context = null)
        {
            // ArgumentException.ThrowIfNullOrWhiteSpace(flagKey, nameof(flagKey));
            return Task.FromResult(GetEvaluation(flagKey, defaultValue, context));
        }

        private ResolutionDetails<T> GetEvaluation<T>(string flagKey, T defaultValue, EvaluationContext context)
        {
            if (!ProviderStatus.Ready.Equals(this._status))
            {
                if (ProviderStatus.NotReady.Equals(this._status))
                {
                    // TODO? throw new ProviderNotReadyError("provider not yet initialized");
                    return new ResolutionDetails<T>(
                        flagKey,
                        defaultValue,
                        errorType: ErrorType.ProviderNotReady,
                        // reason: "provider not yet initialized",
                        errorMessage: "provider not yet initialized"
                        // TODO variant: not ready
                    );
                }
                // TODO? throw new GeneralError("unknown error");
                return new ResolutionDetails<T>(
                    flagKey,
                    defaultValue,
                    errorType: ErrorType.General,
                    // reason: "unknown error",
                    errorMessage: "unknown error"
                    // TODO variant: general error
                );
            }

            if (!this._flags.TryGetValue(flagKey, out var flag))
            {
                // TODO? throw new FlagNotFoundError("flag " + flagKey + "not found");
                return new ResolutionDetails<T>(
                    flagKey,
                    defaultValue,
                    errorType: ErrorType.FlagNotFound,
                    // reason: $"flag {flagKey} not found",
                    errorMessage: $"flag {flagKey} not found"
                    // TODO variant: general error
                );
            }
            
            // return flag.ContextEvaluator.Evaluate<T>(flagKey, defaultValue, flag, context);
            return flag.Evaluate(defaultValue, context);
       }
        
        // EXTRA
        private readonly List<Hook> _hooks = new List<Hook>();
        /// <inheritdoc/>
        public void AddHook(Hook hook) => this._hooks.Add(hook);

        /// <inheritdoc/>
        public override IImmutableList<Hook> GetProviderHooks() => this._hooks.ToImmutableList();
        /// <inheritdoc/>
        public void SetStatus(ProviderStatus status)
        {
            this._status = status;
        }
        internal void SendEvent(ProviderEventTypes eventType)
        {
            this.EventChannel.Writer.WriteAsync(new ProviderEventPayload { Type = eventType, ProviderName = this.GetMetadata().Name });
        }
    }
}
