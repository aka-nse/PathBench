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

        private readonly object _lockToken = new();
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
            // NOTE:
            //   Double-checked locking pattern is used here to reduce locking overhead.
            //   For optimization of hot path, we don't use memory barrier on the first read.
            if (!_methodCounters.TryGetValue(methodName, out var methodCounter))
            {
                lock(_lockToken)
                {
                    var methodCounters = Volatile.Read(ref _methodCounters);
                    if (!methodCounters.TryGetValue(methodName, out methodCounter))
                    {
                        methodCounter = new(this, methodName);
                        _methodCounters = methodCounters.Add(methodName, methodCounter);
                    }
                }
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
