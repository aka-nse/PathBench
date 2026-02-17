using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace PathBench;

partial class CodePathProfiler
{
    /// <summary>
    /// Gets a null implementation of <see cref="CodePathProfiler"/>.
    /// </summary>
    public static CodePathProfiler Empty { get; } = new Empty_();

    private sealed class Empty_ : CodePathProfiler
    {
        private sealed class InvocationProfiler_ : InvocationProfiler
        {
            public override void Dispose()
            {
                // No-op
            }

            public override void MarkCheckpoint(string name, int orderingKey, object? noteProvider = null)
            {
                // No-op
            }

            protected internal override InvocationMeasurementReport CreateMeasurementReport() =>
                new(
                    CounterName: string.Empty,
                    InvocationId: 0,
                    StartAt: default,
                    ManagedThreadId: default,
                    ArgumentsExpression: string.Empty,
                    Duration: default,
                    CodePathMeasurements: []);
        }
        private static readonly InvocationProfiler_ _instance = new ();

        public override string? ClassName => null;

        public override TimeProvider TimeProvider => TimeProvider.System;

        public Empty_()
        {
        }

        public override ImmutableDictionary<string, MethodProfileReport> CreateProfileReports() =>
            [];

        public override InvocationProfiler StartMeasurement(
            [CallerMemberName] string methodName = "",
            object? argumentsExpressionProvider = null) =>
            _instance;
    }
}
