using System.Text;

namespace MtgInventory.Service.Models
{
    public class QueryCardOptions
    {
        public string Name { get; set; }
        public bool IsBasicLand { get; set; }
        public bool IsToken { get; set; }

        public string SetName { get; set; }

        public bool IsSetName => !string.IsNullOrWhiteSpace(SetName) && !"All Sets".Equals(SetName);

        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append($"'{Name}'");

            if (IsBasicLand)
            {
                result.Append(", BasicLand=true");
            }
            
            if (IsToken)
            {
                result.Append(", Token=true");
            }

            if (IsSetName)
            {
                result.Append($", SetCode={SetName}");
            }

            return result.ToString();
        }
    }
}