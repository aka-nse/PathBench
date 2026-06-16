using System.Reflection;
using System.Runtime.CompilerServices;

namespace PathBench.Test;

public partial class MethodProfileReportFormatterGraphvizTest
{
    private static class PrivateAccess
    {
        public const string _typeName =
            $"{nameof(PathBench)}.{nameof(MethodProfileReportFormatter)}+GraphvizStyle_, PathBench";

#if NET10_0_OR_GREATER

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SanitizeIdentifier")]
        public extern static string SanitizeIdentifier(
            [UnsafeAccessorType(_typeName)] object? _,
            string input);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SanitizeLabel")]
        public extern static string SanitizeLabel(
            [UnsafeAccessorType(_typeName)] object? _,
            string input);

#else
        private static readonly Type _GraphvizStyle_ =
            typeof(MethodProfileReportFormatter)
            .GetNestedType("GraphvizStyle_", BindingFlags.NonPublic)!;

        private static readonly MethodInfo _SanitizeIdentifier =
            _GraphvizStyle_.GetMethod("SanitizeIdentifier", BindingFlags.Public | BindingFlags.Static)!;

        private static readonly MethodInfo _SanitizeLabel =
            _GraphvizStyle_.GetMethod("SanitizeLabel", BindingFlags.Public | BindingFlags.Static)!;

        public static string SanitizeIdentifier(object? _, string input)
        {
            return (string)_SanitizeIdentifier.Invoke(null, [input])!;
        }

        public static string SanitizeLabel(object? _, string input)
        {
            return (string)_SanitizeLabel.Invoke(null, [input])!;
        }

#endif
    }

    public static TheoryData<string, string> SanitizeIdentifierTestCases() =>
        new()
        {
            { "", "__" },
            { "foobar", "__foobar" },
            { "123", "__123" },
            { "foo bar", "__foo_u0020_bar" },
            { "foo-bar", "__foo_u002D_bar" },
            { "foo.bar", "__foo_u002E_bar" },
            { "foo@!bar", "__foo_u0040__u0021_bar" },
            { "foo\nbar", "__foo_u000A_bar" },
            { "foo\tbar", "__foo_u0009_bar" },
            { "foo/bar\\baz", "__foo_u002F_bar_u005C_baz" },
            { "ほげほげ", "___u307B__u3052__u307B__u3052_" },
        };

    [Theory, MemberData(nameof(SanitizeIdentifierTestCases))]
    public void SanitizeIdentifier(string input, string expected)
    {
        var actual = PrivateAccess.SanitizeIdentifier(null, input);
        Assert.Equal(expected, actual);
    }


    public static TheoryData<string, string> SanitizeLabelTestCases() =>
        new()
        {
            { "", "" },
            { "foobar", "foobar" },
            { "123", "123" },
            { "foo bar", "foo bar" },
            { "foo-bar", "foo-bar" },
            { "foo.bar", "foo.bar" },
            { "foo@!bar", "foo@!bar" },
            { "foo\nbar", "foo\\nbar" },
            { "foo\tbar", "foo\\tbar" },
            { "foo\\bar", @"foo\\bar" },
            { "foo \"bar\" baz", "foo \\\"bar\\\" baz" },
        };

    [Theory, MemberData(nameof(SanitizeLabelTestCases))]
    public void SanitizeLabel(string input, string expected)
    {
        var actual = PrivateAccess.SanitizeLabel(null, input);
        Assert.Equal(expected, actual);
    }
}
