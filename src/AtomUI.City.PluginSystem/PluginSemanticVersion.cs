using System.Diagnostics.CodeAnalysis;

namespace AtomUI.City.PluginSystem;

internal sealed class PluginSemanticVersion : IComparable<PluginSemanticVersion>
{
    private PluginSemanticVersion(
        int major,
        int minor,
        int patch,
        IReadOnlyList<string> prerelease)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = Array.AsReadOnly(prerelease.ToArray());
    }

    public int Major { get; }

    public int Minor { get; }

    public int Patch { get; }

    public IReadOnlyList<string> Prerelease { get; }

    public int CompareTo(PluginSemanticVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        var coreComparison = Major.CompareTo(other.Major);
        if (coreComparison != 0)
        {
            return coreComparison;
        }

        coreComparison = Minor.CompareTo(other.Minor);
        if (coreComparison != 0)
        {
            return coreComparison;
        }

        coreComparison = Patch.CompareTo(other.Patch);
        if (coreComparison != 0)
        {
            return coreComparison;
        }

        return ComparePrerelease(other);
    }

    public static bool TryParse(string value, [NotNullWhen(true)] out PluginSemanticVersion? version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var buildSeparatorIndex = value.IndexOf('+', StringComparison.Ordinal);
        var valueWithoutBuild = buildSeparatorIndex < 0
            ? value
            : value[..buildSeparatorIndex];
        var build = buildSeparatorIndex < 0
            ? null
            : value[(buildSeparatorIndex + 1)..];

        if (build is not null && !IsValidIdentifierList(build, allowLeadingZeroNumbers: true))
        {
            return false;
        }

        var prereleaseSeparatorIndex = valueWithoutBuild.IndexOf('-', StringComparison.Ordinal);
        var core = prereleaseSeparatorIndex < 0
            ? valueWithoutBuild
            : valueWithoutBuild[..prereleaseSeparatorIndex];
        var prerelease = prereleaseSeparatorIndex < 0
            ? null
            : valueWithoutBuild[(prereleaseSeparatorIndex + 1)..];

        if (prerelease is not null && !IsValidIdentifierList(prerelease, allowLeadingZeroNumbers: false))
        {
            return false;
        }

        var coreParts = core.Split('.');
        if (coreParts.Length != 3 ||
            !TryParseCoreIdentifier(coreParts[0], out var major) ||
            !TryParseCoreIdentifier(coreParts[1], out var minor) ||
            !TryParseCoreIdentifier(coreParts[2], out var patch))
        {
            return false;
        }

        version = new PluginSemanticVersion(
            major,
            minor,
            patch,
            prerelease is null ? [] : prerelease.Split('.'));
        return true;
    }

    private int ComparePrerelease(PluginSemanticVersion other)
    {
        if (Prerelease.Count == 0 && other.Prerelease.Count == 0)
        {
            return 0;
        }

        if (Prerelease.Count == 0)
        {
            return 1;
        }

        if (other.Prerelease.Count == 0)
        {
            return -1;
        }

        var length = Math.Min(Prerelease.Count, other.Prerelease.Count);
        for (var i = 0; i < length; i++)
        {
            var comparison = ComparePrereleaseIdentifier(Prerelease[i], other.Prerelease[i]);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return Prerelease.Count.CompareTo(other.Prerelease.Count);
    }

    private static int ComparePrereleaseIdentifier(string left, string right)
    {
        var leftIsNumeric = IsNumericIdentifier(left);
        var rightIsNumeric = IsNumericIdentifier(right);
        if (leftIsNumeric && rightIsNumeric)
        {
            return CompareNumericIdentifier(left, right);
        }

        if (leftIsNumeric)
        {
            return -1;
        }

        if (rightIsNumeric)
        {
            return 1;
        }

        return string.Compare(left, right, StringComparison.Ordinal);
    }

    private static int CompareNumericIdentifier(string left, string right)
    {
        var trimmedLeft = left.TrimStart('0');
        var trimmedRight = right.TrimStart('0');
        trimmedLeft = trimmedLeft.Length == 0 ? "0" : trimmedLeft;
        trimmedRight = trimmedRight.Length == 0 ? "0" : trimmedRight;

        var lengthComparison = trimmedLeft.Length.CompareTo(trimmedRight.Length);
        return lengthComparison != 0
            ? lengthComparison
            : string.Compare(trimmedLeft, trimmedRight, StringComparison.Ordinal);
    }

    private static bool IsValidIdentifierList(string value, bool allowLeadingZeroNumbers)
    {
        return value.Length > 0 &&
            value
                .Split('.')
                .All(identifier => IsValidIdentifier(identifier, allowLeadingZeroNumbers));
    }

    private static bool IsValidIdentifier(string identifier, bool allowLeadingZeroNumbers)
    {
        if (identifier.Length == 0 ||
            identifier.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
        {
            return false;
        }

        return allowLeadingZeroNumbers ||
            !IsNumericIdentifier(identifier) ||
            IsValidNumericIdentifier(identifier);
    }

    private static bool TryParseCoreIdentifier(string identifier, out int value)
    {
        value = 0;
        return IsValidNumericIdentifier(identifier) &&
            int.TryParse(identifier, out value);
    }

    private static bool IsValidNumericIdentifier(string identifier)
    {
        return IsNumericIdentifier(identifier) &&
            (identifier.Length == 1 || identifier[0] != '0');
    }

    private static bool IsNumericIdentifier(string identifier)
    {
        return identifier.Length > 0 &&
            identifier.All(char.IsAsciiDigit);
    }
}
