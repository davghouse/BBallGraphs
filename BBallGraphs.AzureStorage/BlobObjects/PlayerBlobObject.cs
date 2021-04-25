using BBallGraphs.BasketballReferenceScraper;
using System;
using System.Linq;

namespace BBallGraphs.AzureStorage.BlobObjects
{
    public class PlayerBlobObject : IEquatable<PlayerBlobObject>, IComparable<PlayerBlobObject>
    {
        public PlayerBlobObject() { }

        public PlayerBlobObject(IPlayer player)
        {
            ID = player.ID;
            Name = player.Name;
            FirstSeason = player.FirstSeason;
            LastSeason = player.LastSeason;
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public int FirstSeason { get; set; }
        public int LastSeason { get; set; }

        public bool Equals(PlayerBlobObject other)
            => other != null
            && ID == other.ID
            && Name == other.Name
            && FirstSeason == other.FirstSeason
            && LastSeason == other.LastSeason;

        public override bool Equals(object obj)
            => Equals(obj as PlayerBlobObject);

        public override int GetHashCode()
            => throw new NotImplementedException();

        public int CompareTo(PlayerBlobObject other)
        {
            string lastName = Name.Split().Last();
            string otherLastName = other.Name.Split().Last();

            return lastName != otherLastName ? string.Compare(lastName, otherLastName, StringComparison.Ordinal)
                : Name != other.Name ? string.Compare(Name, other.Name, StringComparison.Ordinal)
                : string.Compare(ID, other.ID, StringComparison.Ordinal);
        }

        public override string ToString()
            => $"{Name} ({FirstSeason} - {LastSeason})";
    }
}
