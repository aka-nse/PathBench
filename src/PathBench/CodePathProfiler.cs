using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace PathBench;

/// <summary>
/// Performance measurement tool for counting code paths.
/// One instance of this type should be related to one type definition.
/// </summary>
public abstract partial class CodePathProfiler
{
    /// <summary> The name of the start checkpoint. </summary>
    public const string StartCheckpointName = "#<start>";

    /// <summary> The name of the end checkpoint. </summary>
    public const string EndCheckpointName = "#<end>";

    /// <summary> The name of the class which this instance related to. </summary>
    public abstract string? ClassName { get; }

    /// <summary> The time provider used for measuring time. </summary>
    public abstract TimeProvider TimeProvider { get; }

    /// <summary>
    /// Creates a new instance of <see cref="CodePathProfiler"/>.
    /// </summary>
    /// <param name="className"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static CodePathProfiler Create(
        string? className = null,
        CodePathProfilerOptions? options = null) =>
        new Impl_(className, options);


    /// <summary>
    /// Starts performance measurement of one invocation of the method.
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="argumentsExpressionProvider"></param>
    /// 
    /// <returns></returns>
    public abstract InvocationProfiler StartMeasurement(
        [CallerMemberName] string methodName = "",
        object? argumentsExpressionProvider = null);


    /// <summary>
    /// Creates profile reports for all measured methods.
    /// </summary>
    /// <returns></returns>
    public abstract ImmutableDictionary<string, MethodProfileReport> CreateProfileReports();
}
