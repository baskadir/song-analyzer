using System.Globalization;

var basePath = AppContext.BaseDirectory;

string inputFilePath = Path.Combine(basePath, "exhibitA-input.csv");
string outputFilePath = Path.Combine(basePath, "output.csv");

var clientSongs = ReadClientSongs(inputFilePath, new DateTime(2016, 8, 10));
var distribution = GetDistribution(clientSongs);
SaveDistributionToCsv(distribution, outputFilePath);

int userCount346 = CountUsersWithExactDistinctCount(clientSongs, 346);
Console.WriteLine($"Users who played 346 distinct songs: {userCount346}");

int maxDistinctSongs = GetMaxDistinctSongs(clientSongs);
Console.WriteLine($"Maximum number of distinct songs played: {maxDistinctSongs}");

Dictionary<string, HashSet<string>> ReadClientSongs(string filePath, DateTime targetDate)
{
    var culture = CultureInfo.InvariantCulture;
    var songs = new Dictionary<string, HashSet<string>>();

    using var reader = new StreamReader(filePath);
    while (!reader.EndOfStream)
    {
        var line = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(line)) continue;

        var parts = line.Split('\t');
        if (parts.Length < 4) continue;

        var songId = parts[1].Trim();
        var clientId = parts[2].Trim();
        var playTsStr = parts[3].Trim();

        if (!DateTime.TryParseExact(
                playTsStr,
                ["dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy"],
                culture,
                DateTimeStyles.None,
                out var playTs))
        {
            continue;
        }

        if (playTs.Date == targetDate)
        {
            if (!songs.ContainsKey(clientId))
            {
                songs[clientId] = new HashSet<string>();
            }
            songs[clientId].Add(songId);
        }
    }

    return songs;
}

IEnumerable<(int DISTINCT_PLAY_COUNT, int CLIENT_COUNT)> GetDistribution(Dictionary<string, HashSet<string>> songs)
{
    return songs
        .Select(kv => kv.Value.Count)
        .GroupBy(count => count)
        .Select(g => (DISTINCT_PLAY_COUNT: g.Key, CLIENT_COUNT: g.Count()))
        .OrderBy(x => x.DISTINCT_PLAY_COUNT);
}

void SaveDistributionToCsv(IEnumerable<(int DISTINCT_PLAY_COUNT, int CLIENT_COUNT)> dist, string outputPath)
{
    var outputLines = new List<string> { "DISTINCT_PLAY_COUNT,CLIENT_COUNT" };
    outputLines.AddRange(dist.Select(d => $"{d.DISTINCT_PLAY_COUNT},{d.CLIENT_COUNT}"));
    File.WriteAllLines(outputPath, outputLines);

    Console.WriteLine($"Distribution saved to: {outputPath}");
}

int CountUsersWithExactDistinctCount(Dictionary<string, HashSet<string>> songs, int distinctCount)
{
    return songs.Count(kv => kv.Value.Count == distinctCount);
}

int GetMaxDistinctSongs(Dictionary<string, HashSet<string>> songs)
{
    return songs.Values.Max(set => set.Count);
}