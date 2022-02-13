using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NftArtGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var traitsDir = @"< INPUT DIRECTORY - CONTAINS TRAITS DIRECTORY >";
            var outputDir = @"< OUTPUT DIRECTORY >";
            var maxSupply = 10;

            var traits = Directory
                .GetDirectories(traitsDir)
                .Select(traitDir => new Trait(
                    Path.GetFileName(traitDir),
                    Directory.GetFiles(traitDir).Select(f => Path.GetFileName(f)).ToArray()))
                .ToArray();

            var selections = new HashSet<TraitSelection>();
            var rnd = new Random();
            while (selections.Count < maxSupply)
            {
                selections.Add(new TraitSelection(traits.Select(t => new TraitValue(t.Name, t.Values[rnd.Next(t.Values.Length)])).ToArray()));
            }

            int id = 0;
            foreach (var selection in selections)
            {
                GenerateNft(id++, selection, traitsDir, outputDir);
            }
        }

        private static void GenerateNft(int id, TraitSelection selection, string traitsDir, string outputDir)
        {
            Console.Write($"Generating NFT #{id}...");
            var nft = Image.Load<Rgba32>(GetTraitPath(selection.Values[0], traitsDir));

            foreach (var trait in selection.Values.Skip(1))
            {
                if (trait.Value.Contains("null", StringComparison.InvariantCultureIgnoreCase)) continue;

                var layer = Image.Load<Rgba32>(GetTraitPath(trait, traitsDir));
                nft.Mutate(o => o.DrawImage(layer, new Point(0, 0), 1.0f));
            }

            nft.Save(Path.Join(outputDir, $"{id}.png"));
            Console.WriteLine("DONE");
        }

        private static string GetTraitPath(TraitValue trait, string traitsDir) => Path.Join(Path.Join(traitsDir, trait.Name), trait.Value);

        public record Trait(string Name, string[] Values);
        public record TraitValue(string Name, string Value);
        public class TraitSelection
        {
            public TraitValue[] Values { get; }

            public TraitSelection(TraitValue[] values)
            {
                Values = values;
            }

            #region Equality implementation.
            public override bool Equals(object? obj) => obj is TraitSelection && this == (TraitSelection)obj;
            public static bool operator ==(TraitSelection a, TraitSelection b) => a.Values.SequenceEqual(b.Values);
            public static bool operator !=(TraitSelection a, TraitSelection b) => !(a == b);
            public override int GetHashCode()
            {
                var hashCode = 0;
                foreach (var v in Values)
                {
                    hashCode ^= v.GetHashCode();
                }
                return hashCode;
            }
            #endregion
        }
    }
}
