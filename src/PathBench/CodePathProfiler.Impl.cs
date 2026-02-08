using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PathBench;

partial class CodePathProfiler
{
    private sealed partial class Impl_ : CodePathProfiler
    {
        private readonly int _recentHistoryCacheSize;
        private readonly int _worstHistoryCacheSize;

        private ImmutableDictionary<string, MethodProfiler> _methodCounters = [];

        public override string? ClassName { get; }

        public override TimeProvider TimeProvider { get; }

        internal Impl_(
            string? className = null,
            CodePathProfilerOptions? options = null)
        {
            _recentHistoryCacheSize = (options ?? CodePathProfilerOptions.Default).RecentHistoryCacheSize;
            _worstHistoryCacheSize = (options ?? CodePathProfilerOptions.Default).WorstHistoryCacheSize;
            ClassName = className ?? new StackFrame(1, false).GetMethod()?.DeclaringType?.FullName;
            TimeProvider = options?.TimeProvider ?? TimeProvider.System;
        }

        public override InvocationProfiler StartMeasurement(
            [CallerMemberName] string methodName = "",
            object? argumentsExpressionProvider = null)
        {
            if(!_methodCounters.TryGetValue(methodName, out var methodCounter))
            {
                _methodCounters.Add(methodName, methodCounter = new(this, methodName));
            }
            var invocation = methodCounter.StartMeasurement(argumentsExpressionProvider);
            return invocation;
        }

        public override ImmutableDictionary<string, MethodProfileReport> CreateProfileReports()
        {
            var builder = ImmutableDictionary.CreateBuilder<string, MethodProfileReport>();
            foreach (var kvp in _methodCounters)
            {
                var report = kvp.Value.CreateReport();
                builder.Add(kvp.Key, report);
            }
            return builder.ToImmutable();
        }
    }
}
