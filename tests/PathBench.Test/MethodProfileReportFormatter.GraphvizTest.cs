using System.Runtime.CompilerServices;

namespace PathBench.Test;

public partial class MethodProfileReportFormatterGraphvizTest
{
    private static class PrivateAccess
    {
        public const string _typeName =
            $"{nameof(PathBench)}.{nameof(MethodProfileReportFormatter)}+GraphvizStyle_, PathBench";

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SanitizeIdentifier")]
        public extern static string SanitizeIdentifier(
            [UnsafeAccessorType(_typeName)] object? _,
            string input);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SanitizeLabel")]
        public extern static string SanitizeLabel(
            [UnsafeAccessorType(_typeName)] object? _,
            string input);
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
