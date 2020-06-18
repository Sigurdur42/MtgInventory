using System;
using System.Globalization;
using LiteDB;

namespace MtgDomain
{
    public class SetReleaseDate : IEquatable<SetReleaseDate>, IComparable<SetReleaseDate>
    {
        private string _dateString;

        public SetReleaseDate()
        {
            AssignSortString();
        }

        public SetReleaseDate(DateTime? date)
        {
            // TODO: Others
            Date = date;
        }

        public DateTime? Date { get; set; }

        public string DateString
        {
            get => _dateString;
            set
            {
                _dateString = value;
                AssignSortString();
            }
        }

        [BsonIgnore]
        public string SortString { get; private set; }

        public bool Equals(SetReleaseDate other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Nullable.Equals(Date, other.Date);
        }

        public int CompareTo(SetReleaseDate other) => SortString.CompareTo(other.SortString);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((SetReleaseDate)obj);
        }

        public override int GetHashCode()
        {
            return Date.GetHashCode();
        }

        private void AssignSortString()
        {
            SortString = Date.HasValue
                ? Date.Value.ToString("o", CultureInfo.InvariantCulture)
                : "";
        }
    }
}