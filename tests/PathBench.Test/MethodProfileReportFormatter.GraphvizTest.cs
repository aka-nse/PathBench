namespace PathBench.Test;

public class MethodProfileReportFormatterGraphvizTest
{
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
        var actual = MethodProfileReportFormatter.GraphvizStyle_.SanitizeIdentifier(input);
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
        var actual = MethodProfileReportFormatter.GraphvizStyle_.SanitizeLabel(input);
        Assert.Equal(expected, actual);
    }
}
